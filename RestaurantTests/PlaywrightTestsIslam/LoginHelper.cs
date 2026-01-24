using Microsoft.Playwright;

namespace PlaywrightTests.Helpers
{
    public static class LoginHelper
    {
        public static async Task LoginAlsEigenaar(IPage page)
        {
            await page.GotoAsync("http://localhost:5122/Gebruiker/Login");

            await page.FillAsync("input[name='Email']", "test1@test1.com");
            await page.FillAsync("input[name='Password']", "Test1Test1.");

            await page.ClickAsync("button[type='submit']");

            // wachten tot login afgerond is
            await page.WaitForURLAsync("**/Gebruiker/MyAccount");
        }
    }
}
