namespace Restaurant.ViewModels
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = "";
        public string Naam { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Rollen { get; set; } = new();
        public bool Actief { get; set; }
    }
}
