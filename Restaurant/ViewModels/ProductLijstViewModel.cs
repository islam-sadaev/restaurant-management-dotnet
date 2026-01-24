namespace Restaurant.ViewModels
{
    public class ProductLijstViewModel
    {
        public List<ProductViewModel> SelectedProducts { get; set; } = new();
        public List<BestellingItemViewModel> Bestellingen { get; set; } = new();
    }
}