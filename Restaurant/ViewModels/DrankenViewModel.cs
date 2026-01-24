namespace Restaurant.ViewModels
{
    public class DrankenViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public string allergenenInfo { get; set; }
        public decimal prijs { get; set; }

        public bool IsSuggestie { get; set; }
        public bool Actief { get; set; }

        public IEnumerable<SelectListItem>? categories { get; set; }

        public int? categorieId { get; set; }

        // Foto upload
        public IFormFile? Foto { get; set; }
    }
}