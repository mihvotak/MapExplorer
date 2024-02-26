using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class RotationsExplorer : ExplorerBase
{
	private class Summator
	{
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
					sb.Append($"{GetWallsSumTextName(pair.Key)}\t{GetLocalDirTextName(pair2.Key)}\t{pair2.Value * 100}\n");
			}
			return sb.ToString();
		}
	}

	public override void Work()
	{
		var calc0 = new Summator();
		var builder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			if (line.Kind == DungeKind.Прыгучести)
				continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			if (dunge.SecretRom.Exists && (dunge.SecretRom.SecretKind == SecretKind.ChangeType || dunge.SecretRom.SecretKind == SecretKind.UnknownMark) && dunge.SecretRom.Visited)
				continue;

			for (int m = 2; m <= dunge.Moves.Count; m++)
			{
				var curr = dunge.Moves[m - 1];
				var prev = dunge.Moves[m - 2];
				bool isSteps = curr.Delta.SumMagnitude == 1 && prev.Delta.SumMagnitude == 1;
				if (!isSteps || (dunge.Voices[m] != null && dunge.Voices[m].Count > 0))
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

		builder.Append('\n');
		builder.Append(calc0.GetResLines());

		string exploreRes = builder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/Rotations.txt", exploreRes);
		TableText = exploreRes;

	}
}
