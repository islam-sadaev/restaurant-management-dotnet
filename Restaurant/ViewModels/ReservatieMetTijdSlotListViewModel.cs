namespace Restaurant.ViewModels
{
    public class ReservatieMetTijdSlotListViewModel
    {
        public DateTime? DatumVan { get; set; }

        public List<ReservatieMetTijdSlotViewModel> Reservaties { get; set; }
    }
}