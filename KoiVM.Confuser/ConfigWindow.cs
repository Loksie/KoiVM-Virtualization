#region

using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using KoiVM.Confuser.Internal;

#endregion

namespace KoiVM.Confuser
{
    [Obfuscation(Exclude = false, Feature = "+ref proxy")]
    internal partial class ConfigWindow : Form
    {
        public ConfigWindow()
        {
            InitializeComponent();
            timer.Start();

            txtId.Text = KoiInfo.settings.KoiID;

            cbUI.Checked = KoiInfo.settings.NoUI;
            cbUpd.Checked = KoiInfo.settings.NoCheck;

            verCurrent.Text = KoiInfo.settings.Version ?? "<< None >>";
        }

        private void txtChanged(object sender, EventArgs e)
        {
            KoiInfo.settings.KoiID = txtId.Text;
        }

        private void cbChanged(object sender, EventArgs e)
        {
            if(sender == cbUI)
                KoiInfo.settings.NoUI = cbUI.Checked;
            if(sender == cbUpd)
                KoiInfo.settings.NoCheck = cbUpd.Checked;
        }

        private void TimerSave(object sender, EventArgs e)
        {
            KoiInfo.settings.Save();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer.Stop();
            KoiInfo.settings.Save();
        }

        protected override void OnShown(EventArgs e)
        {
            CheckVersion();
            base.OnShown(e);
        }

        private void CheckVersion()
        {
            verServer.Text = "<< Loading... >>";
            btnRefr.Enabled = btnDl.Enabled = false;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var sys = new KoiSystem();
                string ver;
                if(string.IsNullOrEmpty(KoiInfo.settings.KoiID))
                {
                    ver = "<< Enter your Koi ID >>";
                }
                else
                {
                    ver = sys.GetVersion(KoiInfo.settings.KoiID);
                    ver = ver ?? "<< Fail to retrieve version >>";
                }
                BeginInvoke(new Action(() =>
                {
                    verServer.Text = ver;
                    btnRefr.Enabled = btnDl.Enabled = true;
                }));
            });
        }

        private void btnRefr_Click(object sender, EventArgs e)
        {
            CheckVersion();
        }

        private void btnDl_Click(object sender, EventArgs e)
        {
            txtId.Enabled = btnRefr.Enabled = btnDl.Enabled = false;

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
                    txtId.Enabled = btnRefr.Enabled = btnDl.Enabled = true;
                    if(success)
                    {
                        verCurrent.Text = KoiInfo.settings.Version = ver;
                        MessageBox.Show("Download finished.", "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        verCurrent.Text = "<< None >>";
                        KoiInfo.settings.Version = "";
                        MessageBox.Show("Login failed.", "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            };
            sys.Login(txtId.Text);
        }

        private void btnDecoder_Click(object sender, EventArgs e)
        {
            try
            {
                if(Assembly.GetExecutingAssembly().ManifestModule.GetType("KoiVM.Confuser.Internal.Fish") == null)
                    KoiInfo.InitKoi(false);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Update to newest version first!\r\n" + ex, "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShowDecoder();
        }

        private void ShowDecoder()
        {
            new DbgDecoder().ShowDialog();
        }
    }
}