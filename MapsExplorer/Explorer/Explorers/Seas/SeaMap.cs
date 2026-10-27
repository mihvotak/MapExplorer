
using System.Collections.Generic;

public class SeaMap
{
    public int StepNum;
    public Dictionary<SeaPosition, SeaCell> Cells = new Dictionary<SeaPosition, SeaCell>();
    public Dictionary<string, SeaPosition> Arks = new Dictionary<string, SeaPosition>();
    public List<SeaLogEntry> LogEntries = new List<SeaLogEntry>();
    public List<SeaRiftActivation> RiftActivations = new List<SeaRiftActivation>();

    public SeaCell GetCell(SeaPosition position)
    {
        SeaCell cell;
        return Cells.TryGetValue(position, out cell) ? cell : null;
    }

    public SeaMap Copy(int stepNum)
    {
        var result = new SeaMap { StepNum = stepNum };
        foreach (var pair in Cells)
        {
            var cell = pair.Value.Copy();
            cell.ArkNumbers.Clear();
            result.Cells.Add(pair.Key, cell);
        }
        return result;
    }
}
