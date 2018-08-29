#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VerAttr = System.Reflection.AssemblyInformationalVersionAttribute;

#endregion

namespace KoiVM.Confuser.Processor
{
    public partial class Main : Form
    {
#if DEBUG || __TRACE
        private const string LIST_FILE = "users.DEBUG.lst";
        private const string CONFIG = "Debug";
#else
		const string LIST_FILE = "users.RELEASE.lst";
		const string CONFIG = "Release";
#endif

        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();
        private UserList userList;
        private readonly Font monoFont = new Font("Consolas", 8);

        public Main()
        {
            InitializeComponent();
            var ver = (VerAttr) GetType().Assembly.GetCustomAttributes(typeof(VerAttr), false)[0];
            Text += string.Format(" - [{0}]", ver.InformationalVersion);

            var dir = Environment.CurrentDirectory;
            while(!File.Exists(Path.Combine(dir, LIST_FILE)))
                dir = Path.GetDirectoryName(dir);


            txtUsrList.Text = Path.Combine(dir, LIST_FILE);
        }

        private void Main_Load(object sender, EventArgs e)
        {
            btnLoad_Click(sender, EventArgs.Empty);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                userList = new UserList(txtUsrList.Text);

                string binPath, pubPath;
                binPath = Path.Combine(Path.GetDirectoryName(userList.UserListFile), CONFIG + "\\bin\\");
                pubPath = Path.Combine(Path.GetDirectoryName(userList.UserListFile), CONFIG + "\\pub\\");
                if(!Directory.Exists(pubPath))
                    Directory.CreateDirectory(pubPath);

                txtBinPath.Text = binPath;
                txtPubPath.Text = pubPath;

                RefreshList();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void RefreshList()
        {
            lstUsers.Items.Clear();
            foreach(var user in userList)
            {
                var verPath = Path.Combine(Path.Combine(txtPubPath.Text, user.GetKoiId()), "VERSION");
                string ver;
                if(File.Exists(verPath))
                {
                    var data = File.ReadAllBytes(verPath);
                    var rc4 = new RC4(Encoding.UTF8.GetBytes(user.GetKoiId()));
                    rc4.Crypt(data, 0, data.Length);
                    ver = Encoding.UTF8.GetString(data);
                }
                else
                {
                    ver = "<< Missing >>";
                }

                var item = new ListViewItem();
                item.Tag = user;
                item.UseItemStyleForSubItems = false;
                item.Text = user.Watermark.ToString("x8");
                item.SubItems.Add(user.ID.ToString("x8"));
                item.SubItems.Add(user.SubscriptionEnd.ToShortDateString());
                item.SubItems.Add(user.Status.ToString());
                item.SubItems.Add(ver);
                item.SubItems.Add(user.UserName);
                item.SubItems.Add(user.Email);
                item.SubItems.Add("");
                foreach(ListViewItem.ListViewSubItem subItem in item.SubItems)
                    subItem.Font = monoFont;

                lstUsers.Items.Add(item);
            }
            txtDep.Text = userList.DeployDir;
        }

        private void btnNewUsr_Click(object sender, EventArgs e)
        {
            var data = new byte[6];
            var newUser = new User();

            do
            {
                rng.GetNonZeroBytes(data);
                newUser.Watermark = BitConverter.ToUInt32(data, 0);
            } while(userList.Any(usr => usr.Watermark == newUser.Watermark));

            do
            {
                rng.GetNonZeroBytes(data);
                newUser.ID = BitConverter.ToUInt32(data, 0);
            } while(userList.Any(usr => usr.ID == newUser.ID));

            do
            {
                rng.GetNonZeroBytes(data);
                newUser.LongID = BitConverter.ToString(data).ToLowerInvariant().Replace("-", "");
            } while(userList.Any(usr => usr.LongID == newUser.LongID));

            newUser.SubscriptionEnd = DateTime.Now;
            newUser.Status = Status.Inactive;

            if(new EditUser(newUser).ShowDialog() == DialogResult.OK)
            {
                userList.Add(newUser);
                RefreshList();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            userList.Save();
            MessageBox.Show("Done saving!");
        }

        private void lstUsers_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                var item = lstUsers.HitTest(e.Location).Item;
                if(item != null)
                {
                    lstUsers.SelectedItems.Clear();
                    item.Selected = true;
                    ctxMenu.Show(Cursor.Position);
                }
            }
        }

        private void itemCopy_Click(object sender, EventArgs e)
        {
            var user = (User) lstUsers.SelectedItems[0].Tag;
            Clipboard.SetText(user.GetKoiId());
        }

        private void itemEdit_Click(object sender, EventArgs e)
        {
            var user = (User) lstUsers.SelectedItems[0].Tag;
            var copy = user.Clone();
            if(new EditUser(copy).ShowDialog() == DialogResult.OK)
            {
                userList[userList.IndexOf(user)] = copy;
                RefreshList();
            }
        }

        private void itemDel_Click(object sender, EventArgs e)
        {
            var user = (User) lstUsers.SelectedItems[0].Tag;
            userList.Remove(user);
            RefreshList();
        }

