using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp;
using Newtonsoft.Json.Linq;

namespace MapsExplorer
{
	public class LogHandler
	{
		private readonly string[] VoiceKinds = { "север", "восток", "юг", "запад", "вниз", "спуск", "лестниц", "вверх", "наверх", "подним" };

		private IConfiguration _config;
		private IBrowsingContext _context;
		private RoutesExplorer _routesExplorer;
		private ExploreMode _mode;
		private Regex _bossWarningRegex;

		public LogHandler()
		{
			_config = Configuration.Default;
			_context = BrowsingContext.New(_config);
			_routesExplorer = new RoutesExplorer();
			string content = System.IO.File.ReadAllText("../../../dungeondb2_ru.dat");
			JObject jObject = JObject.Parse(content);
			string warnings = jObject.GetValue("bossHint").ToString();
			_bossWarningRegex = new Regex(warnings);
		}

		public Dunge GetDunge(DungeLine line, ExploreMode mode = ExploreMode.None)
		{
			_mode = mode;
			string content = GetLogContent(line);
			Dunge dunge = new Dunge(line);
			Parse(content, dunge, false);
			if (dunge.ParseError == "DescSort")
			{
				content = GetLogContent(line, 0, true);
				dunge = new Dunge(line);
				Parse(content, dunge, true);
			}
			return dunge;
		}

		private string GetLogContent(DungeLine line, int bossNum = 0, bool desc = false)
		{
			string dir = Paths.BaseDir + "/" + Utils.GetDateFolderString(line.DateTime);
			string localPath = dir + "/" + line.Hash + (bossNum == 0 ? "" : "_" + bossNum) + (desc ? "_desc" : "") + ".html";
			string content;
			if (File.Exists(localPath))
			{
				content = File.ReadAllText(localPath);
			}
			else
			{
				string address = (bossNum == 0 ? Paths.GetDungeLogPath(line.Hash) : Paths.GetBossLogPath(line.Hash, bossNum)) + (desc ? "?sort=desc" : "");
				content = WebLoader.GetContent(address);
				if (!Directory.Exists(dir))
					Directory.CreateDirectory(dir);
				File.WriteAllText(localPath, content);
			}
			return content;
		}

