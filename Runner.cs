namespace RaceResultsScraper;

public class Runner
{
    public int Bib { get; set; }
    public string RunnerName { get; set; }
    public string Country { get; set; }
    public string FinisherTime { get; set; }

    public Runner(int bib, string runnerName, string country, string finisherTime)
    {
        Bib = bib;
        RunnerName = runnerName;
        Country = country;
        FinisherTime = finisherTime;
    }

    public override string ToString()
    {
        return $"{RunnerName} - {FinisherTime} - {Bib} - {Country}";
    }
}
