using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.ViewModels
{
    public class BestellingViewmodel
    {
        //[ForeignKey("Product")]
        //[Required(ErrorMessage = "U moet een product kiezen.")]
        //public int ProductId { get; set; }
        //[Required(ErrorMessage = "u kunt niet nul producten bestellen.")]
        public int Aantal { get; set; }
        public string? Opmerking { get; set; }

        //public IEnumerable<Product> dranken { get; set; }

        //public IEnumerable<Product> eten { get; set; }

        //public IEnumerable<Product> desserts { get; set; }

        //public IEnumerable<Product> specials { get; set; }.

        public List<ProductViewModel> SelectedProducts { get; set; }
        public List<OrderViewModel> Bestellingen { get; set; }
    }
}

