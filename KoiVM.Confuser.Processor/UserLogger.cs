#region

using System;
using System.Drawing;
using System.Windows.Forms;
using Confuser.Core;

#endregion

namespace KoiVM.Confuser.Processor
{
    public class UserLogger : ILogger
    {
        private readonly ListViewItem.ListViewSubItem item;
        private readonly ListView view;

        public UserLogger(ListView view, ListViewItem.ListViewSubItem item)
        {
            this.view = view;
            this.item = item;
        }

        public void Debug(string msg)
        {
            //view.BeginInvoke(new Action(() => {
            //    item.ForeColor = Color.Gray;
            //    item.Text = msg;
            //}));
        }

        public void DebugFormat(string format, params object[] args)
        {
            Debug(string.Format(format, args));
        }

        public void Info(string msg)
        {
            view.BeginInvoke(new Action(() =>
            {
                item.ForeColor = Color.Black;
                item.Text = msg;
            }));
        }

        public void InfoFormat(string format, params object[] args)
        {
            Info(string.Format(format, args));
        }

        public void Warn(string msg)
        {
            view.BeginInvoke(new Action(() =>
            {
                item.ForeColor = Color.DarkOrange;
                item.Text = msg;
            }));
        }

        public void WarnException(string msg, Exception ex)
        {
            Warn(msg);
        }

        public void WarnFormat(string format, params object[] args)
        {
            Warn(string.Format(format, args));
        }

        public void Error(string msg)
        {
            view.BeginInvoke(new Action(() =>
            {
                item.ForeColor = Color.DarkRed;
                item.Text = msg;
            }));
            MessageBox.Show(msg);
        }

        public void ErrorException(string msg, Exception ex)
        {
            Error(msg);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public void Finish(bool successful)
        {
        }

        public void EndProgress()
        {
        }

        public void Progress(int progress, int overall)
        {
        }
    }
}