namespace Restaurant.ViewModels
{
    public class EtenOverzichtItem
    {
        public int Id { get; set; }
        public string Naam { get; set; }
        public string Beschrijving { get; set; }
        public string AllergenenInfo { get; set; }
        public decimal Prijs { get; set; }
        public string CategorieNaam { get; set; }
        public bool IsSuggestie { get; set; }
        public bool Actief { get; set; }
    }
}