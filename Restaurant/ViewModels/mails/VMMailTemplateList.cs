namespace Restaurant.ViewModels.mails
{
    public class VMMailTemplateList
    {
        public int TemplateId { get; set; }
        public List<SelectListItem> MailTemplates { get; set; } = new List<SelectListItem>();
    }
}
