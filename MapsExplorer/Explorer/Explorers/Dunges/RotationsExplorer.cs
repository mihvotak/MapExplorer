using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class RotationsExplorer : ExplorerBase
{
	private class Summator
	{
		public string Name;

		public Summator(string name) 
		{
			Name = name;
		}

		public Dictionary<int, Dictionary<int, int>> Dicts = new Dictionary<int, Dictionary<int, int>>();
		public Dictionary<int, Dictionary<int, float>> Results = new Dictionary<int, Dictionary<int, float>>();
		public Dictionary<int, int> Sums = new Dictionary<int, int>();

		public string GetWallsSumTextName(int currLocal)
		{
			if (currLocal == 0)
				return "нет";
			if (currLocal == 1)
				return "справа";
			if (currLocal == 2)
				return "спереди";
			if (currLocal == 3)
				return "спереди+справа";
			if (currLocal == 4)
				return "слева";
			if (currLocal == 5)
				return "слева+справа";
			if (currLocal == 6)
				return "слева+спереди";

			return $"??({currLocal})??";
		}

		public string GetLocalDirTextName(int localDir)
		{
			if (localDir == 0)
				return "назад";
			if (localDir == 1)
				return "направо";
			if (localDir == 2)
				return "вперёд";
			if (localDir == 3)
				return "налево";
			return $"??({localDir})??";
		}

		public void Add(int wallsSum, int currLocal)
		{
			if (!Dicts.ContainsKey(wallsSum))
			{
				Dicts.Add(wallsSum, new Dictionary<int, int>());
				Sums.Add(wallsSum, 0);
				Results.Add(wallsSum, new Dictionary<int, float>());
			}
			if (!Dicts[wallsSum].ContainsKey(currLocal))
				Dicts[wallsSum].Add(currLocal, 0);
			Dicts[wallsSum][currLocal]++;
			Sums[wallsSum]++;
		}

		public void Calculate() 
		{
			foreach (var pair in Dicts)
			{
				var key = pair.Key;
				var result = Results[key];
				var sum = Sums[key];
				foreach (var pair2 in pair.Value)
					result.Add(pair2.Key, pair2.Value / (float)sum);
			}
		}

		public string GetResLines()
		{
			StringBuilder sb = new StringBuilder();
			foreach (var pair in Results)
			{
				foreach (var pair2 in pair.Value)
					sb.Append($"{Name}\t{GetWallsSumTextName(pair.Key)}\t{GetLocalDirTextName(pair2.Key)}\t{pair2.Value * 100}\n");
			}
			return sb.ToString();
		}
	}

	public override void Work()
	{
		var calc0 = new Summator("All");
		int steps = 10;
		Summator[] calcByStep = new Summator[steps];
		Summator[] calcAfterVoice = new Summator[steps];
		for (int i = 0; i < steps; i++)
			calcByStep[i] = new Summator($"steps {i}0+");
		for (int i = 0; i < steps; i++)
			calcAfterVoice[i] = new Summator($"afterVoice {i+1}");
		var builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			if (line.DungeKind == DungeKind.Прыгучести)
				continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			if (dunge.SecretRom.Exists && (dunge.SecretRom.SecretKind == SecretKind.ChangeType || dunge.SecretRom.SecretKind == SecretKind.UnknownMark) && dunge.SecretRom.Visited)
				continue;

			int stepAfter = 0;
			for (int m = 1; m <= dunge.Moves.Count; m++)
			{
				var curr = dunge.Moves[m - 1];
				if (dunge.Voices[m] != null && dunge.Voices[m].Count > 0)
				{
					stepAfter = 0;
					continue;
				}
				else
					stepAfter++;
				if (m == 1)
					continue;
				var prev = dunge.Moves[m - 2];
				bool isSteps = curr.Delta.SumMagnitude == 1 && prev.Delta.SumMagnitude == 1;
				if (!isSteps)
					continue;
				Int2 forward = prev.Delta;
				int fIndex = Int2.FourDirections.IndexOf(forward);
				Int2 right = Int2.FourDirections[(fIndex + 1) % 4];
				Map map = dunge.Maps[curr.Floor - 1];
				if (map.GetCell(prev.Pos).Reverse)
					continue;
				bool forwardWall = map.GetCell(prev.Pos + forward).CellKind == CellKind.Wall;
				bool rightWall = map.GetCell(prev.Pos + right).CellKind == CellKind.Wall;
				bool leftWall = map.GetCell(prev.Pos - right).CellKind == CellKind.Wall;
				int walls = (forwardWall ? 1 : 0) + (rightWall ? 1 : 0) + (leftWall ? 1 : 0);
				if (walls >= 3)
					continue;
				int wallsSum = (leftWall ? 4 : 0) + (forwardWall ? 2 : 0) + (rightWall ? 1 : 0);
				var currDelta = curr.Delta;
				int currLocal = 
					currDelta == -forward ? 0 :
					currDelta == right ? 1 :
					currDelta == forward ? 2 :
					currDelta == -right ? 3 :
					-1;
				calc0.Add(wallsSum, currLocal);
				int step10 = (m - 1) / steps;
				calcByStep[step10].Add(wallsSum, currLocal);
				if (stepAfter > 0 && stepAfter <= 10)
					calcAfterVoice[stepAfter - 1].Add(wallsSum, currLocal);
				if (wallsSum == 5 && currLocal == 0)
				{
					List<string> tds = new List<string>();
					tds.Add(line.Link);
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add((m - 1).ToString());
					tds.Add($"{wallsSum}=>{currLocal}".ToString());
					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
				}
			}
			ReportProgress(i);
		}

		calc0.Calculate();
		for (int i = 0; i < steps; i++)
			calcByStep[i].Calculate();
		for (int i = 0; i < steps; i++)
			calcAfterVoice[i].Calculate();

		builder.Append("Общие шансы\n");
		builder.Append(calc0.GetResLines());
		
		builder.Append("Зависимость от номера шага, десятки\n");
		for (int i = 0; i < steps; i++)
			builder.Append(calcByStep[i].GetResLines());

		builder.Append("Зависимость от номера шага после гласа\n");
		for (int i = 0; i < steps; i++)
			builder.Append(calcAfterVoice[i].GetResLines());

		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/Rotations.txt", exploreRes);
		TableText = exploreRes;

	}
}
