using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class CacheHintsExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			Map map = dunge.Maps[dunge.LastFloor - 1];
			bool enough = dunge.Stable != null && dunge.Stable.EnoughInfo;
			builder.Append(line.Hash + "\t" + Utils.GetDateAndTimeString(line.DateTime) + "\t");
			builder.Append(line.Category.ToString() + "\t");
			builder.Append(line.Kind.ToString() + "\t");
			builder.Append(map.Width + "\t" + map.Height + "\t");
			builder.Append((dunge.HintOnCache ? 1 : 0) + "\t");
			builder.Append("\n");
			ReportProgress(i);
		}
		string exploreRes = builder.ToString();
		TableText = exploreRes;
	}
}
