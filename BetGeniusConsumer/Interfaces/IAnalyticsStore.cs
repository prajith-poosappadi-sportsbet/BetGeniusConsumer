namespace BetGeniusConsumer.Interfaces;

public interface IAnalyticsStore
{
    public void Store(string fileName, IEnumerable<object> obj, bool isAppend = false);
}