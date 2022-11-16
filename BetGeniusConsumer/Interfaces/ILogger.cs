namespace BetGeniusConsumer.Interfaces;

public interface ILogger
{
    public void StoreLogs(string log, bool isException = false);
}