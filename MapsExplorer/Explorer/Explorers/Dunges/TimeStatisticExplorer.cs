using MapsExplorer;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class TimeStatisticExplorer : ExplorerBase
{
	public override void Work()
	{
		Plot2d plot = new Plot2d();
		Table tds = new Table();
		for (int i = 0; i < _resultLines.Count; i++)
		{
			LogLine line = _resultLines[i];
			Dunge dunge = DungeonLogHandler.GetDunge(line, _dungeonExploreMode);
			Map map = dunge.Maps[dunge.LastFloor - 1];
			tds.AddRow();
			tds.Add("Ссылка", line.Link);
			tds.Add("Старт", Utils.GetDateAndTimeString(dunge.StartDateTime));
			tds.Add("Финиш", Utils.GetDateAndTimeString(dunge.EndDateTime));
			var time = dunge.EndDateTime - dunge.StartDateTime;
			tds.Add("Время", time.ToString());
			tds.Add("Минуты", time.TotalMinutes.ToString());
			tds.Add("Карта", (dunge.LookAsAqua && dunge.LogLine.Category != MapCategory.Аква) ? "Аква?" : line.Category.ToString());
			tds.Add("Кастомное", line.Custom ? "Кастомное" : "");
			tds.Add("Тип", line.DungeKind.ToString());
			tds.Add("Ширина", map.Width.ToString());
			tds.Add("Высота", map.Height.ToString());
			tds.Add("Результат", line.Vault ? "Кладовка" : line.Success ? "Успех" : "Провал");
			tds.Add("Боссы", dunge.BossFights.ToString());
			tds.Add("Этаж", dunge.LastFloor.ToString());
			tds.Add("Шаги", dunge.Moves.Count.ToString());
			tds.Add("Гласы", dunge.VoicesCount.ToString());
			ReportProgress(i);
		}
		string exploreRes = tds.ToString();
		File.WriteAllText(Paths.ResultsDir + "/" + _dungeonExploreMode + ".txt", exploreRes);
		TableText = exploreRes;
	}
}
