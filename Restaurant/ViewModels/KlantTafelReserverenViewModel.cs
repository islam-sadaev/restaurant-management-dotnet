using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ViewModels
{
    public class KlantTafelReserverenViewModel
    {
        [Column(TypeName = "date")]
        [Required(ErrorMessage = "De datum is verplicht.")]
        public DateTime? Datum { get; set; }
        [Display(Name = "Aantal personen")]

        [Required(ErrorMessage = "Het aantal personen is verplicht.")]
        public int AantalPersonen { get; set; }

        [ForeignKey("TijdSlot")]
        [Display(Name = "Tijdslot")]

        [Required(ErrorMessage = "U moet een tijdslot kiezen.")]
        public int TijdSlotId { get; set; }

        public string? Opmerking { get; set; }

        public IEnumerable<SelectListItem>? BeschikbareTijdsloten { get; set; }
    }
}