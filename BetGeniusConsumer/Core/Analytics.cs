using CsvHelper.Configuration.Attributes;

namespace BetGeniusConsumer.Core;

public class Analytics
{
    [Index(0)]
    public int? Half { get; set; }
    
    [Index(1)]
    public int? Drive { get; set; }
    
    [Index(2)]
    public int? Play { get; set; }
    
    [Index(3)]
    public int? PlaySequence { get; set; }
    
    [Index(4)]
    public int? Action { get; set; }
    
    [Index(5)]
    public int? ActionSequence { get; set; }
    
    [Index(6)]
    public string? ActionTeam { get; set; }
    
    [Index(7)]
    public string? ActionType { get; set; }
    
    [Index(8)]
    public int? ActionYards { get; set; }
    
    [Index(9)]
    public string? PlayStartedAtUtc { get; set; }
    
    [Index(10)]
    public int? PLaySourcePlayId { get; set; }
    
    [Index(11)]
    public string? PlayDescription { get; set; }
    
    [Index(12)]
    public bool? PLayIsConfirmed { get; set; }

    [Index(13)]
    public bool? PlayIsFinished { get; set; }
    
    [Index(14)]
    public string? EventMessageTimeStamp { get; set; }
}