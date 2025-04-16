using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class TeleportsExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			builder.Append(line.Link + "\t" + Utils.GetDateAndTimeString(line.DateTime) + "\t");
			Map map = dunge.Maps[0];
			builder.Append(map.Width + "\t" + map.Height + "\t");
			var traps = map.Cells.FindAll(c => c.CellKind == CellKind.Trap).Count;
			var teleports = map.Cells.FindAll(c => c.CellKind == CellKind.Teleport).Count;
			builder.Append(teleports + "\t");
			builder.Append((traps + teleports) + "\t");
			builder.Append("\n");
			ReportProgress(i);
		}
		string exploreRes = builder.ToString();
		TableText = exploreRes;
	}
}
