using BetGeniusConsumer.Core;
using Newtonsoft.Json.Linq;
using BetGeniusConsumer.Interfaces;

namespace BetGeniusConsumer;

public class Analysis : IAnalysis
{
    private readonly IAnalyticsStore _analyticsStore;
    private readonly string _analysisFileName =
        @"C:\Logs\Analysis_" + DateTime.UtcNow.ToString("yyyy-dd-M--HH-mm-ss") + ".csv";
    private bool _firstEntry = true;
    
    public Analysis(IAnalyticsStore analyticsStore)
    {
        _analyticsStore = analyticsStore;
    }

    public void StoreForAnalysis(string? log)
    {
        if (log == null) return;
        var jsonObj = JObject.Parse(log);

        var firstHalf = jsonObj["firstHalf"];
        var secondHalf = jsonObj["secondHalf"];
        var messageTimeStamp = jsonObj["messageTimestampUtc"]?.ToObject<string>();

        if (firstHalf != null) AnalyseAndStore(firstHalf, 1, messageTimeStamp);
        if (secondHalf != null) AnalyseAndStore(secondHalf, 2, messageTimeStamp);
    }

    private void AnalyseAndStore(JToken data, int half, string? messageTimestamp)
    {
        var halfDrives = data["drives"];

        var drive = 1;
        if (halfDrives == null) return;
        foreach (var halfDrive in halfDrives)
        {
            var analyticsObj = new Analytics();
            analyticsObj.Half = half;
            analyticsObj.Drive = drive++;
            analyticsObj.EventMessageTimeStamp = messageTimestamp;
            var plays = halfDrive["plays"];
            if (plays == null) continue;
            foreach (var play in plays)
            {
                analyticsObj.PlaySequence = play["sequence"]?.ToObject<int?>();
                analyticsObj.PLaySourcePlayId = play["sourcePlayId"]?.ToObject<int?>();
                analyticsObj.PlayDescription = play["description"]?.ToObject<string?>();
                analyticsObj.PlayStartedAtUtc = play["startedAtUtc"]?.ToObject<string?>();
                analyticsObj.PLayIsConfirmed = play["isConfirmed"]?.ToObject<bool?>();
                analyticsObj.PlayIsFinished = play["isFinished"]?.ToObject<bool?>();
                var actions = play["actions"];
                if (actions == null) continue;
                foreach (var action in actions)
                {
                    analyticsObj.ActionSequence = action["sequence"]?.ToObject<int?>();
                    analyticsObj.ActionType = action["type"]?.ToObject<string?>();
                    analyticsObj.ActionYards = action["yards"]?.ToObject<int?>();
                    analyticsObj.ActionTeam = action["team"]?.ToObject<string?>();
                    analyticsObj.ActionType = action["type"]?.ToObject<string?>();
                    //analyticsObjList.Add(analyticsObj);
                    if (_firstEntry)
                    {
                        _analyticsStore.Store(_analysisFileName, new List<object> { analyticsObj });
                        _firstEntry = false;
                    }
                    else
                    {
                        _analyticsStore.Store(_analysisFileName, new List<object> { analyticsObj }, true);
                    }
                }
            }
        }
    }
}