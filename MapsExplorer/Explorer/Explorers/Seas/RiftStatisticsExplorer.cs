using MapsExplorer;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

public class RiftStatisticsExplorer : ExplorerBase
{
    public override void Work()
    {
        var activations = new List<SeaRiftActivation>();
        var countedActivations = new HashSet<string>();
        var unknownActivations = new StringBuilder();
        var errors = new StringBuilder();
        int logCount = 0;
        for (int i = 0; i < _resultLines.Count; i++)
        {
            var line = _resultLines[i];
            var sea = SeaLogHandler.GetSea(line, _seaExploreMode);
            if (sea == null)
            {
                errors.AppendLine(line.Link + ": " + SeaLogHandler.LastError);
                SeaLogHandler.LastError = "";
                ReportProgress(i);
                continue;
            }
            foreach (var activation in sea.RiftActivations)
            {
                // A log may occur more than once in overlapping date ranges.
                string key = line.Hash + "/" + activation.StepNum + "/" + activation.Position;
                if (!countedActivations.Add(key))
                    continue;
                activations.Add(activation);
                if (activation.Result == SeaRiftResult.Unknown || activation.Result == SeaRiftResult.Island)
                    unknownActivations.AppendLine(line.Link + "?s=" + activation.StepNum
                        + "\tХод: " + activation.StepNum + "\tКовчег: " + activation.ArkNumber
                        + "\tРазлом: " + activation.BubbleKind + "\t" + activation.Text);
            }
            logCount++;
            ReportProgress(i);
        }

        var statistics = new StringBuilder();
        statistics.AppendLine("Статистика активаций разломов (наблюдаемые вероятности)");
        AppendRiftStatistics(statistics, activations, SeaCellKind.LargeRift, "Большие разломы");
        AppendRiftStatistics(statistics, activations, SeaCellKind.SmallRift, "Малые разломы");
        TableText = statistics.ToString();
        var result = new StringBuilder();
        result.AppendLine("Всего логов: " + logCount);
        result.AppendLine("Всего активаций: " + activations.Count);
        result.Append(statistics);
        result.AppendLine("\nНеопознанные активации:");
        result.Append(unknownActivations.Length == 0 ? "Нет.\n" : unknownActivations.ToString());
        if (errors.Length > 0)
        {
            result.AppendLine("\nОшибки обработки логов:");
            result.Append(errors);
        }
        ResultText = result.ToString();
        Directory.CreateDirectory(Paths.ResultsDir);
        File.WriteAllText(Path.Combine(Paths.ResultsDir, "RiftStatisticsExplorer.txt"), ResultText);
    }

    private static void AppendRiftStatistics(StringBuilder builder, IEnumerable<SeaRiftActivation> activations,
        SeaCellKind kind, string title)
    {
        var counts = activations.Where(a => a.BubbleKind == kind)
            .GroupBy(a => a.Result == SeaRiftResult.Island ? SeaRiftResult.Unknown : a.Result)
            .ToDictionary(g => g.Key, g => g.Count());
        int total = counts.Values.Sum();
        builder.AppendLine("\n" + title + ": " + total + " активаций");
        builder.AppendLine("Эффект\tКоличество\tВероятность");
        var effects = new[] {
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.LeviathanHintIsland, "Остров с указателем на левиафана (⁇)"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.SuppliesIsland, "Остров провизии"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.RepairIsland, "Остров ремонта"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.PrayerIsland, "Остров молитвы"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.HintIsland, "Остров подсказки"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.MysteryIsland, "Остров-загадка (?)"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.Whirlpool, "Водоворот"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.Repair, "Починка ковчега"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.Supplies, "Провизия"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.Leviathan, "Левиафан"),
            new KeyValuePair<SeaRiftResult, string>(SeaRiftResult.Unknown, "Неопознанный эффект")
        };
        foreach (var effect in effects)
        {
            int count;
            counts.TryGetValue(effect.Key, out count);
            string probability = total == 0 ? "-" : (100.0 * count / total).ToString("0.00", CultureInfo.InvariantCulture) + "%";
            builder.AppendLine(effect.Value + "\t" + count + "\t" + probability);
        }
    }
}
