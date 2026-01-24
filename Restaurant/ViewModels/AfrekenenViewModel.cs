namespace Restaurant.ViewModels
{
    public class AfrekenenViewModel
    {
        public DateTime? Datum { get; set; }

        public int AantalPersonen { get; set; }

        public int TijdSlotId { get; set; }
        public decimal TotaalPrijs;


        public virtual ICollection<BestellingAfrekenenViewModel> Bestellingen { get; set; }
    }
}
