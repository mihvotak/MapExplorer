using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System;
using System.IO;

public class JumpsExplorer : ExplorerBase
{
	public override void Work()
	{
		var resArray = new int[100];
		var builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			if (line.DungeKind != DungeKind.Прыгучести)
				continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			if (dunge.SecretRom.Exists && (dunge.SecretRom.SecretKind == SecretKind.ChangeType || dunge.SecretRom.SecretKind == SecretKind.UnknownMark) && dunge.SecretRom.Visited)
				continue;

			int firstJump = 0;
			int[] possible = new int[6];
			int[] jumps = new int[6];
			int jMany = 0;
			int pMany = 0;
			int impossible = 0;
			for (int m = 2; m <= dunge.Moves.Count; m++)
			{
				var curr = dunge.Moves[m - 1];
				var prev = dunge.Moves[m - 2];
				bool isJump = curr.Floor == prev.Floor &&
					((curr.Pos.x == prev.Pos.x && Math.Abs(curr.Pos.y - prev.Pos.y) == 2)
					|| (Math.Abs(curr.Pos.x - prev.Pos.x) == 2 && curr.Pos.y == prev.Pos.y));
				if (firstJump == 0 && isJump)
				{
					firstJump = m;
				}
				if (m >= 6)
				{
					Map map = dunge.Maps[curr.Floor - 1];
					int[] kinds = new int[4];
					int maxVoice = 0;
					int maxCount = 0;
					int maxDir = -1;
					int dirVoices = 0;
					if (dunge.Voices[m] != null)
					{
						foreach (VoiceKind kind in dunge.Voices[m])
						{
							if (kind <= VoiceKind.запад)
							{
								kinds[(int)kind]++;
								dirVoices++;
							}
						}
						for (int ki = 0; ki < 4; ki++)
						{
							if (kinds[ki] > maxVoice)
							{
								maxVoice = kinds[ki];
								maxCount = 1;
								maxDir = ki;
							}
							else if (kinds[ki] == maxVoice)
							{
								maxCount++;
							}
						}
					}
					bool manyVoices = maxVoice > 0 && maxCount > 1;
					bool badWallVoice = false;

					if (isJump)
					{
						jumps[maxVoice]++;
						possible[maxVoice]++;
						if (dirVoices > maxVoice)
							jMany++;
					}
					else if (curr.Floor == prev.Floor &&
						((curr.Pos.x == prev.Pos.x && Math.Abs(curr.Pos.y - prev.Pos.y) == 1)
					|| (Math.Abs(curr.Pos.x - prev.Pos.x) == 1 && curr.Pos.y == prev.Pos.y)))
					{
						int dirs = 0;
						for (int di = 0; di < 4; di++)
						{
							var dir = Int2.FourDirections[di];
							Int2 pos = prev.Pos + dir * 2;
							Cell cell = pos.x < 0 || pos.x >= map.Width || pos.y < 0 || pos.y >= map.Height ? null : map.GetCell(pos);
							bool wall = cell == null || cell.CellKind == CellKind.Unknown || cell.CellKind == CellKind.Wall;
							if (maxVoice > 0 && maxCount == 1 && di == maxDir && wall)
								badWallVoice = true;
							if (!wall)
								dirs++;
						}
						if (dirs > 0 && !badWallVoice && !manyVoices)
						{
							possible[maxVoice]++;
							if (dirVoices > maxVoice)
								pMany++;
						}
						else
							impossible++;
					}
				}
			}
			resArray[firstJump]++;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(firstJump.ToString());
			tds.Add(jumps[0].ToString());
			tds.Add(possible[0].ToString());
			tds.Add(jumps[1].ToString());
			tds.Add(possible[1].ToString());
			tds.Add(jumps[2].ToString());
			tds.Add(possible[2].ToString());
			tds.Add(jumps[3].ToString());
			tds.Add(possible[3].ToString());
			tds.Add(jMany.ToString());
			tds.Add(pMany.ToString());
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");

			ReportProgress(i);
		}

		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/Jumps.txt", exploreRes);
		TableText = exploreRes;

	}
}
