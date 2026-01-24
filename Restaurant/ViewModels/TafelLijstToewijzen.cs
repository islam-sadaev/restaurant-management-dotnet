namespace Restaurant.ViewModels
{
    public class TafelLijstToewijzen
    {
        public List<Tafel> BeschickbareTafels { get; set; } = new List<Tafel>();
        public List<ReservatieTafelsToewijzen> Reservaties { get; set; } = new List<ReservatieTafelsToewijzen>();
    }
}