		private async void Parse(string html, Dunge dunge, bool isDesc)
		{
			var document = await _context.OpenAsync(req => req.Content(html));
			var centralBlock = document.QuerySelectorAll("div").First(e => e.Id == "central_block");
			var centralBlockContent = centralBlock.QuerySelectorAll("div").First(e => e.Id == "last_items_arena");
			var logContainer0 = centralBlockContent.QuerySelector("div.d_content");
			var logLines0 = logContainer0.QuerySelectorAll("div.new_line");
			var header0 = centralBlockContent.QuerySelector("div.block_h");
			var descLink0 = header0.QuerySelector("a");
			bool desc0 = descLink0.GetAttribute("title").Contains("Прямая сортировка");
			if (desc0 && !isDesc && dunge.ParseError == null)
			{
				dunge.ParseError = "DescSort";
				return;
			}

			string movesBra = "var moves = [";
			string floorsBra = "var d_aura = {";
			int index1 = html.IndexOf(movesBra);
			int index1f = html.IndexOf(floorsBra);
			int indexA = (int)'a';
			bool with2floors = index1f != -1;
			bool floor2found = false;
			dunge.Maps[0] = new Map();
			int[] lastIndexByFloor = new int[2];
			int lastFloor = 0;
			if (index1 > 0)
			{
				int index2 = html.IndexOf("];", index1);
				if (index2 > 0)
				{
					string movesStrFull = html.Substring(index1 + movesBra.Length, index2 - (index1 + movesBra.Length));
					var movesArrStr = movesStrFull.Split(',');
					string[] floorsArrStr = null;
					if (with2floors)
					{
						int index2f = html.IndexOf("}", index1f);
						string floorsStrFull = html.Substring(index1f + floorsBra.Length, index2f - (index1f + floorsBra.Length) - 1);
						floorsArrStr = floorsStrFull.Split(new string[] { "\",\"" }, StringSplitOptions.None);
					}
					for (int i = 0; i < movesArrStr.Length; i++)
					{
						string moveStr = movesArrStr[i];
						int i1 = moveStr.IndexOf("\"");
						int i2 = moveStr.LastIndexOf("\"");
						if (moveStr.IndexOf("\\\\") != -1)
							i1++;
						string moveX = moveStr.Substring(i1 + 1, 1);
						string moveY = moveStr.Substring(i1 + 2, i2 - (i1 + 2));
						int x = (int)moveX[0] - indexA;
						int y = int.Parse(moveY);
						int floorI = 1;
						if (with2floors)
						{
							string floorStr = floorsArrStr[i + 2];
							string floorValue = floorStr.Split(':')[1];
							floorValue = floorValue.Substring(1, floorValue.Length - 1);
							if (floorValue.Substring(floorValue.Length - 2, 1) == "2")
							{
								floorI = 2;
								if (!floor2found)
								{
									floor2found = true;
									dunge.Maps[1] = new Map();
									dunge.Maps[1].EnterPos = new Int2(x, y);
								}
							}
						}
						dunge.Moves.Add(new Step(x, y, floorI));
						dunge.MoveFloors.Add(floorI);
						lastIndexByFloor[floorI - 1] = i;
						lastFloor = floorI;
					}
				}
			}
			dunge.LastFloor = lastFloor;
			dunge.Maps[0].EnterPos = dunge.Moves[0].Pos;
			if (dunge.DungeLine.Success && !dunge.DungeLine.Vault)
				dunge.TreasurePos = dunge.Moves[dunge.Moves.Count - 1];


			{
				string hpBra = "var hp = {";
				string hpKet = "}}";
				int hpIndex1 = html.IndexOf(hpBra);
				if (hpIndex1 > 0)
				{
					int hpIndex2 = html.IndexOf(hpKet, hpIndex1);
					if (hpIndex2 > 0)
					{
						string hpStrFull = html.Substring(hpIndex1 + hpBra.Length - 1, hpIndex2 + hpKet.Length - (hpIndex1 + hpBra.Length - 1));
						try
						{
							JObject jObject = JObject.Parse(hpStrFull);
							var last = jObject.Last;
							var array = jObject.Children().ToArray();
							for (int i = 0; i < array.Length; i++)
							{
								var moveHps = array[i].Children().ToArray();
								int len = moveHps.Length;
								int[] hps = new int[len];
								dunge.Hps.Add(hps);
								for (int j = 0; j < len; j++)
									hps[j] = (int)moveHps[j];
							}
						}
						catch (Exception e)
						{
							Console.WriteLine("Hp Json parsing exception: " + e.Message);
						}
					}
				}
			}


			var rowDivs = document.QuerySelectorAll("div.dml");
			if (rowDivs.Length != 0)
			{
				Map map = dunge.Maps[0];
				map.Height = rowDivs.Length;
				for (int y = 0; y < rowDivs.Length; y++)
				{
					var row = rowDivs[y];
					var cells = row.QuerySelectorAll("div.dmc");
					map.Width = cells.Length;
					for (int x = 0; x < cells.Length; x++)
					{
						var cellEl = cells[x];
						Cell cell = new Cell();
						string symbol = cellEl.TextContent;
						SetCellContent(cell, symbol);
						map.Cells.Add(cell);
						if (cell.CellKind == CellKind.SecretRoom)
							dunge.SecretRom.Exists = true;
					}
				}
			}
			else
			{
				Int2 endPos = new Int2(0, 0);
				Int2? endPos2 = null;
				string bra = "var d_maps = [[[";
				string ket = "]]]";
				int mapsIndex1 = html.IndexOf(bra);
				if (mapsIndex1 > 0)
				{
					int mapsIndex2 = html.IndexOf(ket, mapsIndex1);
					if (mapsIndex2 > 0)
					{
						string mapsStrFull = html.Substring(mapsIndex1 + bra.Length, mapsIndex2 - (mapsIndex1 + bra.Length));
						string[] mapStrArr = mapsStrFull.Split(new string[] { "]], [[" }, StringSplitOptions.None);
						for (int floor = 1; floor <= 2; floor++)
						{
							//int lastMapIndex = mapsStrFull.LastIndexOf("[[");
							//string lastMapStr = mapsStrFull.Substring(lastMapIndex + 2, mapsStrFull.Length - 2 - (lastMapIndex + 2));
							int mapIndex = lastIndexByFloor[floor - 1];
							if (mapIndex == 0)
								continue;
							Map map = dunge.Maps[floor - 1];
							string mapStr = mapStrArr[mapIndex + 1];
							string[] rows = mapStr.Split(new string[] { "], [" }, StringSplitOptions.None);
							map.Height = rows.Length;
							for (int i = 0; i < rows.Length; i++)
							{
								string rowStr = rows[i];
								//if (i != rows.Length - 1)
								//	rowStr = rowStr.Substring(0, rowStr.Length - 3);
								string[] cells = rowStr.Substring(1, rowStr.Length - 2).Split(new string[] { "\", \"" }, StringSplitOptions.None);
								map.Width = cells.Length;
								for (int j = 0; j < cells.Length; j++)
								{
									//int start = j == 0 ? 1 : 2;
									//int end = cells[j].Length - (start + (j == cells.Length - 1 && i == rows.Length - 1 ? 2 : 1));
									string symbol = cells[j];//.Substring(start, end);
									if (cells[j] == "")
										symbol = " ";
									Cell cell = new Cell();
									SetCellContent(cell, symbol);
									map.Cells.Add(cell);
									if (cell.CellKind == CellKind.SecretRoom)
										dunge.SecretRom.Exists = true;
									if (floor == 1 && cell.CellKind == CellKind.Stairs)
										map.StairsPos = new Int2(j, i);
									if (floor == lastFloor && cell.CellKind == CellKind.End)
										endPos = new Int2(j, i);
									if (floor != lastFloor && cell.CellKind == CellKind.End)
										endPos2 = new Int2(j, i);
								}
							}
						}
					}
				}
				{// check last floor
					Step lastStep = dunge.Moves[lastIndexByFloor[lastFloor - 1]];
					Int2 lastMove = lastStep.Pos;
					Int2 diff = endPos - lastMove;
					if (diff.x != 0 || diff.y != 0)
					{
						dunge.Maps[lastFloor - 1].EnterPos += diff;
						for (int i = 0; i < dunge.Moves.Count; i++)
						{
							if (dunge.Moves[i].Floor == lastFloor)
							{
								Step step = dunge.Moves[i];
								step.Pos = dunge.Moves[i].Pos + diff;
								dunge.Moves[i] = step;
							}
						}
					}
				}
				if (endPos2 != null)
				{
					int floor = lastFloor == 2 ? 1 : 2;
					int moveIndex = lastIndexByFloor[floor - 1] ;
					Step lastStep = dunge.Moves[moveIndex];
					Int2 lastMove = lastStep.Pos;
					Int2 diff = endPos2.Value - lastMove;
					if (diff.x != 0 || diff.y != 0)
					{
						dunge.Maps[floor - 1].EnterPos += diff;
						for (int i = 0; i < dunge.Moves.Count; i++)
						{
							if (dunge.Moves[i].Floor == floor)
							{
								Step step = dunge.Moves[i];
								step.Pos = dunge.Moves[i].Pos + diff;
								dunge.Moves[i] = step;
							}
						}
					}
				}

				/*dunge.Maps[0].EnterPos = dunge.Moves[0].Pos;
				for (int i = 0; i < dunge.Moves.Count; i++)
				{
					if (dunge.Moves[i].Floor == 2)
					{
						dunge.Maps[1].EnterPos = dunge.Moves[i].Pos;
						break;

					}
				}*/
				if (dunge.DungeLine.Success && !dunge.DungeLine.Vault)
					dunge.TreasurePos = dunge.Moves[dunge.Moves.Count - 1];
			}
			int prevFloor = 1;
			for (int i = 0; i < dunge.Moves.Count; i++)
			{
				Step step = dunge.Moves[i];
				int floor = step.Floor;
				Int2 movePos = step.Pos;
				Cell cell = dunge.GetCell(movePos, floor);
				cell.Step = i + 1;
				Map map = dunge.Maps[floor - 1];
				foreach (Int2 dir in Int2.EightDirs)
				{
					Int2 nearPos = movePos + dir;
					if (nearPos.x >= 0 && nearPos.x < map.Width && nearPos.y >= 0 && nearPos.y < map.Height)
					{
						Cell nearCell = dunge.GetCell(nearPos, floor);
						if (nearCell.CellKind == CellKind.Unknown)
							nearCell.CellKind = CellKind.UnknownNotWall;
					}
				}
				if (i > 0 && floor > prevFloor)
					dunge.Maps[prevFloor - 1].StairsPos = dunge.Moves[i - 1].Pos;
				if (floor == 1 && dunge.FirstStairMove == 0 && 
					(dunge.Maps[0].GetCell(movePos).CellKind == CellKind.Stairs
					 || (dunge.LastFloor == 2 && dunge.Maps[0].GetCell(movePos).CellKind == CellKind.End)))
					dunge.FirstStairMove = i;
				prevFloor = floor;
			}
			for (int floor = 1; floor <= 2; floor++)
			{
				Map map = dunge.Maps[floor - 1];
				if (map == null)
					continue;
				for (int x = 0; x < map.Width; x++)
				{
					for (int y = 0; y < map.Height; y++)
					{
						Cell cell0 = map.GetCell(x, y);
						if (cell0.CellKind != CellKind.Teleport)
							continue;
						foreach (Int2 dir in Int2.EightDirs)
						{
							Int2 pos = new Int2(x, y) + dir;
							if (pos.x >= 0 && pos.x < map.Width && pos.y >= 0 && pos.y < map.Height)
							{
								Cell cell = map.GetCell(pos.x, pos.y);
								if (cell.CellKind == CellKind.Unknown)
									cell.CellKind = CellKind.UnknownNotWall;
							}
						}
					}
				}
			}

			int wallCount = 0;
			Map lastMap = dunge.Maps[dunge.LastFloor - 1];
			lastMap.IsLeftWall = true;
			for (int i = 0; i < lastMap.Height; i++)
			{
				Cell cell = lastMap.GetCell(0, i);
				if (cell.CellKind == CellKind.Wall)
					wallCount++;
				else if (cell.CellKind != CellKind.Unknown)
				{
					lastMap.IsLeftWall = false;
					break;
				}
			}
			lastMap.IsLeftWall &= wallCount >= 3;
			wallCount = 0;
			lastMap.IsRightWall = true;
			for (int i = 0; i < lastMap.Height; i++)
			{
				Cell cell = lastMap.GetCell(lastMap.Width - 1, i);
				if (cell.CellKind == CellKind.Wall)
					wallCount++;
				else if (cell.CellKind != CellKind.Unknown)
				{
					lastMap.IsRightWall = false;
					break;
				}
			}
			lastMap.IsRightWall &= wallCount >= 3;
			wallCount = 0;
			lastMap.IsTopWall = true;
			for (int i = 0; i < lastMap.Width; i++)
			{
				Cell cell = lastMap.GetCell(i, 0);
				if (cell.CellKind == CellKind.Wall)
					wallCount++;
				else if (cell.CellKind != CellKind.Unknown)
				{
					lastMap.IsTopWall = false;
					break;
				}
			}
			lastMap.IsTopWall &= wallCount >= 3;
			wallCount = 0;
			lastMap.IsBottomWall = true;
			for (int i = 0; i < lastMap.Width; i++)
			{
				Cell cell = lastMap.GetCell(i, lastMap.Height - 1);
				if (cell.CellKind == CellKind.Wall)
					wallCount++;
				else if (cell.CellKind != CellKind.Unknown)
				{
					lastMap.IsBottomWall = false;
					break;
				}
			}
			lastMap.IsBottomWall &= wallCount >= 3;

			// Определение аквариумов
			int minInX = lastMap.IsLeftWall ? 1 : 0;
			int maxInX = lastMap.IsRightWall ? lastMap.Width - 2 : lastMap.Width - 1;
			int minInY = lastMap.IsTopWall ? 1 : 0;
			int maxInY = lastMap.IsBottomWall? lastMap.Height - 2 : lastMap.Height - 1;
			int emptyCount = 0;
			int wallsCount = 0;
			for (int x = minInX; x <= maxInX; x++)
			{
				for (int y = minInY; y <= maxInY; y++)
				{
					Cell cell = lastMap.GetCell(x, y);
					if (cell.CellKind == CellKind.Wall)
						wallsCount++;
					else if (cell.CellKind != CellKind.Wall && cell.CellKind != CellKind.Unknown)
						emptyCount++;
				}
			}
			if (dunge.LastFloor == 1 && (float)wallsCount / (emptyCount + wallsCount) < 0.1f)
				dunge.WrongDetectedAqua = true;

			var blockH = centralBlock.QuerySelector("div.block_h");
			string stepsStr = blockH.TextContent.Split('/')[1];
			dunge.Steps = int.Parse(stepsStr.Substring(1, stepsStr.IndexOf(")") - 1));

			var membersInfo = document.QuerySelectorAll("div").First(e => e.Id == "hero1_info");
			var lines = membersInfo.QuerySelectorAll("div.new_line");
			foreach (var heroLine in lines)
			{
				Member member = new Member();
				var heroNameEl = heroLine.QuerySelector("label.l_capt");
				if (heroNameEl == null)
					continue;
				var linkEl = heroNameEl.QuerySelector("a");
				string link = linkEl.GetAttribute("href");
				string heroName = linkEl.TextContent;
				member.Hero = heroName;
				int godIndex = link.LastIndexOf("/");
				string godName = link.Substring(godIndex + 1, link.Length - (godIndex + 1));
				member.God = godName;
				var hpStr = heroLine.QuerySelector("div.field_content").TextContent;
				int hpIndex = hpStr.IndexOf("/ ");
				hpStr = hpStr.Substring(hpIndex + 2, hpStr.Length - (hpIndex + 2));
				int hp = int.Parse(hpStr);
				member.Hp = hp;
				dunge.Members.Add(member);
				dunge.MembersSumHp += member.Hp;
			}
			dunge.Voices = new List<VoiceKind>[dunge.Moves.Count + 1];
			// Log reading
			{
				var dateDivs = document.QuerySelectorAll("div.lastduelpl_f");
				int dateDivIndex = 1;
				if (dateDivs[0].TextContent.Contains("Другие логи с тем же кодом:"))
					dateDivIndex++;
				var dateDiv = dateDivs[dateDivIndex].QuerySelector("div");
				string dateStr = dateDiv.TextContent;
				string timeZoneStrS = dateStr.Substring(dateStr.Length - 6, 1);
				string timeZoneStrH = dateStr.Substring(dateStr.Length - 5, 2);
				string timeZoneStrM = dateStr.Substring(dateStr.Length - 2, 2);
				int timeZoneH = int.Parse(timeZoneStrH);
				int timeZoneM = int.Parse(timeZoneStrM);
				TimeSpan timeZone = new TimeSpan((timeZoneStrS == "+" ? 1 : -1) * timeZoneH, timeZoneM, 0);
				TimeSpan timeZoneMsk = new TimeSpan(3, 0, 0);
				TimeSpan timeZoneDiff = timeZoneMsk - timeZone;
				string dateTimeStr = dateStr.Substring(6, 16);
				DateTime localFightDateTime = Utils.ParseDateTime(dateTimeStr);
				DateTime fightDateTime = localFightDateTime + timeZoneDiff;
				dunge.DungeLine.DateTime = fightDateTime;

				List<string> dirStrings = new List<string>() { "север", "восток", "юг", "запад", "северо-восток", "юго-восток", "юго-запад", "северо-запад", "очень холодно", "холодно", "свежо", "тепло", "горячо", "очень горячо", "северной", "восточной", "южной", "западной" };
				List<string> moveDirStrings = new List<string>() { "север", "восток", "юг", "запад" };
				List<Int2> moveDirs = new List<Int2>() { new Int2(0, -1), new Int2(1, 0), new Int2(0, 1), new Int2(-1, 0) };
				int lastLineIndex = logLines0.Length - 1;
				int currentStep = 0;
				bool currentIsNew = false;
				bool cellWithHint = false;
				bool isSecretRoom = false;
				int newCellsCounter = 0;
				List<Cell> visited = new List<Cell>();
				List<Cell> cellsWithHint = new List<Cell>();
				int stepLine = 0;
				Int2? moveDir = null;
				Step move = new Step();
				Cell cell = null;
				for (int i = 0; i < logLines0.Length; i++)
				{
					var logLine = logLines0[i];
					bool bold = logLine.ClassList.Contains("d_imp");
					var turnEl = logLine.QuerySelector("div.d_turn");
					string stepS = logLine.GetAttribute("data-t");
					int stepI = stepS == "" ? currentStep : int.Parse(stepS);
					if (stepI != currentStep)
					{
						move = dunge.Moves[stepI - 1];
						cell = dunge.GetCell(move);
						stepLine = 0;
						moveDir = Int2.Zero;
						currentStep = stepI;
						currentIsNew = !visited.Contains(cell);
						if (currentIsNew)
						{
							if (cell.CellKind == CellKind.Boss)
							{
								foreach (Int2 dir in Int2.FourDirections)
								{
									Cell nearCell = dunge.GetCell(move.Pos + dir, move.Floor);
									if (nearCell.BossWarning.IsWarning && !nearCell.BossWarning.IsUsed)
										nearCell.BossWarning.IsUsed = true;
								}
							}

							bool canBeHint = cell.CellKind != CellKind.Boss && cell.CellKind != CellKind.Trap && cell.CellKind != CellKind.Enter && cell.CellKind != CellKind.SecretRoom && cell.CellKind != CellKind.Teleport && move != dunge.Moves[0];
							if (canBeHint)
								dunge.CanBeHints[stepI] = true;
							if (stepI <= 10)
								newCellsCounter = 0;
							else if (canBeHint)
							{
								newCellsCounter++;
								if (dunge.HintMoves.Count == 0)
									dunge.UniqueSteps = newCellsCounter;
							}
						}
						cellWithHint = cell.CellKind == CellKind.Hint && !cellsWithHint.Contains(cell);
						isSecretRoom = cell.CellKind == CellKind.SecretRoom && currentIsNew;
					}
					bool infl = logLine.QuerySelector("div.infl") != null;
					if (infl)
					{
						string txt = logLine.QuerySelector("div.text_content").TextContent.ToLower();
						if (txt.Contains("«"))
						{
							if (dunge.Voices[stepI] == null)
								dunge.Voices[stepI] = new List<VoiceKind>();
							VoiceKind kind = VoiceKind.пусто;
							for (int vi = 0; vi < VoiceKinds.Length; vi++)
							{
								string voiceStr = VoiceKinds[vi];
								if (txt.Contains(voiceStr))
								{
									kind = vi >= 4 ? VoiceKind.лестн : (VoiceKind)vi;
									break;
								}
							}
							dunge.Voices[stepI].Add(kind);
						}
						continue;
					}
					var lineContentEl = logLine.QuerySelector("div.text_content");
					string lineText = lineContentEl.TextContent;
					string turnText = turnEl.TextContent;
					if (bold)
						AddImportant(dunge, lineText);
					if (!infl)
					{
						if (stepLine == 0)
							visited.Add(cell);
						if (lineText.Contains("Приятный голос откуда-то сверху сообщил, что ни единого живого босса в этом подземелье не осталось"))
							dunge.AllBossesFound = true;
						if (_bossWarningRegex.IsMatch(lineText))
						{
							cell.BossWarning.IsWarning = true;
							cell.BossWarning.Step = stepI;
						}
						if (stepI >= 2)
						{
							int indexDot = lineText.IndexOf(".");
							if (indexDot == -1)
								indexDot = lineText.Length - 1;
							int indexQuest = lineText.IndexOf("?");
							if (indexQuest == -1)
								indexQuest = lineText.Length - 1;
							int indexExcl = lineText.IndexOf("!");
							if (indexExcl == -1)
								indexExcl = lineText.Length - 1;
							int indexS1 = Math.Min(indexDot, Math.Min(indexQuest, indexExcl));

							if (stepLine == 0)
							{
								string movePart = lineText.Substring(0, indexS1);
								if (movePart.Contains(" кричит"))
								{
									int tempIndex = lineText.IndexOf(", ссылаясь на сво");
									if (tempIndex == indexS1 + 2)
									{
										indexS1 = lineText.IndexOf(".", indexS1 + 29);
										movePart = lineText.Substring(0, indexS1);
									}
								}
								else if (movePart.Contains("Да пошли мы"))
								{
									indexS1 = lineText.IndexOf(".", indexS1 + 4);
									movePart = lineText.Substring(0, indexS1);
								}
								else if (movePart.Contains("я знаю, где Флинт зарыл свои сокровища") || movePart.Contains("Иду на Вы") || movePart.Contains("Полный, вперёд") || movePart.Contains("Никто ничего не слышал") || movePart.Contains("Эх, была не была") || movePart.Contains("Я вижу свет"))
								{
									indexS1 = lineText.IndexOf(".", indexS1 + 1);
									movePart = lineText.Substring(0, indexS1);
								}

								Step prevMove = dunge.Moves[(stepI - 1) - 1];
								if (move.Floor == prevMove.Floor)
								{
									Int2 prevMovePos = prevMove.Pos;
									Int2 diff = move.Pos - prevMovePos;
									for (int j = 0; j < moveDirStrings.Count; j++)
									{
										string dirStr = moveDirStrings[j];
										if (movePart.IndexOf(dirStr) != -1)
										{
											moveDir = moveDirs[j];
											if (Math.Abs(diff.x) + Math.Abs(diff.y) > 2) // телепорт
											{
												visited.Remove(cell);
												Int2 movePos = prevMovePos + moveDir.Value;
												Cell moveCell = dunge.Maps[move.Floor - 1].GetCell(movePos.x, movePos.y);
												visited.Add(moveCell);
											}
											break;
										}
									}
								}
								if (moveDir == null)
								{
									if (movePart.Contains("Простояв неподвижно на лестнице целый ход") || movePart.Contains("пропускают ход") || movePart.Contains("дольше века длится ход") || movePart.Contains("стоять на месте") || movePart.Contains("дополнительный ход") || movePart.Contains("герои стоят") || movePart.Contains("Ловушка крепко держит героев") || movePart.Contains("побездельничать один ход"))
										moveDir = Int2.Zero;
								}
								if (moveDir == null && !movePart.Contains("камнеед"))
									dunge.UnknownMoveDir = Trim(movePart);
							}
							else
							{
								indexS1 = 0;
							}

							if (cellWithHint || isSecretRoom)
							{
								indexDot = lineText.IndexOf(".", indexS1 + 3);
								if (indexDot == -1)
									indexDot = lineText.Length;
								indexQuest = lineText.IndexOf("?", indexS1 + 3);
								if (indexQuest == -1)
									indexQuest = lineText.Length;
								indexExcl = lineText.IndexOf("!", indexS1 + 3);
								if (indexExcl == -1)
									indexExcl = lineText.Length;
								int indexS2 = Math.Min(indexDot, Math.Min(indexQuest, indexExcl));
								if (indexS2 == lineText.Length)
								{
									stepLine++;
									continue;
								}
								/*indexDot = lineText.LastIndexOf(".");
								if (indexDot == -1)
									indexDot = 0;
								indexQuest = lineText.LastIndexOf("?");
								if (indexQuest == -1)
									indexQuest = 0;
								indexExcl = lineText.LastIndexOf("!");
								if (indexExcl == -1)
									indexExcl = 0;
								int indexL = Math.Max(indexDot, Math.Max(indexQuest, indexExcl));
								if (indexL <= indexF)
								{
									stepLine++;
									continue;
								}
								indexDot = lineText.LastIndexOf(".", indexL - 3);
								if (indexDot == -1)
									indexDot = 0;
								indexQuest = lineText.LastIndexOf("?", indexL - 3);
								if (indexQuest == -1)
									indexQuest = 0;
								indexExcl = lineText.LastIndexOf("!", indexL - 3);
								if (indexExcl == -1)
									indexExcl = 0;
								indexL = Math.Max(indexDot, Math.Max(indexQuest, indexExcl));
								if (indexL <= indexF)
								{
									stepLine++;
									continue;
								}*/

								string sentense = lineText.Substring(indexS1, indexS2 - indexS1);
								if (isSecretRoom)
								{
									if (sentense.Contains("В тайной комнате команда находит брошенный без присмотра пульт безумного ученого") || sentense.Contains("Услышав доносящиеся издалека крики «Зацеп") || sentense.Contains("пульт вмешательства в личную жизнь подземелья") || sentense.Contains("Тайную комнату наполняет шелест летающих ключей") || sentense.Contains("В этой комнате лежит книга судеб") || sentense.Contains("В этой комнате ожидает эвакуатора менее удачливая команда искателей сокровищ"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 1);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("Стащив у сладко спящего охранника связку ключей, приключенцы торопливо вчитываются в бирки"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 5);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									AddSecretRoomEffect(dunge, sentense);
								}
								else if (cellWithHint)
								{
									if (sentense.Contains("покосившийся светофор") || sentense.Contains("Уважаемые вкладчики") || sentense.Contains("Сокровища за углом") || sentense.Contains("Ищете сокровищницу") || sentense.Contains("Вам сокровищницу") || sentense.Contains("носок из путеводного клубка") || sentense.Contains("статуя из белого мрамора") || sentense.Contains("Группа гномов-шахтёров лупит касками по каменному полу пещеры") || sentense.Contains("сдохших от резкой перемены климата") || sentense.Contains("Здесь, судя по всему, когда-то был клад"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 1);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("На полу валяется старый счёт-фактура"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 15);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("суриком нарисована служебная отметка"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 8);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("вдруг послышалось шипение и пробивающиеся сквозь него слова"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 20);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("В углу крутится старый патефон"))
									{
										indexS2 = lineText.IndexOf(".", indexS2 + 57);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("Только не к моей сокровищнице") || sentense.Contains("Пиастры") || sentense.Contains("поднимает с пола записку, гласящую"))
									{
										indexS2 = lineText.IndexOf("!", indexS2 + 1);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("На стене мигает неоновая вывеска"))
									{
										indexS2 = lineText.IndexOf("!", indexS2 + 17);
										sentense = lineText.Substring(indexS1, indexS2 - indexS1);
									}
									else if (sentense.Contains("но улики уже указывают на") || sentense.Contains("сундуки с сокровищами тащили куда-то на") || sentense.Contains("Статуя денежной жабы немигающим взглядом смотрит на") || sentense.Contains("Расположенный в этой комнате угадатель показывает на") || sentense.Contains("Безо всякой на то причины приключенцы чувствуют, что здесь") || sentense.Contains("Шестое чувство и огромная мерцающая стрелка на полу подсказывают приключенцам, что им на") || sentense.Contains("Здесь склонившийся над картой штурман увлечённо прокладывает курс валют на") || sentense.Contains("На стене надпись: «Сокровищ тут нет, зато") || sentense.Contains("Золотые жилы вдоль стен и потолка тянутся на") || sentense.Contains("Судя по следам термической эрозии на стенах и потолке,") || sentense.Contains("По данным синоптиков, в этой комнате") || sentense.Contains("Из кошельков приключенцев выпрыгивают золотые монетки и дружно катятся на") || sentense.Contains("Обитающий здесь дух наживы указывает команде на") || sentense.Contains("При виде героев учёный кот сначала идёт на") || sentense.Contains("В этой комнате сбились настройки климат-контроля, и теперь тут") || sentense.Contains("В погоне за производственными показателями здесь установили сразу две стрелки: на") || sentense.Contains("Приваренный к стене меч-кладенец указывает на") || sentense.Contains("Герои видят, как деньги налогоплательщиков уходят на") || sentense.Contains("Здесь на столе в ряд стоят несколько китайских болванчиков, качающих головой то на") || sentense.Contains("Большие деньги любят тишину, поэтому на") || sentense.Contains("испещрена фраунгоферовыми линиями") || sentense.Contains("Приятный голос откуда-то сверху сообщает, что потерявшие друг друга могут встретиться на") || sentense.Contains("устлан золотистой пыльцой цветка папоротника") || sentense.Contains("Огненные следы бегают здесь по полу, стекаясь на") || sentense.Contains("Золотистая ящерка на стене, сверкнув изумрудными глазками, стремительно уносится на") || sentense.Contains("Приключенцы чувствуют, что одеты не по погоде") || sentense.Contains("Глобальное потепление привело к тому, что в этой комнате") || sentense.Contains("Углеродные аллотропки ведут на") || sentense.Contains("чует едва уловимый запах наживы") || sentense.Contains("Листочки с денежного дерева уносятся вихрем на") || sentense.Contains("трубам гномы откачивают грунтовые воды, заливающие сокровищницу") || sentense.Contains("Настенная живопись повествует о восходе и закате древних империй") || sentense.Contains("Золотистая позёмка метёт на") || sentense.Contains("Шерсть привязанного здесь барана отливает золотом") || sentense.Contains("Корявая надпись на стене гласит") || sentense.Contains("Шестым чувством приключенцы понимают, что идти нужно на") || sentense.Contains("Сидящие у костра двенадцать месяцев опять что-то не поделили — в этой комнате") || sentense.Contains("Разгадав нарисованную на стене древнюю головоломку, приключенцы понимают, что им на") || sentense.Contains("Расположенное здесь рейтинговое агентство характеризует перспективы героев взять клад оценкой") || sentense.Contains("небрежно машет тряпкой уборщица") || sentense.Contains("Сотни маленьких стрелочек, нарисованные здесь на стенах, указывают на") || sentense.Contains("Метеорологи сообщают, что в этой комнате") || sentense.Contains("Следы былой роскоши ведут на") || sentense.Contains("Здесь гадалка предсказывает героям дальную дорогу на") || sentense.Contains("В этом круге ада") || sentense.Contains("Экономический прогноз ситуации однозначен") || sentense.Contains("Призраки спелеологов ведут здесь вечный спор") || sentense.Contains("Лежащий на полу скелет всем своим направлением указывает на") || sentense.Contains("плывут чудесные золотые облачка") || sentense.Contains("Проезжающий в одиночестве на эвакуаторе герой из предыдущей экспедиции со слезами на глазах показывает на") || sentense.Contains("покрыта золотистым мхом") || sentense.Contains("пока герои не увидели клада, он находится на") || sentense.Contains("Расположенное здесь рейтинговое агентство характеризует перспективы героев взять клад оценкой") || sentense.Contains("Случившийся в этой комнате обвал сформировал стрелку на") || sentense.Contains("Живущий здесь гном-гастарбайтер рассказал, что лично строил это подземелье и точно помнит, что к кассе приходилось идти на") || sentense.Contains("Здесь есть указатель на") || sentense.Contains("Мимо них, сверкая копытами, пробегает на") || sentense.Contains("Огромная золотая змея сбивает приключенцев с ног и уползает на") || sentense.Contains("Стоящий здесь на возвышении позолоченный флюгер недвусмысленно указывает на") || sentense.Contains("На вопрос о местонахождении сокровищницы сонный стражник что-то мямлит и укладывается вздремнуть ногами на") || sentense.Contains("Пусть геройсы идут на") || sentense.Contains("ветер приключений упорно дует на") || sentense.Contains("Остроумная отсылка к сокровищнице указывает на") || sentense.Contains("Слышно, как где-то на ") || sentense.Contains("плачут богатые") || sentense.Contains("доносится аромат жирных трофеев") || sentense.Contains("Тусующаяся здесь золотая молодёжь") || sentense.Contains("Здесь висит загадочная карта, а рядом ходит миловидная девушка, рассказывая") || sentense.Contains("Огненные дорожки бегают здесь по полу, стекаясь на") || sentense.Contains("помечены золотыми рунами «Au»") || sentense.Contains("Плавающий в лужице посреди комнаты валютный курс указывает на") || sentense.Contains("Здесь след из опилок тянется") || sentense.Contains("Здесь денег нет, но вы держитесь") || sentense.Contains("вылетает стайка крошечных фей, осыпая команду золотой пыльцой") || sentense.Contains("В этой комнате приключенцы по очереди встают на весы, но стрелка каждый раз показывает на") || sentense.Contains("Здесь свет клином сошёлся, и этот клин явственно обращён остриём на") || sentense.Contains("Они видят колонну золотоносных муравьёв, целеустремлённо движущихся на") || sentense.Contains("Наткнувшись на схему подземелья, они понимают, что за кладом нужно идти на") || sentense.Contains("Когда-то в этой комнате стоял указатель на") || sentense.Contains("Подброшенная в воздух монетка полетела на") || sentense.Contains("Полчища короедов хищно прут на") || sentense.Contains("Луч путеводной звезды, проникающий сквозь дыру в потолке, указывает на") || sentense.Contains("В этой комнате компас уверенно показывает на") || sentense.Contains("Спрошенные о сокровищах гномы упорно молчат и смотрят куда угодно") || sentense.Contains("Выглянувший из будки суфлёр подсказывает приключенцам, что по сюжету сокровища спрятаны на") || sentense.Contains("доносится перебранка прачек и олигархов") || sentense.Contains("Стоящая здесь статуя регулировщика с двумя жезлами указывает на") || sentense.Contains("Здесь приключенцы видят полную карту подземелья в текстовом формате, сообщающую, что боссы везде, ловушки тоже где угодно, а сокровища где-то на") || sentense.Contains("В хрустальном шаре, стоящем на столе посреди комнаты, они видят самих себя, несущих сокровища с") || sentense.Contains("Проходящие здесь курс галотерапии миллионеры высокомерно сообщают путникам, что на") || sentense.Contains("больно натыкается на стрелку") || sentense.Contains("Кто стремится избежать соблазна роскоши, да не пойдёт на") || sentense.Contains("Ожившие тома Энциклобогии на полках активно обмениваются перекрёстными ссылками, ведущими к сокровищу то ли на") || sentense.Contains("Едва уловимый аромат гоферомонов струится с") || sentense.Contains("Огненные стрелки бегают здесь по полу") || sentense.Contains("На стене крупно написано «Мальчики") || sentense.Contains("Посреди комнаты мечется обезьяна и бормочет: «Умные") || sentense.Contains("По данным синоптиков в этой комнате") || sentense.Contains("Гном за решёткой") || sentense.Contains("вдруг впадает в транс, закатывает глаза и хриплым голосом говорит, что нужно двигаться на") || sentense.Contains("На доске объявлений одиноко висит лист, в котором сказано, что сокровища из этой комнаты перетащили на") || sentense.Contains("Гоблин за решёткой") || sentense.Contains("Нарисованные на стенах глазки игриво подмигивают героям и посматривают то") || sentense.Contains("Они входят в роскошный зал с красной ковровой дорожкой, ведущей") || sentense.Contains("Отсюда сорок мышей несут на") || sentense.Contains("Для лучшей защиты от героев сокровищница перенесена на") || sentense.Contains("Стихи белеют") || sentense.Contains("В этой комнате нет ничего особенного, но на карте почему-то появляется стрелка на") || sentense.Contains("Здесь сумасшедший принтер безостановочно печатает листы со стрелками, указывающими на") || sentense.Contains("символ намекает героям, что сокровище в") || sentense.Contains("символ однозначно говорит о том, что сокровищница в") || sentense.Contains("Стоящий здесь указатель настолько облупился, что уже не понять") || sentense.Contains("И треснул мир напополам") || sentense.Contains("Начерченные на полу пифагоровы штаны"))
									{
										//ok
									}
									else
									{
										dunge.UnknownHint = Trim(sentense);
										continue;
									}

									foreach (string dirStr in dirStrings)
									{
										if (sentense.IndexOf(dirStr) != -1)
										{
											Step step = dunge.Moves[stepI - 1];
											if (dunge.GetCell(step).CellKind == CellKind.Hint)
											{
												dunge.HintMoves.Add(stepI);
												dunge.HintCounters.Add(newCellsCounter);
												newCellsCounter = 0;
												cellsWithHint.Add(cell);
												break;
											}
										}
									}
								}
							}
						}
						stepLine++;
					}
					if (i == lastLineIndex)
					{
						Regex regex = new Regex(@"купон на [^\,]+ ©");
						MatchCollection countryMatch = regex.Matches(lineText);
						foreach (Match match in countryMatch)
						{
							dunge.Coupons.Add(match.Value);
						}
					}
				}
			}
			int hintsCount = 0;
			for (int floor = 1; floor <= 2; floor++)
			{
				Map map = dunge.Maps[floor - 1];
				if (map == null)
					continue;
				foreach (Cell cellI in map.Cells)
				{
					if (cellI.CellKind == CellKind.Hint)
						hintsCount++;
				}
			}
			if (dunge.ParseError == null && hintsCount != dunge.HintMoves.Count)
				dunge.ParseError = "Not all hints found in log!";

			var lastduelplEls = document.QuerySelectorAll("div.lastduelpl");
			foreach (var lastduelplEl in lastduelplEls)
			{
				if (lastduelplEl.QuerySelector("span") != null)
					continue;
				var links = lastduelplEl.QuerySelectorAll("a");
				foreach (var link in links)
				{
					string linkContent = link.TextContent;
					int index = linkContent.IndexOf("№");
					if (index > 0)
					{
						int bossNum = int.Parse(linkContent.Substring(index + 1, 1));
						Boss boss = new Boss();
						boss.Num = bossNum;
						string bossLog = GetLogContent(dunge.DungeLine, bossNum);
						var bossDocument = await _context.OpenAsync(req => req.Content(bossLog));
						var bossCentralBlock = bossDocument.QuerySelectorAll("div").First(e => e.Id == "last_items_arena");
						var logContainer = bossDocument.QuerySelector("div.d_content");
						var logLines = logContainer.QuerySelectorAll("div.new_line");
						var header = bossCentralBlock.QuerySelector("div.block_h");
						var descLink = header.QuerySelector("a");
						bool desc = descLink.GetAttribute("title").Contains("Прямая сортировка");
						if (desc)
						{
							bossLog = GetLogContent(dunge.DungeLine, bossNum, true);
							bossDocument = await _context.OpenAsync(req => req.Content(bossLog));
							bossCentralBlock = bossDocument.QuerySelectorAll("div").First(e => e.Id == "last_items_arena");
							logContainer = bossDocument.QuerySelector("div.d_content");
							logLines = logContainer.QuerySelectorAll("div.new_line");
						}

						var dateDivs = bossDocument.QuerySelectorAll("div.lastduelpl_f");
						int dateDivIndex = 1;
						if (dateDivs[0].TextContent.Contains("Другие логи с тем же кодом:"))
							dateDivIndex++;
						var dateDiv = dateDivs[dateDivIndex].QuerySelector("div");
						string dateStr = dateDiv.TextContent;
						string timeZoneStrS = dateStr.Substring(dateStr.Length - 6, 1);
						string timeZoneStrH = dateStr.Substring(dateStr.Length - 5, 2);
						string timeZoneStrM = dateStr.Substring(dateStr.Length - 2, 2);
						int timeZoneH = int.Parse(timeZoneStrH);
						int timeZoneM = int.Parse(timeZoneStrM);
						TimeSpan timeZone = new TimeSpan((timeZoneStrS == "+" ? 1 : -1) * timeZoneH, timeZoneM, 0);
						TimeSpan timeZoneMsk = new TimeSpan(3, 0, 0);
						TimeSpan timeZoneDiff = timeZoneMsk - timeZone;
						string dateTimeStr = dateStr.Substring(6, 16);
						DateTime localFightDateTime = Utils.ParseDateTime(dateTimeStr);
						DateTime fightDateTime = localFightDateTime + timeZoneDiff;

						var bossMembersInfo = bossDocument.QuerySelectorAll("div").First(e => e.Id == "hero1_info");
						var bossMembersLines = bossMembersInfo.QuerySelectorAll("div.new_line");
						int heroesAndPetsCount = 0;
						foreach (var heroLine in bossMembersLines)
							if (heroLine.QuerySelector("label.l_capt") != null)
								heroesAndPetsCount++;

						bool tribbleAnywhere = false;
						bool tribbleFirst = false;
						string tribbleText = "триббл";
						string escape2 = "без добычи";
						int lastLineIndex = logLines.Length - 1;
						bool firstBlock = true;
						for (int i = 0; i < logLines.Length; i++)
						{
							var logLine = logLines[i];
							var turnEl = logLine.QuerySelector("div.d_turn");
							string stepS = logLine.GetAttribute("data-t");
							int stepI = int.Parse(stepS);
							if (stepI == 1)
								firstBlock = false;
							if (i == 0)
							{
								var timeBlock = logLine.QuerySelector("div.d_capt");
								string timeStr = timeBlock.TextContent;
								string hourS = timeStr.Substring(0, 2);
								string minS = timeStr.Substring(3, 2);
								int hour = int.Parse(hourS);
								int min = int.Parse(minS);
								DateTime date = localFightDateTime.Date;
								bool nextDate = localFightDateTime.Hour > hour;
								DateTime startTime = date + new TimeSpan(nextDate ? 1 : 0, hour, min, 0);
								boss.DateTime = startTime + timeZoneDiff;
							}
							bool infl = logLine.QuerySelector("div.infl") != null;
							string lineText = logLine.TextContent;
							string turnText = turnEl.TextContent;
							if (!infl)
							{
								boss.Escape2 |= lineText.Contains(escape2);
								if (firstBlock)
									tribbleFirst = tribbleFirst || lineText.Contains(tribbleText);
								else
									tribbleAnywhere = tribbleAnywhere || lineText.Contains(tribbleText);
							}
							if (i == lastLineIndex)
							{
								Step step = dunge.Moves[stepI - 1];
								boss.Pos = step;
								dunge.GetCell(step).Boss = boss;
							}
						}
						var lastLine = logLines[lastLineIndex];
						string lastLineStepS = lastLine.GetAttribute("data-t");
						int lastLineStep = int.Parse(lastLineStepS);
						for (int i = logLines.Length - 1; i >= 0; i--)
						{
							var logLine = logLines[i];
							string stepS = logLine.GetAttribute("data-t");
							int step = int.Parse(stepS);
							if (step != lastLineStep)
								break;
							int tribblIndex = logLine.TextContent.IndexOf(tribbleText);
							bool tribbl = tribblIndex > 0;
							boss.TribbleInFinal = boss.TribbleInFinal || tribbl;
							if (tribbl)
								boss.TribbleInFinal2 = logLine.TextContent.IndexOf(tribbleText, tribblIndex + 6) > 0;

						}
						boss.TribbleInMiddle = tribbleAnywhere && !boss.TribbleInFinal;
						boss.TribbleInFirst = tribbleFirst;

						string hpBra = "var hp = {";
						string hpKet = "}}";
						int hpIndex1 = bossLog.IndexOf(hpBra);
						if (hpIndex1 > 0)
						{
							int hpIndex2 = bossLog.IndexOf(hpKet, hpIndex1);
							if (hpIndex2 > 0)
							{
								string hpStrFull = bossLog.Substring(hpIndex1 + hpBra.Length - 1, hpIndex2 + hpKet.Length - (hpIndex1 + hpBra.Length - 1));
								try
								{
									JObject jObject = JObject.Parse(hpStrFull);
									var array = jObject.Children().ToArray();
									int[] hps = null;
									for (int i = 0; i < array.Length; i++)
									{
										var moveHps = array[i].First.ToArray();
										int len = moveHps.Count();
										hps = new int[len];
										boss.Hps.Add(hps);
										for (int j = 0; j < len; j++)
										{
											var hp = moveHps[j].First;
											hps[j] = hp.ToString() == "" ? -1 : (int)hp;
										}
									}
									if (hps != null)
									{
										boss.HeroesWin = false;
										for (int j = 0; j < dunge.Members.Count; j++)
											if (hps[j] > 1)
												boss.HeroesWin = true;
										int bossHpIndex = dunge.Members.Count;
										boss.Escape1 = boss.HeroesWin && hps[bossHpIndex] > 0;
									}
								}
								catch (Exception e)
								{
									Console.WriteLine("Hp Json parsing exception: " + e.Message);
								}
							}
						}

						var bossInfo = bossDocument.QuerySelectorAll("div").First(e => e.Id == "hero2_info");
						var block = bossInfo.QuerySelector("div.block");
						var bossLines = block.Children;
						for (int bi = 0; bi < bossLines.Length; bi++)
						{
							var bossLine = bossLines[bi];
							if (!bossLine.ClassList.Contains("new_line"))
								continue;
							var nameEl = bossLine.QuerySelector("label.l_capt");
							if (nameEl == null)
								continue;
							string name = nameEl.QuerySelector("a").TextContent;
							//if (name.IndexOf("Мини-") == 0)
							//	continue;
							var hpStr = bossLine.QuerySelector("div.field_content").TextContent;
							int hpIndex = hpStr.IndexOf("/ ");
							int tIndex = hpStr.IndexOf("\t", hpIndex);
							if (tIndex == -1)
								tIndex = hpStr.Length;
							hpStr = hpStr.Substring(hpIndex + 2, tIndex - (hpIndex + 2));
							int hp = int.Parse(hpStr);
							string meta = bossLines[bi + 1].TextContent;
							if (string.IsNullOrEmpty(meta)) // "Мини-" без свойств
								continue;
							boss.Name = name;
							boss.Hp = hp;
							meta = meta.Substring(1, meta.Length - 2);
							boss.AllAbilsStr = meta;
							string[] abils = meta.Split(',');
							for (int ai = 0; ai < abils.Length; ai++)
							{
								if (ai != 0)
									abils[ai] = abils[ai].Substring(1, abils[ai].Length - 1);
								Ability ability = (Ability)Enum.Parse(typeof(Ability), abils[ai]);
								if (ability == Ability.тащущий)
									ability = Ability.тащащий;
								boss.Abils.Add(ability);
							}
						}
						if (boss.Abils.Count == 4)
							dunge.SfinPos = boss.Pos;
						dunge.Bosses.Add(boss);
					}
				}
			}
			for (int floor = 1; floor <= 2; floor++)
			{
				Map map = dunge.Maps[floor - 1];
				if (map == null)
					continue;
				for (int y = 0; y < map.Height; y++)
				{
					for (int x = 0; x < map.Width; x++)
					{
						Cell cell = map.GetCell(x, y);
						if (cell.BossWarning.IsWarning && !cell.BossWarning.IsUsed)
						{
							Int2 pos = new Int2(x, y);
							int possibleBosses = 0;
							Cell goodCell = null;
							Int2 goodCellPos = Int2.Zero;
							Int2 treasureDelta = dunge.TreasurePos.Pos - pos;
							bool withTreasureT = dunge.TreasurePos.Floor == floor && treasureDelta.SumMagnitude == 1;
							bool withTreasureL = dunge.TreasurePos.Floor == floor && Math.Abs(treasureDelta.x) == 1 && Math.Abs(treasureDelta.y) == 1;
							if (withTreasureL)
							{
								Int2 pos1 = new Int2(pos.x, dunge.TreasurePos.Pos.y);
								Cell cell1 = map.GetCell(pos1);
								Int2 pos2 = new Int2(dunge.TreasurePos.Pos.x, pos.y);
								Cell cell2 = map.GetCell(pos2);
								if (cell1.CellKind == CellKind.Wall || cell2.CellKind == CellKind.Wall)
									withTreasureL = false;
								else
								{
									if (cell1.Step >= 0 && cell1.CellKind != CellKind.Boss && cell2.CanBeBoss())
									{
										goodCell = cell2;
										goodCellPos = pos2;
										possibleBosses = 1;
									}
									else if (cell2.Step >= 0 && cell2.CellKind != CellKind.Boss && cell1.CanBeBoss())
									{
										goodCell = cell1;
										goodCellPos = pos1;
										possibleBosses = 1;
									}
									else
										withTreasureL = false;
								}
							}
							if (!withTreasureL)
							{
								foreach (Int2 dir in Int2.FourDirections)
								{
									Int2 nearPos = pos + dir;
									if (nearPos.x < 0 || nearPos.x >= map.Width || nearPos.y < 0 || nearPos.y >= map.Height)
										continue;
									Cell nearCell = map.GetCell(nearPos);
									if (nearCell.CanBeBoss() &&
										(!withTreasureT || (dunge.TreasurePos.Pos - nearPos).DiagonalMagnitude == 1))
									{
										possibleBosses++;
										goodCell = nearCell;
										goodCellPos = nearPos;
									}
								}
							}
							if (possibleBosses == 1)
							{
								goodCell.CellKind = CellKind.Boss;
								Boss boss = new Boss();
								boss.Pos = new Step(goodCellPos, floor);
								boss.Name = "(вычислен точно)";
								dunge.Bosses.Add(boss);
								goodCell.Boss = boss;
								foreach (Int2 dir in Int2.FourDirections)
								{
									Int2 nearPos = goodCellPos + dir;
									if (nearPos.x < 0 || nearPos.x >= map.Width || nearPos.y < 0 || nearPos.y >= map.Height)
										continue;
									Cell nearCell = map.GetCell(nearPos);
									if (nearCell.BossWarning.IsWarning && !nearCell.BossWarning.IsUsed)
										nearCell.BossWarning.IsUsed = true;
								}
							}
						}
					}
				}
				int needWarnings = 4;
				while (needWarnings >= 2)
				{
					for (int y = 0; y < map.Height; y++)
					{
						for (int x = 0; x < map.Width; x++)
						{
							Cell cell = map.GetCell(x, y);
							if (cell.CanBeBoss())
							{
								Int2 pos = new Int2(x, y);
								int warnings = 0;
								bool canBeBoss = true;
								bool isFiredWarning = false;
								List<Cell> warningCells = new List<Cell>();
								foreach (Int2 dir in Int2.FourDirections)
								{
									Int2 nearPos = pos + dir;
									if (nearPos.x < 0 || nearPos.x >= map.Width || nearPos.y < 0 || nearPos.y >= map.Height)
									{
										// нет подтверждения
									}
									else
									{
										Cell nearCell = map.GetCell(nearPos);
										if (nearCell.BossWarning.IsWarning)
										{
											warnings++;
											if (!nearCell.BossWarning.IsUsed)
											{
												isFiredWarning = true;
												warningCells.Add(nearCell);
											}
										}
										else if (nearCell.Step >= 0)
										{
											canBeBoss = false;
											break;
										}
									}
								}
								if (canBeBoss && isFiredWarning && warnings >= needWarnings)
								{
									foreach (Cell nearCell in warningCells)
										nearCell.BossWarning.IsUsed = true;
									cell.CellKind = CellKind.Boss;
									Boss boss = new Boss();
									boss.Pos = new Step(pos, floor);
									boss.Name = "(вычислен по " + warnings + " подсказкам)";
									dunge.Bosses.Add(boss);
									cell.Boss = boss;
								}
							}
						}
					}
					needWarnings--;
				}
			}

			if (dunge.DungeLine.Category == Category.Stable)
			{
				Map map = dunge.Maps[0];
				dunge.Stable = new Stable();
				int minSize = Math.Min(map.Width, map.Height);
				int maxSize = Math.Max(map.Width, map.Height);
				if (minSize == 8 && maxSize >= 9)
				{
					dunge.Stable.EnoughInfo = true;
					if (true) // без поворотов и достраивания
					{
						for (int i = 0; i < Int2.FourDirections.Length; i++)
						{
							Int2 dir = Int2.FourDirections[i];
							Int2 pos = map.EnterPos + dir;
							if (map.GetCell(pos.x, pos.y).CellKind == CellKind.Wall)
							{
								Int2 cachePos = map.EnterPos + dir * (-5);
								if (dir.x != 0)
								{
									for (int y = 0; y < map.Height; y++)
										if ((y - map.EnterPos.y) % 2 == 1 && map.GetCell(cachePos.x, y).CellKind == CellKind.Hint)
											dunge.HintOnCache = true;
								}
								else // (dir.y != 0)
								{
									for (int x = 0; x < map.Width; x++)
										if ((x - map.EnterPos.x) % 2 == 1 && map.GetCell(x, cachePos.y).CellKind == CellKind.Hint)
											dunge.HintOnCache = true;
								}
							}
						}
					}
					else
					{
						if (minSize == map.Width)
						{
							dunge.Rotation += 1;
							var newCells = new List<Cell>();
							for (int y = 0; y < map.Width; y++)
							{
								for (int x = 0; x < map.Height; x++)
								{
									newCells.Add(map.GetCell(y, map.Height - x - 1));
								}
							}
							map.Cells = newCells;
							int w = map.Width;
							map.Width = map.Height;
							map.Height = w;
						}
						int enterY = map.Cells.IndexOf(map.Cells.Find(c => c.CellKind == CellKind.Enter)) / map.Width;
						if (enterY == 1)
						{
							dunge.Rotation += 2;
							var newCells = new List<Cell>();
							for (int y = 0; y < map.Height; y++)
							{
								for (int x = 0; x < map.Width; x++)
								{
									newCells.Add(map.GetCell(map.Width - x - 1, map.Height - y - 1));
								}
							}
							map.Cells = newCells;
						}
						bool left = map.GetCell(0, 6).CellKind == CellKind.Wall;
						bool right = map.GetCell(map.Width - 1, 6).CellKind == CellKind.Wall;
						bool both13 = !left && !right && map.Width == 13;
						if (left || right || both13)
						{
							if (right && map.Width < 15)
							{
								int delta = 15 - map.Width;
								AddLeft(map, delta);
							}
							else if (both13)
							{
								AddLeft(map, 1);
							}
							dunge.Stable.EnterPosX = map.Cells.IndexOf(map.Cells.Find(c => c.CellKind == CellKind.Enter)) % map.Width;
							dunge.Stable.TreasurePosX = map.Cells.IndexOf(map.Cells.Find(c => c.CellKind == CellKind.End)) % map.Width;
						}
						else
							dunge.Stable.EnoughInfo = false;
					}
				}
			}
			else if (dunge.DungeLine.Category == Category.Chamomile)
			{
				Map map = dunge.Maps[0];
				for (int i = 0; i < dunge.HintMoves.Count; i++)
				{
					Int2 hintPos = dunge.Moves[dunge.HintMoves[i] - 1].Pos;
					Int2 delta = hintPos - map.EnterPos;
					int dx = Math.Abs(delta.x);
					int dy = Math.Abs(delta.y);
					//if ((dx == 4 && dy == 1) || (dx == 1 && dy == 4))
					if (dx == 2 && dy == 2)
						dunge.HintOnCache = true;
				}
			}
			else if (dunge.DungeLine.Category == Category.Usual)
			{
				if (dunge.DungeLine.Success && !dunge.DungeLine.Vault)
				{
					for (int i = 0; i < dunge.HintMoves.Count; i++)
					{
						Int2 hintPos = dunge.Moves[dunge.HintMoves[i] - 1].Pos;
						Int2 delta = hintPos - dunge.TreasurePos.Pos;
						if (Math.Abs(delta.x) == 1 && Math.Abs(delta.y) == 1)
							dunge.HintOnCache = true;
					}
				}
			}

			if (_mode == ExploreMode.CalculateRoutes && dunge.DungeLine.Category == Category.Usual)
				_routesExplorer.Explore(dunge);
			if (dunge.DungeLine.Success && !dunge.DungeLine.Vault)
			{
				Map map = dunge.Maps[dunge.LastFloor - 1];
				Int2 center = dunge.TreasurePos.Pos;
				Cell leftCell = map.GetCell(center.x - 1, center.y + 0);
				Cell rightCell = map.GetCell(center.x + 1, center.y + 0);
				Cell topCell =	map.GetCell(center.x + 0, center.y - 1);
				Cell bottomCell = map.GetCell(center.x + 0, center.y + 1);

				Cell blCell = map.GetCell(center.x - 1, center.y + 1);
				Cell brCell = map.GetCell(center.x + 1, center.y + 1);
				Cell tlCell = map.GetCell(center.x - 1, center.y - 1);
				Cell trCell = map.GetCell(center.x + 1, center.y - 1);

				dunge.TreasureWalls = 0;
				if (topCell.CellKind == CellKind.Wall) dunge.TreasureWalls++;
				if (bottomCell.CellKind == CellKind.Wall) dunge.TreasureWalls++;
				if (leftCell.CellKind == CellKind.Wall) dunge.TreasureWalls++;
				if (rightCell.CellKind == CellKind.Wall) dunge.TreasureWalls++;

				if (leftCell.CellKind == CellKind.Boss || rightCell.CellKind == CellKind.Boss || topCell.CellKind == CellKind.Boss || bottomCell.CellKind == CellKind.Boss)
					dunge.TreasureSchemeKind = TreasureSchemeKind.Near;
				if (tlCell.CellKind == CellKind.Boss || trCell.CellKind == CellKind.Boss || blCell.CellKind == CellKind.Boss || brCell.CellKind == CellKind.Boss)
					dunge.TreasureSchemeKind = TreasureSchemeKind.Diagonal;
				if (dunge.TreasureWalls == 0)
				{
					if (topCell.CellKind == CellKind.Boss || bottomCell.CellKind == CellKind.Boss || leftCell.CellKind == CellKind.Boss || rightCell.CellKind == CellKind.Boss || 
						topCell.CellKind == CellKind.PossibleBoss || bottomCell.CellKind == CellKind.PossibleBoss || leftCell.CellKind == CellKind.PossibleBoss || rightCell.CellKind == CellKind.PossibleBoss)
						dunge.TreasureScheme = TreasureScheme.O_0w0;
					else
						dunge.TreasureScheme = TreasureScheme.O_0w_hz;
				}
				else if (dunge.TreasureWalls == 1)
				{
					if ((topCell.CellKind == CellKind.Wall && (bottomCell.CellKind == CellKind.Boss || bottomCell.CellKind == CellKind.PossibleBoss))
						|| (bottomCell.CellKind == CellKind.Wall && (topCell.CellKind == CellKind.Boss || topCell.CellKind == CellKind.PossibleBoss))
						|| (leftCell.CellKind == CellKind.Wall && (rightCell.CellKind == CellKind.Boss || rightCell.CellKind == CellKind.PossibleBoss))
						|| (rightCell.CellKind == CellKind.Wall && (leftCell.CellKind == CellKind.Boss || leftCell.CellKind == CellKind.PossibleBoss)))
						dunge.TreasureScheme = TreasureScheme.O_1w_1;
					else if (((topCell.CellKind == CellKind.Wall || bottomCell.CellKind == CellKind.Wall) && (leftCell.CellKind == CellKind.Boss || leftCell.CellKind == CellKind.PossibleBoss || rightCell.CellKind == CellKind.Boss || rightCell.CellKind == CellKind.PossibleBoss))
						||
						((leftCell.CellKind == CellKind.Wall || rightCell.CellKind == CellKind.Wall) && (topCell.CellKind == CellKind.Boss || topCell.CellKind == CellKind.PossibleBoss || bottomCell.CellKind == CellKind.Boss || bottomCell.CellKind == CellKind.PossibleBoss)))
						dunge.TreasureScheme = TreasureScheme.O_1w_2;
					else
						dunge.TreasureScheme = TreasureScheme.O_1w_hz;
				}
				else
				if ((leftCell.CellKind == CellKind.Wall && rightCell.CellKind == CellKind.Wall) ||
					(topCell.CellKind == CellKind.Wall && bottomCell.CellKind == CellKind.Wall))
				{
					dunge.TreasureBetweenWalls = true;
					if (leftCell.CellKind == CellKind.Wall && rightCell.CellKind == CellKind.Wall)
					{
						if (dunge.LastFloor == 2 && ((topCell.CellKind == CellKind.Wall && bottomCell.CellKind == CellKind.Boss)
							|| (bottomCell.CellKind == CellKind.Wall && topCell.CellKind == CellKind.Boss)))
							dunge.TreasureScheme = TreasureScheme.Tu;
						else if (dunge.LastFloor == 2 && ((topCell.CellKind != CellKind.Wall && bottomCell.CellKind == CellKind.Boss)
							|| (bottomCell.CellKind != CellKind.Wall && topCell.CellKind == CellKind.Boss)))
							dunge.TreasureScheme = TreasureScheme.Tu_0;
						else if (dunge.LastFloor == 2 && ((tlCell.CellKind == CellKind.Wall || trCell.CellKind == CellKind.Wall || topCell.CellKind == CellKind.Wall) &&
							(blCell.CellKind == CellKind.Wall || brCell.CellKind == CellKind.Wall || bottomCell.CellKind == CellKind.Wall)))
						{
							dunge.TreasureScheme = TreasureScheme.T2;
							if (topCell.CellKind != CellKind.Wall && bottomCell.CellKind != CellKind.Wall)
								dunge.TreasureScheme = TreasureScheme.T2_0;
							else if ((trCell.CellKind != CellKind.Boss && tlCell.CellKind != CellKind.Boss && bottomCell.CellKind == CellKind.Wall && (brCell.CellKind != CellKind.Wall || blCell.CellKind != CellKind.Wall))
								|| (brCell.CellKind != CellKind.Boss && blCell.CellKind != CellKind.Boss && topCell.CellKind == CellKind.Wall && (trCell.CellKind != CellKind.Wall || tlCell.CellKind != CellKind.Wall)))
								dunge.TreasureScheme = TreasureScheme.C;
						}
					}

					if (topCell.CellKind == CellKind.Wall && bottomCell.CellKind == CellKind.Wall)
					{
						if (dunge.LastFloor == 2 && ((rightCell.CellKind == CellKind.Wall && leftCell.CellKind == CellKind.Boss)
							|| (leftCell.CellKind == CellKind.Wall && rightCell.CellKind == CellKind.Boss)))
							dunge.TreasureScheme = TreasureScheme.Tu;
						else if (dunge.LastFloor == 2 && ((rightCell.CellKind != CellKind.Wall && leftCell.CellKind == CellKind.Boss)
							|| (leftCell.CellKind != CellKind.Wall && rightCell.CellKind == CellKind.Boss)))
							dunge.TreasureScheme = TreasureScheme.Tu_0;
						else if (dunge.LastFloor == 2 && ((tlCell.CellKind == CellKind.Wall || blCell.CellKind == CellKind.Wall || leftCell.CellKind == CellKind.Wall) &&
							(trCell.CellKind == CellKind.Wall || brCell.CellKind == CellKind.Wall || rightCell.CellKind == CellKind.Wall)))
						{
							dunge.TreasureScheme = TreasureScheme.T2;
							if (rightCell.CellKind != CellKind.Wall && leftCell.CellKind != CellKind.Wall)
								dunge.TreasureScheme = TreasureScheme.T2_0;
							else if ((trCell.CellKind != CellKind.Boss && brCell.CellKind != CellKind.Boss && leftCell.CellKind == CellKind.Wall && (blCell.CellKind != CellKind.Wall || tlCell.CellKind != CellKind.Wall))
								|| (tlCell.CellKind != CellKind.Boss && blCell.CellKind != CellKind.Boss && rightCell.CellKind == CellKind.Wall && (trCell.CellKind != CellKind.Wall || brCell.CellKind != CellKind.Wall)))
								dunge.TreasureScheme = TreasureScheme.C;
						}
					}
				}
				else if (dunge.TreasureWalls == 2)
					dunge.TreasureScheme = TreasureScheme.L;

			}
		}

		private void SetCellContent(Cell cell, string symbol)
		{
			if (symbol == "?")
				cell.CellKind = CellKind.Unknown;
			else if (symbol == " ")
				cell.CellKind = CellKind.Empty;
			else if (symbol == "#")
				cell.CellKind = CellKind.Wall;
			else if (symbol == "!")
				cell.CellKind = CellKind.Something;
			else if (symbol == "🚪" || symbol == "\ud83d")
				cell.CellKind = CellKind.Enter;
			else if (symbol == "⚠")
				cell.CellKind = CellKind.PossibleBoss;
			else if (symbol == "💀")
				cell.CellKind = CellKind.Boss;
			else if (symbol == "🕳" || symbol == "+")
				cell.CellKind = CellKind.Trap;
			else if (symbol == "✖")
				cell.CellKind = CellKind.SecretRoom;
			else if (symbol == "~")
				cell.CellKind = CellKind.Teleport;
			else if (symbol == "🔒")
				cell.CellKind = CellKind.ClosedTreasure;
			else if (symbol == "◿")
				cell.CellKind = CellKind.Stairs;
			else if (symbol == "@")
				cell.CellKind = CellKind.End;
			else
			{
				cell.CellKind = CellKind.Hint;
				{
					if (symbol == "↑")
						cell.Hint = Hint.С;
					else if (symbol == "→")
						cell.Hint = Hint.В;
					else if (symbol == "↓")
						cell.Hint = Hint.Ю;
					else if (symbol == "←")
						cell.Hint = Hint.З;
					else if (symbol == "↗")
						cell.Hint = Hint.СВ;
					else if (symbol == "↘")
						cell.Hint = Hint.ЮВ;
					else if (symbol == "↙")
						cell.Hint = Hint.ЮЗ;
					else if (symbol == "↖")
						cell.Hint = Hint.СЗ;
					else if (symbol == "∨")
						cell.Hint = Hint.СВ_СЗ;
					else if (symbol == "<")
						cell.Hint = Hint.СВ_ЮВ;
					else if (symbol == "∧")
						cell.Hint = Hint.ЮЗ_ЮВ;
					else if (symbol == ">")
						cell.Hint = Hint.СЗ_ЮЗ;
					else if (symbol == "⌊")
						cell.Hint = Hint.С_В;
					else if (symbol == "⌈")
						cell.Hint = Hint.Ю_В;
					else if (symbol == "⌉")
						cell.Hint = Hint.Ю_З;
					else if (symbol == "⌋")
						cell.Hint = Hint.С_З;
					else if (symbol == "╩")
						cell.Hint = Hint.СП;
					else if (symbol == "╠")
						cell.Hint = Hint.ВП;
					else if (symbol == "╦")
						cell.Hint = Hint.ЮП;
					else if (symbol == "╣")
						cell.Hint = Hint.ЗП;
					else if (symbol == "✵")
						cell.Hint = Hint.ОчХолодно;
					else if (symbol == "❄")
						cell.Hint = Hint.Холодно;
					else if (symbol == "☁")
						cell.Hint = Hint.Прохладно;
					else if (symbol == "♨")
						cell.Hint = Hint.Тепло;
					else if (symbol == "☀")
						cell.Hint = Hint.Горячо;
					else if (symbol == "✺")
						cell.Hint = Hint.ОчГорячо;
					else
					{
						cell.CellKind = CellKind.Unknown;
						throw new Exception("Unknown symbol: " + symbol);
					}
				}
			}
		}

		private void AddLeft(Map map, int delta)
		{
			var newCells = new List<Cell>();
			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width + delta; x++)
				{
					Cell cell = x >= delta ? map.GetCell(x - delta, y) : new Cell() { CellKind = x == 0 && y >= 5 || y == 7 ? CellKind.Wall : CellKind.Unknown };
					newCells.Add(cell);
				}
			}
			map.Width += delta;
			map.Cells = newCells;
		}