        private void Interactive(bool value)
        {
            if(InvokeRequired)
            {
                BeginInvoke(new Action<bool>(Interactive), value);
                return;
            }

            txtUsrList.ReadOnly = !value;
            btnLoad.Enabled = value;
            btnSave.Enabled = value;

            ctxMenu.Enabled = value;

            btnNewUsr.Enabled = value;
            btnGenAll.Enabled = value;
            btnStubGen.Enabled = value;
            btnClean.Enabled = value;
            btnDeploy.Enabled = value;
        }

        private void btnStubGen_Click(object sender, EventArgs e)
        {
            var logger = new StubLogger(lblStubStatus);

            string binPath = txtBinPath.Text, pubPath = txtPubPath.Text;
            Interactive(false);

            Task.Factory.StartNew(() =>
            {
                var path = Path.Combine(binPath, "KoiVM.Confuser.Internal.dll");
                MessageBox.Show(Path.Combine(binPath, "KoiVM.Confuser.Internal.dll"));
                if (File.Exists(path))
                    File.Delete(path);

                foreach(var file in Directory.GetFiles(pubPath))
                    File.Delete(file);

                var internalModule = StubProcessor.Process(binPath, pubPath, logger);
                File.WriteAllBytes(@"C:\Users\Nybher\Desktop\koiVM\Debug\bin\koi.pack", internalModule);

                logger.Info("Done.");
            }).ContinueWith(t => Interactive(true));
        }

        private int generateCount;

        private Task GenerateUser(string binPath, string pubPath, byte[] internalModule, ListViewItem item)
        {
            var user = (User) item.Tag;
            var logger = new UserLogger(lstUsers, item.SubItems[item.SubItems.Count - 1]);

            Interactive(false);
            Interlocked.Increment(ref generateCount);

            return Task.Factory.StartNew(() =>
            {
                new UserProcessor(user, internalModule, logger).Process(binPath, pubPath);
                logger.Info("Done.");
            }).ContinueWith(t =>
            {
                if(Interlocked.Decrement(ref generateCount) == 0)
                    Interactive(true);
            });
        }

        private void itemGen_Click(object sender, EventArgs e)
        {
            string binPath = txtBinPath.Text, pubPath = txtPubPath.Text;

            var internalModulePath = Path.Combine(@"C:\Users\Nybher\Desktop\koiVM\Debug\bin\", "KoiVM.Confuser.exe");
            if(!File.Exists(internalModulePath))
            {
                MessageBox.Show("Internal module does not exist!");
                return;
            }
            var internalModule = File.ReadAllBytes(internalModulePath);

            GenerateUser(binPath, pubPath, internalModule, lstUsers.SelectedItems[0]);
        }

        private void btnGenAll_Click(object sender, EventArgs e)
        {
            string binPath = /*txtBinPath.Text;*/@"C:\Users\Nybher\Desktop\koiVM\Debug\bin\";

            string pubPath = @"C:\Users\Nybher\Desktop\koiVM\Debug\bin\pub\";

            var internalModulePath = Path.Combine(binPath, "KoiVM.Confuser.Internal.dll");
            MessageBox.Show(Path.Combine(binPath, "KoiVM.Confuser.Internal.dll"));
            if(!File.Exists(internalModulePath))
            {
                MessageBox.Show("Internal module does not exist!");
                return;
            }
            var internalModule = File.ReadAllBytes(internalModulePath);

            var queue = new Queue<ListViewItem>();
            foreach(ListViewItem item in lstUsers.Items)
                queue.Enqueue(item);

            Action<Task> cont = null;
            cont = t =>
            {
                lock(queue)
                {
                    if(queue.Count > 0)
                        GenerateUser(binPath, pubPath, internalModule, queue.Dequeue()).ContinueWith(cont);
                }
            };
            lock(queue)
            {
                for(var i = 0; i < Math.Min(4, queue.Count); i++)
                    GenerateUser(binPath, pubPath, internalModule, queue.Dequeue()).ContinueWith(cont);
            }

            MessageBox.Show("wee");
        }

        private static void CopyTo(DirectoryInfo source, DirectoryInfo target)
        {
            foreach(var dir in target.GetDirectories())
                dir.Delete(true);
            foreach(var file in target.GetFiles())
                file.Delete();

            foreach(var dir in source.GetDirectories())
                CopyTo(dir, target.CreateSubdirectory(dir.Name));
            foreach(var file in source.GetFiles())
            {
                if(file.Extension == ".map" || file.Extension == ".pdb")
                    continue;
                if((file.Extension == ".cfg" || file.Extension == ".pack") &&
                   source.GetDirectories().Any())
                    continue;
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure?", "Koi", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                if(Directory.Exists(txtPubPath.Text))
                {
                    Directory.Delete(txtPubPath.Text, true);
                    while(Directory.Exists(txtPubPath.Text))
                        Thread.Sleep(10);
                }
                Directory.CreateDirectory(txtPubPath.Text);
            }
        }

        private void btnDeploy_Click(object sender, EventArgs e)
        {
            if(!Directory.Exists(userList.DeployDir))

                Directory.CreateDirectory(userList.DeployDir);

            CopyTo(new DirectoryInfo(txtPubPath.Text), new DirectoryInfo(userList.DeployDir));

            MessageBox.Show("Deployed!");
        }

        private void txtUsrList_TextChanged(object sender, EventArgs e)
        {

        }
    }
}