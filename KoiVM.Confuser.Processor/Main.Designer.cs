namespace KoiVM.Confuser.Processor {
	partial class Main {
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
            this.txtUsrList = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.lstUsers = new System.Windows.Forms.ListView();
            this.watermark = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.expire = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ver = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.usrName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.email = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.run = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnNewUsr = new System.Windows.Forms.Button();
            this.btnGenAll = new System.Windows.Forms.Button();
            this.txtDep = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.itemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.itemDel = new System.Windows.Forms.ToolStripMenuItem();
            this.itemGen = new System.Windows.Forms.ToolStripMenuItem();
            this.txtPubPath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtBinPath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnStubGen = new System.Windows.Forms.Button();
            this.lblStubStatus = new System.Windows.Forms.Label();
            this.btnDeploy = new System.Windows.Forms.Button();
            this.btnClean = new System.Windows.Forms.Button();
            this.ctxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "User List: ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtUsrList
            // 
            this.txtUsrList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtUsrList.Location = new System.Drawing.Point(81, 9);
            this.txtUsrList.Name = "txtUsrList";
            this.txtUsrList.Size = new System.Drawing.Size(555, 20);
            this.txtUsrList.TabIndex = 1;
            this.txtUsrList.TextChanged += new System.EventHandler(this.txtUsrList_TextChanged);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(711, 8);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(63, 23);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lstUsers
            // 
            this.lstUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.watermark,
            this.id,
            this.expire,
            this.status,
            this.ver,
            this.usrName,
            this.email,
            this.run});
            this.lstUsers.FullRowSelect = true;
            this.lstUsers.GridLines = true;
            this.lstUsers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstUsers.Location = new System.Drawing.Point(15, 115);
            this.lstUsers.MultiSelect = false;
            this.lstUsers.Name = "lstUsers";
            this.lstUsers.Size = new System.Drawing.Size(759, 405);
            this.lstUsers.TabIndex = 3;
            this.lstUsers.UseCompatibleStateImageBehavior = false;
            this.lstUsers.View = System.Windows.Forms.View.Details;
            this.lstUsers.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lstUsers_MouseDown);
            // 
            // watermark
            // 
            this.watermark.Text = "Watermark";
            this.watermark.Width = 70;
            // 
            // id
            // 
            this.id.Text = "Id";
            this.id.Width = 70;
            // 
            // expire
            // 
            this.expire.Text = "Expiration";
            this.expire.Width = 80;
            // 
            // status
            // 
            this.status.Text = "Status";
            this.status.Width = 70;
            // 
            // ver
            // 
            this.ver.Text = "Version";
            this.ver.Width = 150;
            // 
            // usrName
            // 
            this.usrName.Text = "Name";
            this.usrName.Width = 100;
            // 
            // email
            // 
            this.email.Text = "Email";
            this.email.Width = 150;
            // 
            // run
            // 
            this.run.Text = "Run Log";
            this.run.Width = 300;
            // 
            // btnLoad
            // 
            this.btnLoad.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoad.Location = new System.Drawing.Point(642, 8);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(63, 23);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnNewUsr
            // 
            this.btnNewUsr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNewUsr.Location = new System.Drawing.Point(15, 526);
            this.btnNewUsr.Name = "btnNewUsr";
            this.btnNewUsr.Size = new System.Drawing.Size(75, 23);
            this.btnNewUsr.TabIndex = 4;
            this.btnNewUsr.Text = "New User";
            this.btnNewUsr.UseVisualStyleBackColor = true;
            this.btnNewUsr.Click += new System.EventHandler(this.btnNewUsr_Click);
            // 
            // btnGenAll
            // 
            this.btnGenAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGenAll.Location = new System.Drawing.Point(96, 526);
            this.btnGenAll.Name = "btnGenAll";
            this.btnGenAll.Size = new System.Drawing.Size(75, 23);
            this.btnGenAll.TabIndex = 5;
            this.btnGenAll.Text = "Generate All";
            this.btnGenAll.UseVisualStyleBackColor = true;
            this.btnGenAll.Click += new System.EventHandler(this.btnGenAll_Click);
            // 
            // txtDep
            // 
            this.txtDep.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDep.Location = new System.Drawing.Point(96, 63);
            this.txtDep.Name = "txtDep";
            this.txtDep.ReadOnly = true;
            this.txtDep.Size = new System.Drawing.Size(676, 20);
            this.txtDep.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 63);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Deploy Path:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ctxMenu
            // 
            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemCopy,
            this.itemEdit,
            this.itemDel,
            this.itemGen});
            this.ctxMenu.Name = "ctxMenu";
            this.ctxMenu.Size = new System.Drawing.Size(136, 92);
            // 
            // itemCopy
            // 
            this.itemCopy.Name = "itemCopy";
            this.itemCopy.Size = new System.Drawing.Size(135, 22);
            this.itemCopy.Text = "Copy Koi Id";
            this.itemCopy.Click += new System.EventHandler(this.itemCopy_Click);
            // 
            // itemEdit
            // 
            this.itemEdit.Name = "itemEdit";
            this.itemEdit.Size = new System.Drawing.Size(135, 22);
            this.itemEdit.Text = "Edit User...";
            this.itemEdit.Click += new System.EventHandler(this.itemEdit_Click);
            // 
            // itemDel
            // 
            this.itemDel.Name = "itemDel";
            this.itemDel.Size = new System.Drawing.Size(135, 22);
            this.itemDel.Text = "Delete";
            this.itemDel.Click += new System.EventHandler(this.itemDel_Click);
            // 
            // itemGen
            // 
            this.itemGen.Name = "itemGen";
            this.itemGen.Size = new System.Drawing.Size(135, 22);
            this.itemGen.Text = "Generate";
            this.itemGen.Click += new System.EventHandler(this.itemGen_Click);
            // 
            // txtPubPath
            // 
            this.txtPubPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPubPath.Location = new System.Drawing.Point(96, 89);
            this.txtPubPath.Name = "txtPubPath";
            this.txtPubPath.ReadOnly = true;
            this.txtPubPath.Size = new System.Drawing.Size(676, 20);
            this.txtPubPath.TabIndex = 14;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 20);
            this.label4.TabIndex = 13;
            this.label4.Text = "Publish Path:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBinPath
            // 
            this.txtBinPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBinPath.Location = new System.Drawing.Point(96, 37);
            this.txtBinPath.Name = "txtBinPath";
            this.txtBinPath.ReadOnly = true;
            this.txtBinPath.Size = new System.Drawing.Size(676, 20);
            this.txtBinPath.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(12, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 20);
            this.label5.TabIndex = 11;
            this.label5.Text = "Binaries Path:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnStubGen
            // 
            this.btnStubGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStubGen.Location = new System.Drawing.Point(545, 526);
            this.btnStubGen.Name = "btnStubGen";
            this.btnStubGen.Size = new System.Drawing.Size(91, 23);
            this.btnStubGen.TabIndex = 15;
            this.btnStubGen.Text = "Generate Stub";
            this.btnStubGen.UseVisualStyleBackColor = true;
            this.btnStubGen.Click += new System.EventHandler(this.btnStubGen_Click);
            // 
            // lblStubStatus
            // 
            this.lblStubStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStubStatus.AutoEllipsis = true;
            this.lblStubStatus.Location = new System.Drawing.Point(177, 526);
            this.lblStubStatus.Name = "lblStubStatus";
            this.lblStubStatus.Size = new System.Drawing.Size(362, 23);
            this.lblStubStatus.TabIndex = 16;
            this.lblStubStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnDeploy
            // 
            this.btnDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeploy.Location = new System.Drawing.Point(711, 526);
            this.btnDeploy.Name = "btnDeploy";
            this.btnDeploy.Size = new System.Drawing.Size(63, 23);
            this.btnDeploy.TabIndex = 17;
            this.btnDeploy.Text = "Deploy";
            this.btnDeploy.UseVisualStyleBackColor = true;
            this.btnDeploy.Click += new System.EventHandler(this.btnDeploy_Click);
            // 
            // btnClean
            // 
            this.btnClean.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClean.Location = new System.Drawing.Point(642, 526);
            this.btnClean.Name = "btnClean";
            this.btnClean.Size = new System.Drawing.Size(63, 23);
            this.btnClean.TabIndex = 18;
            this.btnClean.Text = "Clean";
            this.btnClean.UseVisualStyleBackColor = true;
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.btnClean);
            this.Controls.Add(this.btnDeploy);
            this.Controls.Add(this.lblStubStatus);
            this.Controls.Add(this.btnStubGen);
            this.Controls.Add(this.txtPubPath);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtBinPath);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtDep);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnGenAll);
            this.Controls.Add(this.btnNewUsr);
            this.Controls.Add(this.lstUsers);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtUsrList);
            this.Controls.Add(this.label1);
            this.Name = "Main";
            this.Text = "Koi Processor";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ctxMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtUsrList;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.ListView lstUsers;
		private System.Windows.Forms.ColumnHeader watermark;
		private System.Windows.Forms.ColumnHeader id;
		private System.Windows.Forms.ColumnHeader expire;
		private System.Windows.Forms.ColumnHeader status;
		private System.Windows.Forms.ColumnHeader usrName;
		private System.Windows.Forms.ColumnHeader email;
		private System.Windows.Forms.ColumnHeader run;
		private System.Windows.Forms.Button btnLoad;
		private System.Windows.Forms.Button btnNewUsr;
		private System.Windows.Forms.Button btnGenAll;
		private System.Windows.Forms.TextBox txtDep;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ContextMenuStrip ctxMenu;
		private System.Windows.Forms.ToolStripMenuItem itemCopy;
		private System.Windows.Forms.ToolStripMenuItem itemEdit;
		private System.Windows.Forms.ToolStripMenuItem itemGen;
		private System.Windows.Forms.ToolStripMenuItem itemDel;
		private System.Windows.Forms.TextBox txtPubPath;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtBinPath;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnStubGen;
		private System.Windows.Forms.Label lblStubStatus;
		private System.Windows.Forms.Button btnDeploy;
		private System.Windows.Forms.Button btnClean;
		private System.Windows.Forms.ColumnHeader ver;
	}
}