		private void AddSecretRoomEffect(Dunge dunge, string text)
		{
			dunge.SecretRom.Visited = true;

			if (text.Contains("В тайной комнате всё вдруг меняется"))
			{
				dunge.SecretRom.SecretKind = SecretKind.ChangeType;
				int index = text.IndexOf(", сколько");
				string newTypeStr = text.Substring(index + 9);
				DungeKind dungeKind;
				if (TestDungeKind(newTypeStr, out dungeKind))
					dunge.DungeLine.Kind = dungeKind;
				else
					throw new Exception("Not realized: " + text);
			}
			else if (text.Contains("дополнительную порцию трофеев") || text.Contains("увеличивают количество золота в сокровищнице") || text.Contains("увеличивают свой призовой фонд") || text.Contains("бонусной порции золота") || text.Contains("добавляет к сметным сокровищам несметные"))
			{
				dunge.SecretRom.SecretKind = SecretKind.MoreTreasures;
			}
			else if (text.Contains("отключить ловушки подземелья") || text.Contains("отключить в подземелье ловушки"))
			{
				dunge.SecretRom.SecretKind = SecretKind.NoTraps;
			}
			else if (text.Contains("тайники подземелья превратились в ловушки") || text.Contains("все ловушки подземелья в тайники") || text.Contains("отравляет все тайники подземелья, превращая их в ловушки"))
			{
				dunge.SecretRom.SecretKind = SecretKind.ExchangeTrapsAndCaches;
			}
			else if (text.Contains("находят ключ от сокровищницы") || text.Contains("переучёт в сокровищнице закончен") || text.Contains("открываются ворота сокровищницы") || text.Contains("мигом узнает подходящий к сокровищнице и сбивает его метлой") || text.Contains("наконец находят заветную — «к сокровищнице"))
			{
				dunge.SecretRom.SecretKind = SecretKind.TreasureOn;
			}
			else if (text.Contains("узнают, как выбить из здешней сокровищницы лишнее бревно") || text.Contains("двойная порция брёвен"))
			{
				dunge.SecretRom.SecretKind = SecretKind.MoreWoods;
			}
			else if (text.Contains("Услышав доносящиеся издалека крики «Зацеп") || text.Contains("финальный босс вскакивает в своей комнате с койки") || text.Contains("Босс отключён") || text.Contains("отправить финального босса на боковую"))
			{
				dunge.SecretRom.SecretKind = SecretKind.NoFinal;
			}
			else if (text.Contains("Фонтан живой воды в тайной комнате приводит в чувство всех приключенцев"))
			{
				dunge.SecretRom.SecretKind = SecretKind.Undead;
			}
			else if (text.Contains("наконец-то может включить себе указатели на сокровищницу") || text.Contains("отправляются протирать стрелки и устанавливать флюгера") || text.Contains("Здесь приключенцы раскрывают клетку со стрелками"))
			{
				dunge.SecretRom.SecretKind = SecretKind.HintsOn;
			}
			else if (text.Contains("перерисовывают себе все интересные места") || text.Contains("тайное становится явным") || text.Contains("они оставляют нашим героям свою карту с пометками") || text.Contains("разбегаются по интересным клеткам"))
			{
				dunge.SecretRom.SecretKind = SecretKind.ShowInterestPlaces;
			}
			else
				dunge.UnknownSecretResult = Trim(text);
		}

