namespace KoiVM.Confuser.Processor {
	partial class EditUser {
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
			this.txtName = new System.Windows.Forms.TextBox();
			this.txtEmail = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.dtExpiration = new System.Windows.Forms.DateTimePicker();
			this.label4 = new System.Windows.Forms.Label();
			this.cbStatus = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(74, 25);
			this.label1.TabIndex = 0;
			this.label1.Text = "User Name: ";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// txtName
			// 
			this.txtName.Location = new System.Drawing.Point(92, 12);
			this.txtName.Name = "txtName";
			this.txtName.Size = new System.Drawing.Size(272, 20);
			this.txtName.TabIndex = 1;
			// 
			// txtEmail
			// 
			this.txtEmail.Location = new System.Drawing.Point(92, 37);
			this.txtEmail.Name = "txtEmail";
			this.txtEmail.Size = new System.Drawing.Size(272, 20);
			this.txtEmail.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(12, 34);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(74, 25);
			this.label2.TabIndex = 2;
			this.label2.Text = "Email: ";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(151, 95);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(12, 60);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(74, 25);
			this.label3.TabIndex = 5;
			this.label3.Text = "Expiration: ";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// dtExpiration
			// 
			this.dtExpiration.Location = new System.Drawing.Point(92, 63);
			this.dtExpiration.Name = "dtExpiration";
			this.dtExpiration.Size = new System.Drawing.Size(126, 20);
			this.dtExpiration.TabIndex = 6;
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(224, 60);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(51, 25);
			this.label4.TabIndex = 7;
			this.label4.Text = "Status: ";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// cbStatus
			// 
			this.cbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbStatus.FormattingEnabled = true;
			this.cbStatus.Location = new System.Drawing.Point(281, 63);
			this.cbStatus.Name = "cbStatus";
			this.cbStatus.Size = new System.Drawing.Size(83, 21);
			this.cbStatus.TabIndex = 8;
			// 
			// EditUser
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(376, 130);
			this.Controls.Add(this.cbStatus);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.dtExpiration);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.txtEmail);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtName);
			this.Controls.Add(this.label1);
			this.Name = "EditUser";
			this.Text = "User Info";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtName;
		private System.Windows.Forms.TextBox txtEmail;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker dtExpiration;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cbStatus;
	}
}