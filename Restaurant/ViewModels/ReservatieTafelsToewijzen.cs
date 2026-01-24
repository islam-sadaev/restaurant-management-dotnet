namespace Restaurant.ViewModels
{
    public class ReservatieTafelsToewijzen
    {
        public int Id { get; set; }
        public string KlantId { get; set; }
        public virtual CustomUser CustomUser { get; set; }
        public int AantalPersonen { get; set; }
        public bool IsAanwezig { get; set; }
        public string? Opmerking { get; set; }

        public int TijdSlotId { get; set; }
        public Tijdslot tijdslot { get; set; }

        public virtual IList<TafelsToewijzenViewModel> tafels { get; set; }
    }
}
