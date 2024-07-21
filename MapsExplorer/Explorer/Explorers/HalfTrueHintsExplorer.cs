using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;

public class HalfTrueHintsExplorer : ExplorerBase
{
	public override void Work()
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			if (!line.Success || line.Vault)
				continue;
			if (line.Kind != DungeKind.Полуправды || line.Kind == DungeKind.Загадки)
				continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			if (dunge.StartKind == DungeKind.Загадки || dunge.StartKind != DungeKind.Полуправды || (dunge.SecretRom != null && (dunge.SecretRom.SecretKind == SecretKind.ChangeType || dunge.SecretRom.SecretKind == SecretKind.UnknownMark) && dunge.SecretRom.Visited))
				continue;
			List<string> tds = new List<string>();
			tds.Add(line.Link);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(line.Category.ToString());
			tds.Add(line.Kind.ToString());
			int totalHints = 0;
			int trueHints = 0;
			int falseHints = 0;
			int termoHints = 0;
			HintCategory category = HintCategory.Unknown;
			Int2 treasure = dunge.TreasurePos.Pos;
			List<Step> used = new List<Step>();
			List<string> res = new List<string>();
			for (int m = 0; m < dunge.Moves.Count; m++)
			{
				Step step = dunge.Moves[m];
				if (used.Contains(step))
					continue;
				used.Add(step);
				Int2 pos = step.Pos;
				Map map = dunge.Maps[step.Floor - 1];
				Cell cell = map.GetCell(pos);
				if (cell.CellKind != CellKind.Hint)
					continue;
				totalHints++;
				if (step.Floor != dunge.LastFloor)
				{
					res.Add("?");
					continue;
				}
				if (cell.HintCategory == HintCategory.Experimental)
				{
					if (category == HintCategory.Unknown)
						category = HintCategory.Experimental;
					res.Add("E");
				}
				else if (cell.HintCategory == HintCategory.Thermo)
				{
					termoHints++;
					if (category == HintCategory.Unknown)
						category = HintCategory.Thermo;
					res.Add("T");
				}
				else
				{
					bool isTrue = false;
					bool isUnknown = false;
					double angle = Math.Atan2(-(treasure.y - pos.y), treasure.x - pos.x);
					if (angle < -Math.PI / 8)
						angle += Math.PI * 2;
					int sector = (int)Math.Round(angle / (Math.PI / 4));
					if (cell.Hint == Hint.С)
						isTrue = sector == 2;
					else if (cell.Hint == Hint.В)
						isTrue = sector == 0;
					else if (cell.Hint == Hint.Ю)
						isTrue = sector == 6;
					else if (cell.Hint == Hint.З)
						isTrue = sector == 4;
					else if (cell.Hint == Hint.СВ)
						isTrue = sector == 1;
					else if (cell.Hint == Hint.ЮВ)
						isTrue = sector == 7;
					else if (cell.Hint == Hint.ЮЗ)
						isTrue = sector == 5;
					else if (cell.Hint == Hint.СЗ)
						isTrue = sector == 3;
					else if (cell.Hint == Hint.СВ_СЗ)
						isTrue = sector == 1 || sector == 3;
					else if (cell.Hint == Hint.СВ_ЮВ)
						isTrue = sector == 1 || sector == 7;
					else if (cell.Hint == Hint.ЮЗ_ЮВ)
						isTrue = sector == 5 || sector == 7;
					else if (cell.Hint == Hint.СЗ_ЮЗ)
						isTrue = sector == 3 || sector == 5;
					else if (cell.Hint == Hint.С_В)
						isTrue = sector == 2 || sector == 0;
					else if (cell.Hint == Hint.Ю_В)
						isTrue = sector == 6 || sector == 0;
					else if (cell.Hint == Hint.Ю_З)
						isTrue = sector == 6 || sector == 4;
					else if (cell.Hint == Hint.С_З)
						isTrue = sector == 2 || sector == 4;
					else if (cell.Hint == Hint.СП)
						isTrue = treasure.y < pos.y;
					else if (cell.Hint == Hint.ВП)
						isTrue = treasure.x > pos.x;
					else if (cell.Hint == Hint.ЮП)
						isTrue = treasure.y > pos.y;
					else if (cell.Hint == Hint.ЗП)
						isTrue = treasure.x < pos.x;
					else
						isUnknown = true;

					if (!isUnknown)
					{
						category = cell.HintCategory;
						if (isTrue)
						{
							trueHints++;
							res.Add("+");
						}
						else
						{
							falseHints++;
							res.Add("-");
						}
					}
					else
						res.Add("!");
				}
			}
			tds.Add(totalHints.ToString());
			tds.Add(termoHints.ToString());
			tds.Add(trueHints.ToString());
			tds.Add(falseHints.ToString());
			tds.Add(string.Join(" ", res));
			tds.Add(dunge.LastFloor.ToString());
			tds.Add(category.ToString());
			string tr = string.Join("\t", tds);
			builder.Append(tr + "\n");
			ReportProgress(i);
		}

		string exploreTab = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/HalfTrueHints.txt", exploreTab);
		TableText = exploreTab;
	}
}
