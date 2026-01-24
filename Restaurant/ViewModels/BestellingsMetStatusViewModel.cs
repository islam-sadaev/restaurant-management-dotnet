using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ViewModels
{
    public class BestellingsMetStatusViewModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Reservatie")]
        [Required(ErrorMessage = "U kunt niets bestellen zonder reservatie")]
        public int ReservatieId { get; set; }

        [DisplayName("Product")]
        public string ProductNaam { get; set; }

        public int Aantal { get; set; }
        public string? Opmerking { get; set; }

        [Column(TypeName = "datetime")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        [DisplayName("Tijd")]
        public DateTime? TijdstipBestelling { get; set; }

        [ForeignKey("Status")]
        [DisplayName("Status")]
        public int StatusId { get; set; }

        public int TypeId { get; set; }

        public string Naam { get; set; }

        public List<string> Tafels { get; set; } //dit zijn de tavel die er zijn aan toegewezen
    }
}