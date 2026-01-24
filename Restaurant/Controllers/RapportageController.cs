using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Data.UnitOfWork;
using Restaurant.ViewModels;
using Restaurant.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

// --- ITEXT7 PACKAGES ---
// Deze bibliotheken gebruik ik voor het genereren van de PDF
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
// Nodig om standaard fonts (zoals Helvetica Bold) correct in te laden
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Eigenaar")]
    public class RapportageController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public RapportageController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // Toont de lege pagina waar je een rapport kunt kiezen (nieuwe lege tablad)
        [HttpGet]
        public IActionResult Dashboard()
        {
            return View(new RapportageDashboardViewModel());
        }

        // Verwerkt de knop 'Genereer Rapport' en toont de tabel op het scherm
        [HttpPost]
        public async Task<IActionResult> Dashboard(RapportageDashboardViewModel model)
        {
            model = await HaalRapportDataOp(model);
            return View(model);
        }

        //  DE PDF METHODE (iText7) 
        // Zorgt ervoor dat het rapport gedownload wordt als PDF bestand
        [HttpPost]
        public async Task<IActionResult> DownloadPdf(RapportageDashboardViewModel model)
        {
            // 1. Data ophalen (hergebruikt dezelfde logica als het dashboard)
            model = await HaalRapportDataOp(model);

            // Veiligheidscheck: Als er geen data is, sturen we de gebruiker terug met een foutmelding
            if (!model.HeeftData)
            {
                TempData["Error"] = "Geen data om te exporteren.";
                return RedirectToAction(nameof(Dashboard));
            }

            // 2. PDF Maken in het geheugen (MemoryStream)
            // We gebruiken 'using' zodat het geheugen netjes wordt opgeruimd na gebruik
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                //  OPLOSSING VOOR SETBOLD ERROR /////////////////////////////////////////////////////
                // De standaard .SetBold() werkt niet altijd in nieuwere versies van iText7.
                // Daarom maken we hier expliciet een 'Bold' font object aan dat we later toewijzen.
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // --- HEADER ---
                // Titel van het rapport (groot, blauw en dikgedrukt)
                document.Add(new Paragraph($"Rapport: {model.Type}")
                    .SetFontSize(20)
                    .SetFont(boldFont) // Hier gebruiken we ons font object
                    .SetFontColor(ColorConstants.BLUE));

                // Algemene info toevoegen
                document.Add(new Paragraph($"Periode: {model.Periode}"));
                document.Add(new Paragraph($"Gegenereerd op: {DateTime.Now:dd/MM/yyyy HH:mm}"));
                document.Add(new Paragraph("\n")); // Witregel voor de netheid

                // --- TABEL BOUWEN ---
                // We checken welk type rapport het is om de juiste kolommen te maken
                if (model.Type == RapportType.Omzet)
                {
                    // Tabel met 5 kolommen. De float array (3, 4, 1...) bepaalt de breedteverhouding van de kolommen.
                    Table table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 4, 1, 2, 2 })).UseAllAvailableWidth();

                    // Headers (Met ons custom boldFont)
                    table.AddHeaderCell(new Paragraph("Tijd").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Product").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Aantal").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Prijs/st").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Totaal").SetFont(boldFont));

                    // Data Rijen invullen
                    foreach (var item in model.Bestellingen)
                    {
                        table.AddCell(item.Tijdstip?.ToString("dd/MM HH:mm") ?? "-");
                        table.AddCell(item.ProductNaam ?? "Onbekend");
                        table.AddCell(item.Aantal.ToString());
                        table.AddCell($"€ {item.EenheidsPrijs:N2}");
                        table.AddCell($"€ {item.TotaalPrijs:N2}");
                    }

                    document.Add(table);

                    // Totaalbedrag onderaan de tabel
                    document.Add(new Paragraph($"\nTOTALE OMZET: € {model.TotaleOmzet:N2}")
                        .SetFont(boldFont)
                        .SetFontSize(14));
                }
                else if (model.Type == RapportType.Reservaties)
                {
                    // Tabel met 3 kolommen voor reservaties
                    Table table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 4, 2 })).UseAllAvailableWidth();

                    // Headers
                    table.AddHeaderCell(new Paragraph("Datum & Tijd").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Klant").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Personen").SetFont(boldFont));

                    // Data Rijen
                    foreach (var item in model.Reservaties)
                    {
                        var naam = item.CustomUser != null
                            ? $"{item.CustomUser.Voornaam} {item.CustomUser.Achternaam}"
                            : "Onbekend";

                        table.AddCell(item.Datum?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                        table.AddCell(naam);
                        table.AddCell($"{item.AantalPersonen} pers.");
                    }

                    document.Add(table);
                    document.Add(new Paragraph($"\nAantal reservaties: {model.Reservaties.Count}"));
                }

                else if (model.Type == RapportType.Feedback)
                {
                    // Tabel met 4 kolommen: Datum, Klant, Sterren, Opmerking
                    Table table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 3, 2, 5 })).UseAllAvailableWidth();

                    // Headers
                    table.AddHeaderCell(new Paragraph("Datum").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Klant").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Score").SetFont(boldFont));
                    table.AddHeaderCell(new Paragraph("Opmerking").SetFont(boldFont));

                    foreach (var item in model.Reservaties)
                    {
                        var naam = item.CustomUser != null
                            ? $"{item.CustomUser.Voornaam} {item.CustomUser.Achternaam}"
                            : "Anoniem";

                        table.AddCell(item.Datum?.ToString("dd/MM/yyyy") ?? "-");
                        table.AddCell(naam);
                        table.AddCell($"{item.EvaluatieAantalSterren} / 5");
                        table.AddCell(item.EvaluatieOpmerkingen ?? "-");
                    }

                    document.Add(table);

                    // Gemiddelde berekenen voor de samenvatting
                    if (model.Reservaties.Any())
                    {
                        double gemiddelde = model.Reservaties.Average(r => r.EvaluatieAantalSterren);
                        document.Add(new Paragraph($"\nGemiddelde score: {gemiddelde:F1} sterren")
                            .SetFont(boldFont)
                            .SetFontSize(12));
                    }
                }

                // 3. Document sluiten (belangrijk!!1!)
                document.Close();

                // We zetten de stream om naar een byte array en sturen dit naar de browser
                // Hierdoor opent de browser (dankzij formtarget="_blank") de PDF
                return File(stream.ToArray(), "application/pdf", $"Rapport_{model.Type}_{DateTime.Now:yyyyMMdd}.pdf");
            }
        }

        // Hulp methode: Hier zit de logica voor het filteren van datums en ophalen uit de DB
        private async Task<RapportageDashboardViewModel> HaalRapportDataOp(RapportageDashboardViewModel model)
        {
            DateTime now = DateTime.Now;
            DateTime van, tot;

            // Periode berekenen
            switch (model.Periode)
            {
                case RapportPeriode.Week:
                    // Berekent de maandag van de huidige week
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    van = now.Date.AddDays(-1 * diff);
                    // Tot zondagavond laat
                    tot = van.AddDays(7).AddTicks(-1);
                    break;

                case RapportPeriode.Maand:
                    // Eerste dag van de maand
                    van = new DateTime(now.Year, now.Month, 1);
                    // Laatste dag van de maand (door 1 maand op te tellen en 1 tick terug te gaan)
                    tot = van.AddMonths(1).AddTicks(-1);
                    break;

                case RapportPeriode.Dag:
                default:
                    // Vandaag 00:00
                    van = now.Date;
                    // Vandaag 23:59:59 (AddTicks(-1) zorgt dat we de hele dag meepakken)
                    tot = now.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            // Ophalen en filteren van RESERVATIES
            if (model.Type == RapportType.Reservaties)
            {
                var alleReservaties = await _uow.ReservatiesRepository.GetAllAsync();
                model.Reservaties = alleReservaties
                    .Where(r => r.Datum.HasValue && r.Datum.Value >= van && r.Datum.Value <= tot)
                    .OrderBy(r => r.Datum).ToList();
                model.HeeftData = model.Reservaties.Any();
            }
            // Ophalen en filteren van OMZET (Bestellingen)
            else if (model.Type == RapportType.Omzet)
            {
                var alleBestellingen = await _uow.BestellingRepository.GetAllAsync();
                var gefilterd = alleBestellingen
                    .Where(b => b.TijdstipBestelling.HasValue && b.TijdstipBestelling.Value >= van && b.TijdstipBestelling.Value <= tot && b.Product != null)
                    .ToList();

                // Mapping naar viewmodel items
                model.Bestellingen = gefilterd.Select(b =>
                {
                    // Historische prijs ophalen:
                    // We zoeken de prijs die geldig was OP HET MOMENT van bestelling.
                    // Zo voorkomen we dat prijsverhogingen in de toekomst de oude omzetcijfers veranderen.
                    var prijs = b.Product.PrijsProducten?
                        .Where(p => p.DatumVanaf <= (b.TijdstipBestelling ?? DateTime.Now))
                        .OrderByDescending(p => p.DatumVanaf)
                        .FirstOrDefault()?.Prijs ?? 0;

                    return new BestellingRapportItem
                    {
                        BestellingId = b.Id,
                        ProductNaam = b.Product.Naam ?? "Onbekend",
                        Aantal = b.Aantal,
                        EenheidsPrijs = prijs,
                        TotaalPrijs = b.Aantal * prijs,
                        Tijdstip = b.TijdstipBestelling
                    };
                }).ToList();

                model.TotaleOmzet = model.Bestellingen.Sum(b => b.TotaalPrijs);
                model.HeeftData = model.Bestellingen.Any();
            }
            else if (model.Type == RapportType.Feedback)
            {
                // 1. Haal alle reservaties op. 
                var alleReservaties = await _uow.ReservatiesRepository.GetAllAsync();

                model.Reservaties = alleReservaties
                    .Where(r => r.Datum.HasValue &&
                                r.Datum.Value >= van &&
                                r.Datum.Value <= tot &&
                                r.EvaluatieAantalSterren > 0) // Filtert de 0-scores uit 
                    .OrderByDescending(r => r.Datum)
                    .ToList();

                model.HeeftData = model.Reservaties.Any();
            }

            return model;
        }
    }
}