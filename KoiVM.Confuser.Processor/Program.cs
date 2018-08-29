#region

using System;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser.Processor
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}