		private string Trim(string text)
		{
			char[] charsToTrim = { ' ', '\t' };
			return text.Trim(charsToTrim);
		}

		private bool TestDungeKind(string text, out DungeKind kind)
		{
			kind = DungeKind.Обыденности;
			if (text.Contains("Бессилия") || text.Contains("Антибожественное покрытие туннелей экранирует влияния"))
				kind = DungeKind.Бессилия;
			else if (text.Contains("Бесшумности") || text.Contains("Боссы здесь тихие и ослабленные"))
				kind = DungeKind.Бесшумности;
			else if (text.Contains("Бодрости") || text.Contains("Здесь лечит сам воздух"))
				kind = DungeKind.Бодрости;
			else if (text.Contains("Густоты") || text.Contains("Пустых комнат здесь почти нет, сплошь тайники да ловушки"))
				kind = DungeKind.Густоты;
			else if (text.Contains("Достатка") || text.Contains("Деньги тут валяются прямо под ногами"))
				kind = DungeKind.Достатка;
			else if (text.Contains("Заброшенности") || text.Contains("Боссы ушли, расставив вместо себя ловушки"))
				kind = DungeKind.Заброшенности;
			else if (text.Contains("Загадки") || text.Contains("У подземелья есть особое свойство, но какое — непонятно"))
				kind = DungeKind.Загадки;
			else if (text.Contains("Залога") || text.Contains("Здесь на входе у каждого берут в залог одно бревно, а для возврата хозяин должен живым дойти до сокровищницы"))
				kind = DungeKind.Залога;
			else if (text.Contains("Заметности") || text.Contains("Хорошее освещение позволяет издалека замечать интересные места"))
				kind = DungeKind.Заметности;
			else if (text.Contains("Изобилия") || text.Contains("В сокровищнице двойная порция бревен"))
				kind = DungeKind.Изобилия;
			else if (text.Contains("Инкассации") || text.Contains("Золото из сокровищницы боссы здесь носят с собой"))
				kind = DungeKind.Инкассации;
			else if (text.Contains("Миграции") || text.Contains("Сила боссов здесь никак не связана с близостью сокровища"))
				kind = DungeKind.Миграции;
			else if (text.Contains("Мольбы") || text.Contains("Здесь почти каждая комната даёт по капле праны"))
				kind = DungeKind.Мольбы;
			else if (text.Contains("Противоречия") || text.Contains("Путаницы") || text.Contains("никак не могут определиться"))
				kind = DungeKind.Противоречия;
			else if (text.Contains("Прыгучести") || text.Contains("Постарайтесь не удариться головой при полетах через клетки"))
				kind = DungeKind.Прыгучести;
			else if (text.Contains("Риска") || text.Contains("Выжившие герои заберут все золото"))
				kind = DungeKind.Риска;
			else if (text.Contains("Сбережения") || text.Contains("После нахождения сокровищницы вся наличность героев автоматически переводится в их сбережения"))
				kind = DungeKind.Сбережения;
			else if (text.Contains("Складчины") || text.Contains("Наличное золото здесь при входе изымается в призовой фонд сокровищницы, где и будет затем раздаваться по справедливости"))
				kind = DungeKind.Складчины;
			else if (text.Contains("Спешки") || text.Contains("Эвакуация здесь происходит через 50 ходов вместо 100"))
				kind = DungeKind.Спешки;
			else if (text.Contains("Термодинамики") || text.Contains("Указатели здесь работают по принципу «горячо-холодно»"))
				kind = DungeKind.Термодинамики;
			else if (text.Contains("Токсичности") || text.Contains("Ядовитый газ в коридорах подрывает здоровье исследователей"))
				kind = DungeKind.Токсичности;
			else if (text.Contains("Чистоты") || text.Contains("Здесь нет ни мощных ловушек, ни серьезных лечилок, ни богатых тайников"))
				kind = DungeKind.Чистоты;

			return kind != DungeKind.Обыденности;
		}

