using Org.BouncyCastle.Bcpg;

namespace Restaurant.ViewModels.Enquete
{
    public class VMReviewIngeven
    {
        public int Score { get; set; }
        public string Opmerking { get; set; }
        public Reservatie reservatie { get; set; }

    }
}
