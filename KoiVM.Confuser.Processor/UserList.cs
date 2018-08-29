#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

#endregion

namespace KoiVM.Confuser.Processor
{
    internal class UserList : List<User>
    {
        public readonly string UserListFile;

        public string DeployDir;

        public UserList(string userList)
        {
            using(var reader = new StreamReader(File.OpenRead(userList)))
            {
                DeployDir = reader.ReadLine();

                while(!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Split(new[] {'\t'}, StringSplitOptions.RemoveEmptyEntries);
                    if(line.Length == 3)
                        continue;
                    if(line.Length != 7)
                        throw new InvalidDataException("Error at " + Count);

                    var usr = new User();
                    usr.Watermark = uint.Parse(line[0], NumberStyles.HexNumber);
                    usr.ID = uint.Parse(line[1], NumberStyles.HexNumber);
                    usr.LongID = line[2];
                    usr.SubscriptionEnd = DateTime.ParseExact(line[3], "dd/MM/yyyy", null);
                    usr.Status = (Status) Enum.Parse(typeof(Status), line[4]);
                    usr.UserName = line[5];
                    usr.Email = line[6];
                    Add(usr);
                }
            }
            UserListFile = userList;
        }

        public void Save()
        {
            using(var writer = new StreamWriter(File.Open(UserListFile, FileMode.Create)))
            {
                writer.WriteLine(DeployDir);
                foreach(var usr in this)
                    writer.WriteLine(string.Join("\t", new object[]
                    {
                        usr.Watermark.ToString("x8"),
                        usr.ID.ToString("x8"),
                        usr.LongID,
                        usr.SubscriptionEnd.ToString("dd/MM/yyyy"),
                        usr.Status.ToString(),
                        usr.UserName,
                        usr.Email
                    }));
            }
        }
    }
}