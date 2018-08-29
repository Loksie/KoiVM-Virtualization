#region

using System;
using System.IO;
using System.Net;
using System.Text;

#endregion

namespace KoiVM.Confuser
{
    internal class KoiSystem
    {
        public event Action<bool> Finish;
        public event Action<double> Progress;

        private uint Hash(string value)
        {
            uint hash = 0;
            for(var i = 0; i < value.Length; i++)
                hash = value[i] ^ (hash * 33);
            return hash;
        }

        public string GetVersion(string koiId)
        {
            try
            {
#if DEBUG || __TRACE
                const string VER_URI = @"http://ki.no-ip.info/koi/{0}/VERSION";
#else
				const string VER_URI = @"https://ki-host.appspot.com/KoiVM/koi/{0}/VERSION";
#endif
                var verUri = new Uri(string.Format(VER_URI, koiId));
                var client = new WebClient();
                var data = client.DownloadData(verUri);

                var rc4 = new RC4(Encoding.UTF8.GetBytes(koiId));
                rc4.Crypt(data, 0, data.Length);
                var ver = Encoding.UTF8.GetString(data);

                if(Hash(ver) == 0x4c3e88e4) // REVOKED
                    File.Delete(Path.Combine(KoiInfo.KoiDirectory, "koi.pack"));

                return ver;
            }
            catch
            {
                return null;
            }
        }

        public void Login(string koiId)
        {
            try
            {
#if DEBUG || __TRACE
                const string PACK_URI = @"http://ki.no-ip.info/koi/{0}/koi.pack";
#else
				const string PACK_URI = @"https://ki-host.appspot.com/KoiVM/koi/{0}/koi.pack";
#endif
                var pack = new Uri(string.Format(PACK_URI, koiId));
                var client = new WebClient();
                var file = Path.Combine(KoiInfo.KoiDirectory, "koi.pack");

                client.DownloadProgressChanged += (sender, e) =>
                {
                    var progressValue = (double) e.BytesReceived / e.TotalBytesToReceive;
                    if(e.TotalBytesToReceive == -1)
                        progressValue = 0;
                    Progress(progressValue);
                };

                client.DownloadFileCompleted += (sender, e) =>
                {
                    if(e.Error != null)
                        if(File.Exists(file))
                            File.Delete(file);
                    Finish(e.Error == null);
                };

                if(File.Exists(file))
                    File.Delete(file);
                client.DownloadFileAsync(pack, file);
            }
            catch
            {
                Finish(false);
            }
        }
    }
}