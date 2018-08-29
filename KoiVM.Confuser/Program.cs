#region

using System;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            KoiInfo.Init();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ConfigWindow());
        }
    }
}