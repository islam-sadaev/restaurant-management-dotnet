namespace Restaurant.ViewModels
{
    public class TafelsToewijzenViewModel
    {
        public int Id { get; set; }
        
        public string? TafelNummer { get; set; }
        public int AantalPersonen { get; set; }
        public bool Actief { get; set; }
        public int MinAantalPersonen { get; set; }

    }
}
