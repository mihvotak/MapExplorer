using AngleSharp;
using MapsExplorer;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

public class PolygonLogHandler : LogHandler
{
	private PolygonExploreMode _mode;

	public PolygonLogHandler() : base()
	{

	}

	public Polygon GetPolygon(LogLine line, PolygonExploreMode mode = PolygonExploreMode.None)
	{
		_mode = mode;
		LastError = "";
		string dir = Paths.PolygonsBaseDir + "/" + Utils.GetDateFolderString(line.DateTime);
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
		Polygon polygon = new Polygon(line);
		Parse(content, polygon, false);
		if (!string.IsNullOrEmpty(LastError))
			return null;
		return polygon;
	}

	private async void Parse(string html, Polygon polygon, bool isDesc)
	{
		var document = await _context.OpenAsync(req => req.Content(html));
		var godsContainer = document.QuerySelectorAll("div").First(e => e.Id == "h_tbl");
		
		var gods = godsContainer.QuerySelectorAll("div.t_line");
		
		for (int i = 0; i < gods.Length; i++)
		{
			var godDiv = gods[i];
			GodResult god = new GodResult();
			var c1 = godDiv.QuerySelector("div.c1");
			string c1text = c1.TextContent;
			god.Letter = c1text.Substring(0, 1);
			int i1 = c1text.IndexOf("+");
			int i2 = c1text.IndexOf("+", i1 + 1);
			god.BossName = c1text.Substring(i1 + 1, i2 - (i1 + 1));
			var link = c1.QuerySelector("a");
			god.Link = link.GetAttribute("href");
			god.GodName = link.TextContent;
			var c2 = godDiv.QuerySelector("div.c2");
			var ple = c2.QuerySelector("div.ple");
			var hpe = ple.QuerySelector("div.hpe");
			var spans = hpe.QuerySelectorAll("span");
			foreach (var span in spans)
			{
				string id = span.GetAttribute("id");
				if (id.Length == 7 && id.Substring(id.Length - 3, 3) == "_hp")
				{
					int.TryParse(span.TextContent, out god.BossEndHP);
				}
			}
			string pleStr = ple.TextContent;
			int index = pleStr.LastIndexOf('/');
			string hp = pleStr.Substring(index + 1, pleStr.Length - (index + 1));
			int.TryParse(hp, out god.BossFullHP);
			var cg = c2.QuerySelector("span.cg");
			var cgText = cg.TextContent;
			cgText = cgText.Substring(0, cgText.IndexOf(']'));
			var tIndex = cgText.LastIndexOf("\t");
			cgText = cgText.Substring(tIndex + 1, cgText.Length - (tIndex + 1));
			int plusIndex = cgText.IndexOf('+');
			if (plusIndex != -1)
				int.TryParse(cgText.Substring(plusIndex - 1, 1), out god.Bytes);
			else
				god.Bytes = 0;
			int braIndex = cgText.IndexOf('[');
			var bits = cgText.Substring(braIndex + 1, cgText.Length - (braIndex + 1));
			int dotIndex = bits.IndexOf('.');
			bits = bits.Substring(0, dotIndex);
			god.Bits = bits.Length;
			polygon.GodResults[i] = god;
		}

		string mapsBra = "var d = [";
		string mapsKet = "];";
		int index1 = html.IndexOf(mapsBra);
		int index1f = html.IndexOf(mapsKet, index1);
		string mapsStr = html.Substring(index1 + mapsBra.Length - 1, index1f - (index1 + mapsBra.Length) + 2);
		//
		JArray all = JArray.Parse(mapsStr);
		string res = "";
		int stepNum = 1;
		PolyMap lastMap = null;
		foreach (var stepObj in all)
		{
			PolyMaps4 maps4 = new PolyMaps4();
			polygon.Maps.Add(maps4);
			maps4.StepNum = stepNum;
			int move = 0;
			foreach (var partObj in stepObj as JArray)
			{
				PolyMap map = new PolyMap();
				maps4.Maps.Add(map);
				map.Move = move;
				var arr = partObj as JArray;
				int step = (int)arr[0];

				var health4 = arr[2] as JObject;
				map.BossA.HP = (int)health4.GetValue(Poly.A);
				map.BossB.HP = (int)health4.GetValue(Poly.B);
				map.BossC.HP = (int)health4.GetValue(Poly.C);
				map.BossD.HP = (int)health4.GetValue(Poly.D);

				var deltaHealth4 = arr[3] as JObject;
				map.BossA.DeltaHP = (int)(deltaHealth4.GetValue(Poly.A) ?? 0);
				map.BossB.DeltaHP = (int)(deltaHealth4.GetValue(Poly.B) ?? 0);
				map.BossC.DeltaHP = (int)(deltaHealth4.GetValue(Poly.C) ?? 0);
				map.BossD.DeltaHP = (int)(deltaHealth4.GetValue(Poly.D) ?? 0);

				var bits4 = arr[4] as JObject;
				map.BossA.Bits = (int)bits4.GetValue(Poly.A);
				map.BossB.Bits = (int)bits4.GetValue(Poly.B);
				map.BossC.Bits = (int)bits4.GetValue(Poly.C);
				map.BossD.Bits = (int)bits4.GetValue(Poly.D);
				if (lastMap != null)
				{
					map.BossA.DeltaBits = map.BossA.Bits - lastMap.BossA.Bits;
					map.BossB.DeltaBits = map.BossB.Bits - lastMap.BossB.Bits;
					map.BossC.DeltaBits = map.BossC.Bits - lastMap.BossC.Bits;
					map.BossD.DeltaBits = map.BossD.Bits - lastMap.BossD.Bits;
				}

				int y = 0;
				foreach (var row in arr[1])
				{
					int x = 0;
					foreach (var cellObj in row as JArray)
					{
						PolyCell cell = new PolyCell();
						if (cellObj.Type.ToString() == "String")
						{
							cell.Value = (string)cellObj;
						}
						else
						{
							var cellArr = cellObj as JArray;
							for (int part = 0; part <  cellArr.Count; part++)
							{
								if (part == 0)
									cell.Value = (string)cellArr[part];
								if (part == 1)
									cell.Arrow = (string)cellArr[part];
								if (part == 2)
								{
									cell.Influence = Poly.GetInfluenceByStr((string)cellArr[part]);
								}
							}
						}
						if (cell.Value == Poly.A || cell.Value == Poly.B || cell.Value == Poly.C || cell.Value == Poly.D)
						{
							BossState boss = map.GetBossByLetter(cell.Value);
							boss.Pos = new Int2(x, y);
							if (lastMap != null)
							{
								BossState last = lastMap.GetBossByLetter(cell.Value);
								if (last.Pos != boss.Pos)
									boss.DeltaPos = boss.Pos - last.Pos;
								if (cell.Value != null)
									boss.ArrowStr = cell.Arrow;
								if (cell.Influence != InfluenceKind.None)
									boss.Influence = cell.Influence;
							}
						}
						x++;
					}
					y++;
				}
				move++;
				lastMap = map;
			}
			stepNum++;
			Debug.WriteLine(res);
		}
	}
}
