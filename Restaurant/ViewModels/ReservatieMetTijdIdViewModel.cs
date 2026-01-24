using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Restaurant.ViewModels
{
    public class ReservatieMetTijdIdViewModel
    {
        public DateTime? Datum { get; set; }

        public int AantalPersonen { get; set; }

        public int TijdSlotId { get; set; }
    }
}