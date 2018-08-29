#region

using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser
{
    internal partial class LoginPrompt : Form
    {
        public LoginPrompt()
        {
            InitializeComponent();
            txtId.Text = KoiInfo.settings.KoiID ?? "";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            txtId.Enabled = false;
            KoiInfo.settings.KoiID = txtId.Text;
            KoiInfo.settings.Save();

            var sys = new KoiSystem();
            var ver = sys.GetVersion(KoiInfo.settings.KoiID);
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
                    btnLogin.Enabled = true;
                    txtId.Enabled = true;
                    if(success)
                    {
                        KoiInfo.settings.Version = ver;
                        KoiInfo.settings.Save();
                        DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        KoiInfo.settings.Version = "";
                        KoiInfo.settings.Save();
                        MessageBox.Show("Login failed.", "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            };
            sys.Login(txtId.Text);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BringToFront();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(!btnLogin.Enabled)
                e.Cancel = true;
            base.OnClosing(e);
        }
    }
}