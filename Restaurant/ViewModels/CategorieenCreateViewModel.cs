namespace Restaurant.ViewModels
{
    public class CategorieenCreateViewModel
    {
        public string? Naam { get; set; }
        public bool Actief { get; set; }
        public int TypeId { get; set; }

        // Foto upload
        public IFormFile? Foto { get; set; }

        public List<SelectListItem> CategorienType { get; set; } = new List<SelectListItem>();
    }
}