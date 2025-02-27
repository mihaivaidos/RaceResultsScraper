namespace RaceResultsScraper.Playwright;

using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class PlaywrightProgram
{
    public static async Task Run()
    {
        List<Runner> runners = await ExtractFromWebsite();
        WriteResultsToFile(runners);
        SearchBib(runners);
    }

    static async Task<List<Runner>> ExtractFromWebsite()
    {
        List<Runner> runners = new List<Runner>();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://www.endu.net/en/events/bergamo-city-run/results/2025");

        // Handle cookies if present
        var acceptButton = await page.QuerySelectorAsync("//button[contains(text(),'Accetta')]");
        if (acceptButton != null)
        {
            await acceptButton.ClickAsync();
        }
        
        // Select the 10km non-competitive race
        await page.SelectOptionAsync("//select[@ng-model='activeRace']", new[] { "number:52275" });

        while (true)
        {
            try
            {
                var currentPageText = await page.InnerTextAsync("//li[@class='page-number active']/a");
                int currentPage = int.Parse(currentPageText.Trim());
                Console.WriteLine("Scraping page: " + currentPage);

                await page.WaitForSelectorAsync(".fixed-table-body tbody tr");
                var rows = await page.QuerySelectorAllAsync(".fixed-table-body tbody tr");

                foreach (var row in rows)
                {
                    var cells = await row.QuerySelectorAllAsync("td");
                    var cellTexts = await Task.WhenAll(cells.Select(cell => cell.InnerTextAsync()));

                    int bib = int.Parse(cellTexts[0].Trim());
                    string[] runnerDetails = cellTexts[1].Split('\n');
                    string runnerName = runnerDetails[0].Trim();
                    string runnerCountry = runnerDetails.Length > 1 ? runnerDetails[1].Trim() : "";
                    string finisherTime = cellTexts[2].Split('\n')[0].Trim();

                    runners.Add(new Runner(bib, runnerName, runnerCountry, finisherTime));
                }

                var pageNumbers = await page.QuerySelectorAllAsync("//ul[@class='pagination']//li/a");
                int lastPage = int.Parse(await pageNumbers[^2].InnerTextAsync());
                
                if (currentPage >= lastPage)
                {
                    Console.WriteLine("Reached the last page.");
                    break;
                }

                var nextButton = await page.QuerySelectorAsync("//*[@id='contenitore']/div[1]/div[1]/div[1]/div[13]/div[1]/div[2]/div[4]/div[2]/ul/li[9]/a");
                if (nextButton != null)
                {
                    await nextButton.ScrollIntoViewIfNeededAsync();
                    await nextButton.ClickAsync();
                    await page.WaitForTimeoutAsync(1000);
                }
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred.");
                break;
            }
        }

        await browser.CloseAsync();
        return runners;
    }

    static void WriteResultsToFile(List<Runner> runners)
    {
        var sortedRunners = runners.OrderBy(r => r.FinisherTime).ToList();
        string filePath = "C:\\Users\\mihai\\RiderProjects\\RaceResultsScraper\\Playwright\\resultsPW.txt";
        using StreamWriter writer = new StreamWriter(filePath);
        writer.WriteLine("place - name - finish time - bib - country");
        int place = 1;
        foreach (var runner in sortedRunners)
        {
            writer.WriteLine($"{place++}. {runner.RunnerName} - {runner.FinisherTime} - {runner.Bib} - {runner.Country}");
        }
        Console.WriteLine("Race results saved to 'resultsPW.txt'.");
    }

    static void SearchBib(List<Runner> runners)
    {
        Console.Write("Enter your bib number: ");
        if (int.TryParse(Console.ReadLine(), out int bib))
        {
            var runner = runners.FirstOrDefault(r => r.Bib == bib);
            if (runner != null)
                Console.WriteLine($"{runner.RunnerName} - {runner.FinisherTime} - {runner.Bib} - {runner.Country}");
            else
                Console.WriteLine("Runner not found.");
        }
        else
        {
            Console.WriteLine("Invalid bib number.");
        }
    }
}