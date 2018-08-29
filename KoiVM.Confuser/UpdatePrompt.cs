#region

using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser
{
    internal partial class UpdatePrompt : Form
    {
        private bool failed;
        private readonly string newVersion;

        public UpdatePrompt(string newVersion)
        {
            InitializeComponent();

            this.newVersion = newVersion;
            verCurrent.Text = KoiInfo.settings.Version ?? "<< None >>";
            verServer.Text = newVersion;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;

            var sys = new KoiSystem();
            sys.Progress += value => BeginInvoke(new Action(() =>
            {
                if(value == 0)
                {
                    progress.Value = 0;
                    progress.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    progress.Style = ProgressBarStyle.Continuous;
                    progress.Value = (int) (value * 1000);
                }
            }));
            sys.Finish += success =>
            {
                BeginInvoke(new Action(() =>
                {
                    btnUpdate.Enabled = true;

                    if(success)
                    {
                        KoiInfo.settings.Version = newVersion;
                        KoiInfo.settings.Save();
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        failed = true;
                        KoiInfo.settings.Version = "";
                        KoiInfo.settings.Save();
                        MessageBox.Show("Login failed.", "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            };
            sys.Login(KoiInfo.settings.KoiID);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BringToFront();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(!btnUpdate.Enabled)
                e.Cancel = true;
            if(!failed)
                DialogResult = DialogResult.OK;
            base.OnClosing(e);
        }
    }
}