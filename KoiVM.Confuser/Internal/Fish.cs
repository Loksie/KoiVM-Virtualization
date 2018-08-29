#region

using System.Reflection;

#endregion

namespace KoiVM.Confuser.Internal
{
    [Obfuscation(Exclude = false, Feature = "+koi;")]
    public static class Fish
    {
        public static readonly string UserName = "{USERNAME}";
        public static readonly string SubscriptionEnd = "{SUBSCRIPTION}";
        public static readonly string Id = "00000000";

        public static readonly object VirtualizerKey = new object();
        public static readonly object MergeKey = new object();
        public static readonly object ExportKey = new object();
    }
}