using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MapsExplorer
{
	public partial class Form1 : Form
	{
		private const string SaveFileName = "/FormDataSave.txt";

		private ListViewer _listViewer = new ListViewer();
		private DungeonExploreMode _dungeonExploreMode;
		private PolygonExploreMode _polygonExploreMode;
		private AvantureKind _avantureKind;

		public Form1()
		{
			InitializeComponent();
			logsBackgroundWorker.WorkerReportsProgress = true;
			logsBackgroundWorker.WorkerSupportsCancellation = true;
			_exploreBackgroundWorker.WorkerReportsProgress = true;
			_exploreBackgroundWorker.WorkerSupportsCancellation = true;
		}

		private void startButton_Click(object sender, EventArgs e)
		{
			if (!logsBackgroundWorker.IsBusy && !_exploreBackgroundWorker.IsBusy)
			{
				progressBar1.Value = 0;
				errorTextBox.Text = "";
				if (!string.IsNullOrEmpty(hashTextBox.Text))
					hashTextBox.Text = "";
				logsBackgroundWorker.RunWorkerAsync();
			}
			else
				errorTextBox.Text = "Фоновый поток уже занят вычислениями. Дождитесь окончания работы и попробуйте снова.";
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			var worker = sender as BackgroundWorker;
			var startDate = dateTimePicker1.Value;
			var endDate = dateTimePicker2.Value;
			string add = "";
			if (_avantureKind == AvantureKind.Dungeon)
			{
				if (successCheckBox.Checked)     //успех
					add += "&r=1";
				if (specialCheckBox.Checked)     //специальное
					add += "&d=spec";
				else if (customCheckBox.Checked) //кастомное
					add += "&d2=custom";
			}
			_listViewer.StartView(_avantureKind, startDate, endDate, add,  OnListViewReady, (percent) => worker.ReportProgress(percent));
		}

		private void OnListViewReady(string error, List<LogLine> resultLines)
		{
			_resultLines = resultLines;
			for (int i = _resultLines.Count - 1; i >= 0; i--)
				if (_resultLines[i].Hash.Length <= 5)
					_resultLines.RemoveAt(i);
			_error = error; 
		}

		private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			progressBar1.Value = e.ProgressPercentage;
		}

		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			progressBar1.Value = 100;

			countTextBox.Text = _resultLines.Count.ToString();
			errorTextBox.Text = _error ?? "";
			StringBuilder builder = new StringBuilder();
			foreach (LogLine line in _resultLines)
				builder.Append(line.ToString() + "\n");
			string all = builder.ToString();
			logsRichTextBox.Text = all;
			if (!Directory.Exists(Paths.ResultsDir))
				Directory.CreateDirectory(Paths.ResultsDir);
			File.WriteAllText(Paths.ResultsDir + "/list.txt", all);
		}

		private List<LogLine> _resultLines;
		private string _error;
		private DungeonLogHandler _dungeonLogHandler = new DungeonLogHandler();
		private PolygonLogHandler _polygonLogHandler = new PolygonLogHandler();

		private void exploreButton_Click(object sender, EventArgs e)
		{
			tableRichTextBox.Text = "";
			resultRichTextBox.Text = "";
			errorTextBox.Text = "";
			_completeCallback = null;
			ReadExploreMode();
			SaveFormData();
			if (!string.IsNullOrEmpty(hashTextBox.Text))
			{
				LogLine line = (hashTextBox.Text.Length > 6) ?
					new LogLine()
					{
						Hash = hashTextBox.Text,
						DateTime = Utils.ParseDateTime("11.11.2011 11:11"),
						AvantureKind = AvantureKind.Dungeon,
						Category = MapCategory.Рандом,
						DungeKind = DungeKind.Неизвестное,
						Success = true
					} :
					new LogLine()
					{
						Hash = hashTextBox.Text,
						DateTime = Utils.ParseDateTime("11.11.2011 11:11"),
						AvantureKind = AvantureKind.Plygon,
						PolygonKind = PolygonKind.Неизвестный,
						Success = true
					};
				_resultLines = new List<LogLine>();
				_resultLines.Add(line);
			}
			else if (_dungeonExploreMode == DungeonExploreMode.None)
			{
				errorTextBox.Text = "Режим не выбран (None)";
				return;
			}
			else if (_resultLines == null || _resultLines.Count == 0)
			{
				errorTextBox.Text = "Сначала нужно ввести хэш лога или получить список логов по датам [Start]";
				return;
			}

			if (!logsBackgroundWorker.IsBusy && !_exploreBackgroundWorker.IsBusy)
			{
				progressBar1.Value = 0;
				_exploreBackgroundWorker.RunWorkerAsync();
			}
			else
				errorTextBox.Text = "Фоновый поток уже занят вычислениями. Дождитесь окончания работы и попробуйте снова.";

		}

		private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
		{
			ExplorerBase explorer = null;
			if (_avantureKind == AvantureKind.Dungeon)
			{
				switch (_dungeonExploreMode)
				{
					case DungeonExploreMode.Rotations: //	Повороты без гласов
						explorer = new RotationsExplorer();
						break;
					case DungeonExploreMode.TribblesWithBosses:    //	Трибблы у боссов
						explorer = new TribblesWithBossesExplorer();
						break;
					case DungeonExploreMode.HalfFinBosses:  //	Полуфиналы
						explorer = new HalfFinBossesExplorer();
						break;
					case DungeonExploreMode.BossesByName:   //	Поиск конкретного босса
						explorer = new BossesByNameExplorer();
						break;
					case DungeonExploreMode.BossesLoot: //	Перечисление выпавшего лута
						explorer = new BossLootExplorer();
						break;
					case DungeonExploreMode.BossAbils:  //	Абилки боссов
						explorer = new BossAbilsExplorer();
						break;
					case DungeonExploreMode.TreasurePos:        //	Положения клада
						explorer = new TreasurePosExplorer();
						break;
					case DungeonExploreMode.StairsPos:      //	Положения лестниц
						explorer = new StairsPosExplorer();
						break;
					case DungeonExploreMode.Voices:         //	Поиск гласов
						explorer = new VoicesExplorer();
						break;
					case DungeonExploreMode.Hints:          //	Подсказки в кастомках
						explorer = new HintsExplorer();
						break;
					case DungeonExploreMode.Stables:            //	Конюшни
						explorer = new StablesExplorer();
						break;
					case DungeonExploreMode.CacheHints:     //	Прикладовые подсказки
						explorer = new CacheHintsExplorer();
						break;
					case DungeonExploreMode.RoutesAndBosses:    //	Непутевые боссы
						explorer = new RoutesAndBossesExplorer();
						break;
					case DungeonExploreMode.Teleports:          //	Телепорты и ловушки
						explorer = new TeleportsExplorer();
						break;
					case DungeonExploreMode.Aquas:              //	Исследование аквариумов
						explorer = new AquasExplorer();
						break;
					case DungeonExploreMode.Walls:              //	Капиталки, размеры данжей
						explorer = new WallsExplorer();
						break;
					case DungeonExploreMode.Jumps:              //	Влияние гласов в прыгучести
						explorer = new JumpsExplorer();
						break;
					case DungeonExploreMode.Coupons:            //	Считаем купоны
						explorer = new CouponsExplorer();
						break;
					case DungeonExploreMode.TimeStatistic:     //	Время в данже
						explorer = new TimeStatisticExplorer();
						break;
					case DungeonExploreMode.HalfTrueHints:     //	Указатели в полуправде
						explorer = new HalfTrueHintsExplorer();
						break;
					case DungeonExploreMode.HeroDamages:     //	Дамаг, наносимый героями
						explorer = new HeroDamageExplorer();
						break;
					case DungeonExploreMode.VoicesAndParts:     //	Гласы и запчасти с боссов
						explorer = new VoicesAndPartsExplorer();
						break;
					default:
						_dungeonLogHandler.LastError = $"Mode {_dungeonExploreMode} not realized";
						break;
				}
			}
			else if (_avantureKind == AvantureKind.Plygon)
			{
				switch (_polygonExploreMode)
				{
					case PolygonExploreMode.SuccesPercent: //	Успешность полигонов
						explorer = new PolySuccessExplorer();
						break;
					default:
						_polygonLogHandler.LastError = $"Mode {_polygonExploreMode} not realized";
						break;
				}
			}
			if (explorer != null)
			{
				LogHandler logHandler = _avantureKind == AvantureKind.Dungeon ? _dungeonLogHandler as LogHandler : _polygonLogHandler;
				explorer.Init(_avantureKind, _dungeonExploreMode, _polygonExploreMode, logHandler, _resultLines, _exploreBackgroundWorker, 
					customCheckBox.Checked,
					checkBoxMinRoute.Checked
					);
				explorer.Work();
				_completeCallback = () =>
				{
					tableRichTextBox.Text = explorer.TableText;
					resultRichTextBox.Text = explorer.ResultText;
				};
			}
		}


		private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{

			if (!string.IsNullOrEmpty(_dungeonLogHandler.LastError))
			{
				errorTextBox.Text = _dungeonLogHandler.LastError;
				_dungeonLogHandler.LastError = null;
			}
			else
			{
				progressBar1.Value = 100;
				_completeCallback?.Invoke();
			}

		}





		private Action _completeCallback;
		

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
				DungeonExploreMode = _dungeonExploreMode,
				PolygonExploreMode = _polygonExploreMode
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

			Form1_ResizeEnd(sender, e);

			_dungeonExploreMode = data.DungeonExploreMode;
			_polygonExploreMode = data.PolygonExploreMode;
			UpdateInterface();
		}

		private void UpdateModes()
		{
			ModeComboBox.Items.Clear();
			if (_avantureKind == AvantureKind.Dungeon)
			{
				foreach (DungeonExploreMode exploreMode in Enum.GetValues((typeof(DungeonExploreMode))))
				{
					ModeComboBox.Items.Add(exploreMode.ToString());
					if (exploreMode == _dungeonExploreMode)
						ModeComboBox.SelectedIndex = ModeComboBox.Items.Count - 1;
				}
			}
			else
			{
				foreach (PolygonExploreMode exploreMode in Enum.GetValues((typeof(PolygonExploreMode))))
				{
					ModeComboBox.Items.Add(exploreMode.ToString());
					if (exploreMode == _polygonExploreMode)
						ModeComboBox.SelectedIndex = ModeComboBox.Items.Count - 1;
				}
			}
		}

		private void UpdateInterface()
		{
			_avantureKind = radioButton1.Checked ? AvantureKind.Dungeon : AvantureKind.Plygon;
			bool isDunge = _avantureKind == AvantureKind.Dungeon;
			dungeBoolGroupBox.Visible = isDunge;

			UpdateModes();
			UpdateDungeElements();
		}

		private void ReadExploreMode()
		{
			if (_avantureKind == AvantureKind.Dungeon)
			{
				if (!Enum.TryParse(ModeComboBox.Text, out _dungeonExploreMode))
					_dungeonExploreMode = DungeonExploreMode.None;
			}
			else
			{
				if (!Enum.TryParse(ModeComboBox.Text, out _polygonExploreMode))
					_polygonExploreMode = PolygonExploreMode.None;
			}
		}

		private void ModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateDungeElements();
		}

		private void UpdateDungeElements()
		{
			ReadExploreMode();
			checkBoxMinRoute.Visible = _avantureKind == AvantureKind.Dungeon && _dungeonExploreMode == DungeonExploreMode.RoutesAndBosses;
		}

		private void Form1_ResizeEnd(object sender, EventArgs e)
		{
			logsRichTextBox.Width = Width - logsRichTextBox.Left - 30;
			tableRichTextBox.Width = Width - tableRichTextBox.Left - 30;
			resultRichTextBox.Width = Width - resultRichTextBox.Left - 30;
			errorTextBox.Width = Width - 40;
			progressBar1.Width = Width - 40;

			float h = (int)(Height - errorTextBox.Height - progressBar1.Height - 130);
			int h3 = (int)(h / 3);
			int space = 20;
			logsRichTextBox.Top = (int)space;
			logsRichTextBox.Height = h3;
			tableRichTextBox.Top = logsRichTextBox.Top + logsRichTextBox.Height + space;
			tableRichTextBox.Height = h3;
			resultRichTextBox.Top = tableRichTextBox.Top + tableRichTextBox.Height + space;
			resultRichTextBox.Height = h3;
			errorTextBox.Top = Height - space * 2 - errorTextBox.Height - progressBar1.Height - 20;
			progressBar1.Top = Height - space - progressBar1.Height - 30;
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			UpdateInterface();
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			UpdateInterface();
		}
	}
}
