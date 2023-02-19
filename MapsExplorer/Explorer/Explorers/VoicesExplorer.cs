using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class VoicesExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder rawBuilder = new StringBuilder();
		StringBuilder resultBuilder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			foreach (List<VoiceKind> voices in dunge.Voices)
			{
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(line.Category.ToString());
				tds.Add(line.Kind.ToString());
				StringBuilder ss = new StringBuilder();
				if (voices != null)
					foreach (var v in voices)
						ss.Append(v.ToString() + ".");
				tds.Add(ss.ToString());
				string tr = string.Join("\t", tds);
				rawBuilder.Append(tr + "\n");
			}
			ReportProgress(i);
		}

		string rawDataTable = rawBuilder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/VoicesRaw.txt", rawDataTable);

		string resData = resultBuilder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/VoicesResult.txt", resData);
		TableText = rawDataTable;
		ResultText = resData;
	}
}
