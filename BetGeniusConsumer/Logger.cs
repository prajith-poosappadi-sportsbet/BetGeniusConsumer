using BetGeniusConsumer.Helpers;
using BetGeniusConsumer.Interfaces;

namespace BetGeniusConsumer;

public class Logger : ILogger
{
    public void StoreLogs(string log, bool isException = false)
    {
        log = $"[{DateTime.UtcNow:s}] {log}";
        Console.WriteLine(log);
        File.AppendAllText(
            isException ? FixtureSpecifics.ErrorLogFile : FixtureSpecifics.FeedLogFile,
            log + Environment.NewLine);
    }
}