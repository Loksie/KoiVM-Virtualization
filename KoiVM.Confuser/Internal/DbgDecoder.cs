#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

#endregion

namespace KoiVM.Confuser.Internal
{
    [Obfuscation(Exclude = false, Feature = "+koi")]
    public partial class DbgDecoder : Form
    {
        public DbgDecoder()
        {
            InitializeComponent();
        }

        private static uint modInv(uint num)
        {
            ulong a = 1UL << 32, b = num % a;
            ulong p0 = 0, p1 = 1;
            while(b != 0)
            {
                if(b == 1) return (uint) p1;
                p0 += a / b * p1;
                a = a % b;

                if(a == 0) break;
                if(a == 1) return (uint) ((1UL << 32) - p0);

                p1 += b / a * p0;
                b = b % a;
            }
            return 0;
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            var mapFile = txtMap.Text;
            if(string.IsNullOrEmpty(txtMap.Text))
            {
                MessageBox.Show("Select a Debug Map file.", "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var mapData = File.ReadAllBytes(mapFile);

            ulong token;
            if(!ulong.TryParse(txtToken.Text, NumberStyles.HexNumber, null, out token))
            {
                MessageBox.Show("Enter a valid Debug Token.", "Koi System", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var stream = new MemoryStream(mapData);
            var aes = new AesManaged();
            aes.IV = aes.Key = Convert.FromBase64String("UkVwAyrARLAy4GmQLL860w==");
            var reader = new BinaryReader(
                new DeflateStream(
                    new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read),
                    CompressionMode.Decompress
                )
            );

            var key = modInv((uint) token | 1);

            var documents = new List<string>();
            var count = reader.ReadUInt32();
            for(uint i = 0; i < count; i++)
                documents.Add(reader.ReadString());

            string dbg;
            try
            {
                var ip = (uint) ((token >> 32) * key);
                while(true)
                {
                    var offset = reader.ReadUInt32();
                    var len = reader.ReadUInt32();
                    var docId = reader.ReadUInt32();
                    var lineNo = reader.ReadUInt32();

                    if(ip >= offset && ip <= offset + len)
                    {
                        dbg = string.Format("Line {0} at {1}", lineNo, documents[(int) docId]);
                        break;
                    }
                }
            }
            catch
            {
                dbg = "Debug info not found.";
            }
            txtInfo.Text = dbg;
        }

        private void txtMap_DragOver(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
        }

        private void txtMap_DragDrop(object sender, DragEventArgs e)
        {
            txtMap.Text = ((string[]) e.Data.GetData(DataFormats.FileDrop))[0];
        }
    }
}