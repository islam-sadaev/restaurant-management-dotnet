using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ViewModels
{
    public class ReservatieMetTijdSlotViewModel
    {
        public int Id { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? Datum { get; set; }

        [DisplayName("Aantal personen")]
        public int AantalPersonen { get; set; }

        [ForeignKey("TijdSlot")]
        public int TijdSlotId { get; set; }

        [DisplayName("Tijdslot")]
        public string? Naam { get; set; }
    }
}