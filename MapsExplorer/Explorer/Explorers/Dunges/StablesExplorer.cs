﻿using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class StablesExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		bool showFull = false;
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			ReportProgress(i);
			if (line.Category != MapCategory.Конюшня)
				continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			bool enough = dunge.Stable != null && dunge.Stable.EnoughInfo;
			builder.Append(line.Hash + "\t" + Utils.GetDateAndTimeString(line.DateTime) + "\t");
			Map map = dunge.Maps[0];
			builder.Append(map.Width + "\t" + map.Height + "\t");
			if (enough)
			{
				builder.Append($"OK\t");
				if (dunge.HintMoves.Count > 0)
				{
					Int2 hintPos = dunge.Moves[dunge.HintMoves[0] - 1].Pos - map.EnterPos;
					builder.Append($"{hintPos.x}\t{hintPos.y}\t");
				}
			}
			else
				builder.Append("not enough");
			builder.Append("\n");
			if (enough && showFull)
			{
				for (int y = 0; y < map.Height; y++)
				{
					for (int x = 0; x < map.Width; x++)
					{
						builder.Append(map.GetCell(x, y).GetSymbol(dunge.Rotation) + " ");
					}
					builder.Append("\n");
				}
				builder.Append("--------------------------\n");
			}
			ReportProgress(i);
		}
		string exploreRes = builder.ToString();
		TableText = exploreRes;
	}
}
