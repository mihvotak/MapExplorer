
using MapsExplorer;
using System.Collections.Generic;

public class Sea
{
    public SeaGodResult[] GodResults = new SeaGodResult[4];

    public LogLine LogLine { get; private set; }

    public List<SeaMap> Maps = new List<SeaMap>();
    public List<SeaRiftActivation> RiftActivations = new List<SeaRiftActivation>();
    public int? MinBorderDistance;
    public int? MaxBorderDistance;
    public HashSet<SeaPosition> LonelyDeepRifts = new HashSet<SeaPosition>();
    public SeaMap GetMap(int stepNum) => Maps[stepNum - 1];

    public Sea(LogLine logLine)
    {
        LogLine = logLine;
    }
}
