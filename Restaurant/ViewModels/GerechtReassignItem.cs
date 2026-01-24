namespace Restaurant.ViewModels
{
    public class GerechtReassignItem
    {
        public int Id { get; set; }
        public string Naam { get; set; } //menu item

        public int NieuweCategorieId { get; set; } //de nieuwe categroieId
    }
}