		private void AddImportant(Dunge dunge, string text)
		{
			DungeKind dungeKind;
			if (TestDungeKind(text, out dungeKind))
			{
				dunge.StartKind = dungeKind;
				if (dunge.DungeLine.Kind == DungeKind.Неизвестное)
					dunge.DungeLine.Kind = dungeKind;
			}
			else if (text.Contains("Отдел оптимизации экскурсий обещает посетителям, дошедшим до сокровища за 40 шагов, награду") || text.Contains("Служба нагнетания информирует, что команде положена награда") || text.Contains("Ассоциация вольных стрелков сообщает, что убившим всех боссов здесь полагается награда") || text.Contains("Общество защиты монстров обещает посетителям, не тронувшим ни одного босса"))
			{
				dunge.MiniQuest = true;
			}
			else if (text.Contains("Истлевшая от времени карта подземелья на стене подсказывает, что в специальной комнате команду ждёт награда") || text.Contains("Выдолбленная прямо на стене подсказка обещает посетителям тайной комнаты") || text.Contains("Сидящая в клетке вещая птица предрекает осмелившимся проникнуть в запретное место") || text.Contains("Незнакомец в капюшоне обещает новым посетителям тайной ложи, что"))
			{
				if (text.Contains("в сокровищнице будет больше трофеев"))
					dunge.SecretRom.SecretKind = SecretKind.MoreTreasures;
				else if (text.Contains("найдется бонусная порция золота"))
					dunge.SecretRom.SecretKind = SecretKind.MoreGold;
				else if (text.Contains("в сокровищнице появится двойная порция бревен"))
					dunge.SecretRom.SecretKind = SecretKind.MoreWoods;
				else if (text.Contains("все ловушки в подземелье отключатся"))
					dunge.SecretRom.SecretKind = SecretKind.NoTraps;
				else if (text.Contains("на карте подземелья отметятся все интересные места"))
					dunge.SecretRom.SecretKind = SecretKind.ShowInterestPlaces;
				else if (text.Contains("только там можно будет найти ключ, открывающий сокровищницу"))
					dunge.SecretRom.SecretKind = SecretKind.TreasureOn;
				else if (text.Contains("указатели на сокровище можно включить только там"))
					dunge.SecretRom.SecretKind = SecretKind.HintsOn;
				else if (text.Contains("все ловушки подземелья превратятся в тайники. Или наоборот"))
					dunge.SecretRom.SecretKind = SecretKind.ExchangeTrapsAndCaches;
				else if (text.Contains("охраняющий сокровищницу босс уйдет со своего поста"))
					dunge.SecretRom.SecretKind = SecretKind.NoFinal;
				else if (text.Contains("павшие герои будут оживлены"))
					dunge.SecretRom.SecretKind = SecretKind.Undead;
				else if (text.Contains("подземелье изменит свой тип"))
					dunge.SecretRom.SecretKind = SecretKind.ChangeType;
				else
					dunge.UnknownImportant = Trim(text);
			}
			else if (text.Contains("чудесным образом появляется отметка тайной комнаты без каких-либо намеков на её смысл") || text.Contains("гонца"))
			{
				dunge.SecretRom.SecretKind = SecretKind.UnknownMark;
			}
			else if (text.Contains("изменит свой тип"))
			{
				dunge.SecretRom.SecretKind = SecretKind.ChangeType;
			}
			else if (text.Contains("спонсор и создатель этого подземелья") || text.Contains("это подземелье божественного происхождения"))
			{
				dunge.IsCustom = true;
			}
			else
			{
				dunge.UnknownImportant = Trim(text);
			}
		}
	}
}
