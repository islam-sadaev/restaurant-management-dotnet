using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ViewModels
{
    public class BevestigingsOverzichtMetIDViewModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("CustomUser")]
        public string KlantId { get; set; }

        [Column(TypeName = "date")]
        [Required(ErrorMessage = "De datum is verplicht.")]
        public DateTime? Datum { get; set; }

        [Required(ErrorMessage = "Het aantal personen is verplicht.")]
        public int AantalPersonen { get; set; }

        [ForeignKey("TijdSlot")]
        [Required(ErrorMessage = "U moet een tijdslot kiezen.")]
        public int TijdSlotId { get; set; }

        public string? Opmerking { get; set; }

        public string? Tijdslot { get; set; }
    }
}