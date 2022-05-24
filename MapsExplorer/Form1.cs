using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MapsExplorer
{
	public partial class Form1 : Form
	{
		private const string SaveFileName = "/FormDataSave.txt";

		private ListViewer _listViewer = new ListViewer();
		private ExploreMode _exploreMode;

		public Form1()
		{
			InitializeComponent();
		}

		private void startButton_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(hashTextBox.Text))
				hashTextBox.Text = "";
			var startDate = dateTimePicker1.Value;
			var endDate = dateTimePicker2.Value;
			string add = "";
			if (successCheckBox.Checked)     //успех
				add += "&r=1";
			if (specialCheckBox.Checked)     //специальное
				add += "&d=spec";
			else if (customCheckBox.Checked) //кастомное
				add += "&d2=custom";
			_listViewer.StartView(startDate, endDate, add,  OnListViewReady, OnProgress);
		}

		private void OnListViewReady(string error, List<DungeLine> resultLines)
		{
			_resultLines = resultLines;
			for (int i = _resultLines.Count - 1; i >= 0; i--)
				if (_resultLines[i].Hash.Length <= 5)
					_resultLines.RemoveAt(i);
			countTextBox.Text = resultLines.Count.ToString();
			errorTextBox.Text = error ?? "";
			StringBuilder builder = new StringBuilder();
			foreach (DungeLine line in resultLines)
				builder.Append(line.ToString() + "\n");
			string all = builder.ToString();
			logsRichTextBox.Text = all;
			File.WriteAllText(Paths.ResultsDir + "/list.txt", all);
		}

		private void OnProgress(int value)
		{
			progressBar1.Value = value;
		}

		private List<DungeLine> _resultLines;
		LogHandler _logHandler = new LogHandler();

		private void exploreButton_Click(object sender, EventArgs e)
		{
			tableRichTextBox.Text = "";
			resultRichTextBox.Text = "";
			errorTextBox.Text = "";
			ReadExploreMode();
			SaveFormData();
			if (!string.IsNullOrEmpty(hashTextBox.Text))
			{
				DungeLine line = new DungeLine()
				{
					Hash = hashTextBox.Text,
					DateTime = Utils.ParseDateTime("11.11.2011 11:11"),
					Category = Category.Usual,
					Kind = DungeKind.Неизвестное,
					Success = true
				};
				_resultLines = new List<DungeLine>();
				_resultLines.Add(line);
				/*Dunge dunge = _logHandler.GetDunge(line);
				string s = dunge.DungeLine.Link + "\t" + dunge.Steps + " steps\n";
				if (dunge.Bosses.Count > 0)
				{
					var boss = dunge.Bosses[0];
					s += boss.Name + " " + boss.AllAbilsStr;
				}
				richTextBox2.Text = s;
				return;*/
			}
			else if (_resultLines == null || _resultLines.Count == 0)
			{
				errorTextBox.Text = "Сначала нужно ввести хэш или получить список логов по датам (Start)";
				return;
			}
			switch (_exploreMode)
			{ 
				case ExploreMode.CountBossesAndTribbles:
					CalculateBossesAndTribbles();	//	Трибблы у боссов
					break;
				case ExploreMode.Count2AbilBosses:
					Count2AbilBosses();             //	Полуфиналы
					break;
				case ExploreMode.SearchBossesByName:
					SearchBossesByName();           //	Поиск конкретного босса
					break;
				case ExploreMode.SearchTreasurePos:
					SearchTreasurePos();            //	Положения клада
					break;
				case ExploreMode.SearchStairsPos:
					SearchStairsPos();				//	Положения лестниц
					break;
				case ExploreMode.SearchVoices:
					SearchVoices();                 //	Поиск гласов
					break;
				case ExploreMode.SearchHints:
					SearchHints();                  //	Подсказки в кастомках
					break;
				case ExploreMode.ExploreStables:
					ExploreStables();               //	Конюшни
					break;
				case ExploreMode.ExploreCacheHints:
					ExploreCacheHints();			//	Прикладовые подсказки
					break;
				case ExploreMode.CalculateRoutes:
					CalculateRoutes();				//	Непутевые боссы
					break;
				case ExploreMode.ExploreTeleports:
					ExploreTeleports();				//	Телепорты и ловушки
					break;
				case ExploreMode.SearchAquas:
					SearchAquas();                  //	Исследование аквариумов
					break;
				case ExploreMode.SearchWalls:
					SearchWalls();                  //	Капиталки, размеры данжей
					break;
				case ExploreMode.SearchJumps:
					SearchJumps();                  //	Влияние гласов в прыгучести
					break;
				default:
					errorTextBox.Text = $"Mode {_exploreMode} not realized";
					break;
			}
			progressBar1.Value = 100;
		}

		private void SearchAquas()
		{
			bool showOne = _resultLines.Count == 1;
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				if (line.Category != Category.Aqua)
					continue;
				if (!line.Success)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				Map map = dunge.Maps[0];
				if (map.BadRouteWalls)
					continue;
				Int2 treasureDelta = dunge.TreasurePos.Pos - map.EnterPos;
				bool circle = true;
				int walls = 0;
				if (map.IsLeftWall)
					walls++;
				if (map.IsRightWall)
					walls++;
				if (map.IsTopWall)
					walls++;
				if (map.IsBottomWall)
					walls++;
				if (walls < 3)
					continue;
				foreach (Step move in dunge.Moves)
				{
					Int2 movePos = move.Pos;
					if (!(movePos.x == 1 || movePos.x == map.Width - 2 || movePos.y == 1 || movePos.y == map.Height - 2))
						circle = false;
				}
				if (!circle)
					continue;
				bool positiveX = treasureDelta.x > 0;
				bool positiveY = treasureDelta.y > 0;
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(dunge.WrongDetectedAqua ? "Aqua!" : dunge.DungeLine.Category.ToString());
				tds.Add(dunge.TreasurePos.Pos.x + "");
				tds.Add(dunge.TreasurePos.Pos.y + "");
				tds.Add(line.Kind.ToString());
				tds.Add(treasureDelta.x + "");
				tds.Add(treasureDelta.y + "");
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");

				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			string exploreTab = builder.ToString();
			tableRichTextBox.Text = exploreTab;
			File.WriteAllText(Paths.ResultsDir + "/SearchAquaTab.txt", exploreTab);
		}

		private void SearchWalls()
		{
			bool showOne = _resultLines.Count == 1;
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				//if (line.Category != Category.Aqua)
				//	continue;
				//if (!line.Success)
				//	continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				Map map = dunge.Maps[dunge.LastFloor - 1];
				if (map.BadRouteWalls)
					continue;
				bool walls = map.IsLeftWall && map.IsRightWall && map.IsTopWall && map.IsBottomWall;
				if (!walls)
					continue;
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(dunge.WrongDetectedAqua ? "Aqua!" : dunge.DungeLine.Category.ToString());
				tds.Add(line.Kind.ToString());
				tds.Add(map.Width + "");
				tds.Add(map.Height + "");
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");

				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			string exploreTab = builder.ToString();
			tableRichTextBox.Text = exploreTab;
			File.WriteAllText(Paths.ResultsDir + "/SearchWallsTab.txt", exploreTab);
		}

		private void ExploreTeleports()
		{
			StringBuilder builder = new StringBuilder();
			bool showFull = false;
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
				if (line.Category != Category.Stable)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				bool enough = dunge.Stable != null && dunge.Stable.EnoughInfo;
				builder.Append(line.Link + "\t" + Utils.GetDateAndTimeString(line.DateTime) + "\t");
				Map map = dunge.Maps[0];
				builder.Append(map.Width + "\t" + map.Height + "\t");
				if (enough)  
				{
					builder.Append($"OK\t");
					if (dunge.HintMoves.Count > 0)
					{
						Int2 hintPos = dunge.Moves[dunge.HintMoves[0] - 1].Pos - map.EnterPos;
						builder.Append($"{hintPos.x}\t{hintPos.y}\t");
					}
					else
						builder.Append("\t\t");
				}
				else
					builder.Append("not enough\t\t\t");
				var teleports = map.Cells.FindAll(c => c.CellKind == CellKind.Teleport).Count;
				builder.Append(teleports + "\t");
				builder.Append("\n");
				if (enough && showFull)
				{
					for (int y = 0; y < map.Height; y++)
					{
						for (int x = 0; x < map.Width; x++)
						{
							builder.Append(map.GetCell(x, y).GetSymbol(dunge.Rotation) + " ");
						}
						builder.Append("\n");
					}
					builder.Append("--------------------------\n");
				}
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
		}

		private void CalculateRoutes()
		{
			bool showOne = _resultLines.Count == 1;
			int half = 20;
			int halfHalf = 10;
			int[,] noRoutes = new int[half * 2, half * 2];
			int[,] routes = new int[half * 2, half * 2];
			int[,] canRoutes = new int[half * 2, half * 2];
			int[,] all1 = new int[half * 2, half * 2];
			StringBuilder builder = new StringBuilder();
			StringBuilder builder2 = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				if (line.Category != Category.Usual)
					continue;
				if (!line.Success || line.Vault)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				Map map = dunge.Maps[dunge.LastFloor - 1];
				if (map.BadRouteWalls)
					continue;
				if (dunge.WrongDetectedAqua || !dunge.IsNormalBosses())
					continue;
				//if (!dunge.AllBossesFound)
				//	continue;
				Int2 treasureDelta = dunge.TreasurePos.Pos - map.EnterPos;
				bool positiveX = treasureDelta.x > 0;
				bool positiveY = treasureDelta.y > 0;
				foreach (Boss boss in dunge.Bosses)
				{
					List<string> tds = new List<string>();
					if (boss.Pos.Floor != dunge.LastFloor)
						continue;
					tds.Add(line.Link);
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(dunge.WrongDetectedAqua ? "Aqua!" : dunge.DungeLine.Category.ToString());
					tds.Add(line.Kind.ToString());
					Int2 delta = boss.Pos.Pos - map.EnterPos;
					tds.Add(delta.x + "");
					tds.Add(delta.y + "");
					tds.Add(boss.Name);
					Int2 delta2 = delta;
					if (!positiveX)
						delta2.x = -delta2.x;
					if (!positiveY)
						delta2.y = -delta2.y;
					tds.Add(delta2.x + "");
					tds.Add(delta2.y + "");

					bool hasWalls = (positiveY ? map.IsTopWall : map.IsBottomWall) && (positiveX ? map.IsLeftWall : map.IsRightWall);
					if (!hasWalls) 	continue;
					int bossFromWallY = positiveY ? boss.Pos.Pos.y : map.Height - boss.Pos.Pos.y - 1;
					int bossFromWallX = positiveX ? boss.Pos.Pos.x : map.Width - boss.Pos.Pos.x - 1;
					int enterFromWallY = positiveY ? map.EnterPos.y : map.Height - map.EnterPos.y - 1;
					int enterFromWallX = positiveX ? map.EnterPos.x : map.Width - map.EnterPos.x - 1;
					bool closestX = bossFromWallX < bossFromWallY;
					int bossFromWall = closestX ? bossFromWallX : bossFromWallY;
					int enterFromWall = closestX ? enterFromWallX : enterFromWallY;
					tds.Add(hasWalls ? "wall" : "no");
					tds.Add(bossFromWall + "");
					tds.Add(enterFromWall + "");
					tds.Add("|");
					tds.Add(boss.Abils.Count + "");
					tds.Add(boss.IsFinal ? "Финальный" : (boss.IsRouting ? "Путевой" : (boss.CanBeRouting ? "Пут?" : (delta2.x > 0 && delta2.y > 0 ? "Блуждун" : (delta2.x < 0 && delta2.y < 0 ? "Угловой" : "Пристен")))));
					tds.Add(dunge.Bosses.Count + "");
					if ((boss.Abils.Count == 1 || boss.Abils.Count == 0) && !boss.IsFinal)
					//if (boss.Abils.Count == 1)
					{
						if (!boss.IsRouting && !boss.CanBeRouting)
							noRoutes[delta2.x + half, delta2.y + half]++;
						else if (boss.IsRouting)
							routes[delta2.x + half, delta2.y + half]++;
						else if (boss.CanBeRouting)
							canRoutes[delta2.x + half, delta2.y + half]++;
						all1[delta2.x + half, delta2.y + half]++;
					}

					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");

				}
				if (showOne)
				{
					for (int y = 0; y < map.Height; y++)
					{
						for (int x = 0; x < map.Width; x++)
						{
							Cell cell = map.GetCell(x, y);
							string sym = cell.GetSymbol(dunge.Rotation);
							RoutesExplorer.RouteCell rc = map.Grid.GetCell(x, y);
							bool calculatedBoss = cell.CellKind == CellKind.Boss && cell.Boss.BossPower == 0;
							if (calculatedBoss)
								sym = "Б!";
							int waiPointIndex = 0;
							if (rc != null)
							{
								if (checkBoxMinRoute.Checked)
								{
									if (rc.MinPathLens[waiPointIndex] > -1 && cell.CellKind != CellKind.Boss && cell.CellKind != CellKind.Wall)
										sym = rc.MinPathLens[waiPointIndex] + "";
									if (rc.IsMinRoute[waiPointIndex])
										sym = "[" + sym + "]";
								}
								else
								{
									if (rc.RealPathLens[waiPointIndex] > -1 && cell.CellKind != CellKind.Boss && cell.CellKind != CellKind.Wall)
										sym = rc.RealPathLens[waiPointIndex] + "";
									if (rc.IsRealRoute[waiPointIndex])
										sym = "[" + sym + "]";
								}
							}
							builder.Append(sym + "\t");
						}
						builder.Append("\n");
					}
				}
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			void WriteResultGraph(string name, int[,] data)
			{
				builder2.Append(name + "\n");
				data[half, half] = -1;
				builder2.Append("\t"+ "\t");
				for (int x = -halfHalf; x <= halfHalf; x++)
					builder2.Append(x + "\t");
				builder2.Append("\n");
				for (int y = -halfHalf; y <= halfHalf; y++)
				{
					builder2.Append("\t" + y + "\t");
					for (int x = -halfHalf; x <= halfHalf; x++)
						builder2.Append(data[x + half, y + half] + "\t");
					builder2.Append("\n");
				}
				builder2.Append("\n");
			}
			WriteResultGraph("Не путевые", noRoutes);
			WriteResultGraph("Путевые", routes);
			WriteResultGraph("Возможно путевые", canRoutes);
			WriteResultGraph("Все однушки", all1);

			string exploreTab = builder.ToString();
			tableRichTextBox.Text = exploreTab;
			File.WriteAllText(Paths.ResultsDir + "/RoutesTab.txt", exploreTab);

			string exploreRes = builder2.ToString();
			resultRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/RoutesResult.txt", exploreRes);
		}

		private void ExploreStables()
		{
			StringBuilder builder = new StringBuilder();
			bool showFull = false;
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
				if (line.Category != Category.Stable)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				bool enough = dunge.Stable != null && dunge.Stable.EnoughInfo;
				builder.Append(line.Hash + "\t" + Utils.GetDateAndTimeString(line.DateTime) + "\t");
				Map map = dunge.Maps[0];
				builder.Append(map.Width + "\t" + map.Height + "\t");
				if (enough)
				{
					builder.Append($"OK\t");
					if (dunge.HintMoves.Count > 0)
					{
						Int2 hintPos = dunge.Moves[dunge.HintMoves[0] - 1].Pos - map.EnterPos;
						builder.Append($"{hintPos.x}\t{hintPos.y}\t");
					}
				}
				else
					builder.Append("not enough");
				builder.Append("\n");
				if (enough && showFull)
				{
					for (int y = 0; y < map.Height; y++)
					{
						for (int x = 0; x < map.Width; x++)
						{
							builder.Append(map.GetCell(x, y).GetSymbol(dunge.Rotation) + " ");
						}
						builder.Append("\n");
					}
					builder.Append("--------------------------\n");
				}
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
		}

		private void ExploreCacheHints()
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				Map map = dunge.Maps[dunge.LastFloor - 1];
				bool enough = dunge.Stable != null && dunge.Stable.EnoughInfo;
				builder.Append(line.Hash + "\t" + Utils.GetDateAndTimeString(line.DateTime) + "\t");
				builder.Append(line.Category.ToString() + "\t");
				builder.Append(line.Kind.ToString() + "\t");
				builder.Append(map.Width + "\t" + map.Height + "\t");
				builder.Append((dunge.HintOnCache ? 1 : 0) + "\t");
				builder.Append("\n");
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
		}

		private void Count2AbilBosses() // Таблица всех полуфиналов
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				int count = 0;
				string s = "";
				foreach (Boss boss in dunge.Bosses)
				{
					if (boss.Abils.Count == 2)
					{
						count++;
						if (boss.Pos.Floor == dunge.TreasurePos.Floor && Math.Abs(boss.Pos.Pos.x - dunge.TreasurePos.Pos.x) == 1 && Math.Abs(boss.Pos.Pos.y - dunge.TreasurePos.Pos.y) == 1)
							s = dunge.DungeLine.GetBossLink(boss.Num);
					}
				}
				{
					List<string> tds = new List<string>();
					tds.Add(line.Hash);
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(line.Category.ToString());
					tds.Add(line.Kind.ToString());
					tds.Add(dunge.Steps.ToString());
					tds.Add(dunge.Members.Count.ToString());
					tds.Add(count.ToString());
					tds.Add(s);
					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
				}
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/BossesResult22.txt", exploreRes);
		}

		private void SearchBossesByName() // Поиск боссов по имени
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				foreach (var boss in dunge.Bosses)
				{
					if (boss.Name.Contains("Микро-"))
					{
						List<string> tds = new List<string>();
						tds.Add(dunge.DungeLine.GetBossLink(boss.Num));
						tds.Add(Utils.GetDateAndTimeString(line.DateTime));
						tds.Add(line.Category.ToString());
						tds.Add(line.Kind.ToString());
						tds.Add(boss.Name.ToString());
						tds.Add(boss.Abils.Count.ToString());
						tds.Add(boss.AllAbilsStr.ToString());
						string tr = string.Join("\t", tds);
						builder.Append(tr + "\n");
					}
				}
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/BossesResult22.txt", exploreRes);
		}

		private const int MaxStep = 100;
		class HintCounter {
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

		private void SearchVoices() // Гласы
		{
			StringBuilder rawBuilder = new StringBuilder();
			StringBuilder resultBuilder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				foreach (List<VoiceKind> voices in dunge.Voices)
				{
					List<string> tds = new List<string>();
					tds.Add(line.Link);
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(line.Category.ToString());
					tds.Add(line.Kind.ToString());
					StringBuilder ss = new StringBuilder();
					if (voices != null)
						foreach (var v in voices)
							ss.Append(v.ToString() + ".");
					tds.Add(ss.ToString());
					string tr = string.Join("\t", tds);
					rawBuilder.Append(tr + "\n");
				}
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			string rawDataTable = rawBuilder.ToString();
			tableRichTextBox.Text = rawDataTable;
			File.WriteAllText(Paths.ResultsDir + "/SearchVoicesRaw.txt", rawDataTable);

			string resData = resultBuilder.ToString();
			resultRichTextBox.Text = resData;
			File.WriteAllText(Paths.ResultsDir + "/SearchVoicesResult.txt", resData);
		}

		private void SearchHints() // Таблица подсказок
		{
			HintCounter normalCounter = new HintCounter() { name = "обычный"};
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
				if (dunge.IsCustom && !customCheckBox.Checked)
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
							for (int j = dunge.HintMoves[0]+1; j < dunge.HintMoves[1]; j++)
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
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			AddHintResultsToBuilder(resultBuilder, normalCounter);
			AddHintResultsToBuilder(resultBuilder, thermoCounter);
			AddHintResultsToBuilder(resultBuilder, quickCounter);
			AddHintResultsToBuilder(resultBuilder, moreCounter);

			string rawDataTable = rawBuilder.ToString();
			tableRichTextBox.Text = rawDataTable;
			File.WriteAllText(Paths.ResultsDir + "/SearchHintsRaw.txt", rawDataTable);

			string resData = resultBuilder.ToString();
			resultRichTextBox.Text = resData;
			File.WriteAllText(Paths.ResultsDir + "/SearchHintsResult.txt", resData);
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

		private void SearchTreasurePos() // Таблица смещений сокры от входа
		{
			int needFloor = 2; bool from1FloorEntrance = false;
			StringBuilder builder = new StringBuilder();
			Plot2d plot = new Plot2d();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				if (!line.Success || line.Vault)
					continue;
				if (needFloor == 1 && line.Category != Category.Usual)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				if (dunge.LastFloor != needFloor)
					continue;
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(line.Kind.ToString());
				tds.Add(dunge.Steps.ToString());
				Map map0 = dunge.Maps[0];
				Map map = dunge.Maps[dunge.LastFloor - 1];
				tds.Add(map.Width.ToString());
				tds.Add(map.Height.ToString());
				tds.Add(dunge.Bosses.Count.ToString());
				if (line.Success && !line.Vault)
				{
					Int2 delta = dunge.TreasurePos.Pos - map.EnterPos;
					if (from1FloorEntrance)
						delta += (map0.StairsPos - map0.EnterPos);
					int x = delta.x;
					int y = -delta.y;
					tds.Add(x.ToString());
					tds.Add(y.ToString());
					plot.Inc(x, y);
				}
				tds.Add(dunge.TreasureBetweenWalls ? "1" : "0");
				tds.Add(dunge.TreasureSchemeKind.ToString());
				tds.Add(dunge.TreasureScheme.ToString());
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/BossesResult.txt", exploreRes);
			string s = plot.GetRes(10);
			s += plot.GetRes4(10);
			s += plot.GetRes8(10);
			resultRichTextBox.Text = s;
		}

		private void SearchStairsPos() // Таблица смещений ЛЕСТНИЦЫ от входа
		{
			StringBuilder builder = new StringBuilder();
			Plot2d plot = new Plot2d();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				if (line.Category != Category.Usual)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				Map map = dunge.Maps[0];
				if (map.StairsPos == Int2.Zero)
					continue;
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(line.Kind.ToString());
				tds.Add(map.Width.ToString());
				tds.Add(map.Height.ToString());
				Int2 delta = map.StairsPos - map.EnterPos;
				int x = delta.x;
				int y = -delta.y;
				tds.Add(x.ToString());
				tds.Add(y.ToString());
				tds.Add(dunge.FirstStairMove.ToString());
				tds.Add(dunge.LastFloor.ToString());
				plot.Inc(x, y);
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}
			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/BossesResult.txt", exploreRes);
			string s = plot.GetRes(5);
			s += plot.GetRes4(5);
			resultRichTextBox.Text = s;
		}

		private void CalculateBossesAndTribbles() // Таблица всех встреченных боссов, их абилок и хп
		{
			List<DateTime> tribbleDays = new List<DateTime>();
			tribbleDays.Add(new DateTime(2019, 12, 30));
			tribbleDays.Add(new DateTime(2020, 01, 02));
			tribbleDays.Add(new DateTime(2020, 01, 25));
			tribbleDays.Add(new DateTime(2020, 03, 11));
			tribbleDays.Add(new DateTime(2020, 03, 18));
			tribbleDays.Add(new DateTime(2020, 04, 04));
			tribbleDays.Add(new DateTime(2020, 04, 24));
			tribbleDays.Add(new DateTime(2020, 05, 21));
			tribbleDays.Add(new DateTime(2020, 06, 01));
			tribbleDays.Add(new DateTime(2020, 06, 06));

			StringBuilder builder = new StringBuilder();

			int tMax = 4;
			int[] tAll = new int[tMax];
			int[] tTribbles = new int[tMax];

			int abMax = 4;
			int[] abAll = new int[abMax];
			int[] abTribbles = new int[abMax];

			int total = 0;
			int loses = 0;
			int escapes = 0;
			int winNoEsc = 0;
			int tribbles = 0;
			int mids = 0;
			int firsts = 0;

			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				//if (line.Category != Category.Usual)
				//	continue;
				//if (line.Hash != "apn8pky5p")
				//	continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				int killedBosses = dunge.Bosses.Count(b => b.HeroesWin);
				bool time1 = line.DateTime < new DateTime(2019, 10, 29);
				bool time2 = !time1 && line.DateTime < new DateTime(2019, 12, 27);

				for (int ii = 0; ii < dunge.Bosses.Count; ii++)
				{
					var boss = dunge.Bosses[ii];
					if (boss.Num == 0)
						continue;
					List<string> tds = new List<string>();
					tds.Add(line.Link);
					tds.Add(Utils.GetDateAndTimeString(line.DateTime));
					tds.Add(Utils.GetDateAndTimeString(boss.DateTime));
					int period = time1 ? 1 : (time2 ? 2 : 3);
					if (period == 3)
					{
						foreach (DateTime dateTime in tribbleDays)
							if (dateTime.Date == boss.DateTime.Date)
								period = 4;
					}
					total++;
					if (!boss.HeroesWin)
						loses++;
					else if (boss.Escape1)
						escapes++;
					else
					{
						winNoEsc++;
						tAll[period - 1]++;
						abAll[boss.Abils.Count - 1]++;
						if (boss.TribbleInFinal)
						{
							tribbles++;
							tTribbles[period - 1]++;
							abTribbles[boss.Abils.Count - 1]++;
						}
					}
					
					if (boss.TribbleInMiddle)
						mids++;
					if (boss.TribbleInFirst)
						firsts++;
					tds.Add(period.ToString());
					tds.Add(line.Success ? (line.Vault ? "v" : "s") : "f");
					tds.Add(line.Category.ToString());
					//tds.Add(line.Kind.ToString());
					//tds.Add(line.Kind == DungeKind.Бесшумности ? "б" : "");
					//tds.Add(dunge.Steps.ToString());
					tds.Add(line.GetBossLink(boss.Num));
					tds.Add(boss.Num.ToString());
					tds.Add(dunge.Bosses.Count.ToString());
					tds.Add(killedBosses.ToString());
					//tds.Add(dunge.Members.Count.ToString());
					//tds.Add(dunge.MembersSumHp.ToString());
					tds.Add(boss.Abils.Count.ToString());
					tds.Add(boss.AllAbilsStr);
					//tds.Add(boss.Abils.Contains(Ability.мощный) ? "м" : "");
					//tds.Add(boss.Abils.Contains(Ability.мутирующий) ? "му" : "");
					tds.Add(boss.Name);
					//tds.Add(boss.Hp.ToString());
					//tds.Add("");
					//tds.Add((Math.Abs(posX)).ToString());
					//tds.Add((Math.Abs(posY)).ToString());
					tds.Add(boss.HeroesWin ? "win" : "lose");
					tds.Add(boss.TribbleInFinal ? "1" : "");
					tds.Add(boss.Escape1 ? "escape" : "");
					Step step = boss.Pos;
					tds.Add(step.Floor.ToString());
					Map map = dunge.Maps[step.Floor - 1];
					int posX = step.Pos.x - map.EnterPos.x;
					int posY = step.Pos.y - map.EnterPos.y;
					tds.Add(posX.ToString());
					tds.Add((-posY).ToString());
					/*string posS = "";
					if (boss.Abils.Count == 1)
					{
						if ((Math.Abs(posX) > 5 || Math.Abs(posY) > 5) &&
							(!line.Success || line.Vault || (boss.Pos - dunge.TreasurePos).DiagonalMagnitude > 1))
							posS = "out";
						if (posX == 0 || posY == 0)
							posS = "in";
					}
					tds.Add(posS);
					*/
					string tr = string.Join("\t", tds);
					builder.Append(tr + "\n");
				}
				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			builder.Append("\n");
			builder.Append($"Dunges\t{_resultLines.Count}\n");
			builder.Append($"Bosses\t{total}\n");
			builder.Append($"Lose\t{loses}\n");
			builder.Append($"Escape\t{escapes}\n");
			builder.Append($"Win\t{winNoEsc}\n");
			builder.Append($"Tribbles\t{tribbles}\t{((double)tribbles / winNoEsc * 100).ToString("f2")}%\n");
			builder.Append("\n");
			builder.Append($"Middle\t{mids}\n");
			builder.Append($"First\t{firsts}\n");
			builder.Append("\n");

			builder.Append("\n");
			builder.Append("Period\t");
			for (int i = 1; i <= tMax; i++)
				builder.Append(i + "\t");
			builder.Append("\n");
			builder.Append("Wins\t");
			for (int i = 1; i <= tMax; i++)
				builder.Append(tAll[i-1] + "\t");
			builder.Append("\n");
			builder.Append("Tribbles\t");
			for (int i = 1; i <= tMax; i++)
				builder.Append(tTribbles[i - 1] + "\t");
			builder.Append("\n");
			builder.Append("Percent\t");
			for (int i = 1; i <= tMax; i++)
				builder.Append(((double)tTribbles[i - 1] / tAll[i - 1] * 100).ToString("f2") + "%\t");
			builder.Append("\n");

			builder.Append("\n");
			builder.Append("Abils:\t");
			for (int i = 1; i <= abMax; i++)
				builder.Append(i + "\t");
			builder.Append("\n");
			builder.Append("Wins\t");
			for (int i = 1; i <= abMax; i++)
				builder.Append(abAll[i - 1] + "\t");
			builder.Append("\n");
			builder.Append("Tribbles\t");
			for (int i = 1; i <= abMax; i++)
				builder.Append(abTribbles[i - 1] + "\t");
			builder.Append("\n");
			builder.Append("Percent\t");
			for (int i = 1; i <= abMax; i++)
				builder.Append(((double)abTribbles[i - 1] / abAll[i - 1] * 100).ToString("f2") + "%\t");
			builder.Append("\n");

			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/BossesResult.txt", exploreRes);
		}

		private void SearchJumps() // Влияние гласов в прыгучести
		{
			var resArray = new int[100];
			var builder = new StringBuilder();
			for (int i = 0; i < _resultLines.Count; i++)
			{
				DungeLine line = _resultLines[i];
				if (line.Kind != DungeKind.Прыгучести)
					continue;
				Dunge dunge = _logHandler.GetDunge(line, _exploreMode);
				if (dunge.SecretRom.Exists && (dunge.SecretRom.SecretKind == SecretKind.ChangeType || dunge.SecretRom.SecretKind == SecretKind.UnknownMark) && dunge.SecretRom.Visited)
					continue;

				int firstJump = 0;
				int[] possible = new int[6];
				int[] jumps = new int[6];
				int jMany = 0;
				int pMany = 0;
				int impossible = 0;
				for (int m = 2; m <= dunge.Moves.Count; m++)
				{
					var curr = dunge.Moves[m - 1];
					var prev = dunge.Moves[m - 2];
					bool isJump = curr.Floor == prev.Floor &&
						((curr.Pos.x == prev.Pos.x && Math.Abs(curr.Pos.y - prev.Pos.y) == 2)
						|| (Math.Abs(curr.Pos.x - prev.Pos.x) == 2 && curr.Pos.y == prev.Pos.y));
					if (firstJump == 0 && isJump)
					{
						firstJump = m;
					}
					if (m >= 6)
					{
						Map map = dunge.Maps[curr.Floor - 1];
						int[] kinds = new int[4];
						int maxVoice = 0;
						int maxCount = 0;
						int maxDir = -1;
						int dirVoices = 0;
						if (dunge.Voices[m] != null)
						{
							foreach (VoiceKind kind in dunge.Voices[m])
							{
								if (kind <= VoiceKind.запад)
								{
									kinds[(int)kind]++;
									dirVoices++;
								}
							}
							for (int ki = 0; ki < 4; ki++)
							{
								if (kinds[ki] > maxVoice)
								{
									maxVoice = kinds[ki];
									maxCount = 1;
									maxDir = ki;
								}
								else if (kinds[ki] == maxVoice)
								{
									maxCount++;
								}
							}
						}
						bool manyVoices = maxVoice > 0 && maxCount > 1;
						bool badWallVoice = false;

						if (isJump)
						{
							jumps[maxVoice]++;
							possible[maxVoice]++;
							if (dirVoices > maxVoice)
								jMany++;
						}
						else if (curr.Floor == prev.Floor &&
							((curr.Pos.x == prev.Pos.x && Math.Abs(curr.Pos.y - prev.Pos.y) == 1)
						|| (Math.Abs(curr.Pos.x - prev.Pos.x) == 1 && curr.Pos.y == prev.Pos.y)))
						{
							int dirs = 0;
							for (int di = 0; di < 4; di++)
							{
								var dir = Int2.FourDirections[di];
								Int2 pos = prev.Pos + dir * 2;
								Cell cell = pos.x < 0 || pos.x >= map.Width || pos.y < 0 || pos.y >= map.Height ? null : map.GetCell(pos);
								bool wall = cell == null || cell.CellKind == CellKind.Unknown || cell.CellKind == CellKind.Wall;
								if (maxVoice > 0 && maxCount == 1 && di == maxDir && wall)
									badWallVoice = true;
								if (!wall)
									dirs++;
							}
							if (dirs > 0 && !badWallVoice && !manyVoices)
							{
								possible[maxVoice]++;
								if (dirVoices > maxVoice)
									pMany++;
							}
							else
								impossible++;
						}
					}
				}
				resArray[firstJump]++;
				List<string> tds = new List<string>();
				tds.Add(line.Link);
				tds.Add(Utils.GetDateAndTimeString(line.DateTime));
				tds.Add(firstJump.ToString());
				tds.Add(jumps[0].ToString());
				tds.Add(possible[0].ToString());
				tds.Add(jumps[1].ToString());
				tds.Add(possible[1].ToString());
				tds.Add(jumps[2].ToString());
				tds.Add(possible[2].ToString());
				tds.Add(jumps[3].ToString());
				tds.Add(possible[3].ToString());
				tds.Add(jMany.ToString());
				tds.Add(pMany.ToString());
				string tr = string.Join("\t", tds);
				builder.Append(tr + "\n");

				progressBar1.Value = (int)((double)(i + 1) / _resultLines.Count * 100);
			}

			string exploreRes = builder.ToString();
			tableRichTextBox.Text = exploreRes;
			File.WriteAllText(Paths.ResultsDir + "/SearchJumps.txt", exploreRes);
		}

		private void SaveFormData()
		{
			if (!Directory.Exists(Paths.SaveDir))
				Directory.CreateDirectory(Paths.SaveDir);
			var filePath = Paths.SaveDir + SaveFileName;
			FormData data = new FormData()
			{
				StartDate = dateTimePicker1.Value,
				EndDate = dateTimePicker2.Value,
				Success = successCheckBox.Checked,
				Special = specialCheckBox.Checked,
				Custom = customCheckBox.Checked,
				Hash = hashTextBox.Text,
				ExploreMode = _exploreMode
			};
			string json = data.ToString();
			File.WriteAllText(filePath, json);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			FormData data = FormData.ReadFromFile(Paths.SaveDir + SaveFileName);
			if (data != null)
			{
				dateTimePicker1.Value = data.StartDate;
				dateTimePicker2.Value = data.EndDate;
				successCheckBox.Checked = data.Success;
				specialCheckBox.Checked = data.Special;
				customCheckBox.Checked = data.Custom;
				hashTextBox.Text = data.Hash;
			}

			ModeComboBox.Items.Clear();
			foreach (ExploreMode exploreMode in Enum.GetValues((typeof(ExploreMode))))
			{
				ModeComboBox.Items.Add(exploreMode.ToString());
				if (exploreMode == data.ExploreMode)
					ModeComboBox.SelectedIndex = ModeComboBox.Items.Count - 1;
			}

			ReadExploreMode();
		}

		private void ReadExploreMode()
		{
			if (!Enum.TryParse(ModeComboBox.Text, out _exploreMode))
				_exploreMode = ExploreMode.None;
		}

		private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ReadExploreMode();
			checkBoxMinRoute.Visible = _exploreMode == ExploreMode.CalculateRoutes;
		}
	}
}
