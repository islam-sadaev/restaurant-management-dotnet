namespace Restaurant.ViewModels.mails
{
    public class VMNiewsbrief
    {
        public int Id { get; set; }
        public List<string> Ontvangers { get; set; }
        public string Naam { get; set; }
        public string Onderwerp { get; set; }
        public string Body { get; set; }
    }
}
