using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using RaceResultsScraper;

class Program
{
    static void Main()
    {
        List<Runner> runners = ExtractFromWebsite();
        WriteResultsToFile(runners);
        SearchBib(runners);
    }

    static List<Runner> ExtractFromWebsite()
    {
        List<Runner> runners = new List<Runner>();
        IWebDriver driver = new ChromeDriver();
        driver.Navigate().GoToUrl("https://www.endu.net/en/events/bergamo-city-run/results/2025");

        IWebElement raceSelectButton = driver.FindElement(By.XPath("//select[@ng-model='activeRace']"));
        var selectElement = new OpenQA.Selenium.Support.UI.SelectElement(raceSelectButton);
        selectElement.SelectByValue("number:52275");

        try
        {
            IWebElement closeButton = driver.FindElement(By.XPath("//button[contains(text(),'Accetta')]"));
            closeButton.Click();
        }
        catch (Exception)
        {
            Console.WriteLine("No cookie banner found, continuing...");
        }

        while (true)
        {
            try
            {
                IWebElement activePage = driver.FindElement(By.XPath("//li[@class='page-number active']/a"));
                int currentPage = int.Parse(activePage.Text.Trim());
                Console.WriteLine("Scraping page: " + currentPage);

                IWebElement resultsTable = driver.FindElement(By.ClassName("fixed-table-body"));
                var rows = resultsTable.FindElements(By.XPath(".//tbody/tr"));

                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    int bib = int.Parse(cells[0].Text.Trim());
                    string[] runnerInfo = cells[1].Text.Trim().Split('\n');
                    string runnerName = runnerInfo[0].Trim();
                    string runnerCountry = runnerInfo.Length > 1 ? runnerInfo[1].Trim() : "Unknown";
                    string finisherTime = cells[2].Text.Trim().Split('\n')[0].Trim();
                    runners.Add(new Runner(bib, runnerName, runnerCountry, finisherTime));
                }

                var allPages = driver.FindElements(By.XPath("//ul[@class='pagination']//li/a"));
                int lastPage = int.Parse(allPages[allPages.Count - 2].Text.Trim());
                if (currentPage >= lastPage) break;

                IWebElement nextButton = driver.FindElement(By.XPath("//*[@id='contenitore']/div[1]/div[1]/div[1]/div[13]/div[1]/div[2]/div[4]/div[2]/ul/li[9]/a"));
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", nextButton);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", nextButton);
            }
            catch (Exception)
            {
                Console.WriteLine("An error occurred.");
                break;
            }
        }

        driver.Quit();
        return runners;
    }

    static void WriteResultsToFile(List<Runner> runners)
    {
        var sortedRunners = runners.OrderBy(r => r.FinisherTime).ToList();
        using (StreamWriter writer = new StreamWriter(@"C:\Users\mihai\RiderProjects\RaceResultsScraper\results.txt"))
        {
            writer.WriteLine("place - name - finish time - bib - country");
            int place = 1;
            foreach (var runner in sortedRunners)
            {
                writer.WriteLine($"{place++}. {runner}");
            }
        }
        Console.WriteLine("Race results saved to 'results.txt'.");
    }

    static void SearchBib(List<Runner> runners)
    {
        Console.Write("Enter your bib number: ");
        if (int.TryParse(Console.ReadLine(), out int bib))
        {
            var runner = runners.FirstOrDefault(r => r.Bib == bib);
            if (runner != null)
                Console.WriteLine(runner);
            else
                Console.WriteLine("Runner not found.");
        }
        else
        {
            Console.WriteLine("Invalid bib number.");
        }
    }
}
