namespace KoiVM.Confuser {
	partial class ConfigWindow {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.verCurrent = new System.Windows.Forms.TextBox();
			this.verServer = new System.Windows.Forms.TextBox();
			this.btnRefr = new System.Windows.Forms.Button();
			this.btnDl = new System.Windows.Forms.Button();
			this.progress = new System.Windows.Forms.ProgressBar();
			this.txtId = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cbUI = new System.Windows.Forms.CheckBox();
			this.cbUpd = new System.Windows.Forms.CheckBox();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.btnDecoder = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 52);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(127, 29);
			this.label1.TabIndex = 0;
			this.label1.Text = "Current Version: ";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(127, 29);
			this.label2.TabIndex = 1;
			this.label2.Text = "Server Version: ";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// verCurrent
			// 
			this.verCurrent.Location = new System.Drawing.Point(145, 53);
			this.verCurrent.Name = "verCurrent";
			this.verCurrent.ReadOnly = true;
			this.verCurrent.Size = new System.Drawing.Size(227, 29);
			this.verCurrent.TabIndex = 2;
			// 
			// verServer
			// 
			this.verServer.Location = new System.Drawing.Point(145, 88);
			this.verServer.Name = "verServer";
			this.verServer.ReadOnly = true;
			this.verServer.Size = new System.Drawing.Size(227, 29);
			this.verServer.TabIndex = 3;
			// 
			// btnRefr
			// 
			this.btnRefr.Location = new System.Drawing.Point(16, 128);
			this.btnRefr.Name = "btnRefr";
			this.btnRefr.Size = new System.Drawing.Size(84, 33);
			this.btnRefr.TabIndex = 4;
			this.btnRefr.Text = "Refresh";
			this.btnRefr.UseVisualStyleBackColor = true;
			this.btnRefr.Click += new System.EventHandler(this.btnRefr_Click);
			// 
			// btnDl
			// 
			this.btnDl.Location = new System.Drawing.Point(106, 128);
			this.btnDl.Name = "btnDl";
			this.btnDl.Size = new System.Drawing.Size(93, 33);
			this.btnDl.TabIndex = 5;
			this.btnDl.Text = "Download";
			this.btnDl.UseVisualStyleBackColor = true;
			this.btnDl.Click += new System.EventHandler(this.btnDl_Click);
			// 
			// progress
			// 
			this.progress.Location = new System.Drawing.Point(205, 128);
			this.progress.Maximum = 1000;
			this.progress.Name = "progress";
			this.progress.Size = new System.Drawing.Size(167, 33);
			this.progress.TabIndex = 6;
			// 
			// txtId
			// 
			this.txtId.Location = new System.Drawing.Point(145, 12);
			this.txtId.Name = "txtId";
			this.txtId.Size = new System.Drawing.Size(227, 29);
			this.txtId.TabIndex = 8;
			this.txtId.TextChanged += new System.EventHandler(this.txtChanged);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(12, 11);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(127, 29);
			this.label3.TabIndex = 7;
			this.label3.Text = "Koi ID: ";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cbUI
			// 
			this.cbUI.AutoSize = true;
			this.cbUI.Location = new System.Drawing.Point(14, 167);
			this.cbUI.Name = "cbUI";
			this.cbUI.Size = new System.Drawing.Size(125, 25);
			this.cbUI.TabIndex = 9;
			this.cbUI.Text = "No UI Prompt";
			this.cbUI.UseVisualStyleBackColor = true;
			this.cbUI.CheckedChanged += new System.EventHandler(this.cbChanged);
			// 
			// cbUpd
			// 
			this.cbUpd.AutoSize = true;
			this.cbUpd.Location = new System.Drawing.Point(220, 167);
			this.cbUpd.Name = "cbUpd";
			this.cbUpd.Size = new System.Drawing.Size(150, 25);
			this.cbUpd.TabIndex = 12;
			this.cbUpd.Text = "No Check Update";
			this.cbUpd.UseVisualStyleBackColor = true;
			this.cbUpd.CheckedChanged += new System.EventHandler(this.cbChanged);
			// 
			// timer
			// 
			this.timer.Tick += new System.EventHandler(this.TimerSave);
			// 
			// btnDecoder
			// 
			this.btnDecoder.Location = new System.Drawing.Point(205, 198);
			this.btnDecoder.Name = "btnDecoder";
			this.btnDecoder.Size = new System.Drawing.Size(165, 33);
			this.btnDecoder.TabIndex = 13;
			this.btnDecoder.Text = "Line No. Decoder";
			this.btnDecoder.UseVisualStyleBackColor = true;
			this.btnDecoder.Click += new System.EventHandler(this.btnDecoder_Click);
			// 
			// ConfigWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(384, 242);
			this.Controls.Add(this.btnDecoder);
			this.Controls.Add(this.cbUpd);
			this.Controls.Add(this.cbUI);
			this.Controls.Add(this.txtId);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.progress);
			this.Controls.Add(this.btnDl);
			this.Controls.Add(this.btnRefr);
			this.Controls.Add(this.verServer);
			this.Controls.Add(this.verCurrent);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "ConfigWindow";
			this.Text = "KoiVM Configuration";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox verCurrent;
		private System.Windows.Forms.TextBox verServer;
		private System.Windows.Forms.Button btnRefr;
		private System.Windows.Forms.Button btnDl;
		private System.Windows.Forms.ProgressBar progress;
		private System.Windows.Forms.TextBox txtId;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox cbUI;
		private System.Windows.Forms.CheckBox cbUpd;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Button btnDecoder;
	}
}