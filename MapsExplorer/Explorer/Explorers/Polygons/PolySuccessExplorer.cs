using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

public class PolySuccessExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		int counter = 0;
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			Polygon polygon = PolygonLogHandler.GetPolygon(line, _polygonExploreMode);
			List<string> tds = new List<string>();
			tds.Add(polygon.LogLine.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(string.Join("|", polygon.GodResults.Select(g => g.BossFullHP.ToString())));
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");
			counter++;
			ReportProgress(i);
		}
		builder.AppendLine("Total dunges: " + counter);
		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/PlygonSuccessExplorer.txt", exploreRes);
		TableText = exploreRes;
	}
}
