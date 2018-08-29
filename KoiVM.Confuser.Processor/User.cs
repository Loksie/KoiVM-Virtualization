#region

using System;

#endregion

namespace KoiVM.Confuser.Processor
{
    internal enum Status
    {
        Valid,
        Inactive,
        Revoked
    }

    internal class User
    {
        public string Email;
        public uint ID;
        public string LongID;
        public Status Status;
        public DateTime SubscriptionEnd;
        public string UserName;
        public uint Watermark;

        public User Clone()
        {
            return (User) MemberwiseClone();
        }

        public string GetKoiId()
        {
            return string.Format("{0}_{1:x8}", UserName, ID);
        }
    }
}