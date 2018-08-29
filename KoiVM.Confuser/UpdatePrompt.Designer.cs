namespace KoiVM.Confuser {
	partial class UpdatePrompt {
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
			this.btnUpdate = new System.Windows.Forms.Button();
			this.progress = new System.Windows.Forms.ProgressBar();
			this.verServer = new System.Windows.Forms.TextBox();
			this.verCurrent = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnUpdate
			// 
			this.btnUpdate.Location = new System.Drawing.Point(156, 80);
			this.btnUpdate.Name = "btnUpdate";
			this.btnUpdate.Size = new System.Drawing.Size(83, 31);
			this.btnUpdate.TabIndex = 2;
			this.btnUpdate.Text = "Update";
			this.btnUpdate.UseVisualStyleBackColor = true;
			this.btnUpdate.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// progress
			// 
			this.progress.Location = new System.Drawing.Point(17, 136);
			this.progress.Maximum = 1000;
			this.progress.Name = "progress";
			this.progress.Size = new System.Drawing.Size(365, 23);
			this.progress.TabIndex = 3;
			// 
			// verServer
			// 
			this.verServer.Location = new System.Drawing.Point(145, 45);
			this.verServer.Name = "verServer";
			this.verServer.ReadOnly = true;
			this.verServer.Size = new System.Drawing.Size(237, 29);
			this.verServer.TabIndex = 7;
			// 
			// verCurrent
			// 
			this.verCurrent.Location = new System.Drawing.Point(145, 10);
			this.verCurrent.Name = "verCurrent";
			this.verCurrent.ReadOnly = true;
			this.verCurrent.Size = new System.Drawing.Size(237, 29);
			this.verCurrent.TabIndex = 6;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 45);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(127, 29);
			this.label2.TabIndex = 5;
			this.label2.Text = "Server Version: ";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(127, 29);
			this.label1.TabIndex = 4;
			this.label1.Text = "Current Version: ";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UpdatePrompt
			// 
			this.AcceptButton = this.btnUpdate;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(394, 171);
			this.Controls.Add(this.verServer);
			this.Controls.Add(this.verCurrent);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.progress);
			this.Controls.Add(this.btnUpdate);
			this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximumSize = new System.Drawing.Size(400, 200);
			this.MinimumSize = new System.Drawing.Size(400, 200);
			this.Name = "UpdatePrompt";
			this.Text = "KoiVM - New Version";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnUpdate;
		private System.Windows.Forms.ProgressBar progress;
		private System.Windows.Forms.TextBox verServer;
		private System.Windows.Forms.TextBox verCurrent;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
	}
}