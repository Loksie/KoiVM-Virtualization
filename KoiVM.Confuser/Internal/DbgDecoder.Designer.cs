namespace KoiVM.Confuser.Internal {
	partial class DbgDecoder {
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
			this.txtMap = new System.Windows.Forms.TextBox();
			this.btnDecode = new System.Windows.Forms.Button();
			this.txtToken = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtInfo = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 15);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(126, 21);
			this.label1.TabIndex = 0;
			this.label1.Text = "Debug Map File :";
			// 
			// txtMap
			// 
			this.txtMap.AllowDrop = true;
			this.txtMap.Location = new System.Drawing.Point(141, 12);
			this.txtMap.Name = "txtMap";
			this.txtMap.Size = new System.Drawing.Size(241, 29);
			this.txtMap.TabIndex = 1;
			this.txtMap.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtMap_DragDrop);
			this.txtMap.DragOver += new System.Windows.Forms.DragEventHandler(this.txtMap_DragOver);
			// 
			// btnDecode
			// 
			this.btnDecode.Location = new System.Drawing.Point(156, 82);
			this.btnDecode.Name = "btnDecode";
			this.btnDecode.Size = new System.Drawing.Size(83, 31);
			this.btnDecode.TabIndex = 2;
			this.btnDecode.Text = "Decode";
			this.btnDecode.UseVisualStyleBackColor = true;
			this.btnDecode.Click += new System.EventHandler(this.btnDecode_Click);
			// 
			// txtToken
			// 
			this.txtToken.Location = new System.Drawing.Point(141, 47);
			this.txtToken.Name = "txtToken";
			this.txtToken.Size = new System.Drawing.Size(241, 29);
			this.txtToken.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(27, 50);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(107, 21);
			this.label2.TabIndex = 3;
			this.label2.Text = "Debug Token :";
			// 
			// txtInfo
			// 
			this.txtInfo.Location = new System.Drawing.Point(12, 119);
			this.txtInfo.Multiline = true;
			this.txtInfo.Name = "txtInfo";
			this.txtInfo.ReadOnly = true;
			this.txtInfo.Size = new System.Drawing.Size(370, 140);
			this.txtInfo.TabIndex = 5;
			// 
			// DbgDecoder
			// 
			this.AcceptButton = this.btnDecode;
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 21F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(394, 271);
			this.Controls.Add(this.txtInfo);
			this.Controls.Add(this.txtToken);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnDecode);
			this.Controls.Add(this.txtMap);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.MaximumSize = new System.Drawing.Size(600, 400);
			this.MinimumSize = new System.Drawing.Size(300, 200);
			this.Name = "DbgDecoder";
			this.Text = "Debug Info Decoder";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtMap;
		private System.Windows.Forms.Button btnDecode;
		private System.Windows.Forms.TextBox txtToken;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtInfo;
	}
}