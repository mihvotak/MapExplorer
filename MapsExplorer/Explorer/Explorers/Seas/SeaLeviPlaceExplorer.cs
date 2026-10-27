using MapsExplorer;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class SeaLeviPlaceExplorer : ExplorerBase
{
    public override void Work()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Log\tDate\tArk\tBubblePosition\tBubblePortDestance\tLeviPosition\tLeviPortDestance\tMinBorderDistance\tMaxBorderDistance");
        StringBuilder errors = new StringBuilder();
        int counter = 0;
        int bubbleCounter = 0;
        int minBorderDirtance = int.MaxValue;
        var smallSeaPositions = new HashSet<SeaPosition>();
        var otherSeaPositions = new HashSet<SeaPosition>();
        var smallLonelyPositions = new HashSet<SeaPosition>();
        var otherLonelyPositions = new HashSet<SeaPosition>();
        int lonelyCounter = 0;
        for (int i = 0; i < _resultLines.Count; i++)
        {
            LogLine line = _resultLines[i];
            Sea sea = SeaLogHandler.GetSea(line, _seaExploreMode);
            if (sea == null)
            {
                errors.AppendLine(line.Link + ": " + SeaLogHandler.LastError);
                SeaLogHandler.LastError = "";
                ReportProgress(i);
                continue;
            }
            bool isSmall = line.SeaKind == SeaKind.Малое || line.SeaKind2 == SeaKind.Малое;
            (isSmall ? smallLonelyPositions : otherLonelyPositions).UnionWith(sea.LonelyDeepRifts);
            lonelyCounter += sea.LonelyDeepRifts.Count;
            var positions = new HashSet<SeaPosition>();
            foreach (var activation in sea.RiftActivations.Where(a => a.Result == SeaRiftResult.Leviathan))
            {
                if (!positions.Add(activation.Position))
                    continue;
                builder.AppendLine(string.Join("\t", new[] { line.Link, Utils.GetDateAndTimeString(line.DateTime),
                    activation.ArkNumber, 
                    activation.Position.ToString(), activation.Position.PortDistance.ToString(), activation.LeviathanPosition.Value.ToString(), activation.LeviathanPosition.Value.PortDistance.ToString(),
                    sea.MinBorderDistance?.ToString() ?? "-", sea.MaxBorderDistance?.ToString() ?? "-" }));
                bubbleCounter++;
                if (sea.MaxBorderDistance != null)
                {
                    int distance = sea.MaxBorderDistance.Value - activation.Position.PortDistance;
                    if (distance < minBorderDirtance)
                        minBorderDirtance = distance;
                }
                var mapPositions = isSmall ? smallSeaPositions : otherSeaPositions;
                // Mark the activated rift, not the adjacent LeviathanPosition.
                mapPositions.Add(activation.Position);
            }
            counter++;
            ReportProgress(i);
        }
        string exploreRes = builder.ToString();
        Directory.CreateDirectory(Paths.ResultsDir);
        File.WriteAllText(Paths.ResultsDir + "/SeaLeviPlaceExplorer.txt", exploreRes);
        SeaStatMap.Save(Path.Combine(Paths.ResultsDir, "SeaLeviPlaceExplorer_Small.png"),
            smallSeaPositions, "Глубокие разломы — малые карты", SeaStatMap.SmallSeaMaxDistance, smallLonelyPositions);
        SeaStatMap.Save(Path.Combine(Paths.ResultsDir, "SeaLeviPlaceExplorer_Other.png"),
            otherSeaPositions, "Глубокие разломы — большие карты", SeaStatMap.MaxDistance, otherLonelyPositions);
        TableText = exploreRes;
        ResultText = "Всего логов: " + counter + "\nЛевиафанов: " + bubbleCounter
            + "\nОдиноких глубоких разломов: " + lonelyCounter
            + "\nМинимальное расстояние от пузыря до края: " + minBorderDirtance + "\n" + errors;
    }
}
