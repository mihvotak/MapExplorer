using MapsExplorer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class SeaLeviPlaceExplorer : ExplorerBase
{
    public override void Work()
    {
        StringBuilder builder = new StringBuilder();
        int counter = 0;
        for (int i = 0; i < _resultLines.Count; i++)
        {
            LogLine line = _resultLines[i];
            Sea sea = SeaLogHandler.GetSea(line, _seaExploreMode);
            List<string> tds = new List<string>();
            tds.Add(sea.LogLine.Link);
            tds.Add(Utils.GetDateAndTimeString(line.DateTime));
            //tds.Add(string.Join("|", sea.GodResults.Select(g => g.Prizes.ToString())));
            //tds.Add(string.Join("|", sea.GodResults.Select(g => g.Relicts.ToString())));
            string tr = string.Join("\t", tds);
            builder.Append(tr + "\n");
            counter++;
            ReportProgress(i);
        }
        builder.AppendLine("Total seas: " + counter);
        string exploreRes = builder.ToString();
        File.WriteAllText(Paths.ResultsDir + "/SeaLeviPlaceExplorer.txt", exploreRes);
        TableText = exploreRes;
    }
}
