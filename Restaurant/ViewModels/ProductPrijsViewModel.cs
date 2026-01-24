using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ViewModels
{
    public class ProductPrijsViewModel
    {
        public int Id { get; set; }
        public string? Naam { get; set; }
        public string? Beschrijving { get; set; }
        public string? AllergenenInfo { get; set; }

        [ForeignKey("Categorie")]
        public int CategorieId { get; set; }

        public bool Actief { get; set; }
        public bool IsSuggestie { get; set; }

        public decimal? Prijs { get; set; }
    }
}