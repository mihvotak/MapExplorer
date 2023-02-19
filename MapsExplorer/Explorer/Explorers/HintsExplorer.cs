using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class HintsExplorer : ExplorerBase
{
	private const int MaxStep = 100;
	class HintCounter
	{
		public string name;
		public int total = 0;
		public int[] firstHintMoves = new int[MaxStep + 1];
		public int[] firstNoHintMoves = new int[MaxStep + 1];
		public int[] endWithoutHint = new int[MaxStep + 1];
		public int[] firstHintUniqueMoves = new int[MaxStep + 1];
		public int[] firstNoHintUniqueMoves = new int[MaxStep + 1];

		public int[] secondHintMoves = new int[MaxStep + 1];
		public int[] secondDeltaHintMoves = new int[MaxStep + 1];
		public int[] secondHintUniqueMoves = new int[MaxStep + 1];
		public int[] secondNoHintMoves = new int[MaxStep + 1];
		public int[] secondDeltaNoHintMoves = new int[MaxStep + 1];

		public int[] thirdHintMoves = new int[MaxStep + 1];
		public int[] thirdDeltaHintMoves = new int[MaxStep + 1];
	}

	public override void Work()
	{
		HintCounter normalCounter = new HintCounter() { name = "обычный" };
		HintCounter thermoCounter = new HintCounter() { name = "термо" };
		HintCounter quickCounter = new HintCounter() { name = "спешка" };
		HintCounter moreCounter = new HintCounter() { name = "густота" };
		StringBuilder rawBuilder = new StringBuilder();
		StringBuilder resultBuilder = new StringBuilder();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			DungeLine line = _resultLines[i];
			if (line.Category != Category.Usual)
				continue;
			Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
			if (dunge.IsCustom && !_customCheckBoxChecked)
				continue;
			List<string> tds = new List<string>();
			tds.Add(line.Hash);
			tds.Add(Utils.GetDateAndTimeString(line.DateTime));
			tds.Add(line.Category.ToString());
			tds.Add(line.Kind.ToString());
			tds.Add(dunge.Steps.ToString());
			tds.Add(dunge.Bosses.Count.ToString());
			tds.Add(dunge.SecretRom.Exists ? dunge.SecretRom.SecretKind.ToString() : "no secret");
			if (dunge.UnknownImportant != null)
				tds.Add("UnknownImportant: " + dunge.UnknownImportant);
			else if (dunge.UnknownSecretResult != null)
				tds.Add("UnknownSecretResult: " + dunge.UnknownSecretResult);
			else if (dunge.UnknownMoveDir != null)
				tds.Add("UnknownMoveDir: " + dunge.UnknownMoveDir);
			else if (dunge.UnknownHint != null)
				tds.Add("UnknownHint: " + dunge.UnknownHint);
			else
				tds.Add("");
			HintCounter counter = null;
			if (!dunge.SecretRom.Exists || (dunge.SecretRom.SecretKind != SecretKind.ChangeType && dunge.SecretRom.SecretKind != SecretKind.UnknownMark && dunge.SecretRom.SecretKind != SecretKind.HintsOn))
			{
				if (dunge.DungeLine.Kind != DungeKind.Термодинамики && dunge.DungeLine.Kind != DungeKind.Спешки && dunge.DungeLine.Kind != DungeKind.Густоты && dunge.DungeLine.Kind != DungeKind.Загадки
				&& dunge.StartKind != DungeKind.Термодинамики && dunge.StartKind != DungeKind.Спешки && dunge.StartKind != DungeKind.Густоты && dunge.StartKind != DungeKind.Загадки)
					counter = normalCounter;
				else if (dunge.DungeLine.Kind == DungeKind.Термодинамики && dunge.StartKind == DungeKind.Термодинамики)
					counter = thermoCounter;
				else if (dunge.DungeLine.Kind == DungeKind.Спешки && dunge.StartKind == DungeKind.Спешки)
					counter = quickCounter;
				else if (dunge.DungeLine.Kind == DungeKind.Густоты && dunge.StartKind == DungeKind.Густоты)
					counter = moreCounter;
			}
			if (counter != null)
			{
				tds.Add(counter.name);
				counter.total++;
				if (dunge.HintMoves.Count > 0)
				{
					counter.firstHintMoves[dunge.HintMoves[0]]++;
					counter.firstHintUniqueMoves[dunge.HintCounters[0]]++;
					for (int j = 1; j < dunge.HintMoves[0]; j++)
						if (dunge.CanBeHints[j])
							counter.firstNoHintMoves[j]++;
					for (int j = 1; j < dunge.HintCounters[0]; j++)
						counter.firstNoHintUniqueMoves[j]++;
					if (dunge.HintMoves.Count >= 2)
					{
						counter.secondHintMoves[dunge.HintMoves[1]]++;
						for (int j = dunge.HintMoves[0] + 1; j < dunge.HintMoves[1]; j++)
							if (dunge.CanBeHints[j])
								counter.secondNoHintMoves[j]++;
						counter.secondDeltaHintMoves[dunge.HintMoves[1] - dunge.HintMoves[0]]++;
						for (int j = dunge.HintMoves[0] + 1; j < dunge.HintMoves[1]; j++)
							if (dunge.CanBeHints[j])
								counter.secondDeltaNoHintMoves[j - dunge.HintMoves[0]]++;
						counter.secondHintUniqueMoves[dunge.HintCounters[1]]++;
						if (dunge.HintMoves.Count >= 3)
						{
							counter.thirdHintMoves[dunge.HintMoves[2]]++;
							counter.thirdDeltaHintMoves[dunge.HintMoves[2] - dunge.HintMoves[1]]++;
						}
					}
					else
					{
						for (int j = dunge.HintMoves[0] + 1; j < dunge.Moves.Count + 1; j++)
							if (dunge.CanBeHints[j])
								counter.secondNoHintMoves[j]++;
						for (int j = dunge.HintMoves[0] + 1; j < dunge.Moves.Count + 1; j++)
							if (dunge.CanBeHints[j])
								counter.secondDeltaNoHintMoves[j - dunge.HintMoves[0]]++;
					}
				}
				else
				{
					counter.endWithoutHint[dunge.Moves.Count + 1]++;
					for (int j = 1; j <= dunge.Moves.Count + 1; j++)
						if (dunge.CanBeHints[j])
							counter.firstNoHintMoves[j]++;
					for (int j = 1; j <= dunge.UniqueSteps; j++)
						counter.firstNoHintUniqueMoves[j]++;
				}
			}
			else
				tds.Add("not counted");

			if (dunge.HintMoves.Count > 0)
			{
				tds.Add(dunge.HintCounters[0].ToString());
				tds.Add(dunge.HintCounters.Count > 1 ? dunge.HintCounters[1].ToString() : "-");
				tds.Add("|");
				tds.Add(dunge.HintMoves[0].ToString());
				tds.Add(dunge.HintMoves.Count >= 2 ? dunge.HintMoves[1].ToString() : "-");
				tds.Add(dunge.HintMoves.Count >= 3 ? dunge.HintMoves[2].ToString() : "-");
				tds.Add("|");
				tds.Add(dunge.HintMoves.Count >= 2 ? (dunge.HintMoves[1] - dunge.HintMoves[0]).ToString() : "-");
				tds.Add(dunge.HintMoves.Count >= 3 ? (dunge.HintMoves[2] - dunge.HintMoves[1]).ToString() : "-");
			}
			else
			{
				tds.Add("-");
				tds.Add("-");
				tds.Add("|");
				tds.Add("-");
				tds.Add("-");
				tds.Add("-");
				tds.Add("|");
				tds.Add("-");
				tds.Add("-");
			}

			string tr = string.Join("\t", tds);
			rawBuilder.Append(tr + "\n");
			ReportProgress(i);
		}

		AddHintResultsToBuilder(resultBuilder, normalCounter);
		AddHintResultsToBuilder(resultBuilder, thermoCounter);
		AddHintResultsToBuilder(resultBuilder, quickCounter);
		AddHintResultsToBuilder(resultBuilder, moreCounter);

		string rawDataTable = rawBuilder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/SearchHintsRaw.txt", rawDataTable);

		string resData = resultBuilder.ToString();
		File.WriteAllText(Paths.ResultsDir + "/SearchHintsResult.txt", resData);
		TableText = rawDataTable;
		ResultText = resData;
	}

	private void AddHintResultsToBuilder(StringBuilder builder, HintCounter counter)
	{
		builder.Append("\n================= " + counter.name + " ====================");
		builder.Append("\ntotal:\t" + counter.total + "\n");
		builder.Append("\nstep:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(i + "\t");
		builder.Append("\nfirst:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.firstHintMoves[i] + "\t");
		builder.Append("\nno first:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.firstNoHintMoves[i] + "\t");
		builder.Append("\nchance first:\t");
		for (int i = 0; i < MaxStep; i++)
		{
			int all = counter.firstHintMoves[i] + counter.firstNoHintMoves[i];
			string chance = all == 0 ? "" : ((float)counter.firstHintMoves[i] / all).ToString("f2");
			builder.Append(chance + "\t");
		}
		builder.Append("\nend without hint:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.endWithoutHint[i] + "\t");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\nstep:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(i + "\t");
		builder.Append("\nfirst unique:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.firstHintUniqueMoves[i] + "\t");
		builder.Append("\nno first unique:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.firstNoHintUniqueMoves[i] + "\t");
		builder.Append("\nchance first unique:\t");
		for (int i = 0; i < MaxStep; i++)
		{
			int all = counter.firstHintUniqueMoves[i] + counter.firstNoHintUniqueMoves[i];
			string chance = all == 0 ? "" : ((float)counter.firstHintUniqueMoves[i] / all).ToString("f2");
			builder.Append(chance + "\t");
		}
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\nsecond:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.secondHintMoves[i] + "\t");
		builder.Append("\nchance second:\t");
		for (int i = 0; i < MaxStep; i++)
		{
			int all = counter.secondHintMoves[i] + counter.secondNoHintMoves[i];
			string chance = all == 0 ? "" : ((float)counter.secondHintMoves[i] / all).ToString("f2");
			builder.Append(chance + "\t");
		}
		builder.Append("\nsecond delta:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.secondDeltaHintMoves[i] + "\t");
		builder.Append("\nchance second delta:\t");
		for (int i = 0; i < MaxStep; i++)
		{
			int all = counter.secondDeltaHintMoves[i] + counter.secondDeltaNoHintMoves[i];
			string chance = all == 0 ? "" : ((float)counter.secondDeltaHintMoves[i] / all).ToString("f2");
			builder.Append(chance + "\t");
		}
		builder.Append("\n");
		builder.Append("\nsecond unique:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.secondHintUniqueMoves[i] + "\t");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\nthird:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.thirdHintMoves[i] + "\t");
		builder.Append("\nthird delta:\t");
		for (int i = 0; i < MaxStep; i++)
			builder.Append(counter.thirdDeltaHintMoves[i] + "\t");
		builder.Append("\n");
		builder.Append("\n");
		builder.Append("\n");
	}

}
