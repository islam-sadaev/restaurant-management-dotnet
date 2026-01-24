using System.ComponentModel.DataAnnotations;
using Restaurant.Models;

namespace Restaurant.ViewModels
{
    public enum RapportType
    {
        Reservaties,
        Omzet,
        Feedback
    }

    public enum RapportPeriode
    {
        Dag,
        Week,
        Maand
    }

    public class BestellingRapportItem
    {
        public int BestellingId { get; set; }
        public string ProductNaam { get; set; } = string.Empty;
        public int Aantal { get; set; }
        public decimal EenheidsPrijs { get; set; }
        public decimal TotaalPrijs { get; set; }
        public DateTime? Tijdstip { get; set; }
    }

    public class RapportageDashboardViewModel
    {
        public RapportType Type { get; set; }
        public RapportPeriode Periode { get; set; }
        public bool HeeftData { get; set; }
        public List<Reservatie>? Reservaties { get; set; }
        public List<BestellingRapportItem>? Bestellingen { get; set; }
        public decimal TotaleOmzet { get; set; }

        // Bepaal de periode op basis van de geselecteerde optie
        public (DateTime van, DateTime tot) BepaalPeriode()
        {
            var eindDatum = DateTime.Now; // Tot nu
            var startDatum = DateTime.Today;

            switch (Periode)
            {
                case RapportPeriode.Dag:
                    startDatum = DateTime.Today; // Vandaag 00:00
                    break;

                case RapportPeriode.Week:
                    startDatum = DateTime.Today.AddDays(-7); // 7 dagen terug
                    break;

                case RapportPeriode.Maand:
                    startDatum = DateTime.Today.AddMonths(-1); // 1 maand terug
                    break;
            }

            return (startDatum, eindDatum);
        }
    }
}