namespace Restaurant.ViewModels
{
    public class CategorieReassignViewModel
    {
        public int Id { get; set; }
        public string Naam { get; set; }

        public List<GerechtReassignItem> Gerechten { get; set; } // de gerecht die in verwijdere categorie zit

        public List<SelectListItem> Categorieen { get; set; } //de categorien die niet worden verwijdert
    }
}