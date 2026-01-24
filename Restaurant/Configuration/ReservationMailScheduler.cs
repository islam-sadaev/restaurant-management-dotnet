using Microsoft.EntityFrameworkCore; // Nodig voor Includes als je die in de repository gebruikt
using Restaurant.Configuration.MailService;
using Restaurant.Models; // Nodig voor Reservatie en CustomUser modellen
using Restaurant.Data; // Aangenomen dat je RestaurantContext hier zit (voor DB access binnen scope)

namespace Restaurant.Configuration
{
    // ReservationMailScheduler.cs

    // IHostedService / BackgroundService is een Singleton
    public class ReservationMailScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReservationMailScheduler> _logger;

        private readonly TimeSpan _delay = TimeSpan.FromHours(24); // Voer elke 24 uur uit
        //private readonly TimeSpan _delay = TimeSpan.FromSeconds(30); is voor testen

        // Alleen Singleton services (zoals ILogger en IServiceProvider) mogen hier staan.
        public ReservationMailScheduler(IServiceProvider serviceProvider, ILogger<ReservationMailScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reservatie Mail Scheduler draait.");

            // ... (Logica om de starttijd in te stellen, bijvoorbeeld 01:00 uur 's nachts) ...
            var now = DateTime.Now;
            // Dit zorgt ervoor dat de service wacht tot 01:00 uur de volgende dag.
            var nextRunTime = now.Date.AddDays(1).AddHours(1);
            var initialDelay = nextRunTime - now;

            if (initialDelay < TimeSpan.Zero)
            {
                nextRunTime = nextRunTime.AddDays(1);
                initialDelay = nextRunTime - now;
            }

            await Task.Delay(initialDelay, stoppingToken);

            // await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // Geef de app 5 seconden om op te starten

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starten van de Welkomstmail taak.");

                // Belangrijk: Creëer een scope en haal de services op.
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

                    // Roep de methode aan en geef de services mee.
                    await SendMails(context, emailSender);
                }

                _logger.LogInformation("Welkomstmail taak voltooid. Wachten op volgende cyclus.");
                await Task.Delay(_delay, stoppingToken); // Wacht 24 uur
            }
        }

        private async Task SendMails(IUnitOfWork context, IEmailSender emailSender)
        {
            // De datum die we over 7 dagen verwachten.
            DateTime sevenDaysFromNow = DateTime.Now.Date.AddDays(7);

            _logger.LogInformation($"Start zoeken naar reserveringen voor {sevenDaysFromNow:yyyy-MM-dd}.");

            var reservaties = await context.ReservatiesRepository.GetAllReservatieMetTijdSlotAsync();

            var aankomendeReserveringen = reservaties
                .Where(r => r.Datum.HasValue && r.Datum.Value.Date <= sevenDaysFromNow && r.WelkomstmailVerstuurdOp == null)
                .ToList();

            // Controleer of er reserveringen zijn gevonden.
            if (!aankomendeReserveringen.Any())
            {
                _logger.LogInformation("Geen reserveringen gevonden waarvoor een welkomstmail verstuurd moet worden.");
                return;
            }

            // Stap 2: Haal de welkomstmail-template op (via ID 2)
            var welkomstTemplate = await context.MailRepository.GetByIdAsync(2);

            if (welkomstTemplate == null)
            {
                _logger.LogError("Welkomstmail template met ID 2 niet gevonden. Mailversturing geannuleerd.");
                return;
            }

            // Stap 3: Loop door de reserveringen en verstuur de mail
            foreach (var reservering in aankomendeReserveringen)
            {
                try
                {
                    // Check of klant en email bestaan
                    if (reservering.CustomUser == null || string.IsNullOrEmpty(reservering.CustomUser.Email))
                    {
                        _logger.LogWarning($"Reservatie {reservering.Id} heeft geen CustomUser of E-mail. Overslaan.");
                        continue;
                    }

                    // Haal de tijd (string of object) op.
                    string gereserveerdeTijd = reservering.Tijdslot != null ? reservering.Tijdslot.Naam : "onbekende tijd";
                    string klantNaam = reservering.CustomUser.UserName ?? "Gast";
                    string ontvanger = reservering.CustomUser.Email;

                    // Vervang de placeholders in de mailbody
                    string gepersonaliseerdeBody = welkomstTemplate.Body
                        .Replace("[VOORNAAM]", klantNaam)
                        .Replace("[DATUM]", reservering.Datum.Value.ToShortDateString())
                        .Replace("[TIJD]", gereserveerdeTijd)
                        .Replace("[AANTAL]", reservering.AantalPersonen.ToString());

                    // Verstuur de mail
                    emailSender.SendEmailAsync(ontvanger, welkomstTemplate.Onderwerp, gepersonaliseerdeBody);

                    // Markeer de reservering als verzonden met de huidige datum
                    reservering.WelkomstmailVerstuurdOp = DateTime.Now.Date;
                    context.ReservatiesRepository.Update(reservering);

                    _logger.LogInformation($"Welkomstmail succesvol verstuurd naar {ontvanger} voor reservering {reservering.Id}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Fout bij het versturen van welkomstmail voor reservering {reservering.Id} naar {reservering.CustomUser?.Email}.");
                }
            }

            // Stap 4: Sla de wijzigingen op in de database
            await context.SaveChangesAsync();
            _logger.LogInformation("Welkomstmail taak voltooid. Database wijzigingen opgeslagen.");
        }
    }
}