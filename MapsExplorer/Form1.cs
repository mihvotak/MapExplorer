using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
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
			if (successCheckBox.Checked)     //успех
				add += "&r=1";
			if (specialCheckBox.Checked)     //специальное
				add += "&d=spec";
			else if (customCheckBox.Checked) //кастомное
				add += "&d2=custom";
			_listViewer.StartView(startDate, endDate, add,  OnListViewReady, (percent) => worker.ReportProgress(percent));
		}

		private void OnListViewReady(string error, List<DungeLine> resultLines)
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
			foreach (DungeLine line in _resultLines)
				builder.Append(line.ToString() + "\n");
			string all = builder.ToString();
			logsRichTextBox.Text = all;
			if (!Directory.Exists(Paths.ResultsDir))
				Directory.CreateDirectory(Paths.ResultsDir);
			File.WriteAllText(Paths.ResultsDir + "/list.txt", all);
		}

		private List<DungeLine> _resultLines;
		private string _error;
		LogHandler _logHandler = new LogHandler();

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
				DungeLine line = new DungeLine()
				{
					Hash = hashTextBox.Text,
					DateTime = Utils.ParseDateTime("11.11.2011 11:11"),
					Category = Category.Рандом,
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
			else if (_exploreMode == ExploreMode.None)
			{
				errorTextBox.Text = "Режим не выбран (None)";
				return;
			}
			else if (_resultLines == null || _resultLines.Count == 0)
			{
				errorTextBox.Text = "Сначала нужно ввести хэш данжа или получить список логов по датам [Start]";
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
			switch (_exploreMode)
			{ 
				case ExploreMode.Rotations: //	Повороты без гласов
					explorer = new RotationsExplorer();	
					break;
				case ExploreMode.TribblesWithBosses:    //	Трибблы у боссов
					explorer = new TribblesWithBossesExplorer();
					break;
				case ExploreMode.HalfFinBosses:	//	Полуфиналы
					explorer = new HalfFinBossesExplorer();             
					break;
				case ExploreMode.BossesByName:	//	Поиск конкретного босса
					explorer = new BossesByNameExplorer();           
					break;
				case ExploreMode.TreasurePos:		//	Положения клада
					explorer = new TreasurePosExplorer();            
					break;
				case ExploreMode.StairsPos:		//	Положения лестниц
					explorer = new StairsPosExplorer();				
					break;
				case ExploreMode.Voices:			//	Поиск гласов
					explorer = new VoicesExplorer();                 
					break;
				case ExploreMode.Hints:			//	Подсказки в кастомках
					explorer = new HintsExplorer();                  
					break;
				case ExploreMode.Stables:			//	Конюшни
					explorer = new StablesExplorer();               
					break;
				case ExploreMode.CacheHints:		//	Прикладовые подсказки
					explorer = new CacheHintsExplorer();			
					break;
				case ExploreMode.RoutesAndBosses:	//	Непутевые боссы
					explorer = new RoutesAndBossesExplorer();				
					break;
				case ExploreMode.Teleports:			//	Телепорты и ловушки
					explorer = new TeleportsExplorer();				
					break;
				case ExploreMode.Aquas:				//	Исследование аквариумов
					explorer = new AquasExplorer();                  
					break;
				case ExploreMode.Walls:				//	Капиталки, размеры данжей
					explorer = new WallsExplorer();                  
					break;
				case ExploreMode.Jumps:				//	Влияние гласов в прыгучести
					explorer = new JumpsExplorer();                  
					break;
				case ExploreMode.Coupons:			//	Считаем купоны
					explorer = new CouponsExplorer();
					break;
				case ExploreMode.TimeStatistic:     //	Время в данже
					explorer = new TimeStatisticExplorer();
					break;
				case ExploreMode.HalfTrueHints:     //	Указатели в полуправде
					explorer = new HalfTrueHintsExplorer();
					break;
				default:
					_logHandler.LastError = $"Mode {_exploreMode} not realized";
					break;
			}
			if (explorer != null)
			{
				explorer.Init(_exploreMode, _logHandler, _resultLines, _exploreBackgroundWorker, 
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

			if (!string.IsNullOrEmpty(_logHandler.LastError))
			{
				errorTextBox.Text = _logHandler.LastError;
				_logHandler.LastError = null;
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

			Form1_ResizeEnd(sender, e);

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
			checkBoxMinRoute.Visible = _exploreMode == ExploreMode.RoutesAndBosses;
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
	}
}
