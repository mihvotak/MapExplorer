using AngleSharp.Html.Parser;
using MapsExplorer;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class SeaLogHandler : LogHandler
{
    private SeaExploreMode _mode;

    public SeaLogHandler() : base()
    {

    }

    public Sea GetSea(LogLine line, SeaExploreMode mode = SeaExploreMode.None)
    {
        _mode = mode;
        LastError = "";
        string dir = Paths.SeasBaseDir + "/" + Utils.GetDateFolderString(line.DateTime);
        string localPath = dir + "/" + line.Hash + ".html";
        string content = null;
        if (File.Exists(localPath))
        {
            content = File.ReadAllText(localPath);
        }
        else
        {
            string address = Paths.GetLogPath(line.Hash);
            content = WebLoader.GetContent(address, out LastError);
            if (string.IsNullOrEmpty(content))
                return null;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(localPath, content);
        }

        if (string.IsNullOrEmpty(content))
            return null;
        Sea sea = new Sea(line);
        try
        {
            Parse(content, sea);
        }
        catch (Exception error) when (error is FormatException || error is JsonException || error is InvalidOperationException || error is OverflowException)
        {
            LastError = "Sea " + line.Hash + ": " + error.Message;
            return null;
        }
        if (!string.IsNullOrEmpty(LastError))
            return null;
        return sea;
    }

    // Synchronous by design: callers must receive a completely parsed history.
    public Sea Parse(string html, LogLine line)
    {
        var sea = new Sea(line);
        Parse(html, sea);
        return sea;
    }

    private static JToken ReadVariable(string script, string name, JToken fallback)
    {
        var match = Regex.Match(script, @"\bvar\s+" + Regex.Escape(name) + @"\s*=\s*");
        if (!match.Success)
            return fallback;
        using (var text = new StringReader(script.Substring(match.Index + match.Length)))
        using (var reader = new JsonTextReader(text))
            return JToken.ReadFrom(reader);
    }

    private void Parse(string html, Sea sea)
    {
        var document = new HtmlParser().ParseDocument(html);
        var script = string.Join("\n", document.QuerySelectorAll("script").Select(s => s.TextContent));
        var initial = (JArray)ReadVariable(script, "m", new JArray());
        var changes = (JObject)ReadVariable(script, "mc", new JObject());
        var tracks = (JObject)ReadVariable(script, "tr", new JObject());
        var hints = (JObject)ReadVariable(script, "smhc", new JObject());
        var staticHints = (JArray)ReadVariable(script, "smh", new JArray());
        if (initial.Count == 0 && !changes.Properties().Any())
            throw new FormatException("В логе не найдена история морской карты (m/mc).");

        foreach (var row in document.QuerySelectorAll("#h_tbl .t_line"))
        {
            var heading = row.QuerySelector(".c1");
            if (heading == null)
                continue;
            var number = Regex.Match(heading.TextContent, @"^\s*([1-4])\.\s*(.*?)\s*/", RegexOptions.Singleline);
            if (!number.Success)
                continue;
            var god = new SeaGodResult { Number = number.Groups[1].Value, HeroName = number.Groups[2].Value.Trim() };
            var link = heading.QuerySelector("a");
            god.GodName = link == null ? "" : link.TextContent.Trim();
            god.Link = link == null ? "" : link.GetAttribute("href");
            int.TryParse(row.QuerySelector("#pl_" + god.Number + "_hp")?.TextContent, out god.ArkEndHP);
            int.TryParse(row.QuerySelector("#pl_" + god.Number + "_w")?.TextContent, out god.ArkEndFood);
            var cargo = row.QuerySelector(".cg")?.TextContent ?? "";
            god.Prizes = Regex.Matches(cargo, @"\[[^\]]*[^\s\[\]\u00a0][^\]]*\]").Count;
            god.Relicts = Regex.Matches(cargo, "🏺").Count;
            sea.GodResults[int.Parse(god.Number) - 1] = god;
        }

        var entries = new List<SeaLogEntry>();
        foreach (var row in document.QuerySelectorAll(".new_line[data-t]"))
        {
            int step;
            if (!int.TryParse(row.GetAttribute("data-t"), out step) || step < 1)
                continue;
            var actor = Regex.Match(row.ClassName ?? "", @"\bsaild_([1-4])\b");
            var text = row.QuerySelector(".text_content .t") ?? row.QuerySelector(".text_content");
            if (text != null)
                entries.Add(new SeaLogEntry { StepNum = step, ArkNumber = actor.Success ? actor.Groups[1].Value : null,
                    Text = Regex.Replace(text.TextContent, @"\s+", " ").Trim() });
        }

        int totalSteps = entries.Select(e => e.StepNum).DefaultIfEmpty(1).Max();
        foreach (var change in changes.Properties())
            totalSteps = Math.Max(totalSteps, int.Parse(change.Name));
        foreach (var track in tracks.Properties())
            totalSteps = Math.Max(totalSteps, ((JArray)track.Value).Count);
        int lastStep;
        if (int.TryParse(document.QuerySelector("#turn_num")?.TextContent, out lastStep))
            totalSteps = Math.Max(totalSteps, lastStep);

        var previous = new SeaMap();
        ApplyCells(previous, initial);
        // smh contains final hints; do not leak them into earlier frames if smhc is present.
        if (!hints.Properties().Any())
            ApplyHints(previous, staticHints);
        for (int step = 1; step <= totalSteps; step++)
        {
            var map = previous.Copy(step);
            var delta = changes[step.ToString()] as JArray;
            if (delta != null)
                ApplyCells(map, delta);
            var hintDelta = hints[step.ToString()] as JArray;
            if (hintDelta != null)
                ApplyHints(map, hintDelta);
            foreach (var track in tracks.Properties())
            {
                var positions = (JArray)track.Value;
                if (step > positions.Count || positions[step - 1].Type != JTokenType.String)
                    continue;
                var position = SeaPosition.Parse((string)positions[step - 1]);
                map.Arks.Add(track.Name, position);
                GetOrAddCell(map, position).ArkNumbers.Add(track.Name);
            }
            map.LogEntries.AddRange(entries.Where(e => e.StepNum == step));
            FindActivations(sea, previous, map);
            foreach (var border in map.Cells.Values.Where(c => c.IsBorder))
            {
                int distance = border.Position.DistanceTo(new SeaPosition(0, 0, 0));
                if (!sea.MinBorderDistance.HasValue || distance < sea.MinBorderDistance.Value)
                    sea.MinBorderDistance = distance;
                if (!sea.MaxBorderDistance.HasValue || distance > sea.MaxBorderDistance.Value)
                    sea.MaxBorderDistance = distance;
            }
            sea.Maps.Add(map);
            previous = map;
        }
        FindLonelyDeepRifts(sea);
    }

    private static void FindLonelyDeepRifts(Sea sea)
    {
        var opened = new HashSet<SeaPosition>();
        var bubbles = new HashSet<SeaPosition>();
        var deepRifts = new HashSet<SeaPosition>();
        foreach (var map in sea.Maps)
            foreach (var cell in map.Cells.Values)
            {
                if (cell.Kind != SeaCellKind.Unknown)
                    opened.Add(cell.Position);
                if (cell.IsBubble)
                    bubbles.Add(cell.Position);
                if (cell.Kind == SeaCellKind.LargeRift)
                    deepRifts.Add(cell.Position);
            }
        // Remember consumed bubbles too: their disappearance must not create false isolation.
        foreach (var position in deepRifts)
            if (position.GetNeighbours().All(p => opened.Contains(p) && !bubbles.Contains(p)))
                sea.LonelyDeepRifts.Add(position);
    }

    private static SeaCell GetOrAddCell(SeaMap map, SeaPosition position)
    {
        SeaCell cell;
        if (!map.Cells.TryGetValue(position, out cell))
        {
            cell = new SeaCell { Position = position, Kind = SeaCellKind.Unknown, Symbol = '?' };
            map.Cells.Add(position, cell);
        }
        return cell;
    }

    private static SeaPosition Decode(long packed, out char symbol)
    {
        uint value = unchecked((uint)packed);
        symbol = (char)(value >> 24);
        return new SeaPosition(unchecked((sbyte)(value & 255)), unchecked((sbyte)((value >> 8) & 255)),
            unchecked((sbyte)((value >> 16) & 255)));
    }

    private static void ApplyHints(SeaMap map, JArray values)
    {
        foreach (var value in values)
        {
            char symbol;
            var position = Decode((long)value, out symbol);
            GetOrAddCell(map, position).InterestHint = symbol;
        }
    }

    private static void ApplyCells(SeaMap map, JArray values)
    {
        foreach (var value in values)
        {
            char symbol;
            var position = Decode((long)value, out symbol);
            var cell = GetOrAddCell(map, position);
            cell.Symbol = symbol;
            cell.ObjectKind = SeaObjectKind.None;
            cell.Treasure = SeaTreasureKind.None;
            switch (symbol)
            {
                case '?': case '_': cell.Kind = SeaCellKind.Unknown; break;
                case ' ': cell.Kind = SeaCellKind.Sea; break;
                case '@': cell.Kind = SeaCellKind.Whirlpool; break;
                case 'p': cell.Kind = SeaCellKind.Port; break;
                case '#': cell.Kind = SeaCellKind.Border; break;
                case '$': cell.Kind = SeaCellKind.ClosedBorder; break;
                case ',': cell.Kind = SeaCellKind.Reefs; break;
                case ';': cell.Kind = SeaCellKind.Ice; break;
                case 'm': cell.Kind = SeaCellKind.Lighthouse; break;
                case 'K': cell.Kind = SeaCellKind.SmallRift; break;
                case 'L': cell.Kind = SeaCellKind.LargeRift; break;
                case 'b': cell.ObjectKind = SeaObjectKind.Fish; break;
                case 'B': cell.ObjectKind = SeaObjectKind.WanderingFish; break;
                case 'C': cell.ObjectKind = SeaObjectKind.Giant; break;
                case 'T': cell.ObjectKind = SeaObjectKind.Leviathan; break;
                case 'W': cell.ObjectKind = SeaObjectKind.SummonedLeva; break;
                case 'g': case 'G':
                    cell.Treasure = SeaTreasureKind.Treasure;
                    if (symbol == 'G') cell.Kind = SeaCellKind.Island;
                    break;
                case 's': cell.Treasure = SeaTreasureKind.Relic; break;
                default:
                    // All island variants (including directional/radius hints) share terrain.
                    if ("Ii<>nvOPQRSrtyuo[]Mf".Contains(symbol))
                        cell.Kind = SeaCellKind.Island;
                    break;
            }
            // An object can temporarily cover a bubble/reef: keep known underlying terrain.
            if ((cell.ObjectKind != SeaObjectKind.None || cell.Treasure != SeaTreasureKind.None)
                && cell.Kind == SeaCellKind.Unknown)
                cell.Kind = SeaCellKind.Sea;
        }
    }

    private static void FindActivations(Sea sea, SeaMap previous, SeaMap map)
    {
        foreach (var ark in map.Arks)
        {
            var bubble = previous.GetCell(ark.Value);
            var cell = map.GetCell(ark.Value);
            // Merely entering a bubble, or a wandering monster covering it, is not activation.
            if (bubble == null || !bubble.IsBubble || cell == null || cell.IsBubble || cell.Symbol != ' ')
                continue;
            SeaPosition previousPosition;
            if (!previous.Arks.TryGetValue(ark.Key, out previousPosition) || !previousPosition.Equals(ark.Value))
                continue;
            var messages = map.LogEntries.Where(e => e.ArkNumber == ark.Key).ToList();
            var text = string.Join(" ", messages.Select(e => e.Text));
            var result = IdentifyRiftResult(previous, map, ark.Value, text);

            var activation = new SeaRiftActivation { StepNum = map.StepNum, Position = ark.Value,
                ArkNumber = ark.Key, BubbleKind = bubble.Kind, Result = result, Text = text };
            // Multiple arks may share a hex. Prefer the one whose chronicle identifies the result.
            var existing = map.RiftActivations.FirstOrDefault(a => a.Position.Equals(ark.Value));
            if (existing != null)
            {
                if (existing.Result != SeaRiftResult.Unknown || result == SeaRiftResult.Unknown)
                    continue;
                map.RiftActivations.Remove(existing);
                sea.RiftActivations.Remove(existing);
            }
            if (result == SeaRiftResult.Leviathan)
            {
                var spawned = map.Cells.Values.Where(c => c.ObjectKind == SeaObjectKind.Leviathan
                    && c.Position.DistanceTo(ark.Value) == 1
                    && previous.GetCell(c.Position)?.ObjectKind != SeaObjectKind.Leviathan).ToList();
                if (spawned.Count == 1)
                    activation.LeviathanPosition = spawned[0].Position;
            }
            map.RiftActivations.Add(activation);
            sea.RiftActivations.Add(activation);
        }
    }

    private static SeaRiftResult IdentifyRiftResult(SeaMap previous, SeaMap map, SeaPosition position, string text)
    {
        // Match activation phrases, not unrelated influences, island visits or ordinary combat.
        if (Regex.IsMatch(text, @"Левиафан.*(?:выбрасывает на поверхность гигантское щупальце|поднимается из глубин и тут же таранит)|из бездонных глубин восста[её]т могучий Левиафан", RegexOptions.IgnoreCase))
            return SeaRiftResult.Leviathan;
        if (Regex.IsMatch(text, @"возникает огромный водоворот", RegexOptions.IgnoreCase))
            return SeaRiftResult.Whirlpool;
        if (Regex.IsMatch(text, @"Подземный взрыв.*чинит ковчег", RegexOptions.IgnoreCase))
            return SeaRiftResult.Repair;
        if (Regex.IsMatch(text, @"Ковчег вызывает извержение.*варит всю рыбу", RegexOptions.IgnoreCase))
            return SeaRiftResult.Supplies;

        if (Regex.IsMatch(text, @"остров|клочок суши", RegexOptions.IgnoreCase))
        {
            // Only a newly raised neighbouring island counts, not an existing nearby island
            // whose hint changed or which was explored during the same turn.
            var islands = position.GetNeighbours().Select(p => map.GetCell(p))
                .Where(c => c != null && c.Kind == SeaCellKind.Island
                    && previous.GetCell(c.Position)?.Kind != SeaCellKind.Island
                    && previous.GetCell(c.Position)?.Kind != SeaCellKind.Lighthouse).ToList();
            if (islands.Count == 1)
                switch (islands[0].Symbol)
                {
                    case 'O': return SeaRiftResult.LeviathanHintIsland; // double question mark
                    case '<': return SeaRiftResult.SuppliesIsland;
                    case 'n': return SeaRiftResult.RepairIsland;
                    case 'v': return SeaRiftResult.PrayerIsland;
                    case '>': return SeaRiftResult.HintIsland;
                    case 'i': return SeaRiftResult.MysteryIsland;
                }
        }
        return SeaRiftResult.Unknown;
    }
}

