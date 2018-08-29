#region

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace KoiVM.Confuser
{
    internal class SimpleSettings
    {
        private readonly SortedDictionary<string, string> values;
        private bool changed;

        public SimpleSettings()
        {
            values = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var cfgFile = Path.Combine(KoiInfo.KoiDirectory, "koi.cfg");
            if(File.Exists(cfgFile))
                using(var rdr = new StreamReader(File.OpenRead(cfgFile)))
                {
                    string line;
                    var lineNum = 1;
                    while((line = rdr.ReadLine()) != null)
                    {
                        if(string.IsNullOrEmpty(line))
                            continue;
                        var i = line.IndexOf(":");
                        if(i == -1) throw new ArgumentException("Invalid settings.");
                        var val = line.Substring(i + 1);

                        values.Add(line.Substring(0, i),
                            val.Equals("null", StringComparison.InvariantCultureIgnoreCase) ? null : val);
                        lineNum++;
                    }
                }
        }

        public string GetValue(string key, string def)
        {
            string ret;
            if(!values.TryGetValue(key, out ret))
            {
                if(def == null) throw new ArgumentException(string.Format("'{0}' does not exist in settings.", key));
                ret = values[key] = def;
                changed = true;
            }
            return ret;
        }

        public T GetValue<T>(string key, string def)
        {
            string ret;
            if(!values.TryGetValue(key, out ret))
            {
                if(def == null) throw new ArgumentException(string.Format("'{0}' does not exist in settings.", key));
                ret = values[key] = def;
                changed = true;
            }
            return (T) Convert.ChangeType(ret, typeof(T));
        }

        public void SetValue(string key, string val)
        {
            if(!values.ContainsKey(key) || values[key] != val)
            {
                values[key] = val;
                changed = true;
            }
        }

        public void Save()
        {
            if(!changed) return;
            var cfgFile = Path.Combine(KoiInfo.KoiDirectory, "koi.cfg");
            using(var writer = new StreamWriter(File.Open(cfgFile, FileMode.Create, FileAccess.Write)))
            {
                foreach(var entry in values)
                    writer.WriteLine("{0}:{1}", entry.Key, entry.Value ?? "null");
                writer.Flush();
            }
            changed = false;
        }
    }
}