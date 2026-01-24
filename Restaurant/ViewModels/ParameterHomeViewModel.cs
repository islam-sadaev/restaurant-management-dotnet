namespace Restaurant.ViewModels
{
    public class ParameterHomeViewModel
    {


        public string? RestaurantAdres { get; set; }
        public string? OpeningstijdWeekdag { get; set; }
        public string? SluitingstijdWeekdag { get; set; }
        public string? OpeningstijdWeekend { get; set; }
        public string? SluitingstijdWeekend { get; set; }
        public string? RestaurantNaam { get; set; }
        public IEnumerable<Reservatie> Reviews { get; set; }
    }
}