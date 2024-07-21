namespace MapsExplorer
{
	partial class Form1
	{
		/// <summary>
		/// Обязательная переменная конструктора.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Освободить все используемые ресурсы.
		/// </summary>
		/// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Код, автоматически созданный конструктором форм Windows

		/// <summary>
		/// Требуемый метод для поддержки конструктора — не изменяйте 
		/// содержимое этого метода с помощью редактора кода.
		/// </summary>
		private void InitializeComponent()
		{
			this.startButton = new System.Windows.Forms.Button();
			this.logsRichTextBox = new System.Windows.Forms.RichTextBox();
			this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
			this.dateTimePicker2 = new System.Windows.Forms.DateTimePicker();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.exploreButton = new System.Windows.Forms.Button();
			this.countTextBox = new System.Windows.Forms.TextBox();
			this.errorTextBox = new System.Windows.Forms.TextBox();
			this.tableRichTextBox = new System.Windows.Forms.RichTextBox();
			this.specialCheckBox = new System.Windows.Forms.CheckBox();
			this.successCheckBox = new System.Windows.Forms.CheckBox();
			this.resultRichTextBox = new System.Windows.Forms.RichTextBox();
			this.hashTextBox = new System.Windows.Forms.TextBox();
			this.checkBoxMinRoute = new System.Windows.Forms.CheckBox();
			this.customCheckBox = new System.Windows.Forms.CheckBox();
			this.ModeComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.logsBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			this._exploreBackgroundWorker = new System.ComponentModel.BackgroundWorker();
			this.SuspendLayout();
			// 
			// startButton
			// 
			this.startButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.startButton.Location = new System.Drawing.Point(39, 256);
			this.startButton.Margin = new System.Windows.Forms.Padding(4);
			this.startButton.Name = "startButton";
			this.startButton.Size = new System.Drawing.Size(156, 41);
			this.startButton.TabIndex = 2;
			this.startButton.Text = "Start";
			this.startButton.UseVisualStyleBackColor = true;
			this.startButton.Click += new System.EventHandler(this.startButton_Click);
			// 
			// logsRichTextBox
			// 
			this.logsRichTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.logsRichTextBox.Location = new System.Drawing.Point(255, 38);
			this.logsRichTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.logsRichTextBox.Name = "logsRichTextBox";
			this.logsRichTextBox.Size = new System.Drawing.Size(1908, 187);
			this.logsRichTextBox.TabIndex = 3;
			this.logsRichTextBox.Text = "";
			// 
			// dateTimePicker1
			// 
			this.dateTimePicker1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.dateTimePicker1.Location = new System.Drawing.Point(36, 38);
			this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(4);
			this.dateTimePicker1.Name = "dateTimePicker1";
			this.dateTimePicker1.Size = new System.Drawing.Size(203, 30);
			this.dateTimePicker1.TabIndex = 4;
			this.dateTimePicker1.Value = new System.DateTime(2021, 4, 26, 0, 0, 0, 0);
			// 
			// dateTimePicker2
			// 
			this.dateTimePicker2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.dateTimePicker2.Location = new System.Drawing.Point(36, 84);
			this.dateTimePicker2.Margin = new System.Windows.Forms.Padding(4);
			this.dateTimePicker2.Name = "dateTimePicker2";
			this.dateTimePicker2.Size = new System.Drawing.Size(203, 30);
			this.dateTimePicker2.TabIndex = 5;
			this.dateTimePicker2.Value = new System.DateTime(2022, 4, 28, 0, 0, 0, 0);
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(16, 782);
			this.progressBar1.Margin = new System.Windows.Forms.Padding(4);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(2148, 28);
			this.progressBar1.TabIndex = 6;
			// 
			// exploreButton
			// 
			this.exploreButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.exploreButton.Location = new System.Drawing.Point(31, 587);
			this.exploreButton.Margin = new System.Windows.Forms.Padding(4);
			this.exploreButton.Name = "exploreButton";
			this.exploreButton.Size = new System.Drawing.Size(156, 41);
			this.exploreButton.TabIndex = 7;
			this.exploreButton.Text = "Explore";
			this.exploreButton.UseVisualStyleBackColor = true;
			this.exploreButton.Click += new System.EventHandler(this.exploreButton_Click);
			// 
			// countTextBox
			// 
			this.countTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.countTextBox.Location = new System.Drawing.Point(99, 320);
			this.countTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.countTextBox.Name = "countTextBox";
			this.countTextBox.ReadOnly = true;
			this.countTextBox.Size = new System.Drawing.Size(95, 30);
			this.countTextBox.TabIndex = 8;
			this.countTextBox.Text = "1";
			// 
			// errorTextBox
			// 
			this.errorTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.errorTextBox.ForeColor = System.Drawing.Color.Red;
			this.errorTextBox.Location = new System.Drawing.Point(17, 741);
			this.errorTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.errorTextBox.Name = "errorTextBox";
			this.errorTextBox.Size = new System.Drawing.Size(2145, 30);
			this.errorTextBox.TabIndex = 9;
			// 
			// tableRichTextBox
			// 
			this.tableRichTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.tableRichTextBox.Location = new System.Drawing.Point(255, 262);
			this.tableRichTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.tableRichTextBox.Name = "tableRichTextBox";
			this.tableRichTextBox.Size = new System.Drawing.Size(1908, 221);
			this.tableRichTextBox.TabIndex = 10;
			this.tableRichTextBox.Text = "";
			// 
			// specialCheckBox
			// 
			this.specialCheckBox.AutoSize = true;
			this.specialCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.specialCheckBox.Location = new System.Drawing.Point(39, 171);
			this.specialCheckBox.Margin = new System.Windows.Forms.Padding(4);
			this.specialCheckBox.Name = "specialCheckBox";
			this.specialCheckBox.Size = new System.Drawing.Size(157, 29);
			this.specialCheckBox.TabIndex = 11;
			this.specialCheckBox.Text = "Специальное";
			this.specialCheckBox.UseVisualStyleBackColor = true;
			// 
			// successCheckBox
			// 
			this.successCheckBox.AutoSize = true;
			this.successCheckBox.Checked = true;
			this.successCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.successCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.successCheckBox.Location = new System.Drawing.Point(39, 137);
			this.successCheckBox.Margin = new System.Windows.Forms.Padding(4);
			this.successCheckBox.Name = "successCheckBox";
			this.successCheckBox.Size = new System.Drawing.Size(88, 29);
			this.successCheckBox.TabIndex = 12;
			this.successCheckBox.Text = "Успех";
			this.successCheckBox.UseVisualStyleBackColor = true;
			// 
			// resultRichTextBox
			// 
			this.resultRichTextBox.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.resultRichTextBox.Location = new System.Drawing.Point(255, 502);
			this.resultRichTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.resultRichTextBox.Name = "resultRichTextBox";
			this.resultRichTextBox.Size = new System.Drawing.Size(1908, 221);
			this.resultRichTextBox.TabIndex = 13;
			this.resultRichTextBox.Text = "";
			// 
			// hashTextBox
			// 
			this.hashTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.hashTextBox.Location = new System.Drawing.Point(107, 380);
			this.hashTextBox.Margin = new System.Windows.Forms.Padding(4);
			this.hashTextBox.Name = "hashTextBox";
			this.hashTextBox.Size = new System.Drawing.Size(119, 30);
			this.hashTextBox.TabIndex = 14;
			this.hashTextBox.Text = "x4ju7jptb";
			// 
			// checkBoxMinRoute
			// 
			this.checkBoxMinRoute.AutoSize = true;
			this.checkBoxMinRoute.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.checkBoxMinRoute.Location = new System.Drawing.Point(39, 529);
			this.checkBoxMinRoute.Margin = new System.Windows.Forms.Padding(4);
			this.checkBoxMinRoute.Name = "checkBoxMinRoute";
			this.checkBoxMinRoute.Size = new System.Drawing.Size(117, 29);
			this.checkBoxMinRoute.TabIndex = 15;
			this.checkBoxMinRoute.Text = "MinRoute";
			this.checkBoxMinRoute.UseVisualStyleBackColor = true;
			// 
			// customCheckBox
			// 
			this.customCheckBox.AutoSize = true;
			this.customCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.customCheckBox.Location = new System.Drawing.Point(39, 208);
			this.customCheckBox.Margin = new System.Windows.Forms.Padding(4);
			this.customCheckBox.Name = "customCheckBox";
			this.customCheckBox.Size = new System.Drawing.Size(139, 29);
			this.customCheckBox.TabIndex = 16;
			this.customCheckBox.Text = "Кастомное";
			this.customCheckBox.UseVisualStyleBackColor = true;
			// 
			// ModeComboBox
			// 
			this.ModeComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.ModeComboBox.FormattingEnabled = true;
			this.ModeComboBox.Location = new System.Drawing.Point(8, 490);
			this.ModeComboBox.Margin = new System.Windows.Forms.Padding(4);
			this.ModeComboBox.Name = "ModeComboBox";
			this.ModeComboBox.Size = new System.Drawing.Size(217, 30);
			this.ModeComboBox.TabIndex = 17;
			this.ModeComboBox.SelectedIndexChanged += new System.EventHandler(this.ModeComboBox_SelectedIndexChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label1.Location = new System.Drawing.Point(8, 466);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(64, 20);
			this.label1.TabIndex = 18;
			this.label1.Text = "Режим";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label2.Location = new System.Drawing.Point(35, 327);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 20);
			this.label2.TabIndex = 19;
			this.label2.Text = "Строк";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label3.Location = new System.Drawing.Point(12, 388);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(88, 20);
			this.label3.TabIndex = 20;
			this.label3.Text = "Хэш лога";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label4.Location = new System.Drawing.Point(12, 46);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(18, 20);
			this.label4.TabIndex = 21;
			this.label4.Text = "с";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.label5.Location = new System.Drawing.Point(4, 91);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(29, 20);
			this.label5.TabIndex = 22;
			this.label5.Text = "по";
			// 
			// logsBackgroundWorker
			// 
			this.logsBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
			this.logsBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
			this.logsBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
			// 
			// _exploreBackgroundWorker
			// 
			this._exploreBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
			this._exploreBackgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
			this._exploreBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1762, 820);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ModeComboBox);
			this.Controls.Add(this.customCheckBox);
			this.Controls.Add(this.checkBoxMinRoute);
			this.Controls.Add(this.hashTextBox);
			this.Controls.Add(this.resultRichTextBox);
			this.Controls.Add(this.successCheckBox);
			this.Controls.Add(this.specialCheckBox);
			this.Controls.Add(this.tableRichTextBox);
			this.Controls.Add(this.errorTextBox);
			this.Controls.Add(this.countTextBox);
			this.Controls.Add(this.exploreButton);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.dateTimePicker2);
			this.Controls.Add(this.dateTimePicker1);
			this.Controls.Add(this.logsRichTextBox);
			this.Controls.Add(this.startButton);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "Form1";
			this.Text = "MapExplorer";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResizeEnd += new System.EventHandler(this.Form1_ResizeEnd);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Button startButton;
		private System.Windows.Forms.RichTextBox logsRichTextBox;
		private System.Windows.Forms.DateTimePicker dateTimePicker1;
		private System.Windows.Forms.DateTimePicker dateTimePicker2;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.Button exploreButton;
		private System.Windows.Forms.TextBox countTextBox;
		private System.Windows.Forms.TextBox errorTextBox;
		private System.Windows.Forms.RichTextBox tableRichTextBox;
		private System.Windows.Forms.CheckBox specialCheckBox;
		private System.Windows.Forms.CheckBox successCheckBox;
		private System.Windows.Forms.RichTextBox resultRichTextBox;
		private System.Windows.Forms.TextBox hashTextBox;
		private System.Windows.Forms.CheckBox checkBoxMinRoute;
        private System.Windows.Forms.CheckBox customCheckBox;
        private System.Windows.Forms.ComboBox ModeComboBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
        private System.ComponentModel.BackgroundWorker logsBackgroundWorker;
		private System.ComponentModel.BackgroundWorker _exploreBackgroundWorker;
	}
}

