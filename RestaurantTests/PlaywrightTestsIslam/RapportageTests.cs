using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using PlaywrightTests.Helpers;
using NUnit.Framework;

[TestFixture]
public class RapportageTests : PageTest
{

    [Test]
    public async Task Eigenaar_Kan_ReservatieRapport_Genereren()
    {
        // Login
        await LoginHelper.LoginAlsEigenaar(Page);

        // ga nr rapportage
        await Page.GotoAsync("http://localhost:5122/Rapportage/Dashboard");

        //sellecteer Reservatie voor type en Maand voor periode
        await Page.SelectOptionAsync("select[name='Type']", "Reservaties");
        await Page.SelectOptionAsync("select[name='Periode']", "Maand");

        //druk op genereer rapport
        await Page.ClickAsync("button:text('Genereer rapport')");

        // kijk of de tabel zichtbaar is
        var tabel = Page.Locator("table");
        await Expect(tabel).ToBeVisibleAsync();
    }

    [Test]
    public async Task Eigenaar_Kan_Rapport_Downloaden_Als_Pdf()
    {
        //Login
        await LoginHelper.LoginAlsEigenaar(Page);

        //ga nr rapportgae
        await Page.GotoAsync("http://localhost:5122/Rapportage/Dashboard");

        //sellecteer Reservatie voor type en Maand voor periode
        await Page.SelectOptionAsync("select[name='Type']", "Reservaties");
        await Page.SelectOptionAsync("select[name='Periode']", "Maand");

        //klik op genereer rapport
        await Page.ClickAsync("button:text('Genereer rapport')");

        // druk op de knop om de donwload te beginne
        var download = await Page.RunAndWaitForDownloadAsync(async () =>
        {
            await Page.ClickAsync("button:text('Download PDF')");
        });

        // Kijke of de pdf begint met Rapport_ en eindigt met .pdf
        StringAssert.StartsWith("Rapport_", download.SuggestedFilename);
        StringAssert.EndsWith(".pdf", download.SuggestedFilename);
    }
}