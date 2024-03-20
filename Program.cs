using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

class Program
{
    static async Task Main(string[] args)
    {
        // Launch a browser instance
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        // Create a new browser context
        var context = await browser.NewContextAsync();

        // Create a new page
        var page = await context.NewPageAsync();

        // Navigate to the URL you want to scrape
        await page.GotoAsync("https://app.storypark.com/users/sign_in");

        // Extract data from the page
        await page.FillAsync("input[id='user_email']", "");
        await page.FillAsync("input[id='user_password']", "");

        await page.Locator("//a[text()='Log in']").ClickAsync();

        await page.GetByTitle("Read full story").First.ClickAsync();

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await page.WaitForSelectorAsync("img");

        await DownloadImagesFromPageAsync(page);

        // Close the browser
        await browser.CloseAsync();
    }

    static async Task DownloadImagesFromPageAsync(IPage page)
    {
        // Get all image elements on the page
        var imageHandles = await page.QuerySelectorAllAsync("img");

        int count = 0;
        foreach (var handle in imageHandles)
        {
            var src = await handle.GetAttributeAsync("src");
            if (!string.IsNullOrEmpty(src))
            {
                using WebClient client = new();
                var url = new Uri($"{src}.jpeg");
                client.DownloadFileAsync(url, @$"file-{count}.jpeg");
                Console.WriteLine($"Downloading: {url}");
            }
            count += 1;
        }
    }

}
