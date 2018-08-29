#region

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Confuser.Core;
using Confuser.Renamer;
using Ookii.Dialogs.Wpf;

#endregion

namespace ConfuserEx
{
    /// <summary>
    ///     Interaction logic for StackTraceDecoder.xaml
    /// </summary>
    public partial class StackTraceDecoder : Window
    {
        private readonly Regex mapSymbolMatcher = new Regex("_[a-zA-Z0-9]+");
        private readonly Regex passSymbolMatcher = new Regex("[a-zA-Z0-9_$]{23,}");

        private readonly Dictionary<string, string> symMap = new Dictionary<string, string>();
        private ReversibleRenamer renamer;

        public StackTraceDecoder()
        {
            InitializeComponent();
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(File.Exists(PathBox.Text))
                LoadSymMap(PathBox.Text);
        }

        private void LoadSymMap(string path)
        {
            var shortPath = path;
            if(path.Length > 35)
                shortPath = "..." + path.Substring(path.Length - 35, 35);

            try
            {
                symMap.Clear();
                using(var reader = new StreamReader(File.OpenRead(path)))
                {
                    var line = reader.ReadLine();
                    while(line != null)
                    {
                        var tabIndex = line.IndexOf('\t');
                        if(tabIndex == -1)
                            throw new FileFormatException();
                        symMap.Add(line.Substring(0, tabIndex), line.Substring(tabIndex + 1));
                        line = reader.ReadLine();
                    }
                }
                status.Content = "Loaded symbol map from '" + shortPath + "' successfully.";
            }
            catch
            {
                status.Content = "Failed to load symbol map from '" + shortPath + "'.";
            }
        }

        private void ChooseMapPath(object sender, RoutedEventArgs e)
        {
            var ofd = new VistaOpenFileDialog();
            ofd.Filter = "Symbol maps (*.map)|*.map|All Files (*.*)|*.*";
            if(ofd.ShowDialog() ?? false) PathBox.Text = ofd.FileName;
        }

        private void Decode_Click(object sender, RoutedEventArgs e)
        {
            var trace = stackTrace.Text;
            if(optSym.IsChecked ?? true)
            {
                stackTrace.Text = mapSymbolMatcher.Replace(trace, DecodeSymbolMap);
            }
            else
            {
                renamer = new ReversibleRenamer(PassBox.Text);
                stackTrace.Text = passSymbolMatcher.Replace(trace, DecodeSymbolPass);
            }
        }

        private string DecodeSymbolMap(Match match)
        {
            var sym = match.Value;
            return symMap.GetValueOrDefault(sym, sym);
        }

        private string DecodeSymbolPass(Match match)
        {
            var sym = match.Value;
            try
            {
                return renamer.Decrypt(sym);
            }
            catch
            {
                return sym;
            }
        }
    }
}