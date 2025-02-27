using RaceResultsScraper.Playwright;
using RaceResultsScraper.Selenium;

namespace RaceResultsScraper;

public class Program
{
    static async Task Main()
    {
        Console.WriteLine("Choose the scraper to run:");
        Console.WriteLine("1 - Selenium");
        Console.WriteLine("2 - Playwright");
        Console.Write("Enter your choice: ");
        
        var choice = Console.ReadLine()!;

        switch (choice)
        {
            case "1":
                SeleniumProgram.Run();
                break;
            case "2":
                await PlaywrightProgram.Run();
                break;
            default:
                Console.WriteLine("Invalid choice. Exiting...");
                break;
        }
    }
}