
using MapsExplorer;
using System.Collections.Generic;
using System.Text;

public class CouponsExplorer : ExplorerBase
{
	public override void Work()
	{
		var builder = new StringBuilder();
		Dictionary<string, int> results = new Dictionary<string, int>();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			if (!line.Success)
				continue;
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			foreach (string coupon in dunge.Coupons)
			{
				if (!results.ContainsKey(coupon))
					results.Add(coupon, 0);
				results[coupon]++;
			}
			ReportProgress(i);
		}
		foreach (var pair in results)
		{
			builder.Append(pair.Key + "\t" + pair.Value + "\n");
		}
		TableText = builder.ToString();
	}
}
