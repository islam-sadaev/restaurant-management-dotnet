using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Restaurant.ViewModels
{
    public class ParameterViewModel
    {
        // defautls als je iets verwijderd komen deze erin te staan

        public static readonly Dictionary<string, string> Defaults = new Dictionary<string, string>
        {
            // --- Algemeen ---
            { "RestaurantNaam", "The Canopy" },
            { "RestaurantAdres", "Korenmarkt 15, 2800 Mechelen" },
            { "RestaurantEmail", "info@thecanopy.be" },
            { "RestaurantTelefoon", "+32 15 12 34 56" },

            // --- Cijfers ---
            { "MaxReservatiesDagelijks", "50" },
            { "MaxPersonenPerReservatie", "10" },
            { "MinPersonenPerReservatie", "1" },
            { "AnnulatieTermijn", "24" }, // Uren
            { "MaxDagenVooruitReserveren", "90" },

            // --- Communicatie ---
            { "WelkomstmailVerzendDagen", "1" },
            { "EvaluatiemailVerzendDagen", "1" },
            { "EmailVerzendingActief", "True" },
            { "ReservatieBevestigingVereist", "True" },

            // --- Financieel ---
            { "BTWPercentage", "21" },
            { "ServiceKostPercentage", "0" },

            // --- Openingstijden (Formaat HH:mm) ---
            { "OpeningstijdWeekdag", "17:00" },
            { "SluitingstijdWeekdag", "23:00" },
            { "OpeningstijdWeekend", "12:00" },
            { "SluitingstijdWeekend", "24:00" },
            { "KeukenSluitingstijd", "21:30" }
        };

        public int Id { get; set; }

        public string? RestaurantNaam { get; set; }

        public string? RestaurantAdres { get; set; }

        [DataType(DataType.EmailAddress)]
        public string? RestaurantEmail { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string? RestaurantTelefoon { get; set; }

        public int? MaxReservatiesDagelijks { get; set; }

        public int? MaxPersonenPerReservatie { get; set; }

        public int? MinPersonenPerReservatie { get; set; }

        public int? AnnulatieTermijn { get; set; }

        public int? MaxDagenVooruitReserveren { get; set; }

        public int? WelkomstmailVerzendDagen { get; set; }

        public int? EvaluatiemailVerzendDagen { get; set; }

        [Range(0, 100)]
        public decimal? BTWPercentage { get; set; }

        [Range(0, 100)]
        public decimal? ServiceKostPercentage { get; set; }

        public bool EmailVerzendingActief { get; set; }

        public bool ReservatieBevestigingVereist { get; set; }

        [DataType(DataType.Time)]
        public string? OpeningstijdWeekdag { get; set; }

        [DataType(DataType.Time)]
        public string? SluitingstijdWeekdag { get; set; }

        [DataType(DataType.Time)]
        public string? OpeningstijdWeekend { get; set; }

        [DataType(DataType.Time)]
        public string? SluitingstijdWeekend { get; set; }

        [DataType(DataType.Time)]
        public string? KeukenSluitingstijd { get; set; }
    }
}