namespace KoiVM.Confuser {
	partial class LoginPrompt {
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
			this.label1 = new System.Windows.Forms.Label();
			this.txtId = new System.Windows.Forms.TextBox();
			this.btnLogin = new System.Windows.Forms.Button();
			this.progress = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 38);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(58, 21);
			this.label1.TabIndex = 0;
			this.label1.Text = "Koi ID :";
			// 
			// txtId
			// 
			this.txtId.Location = new System.Drawing.Point(78, 35);
			this.txtId.Name = "txtId";
			this.txtId.Size = new System.Drawing.Size(204, 29);
			this.txtId.TabIndex = 1;
			// 
			// btnLogin
			// 
			this.btnLogin.Location = new System.Drawing.Point(106, 70);
			this.btnLogin.Name = "btnLogin";
			this.btnLogin.Size = new System.Drawing.Size(83, 31);
			this.btnLogin.TabIndex = 2;
			this.btnLogin.Text = "Login";
			this.btnLogin.UseVisualStyleBackColor = true;
			this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
			// 
			// progress
			// 
			this.progress.Location = new System.Drawing.Point(17, 119);
			this.progress.Maximum = 1000;
			this.progress.Name = "progress";
			this.progress.Size = new System.Drawing.Size(265, 23);
			this.progress.TabIndex = 3;
			// 
			// LoginPrompt
			// 
			this.AcceptButton = this.btnLogin;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(294, 171);
			this.Controls.Add(this.progress);
			this.Controls.Add(this.btnLogin);
			this.Controls.Add(this.txtId);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximumSize = new System.Drawing.Size(300, 200);
			this.MinimumSize = new System.Drawing.Size(300, 200);
			this.Name = "LoginPrompt";
			this.Text = "Login to Koi System";
			this.TopMost = true;
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtId;
		private System.Windows.Forms.Button btnLogin;
		private System.Windows.Forms.ProgressBar progress;
	}
}