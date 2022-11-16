namespace BetGeniusConsumer.Helpers;

public static class FixtureSpecifics
{
    public const string SourceId = "GeniusPremium";
    public const int SportId = 17;
    
    // Urls
    public const string BaseUrl = "https://platform.uat.matchstate.api.geniussports.com/api/v1";
    public const string AuthUrl = "https://uat.auth.api.geniussports.com/oauth2/token";
    
    // Set the duration so that you would get at least 1 fixture
    public static readonly string FromDateTimeString = DateTime.UtcNow.AddHours(-2).ToString("s");
    public static readonly string ToDateTimeString = DateTime.UtcNow.AddDays(2).ToString("s");

    // Logs
    public static readonly string ErrorLogFile =
        @"C:\source\BetGeniusConsumer\Error_" + DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss") + ".txt";
    public static readonly string FeedLogFile = 
        @"C:\source\BetGeniusConsumer\PushFeedAblyLogs_" + DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss") + ".txt";
}