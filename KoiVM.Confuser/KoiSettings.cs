namespace KoiVM.Confuser
{
    internal class KoiSettings : SimpleSettings
    {
        public bool NoUI
        {
            get
            {
                bool value;
                if(bool.TryParse(GetValue("noUI", "false"), out value))
                    return value;
                SetValue("noUI", "false");
                return false;
            }
            set { SetValue("noUI", value.ToString().ToLowerInvariant()); }
        }

        public bool NoCheck
        {
            get
            {
                bool value;
                if(bool.TryParse(GetValue("noCheck", "false"), out value))
                    return value;
                SetValue("noCheck", "false");
                return false;
            }
            set { SetValue("noCheck", value.ToString().ToLowerInvariant()); }
        }

        public string Version
        {
            get
            {
                var value = GetValue("ver", "");
                return string.IsNullOrEmpty(value) ? null : value;
            }
            set { SetValue("ver", value); }
        }

        public string KoiID
        {
            get
            {
                var value = GetValue("id", "");
                return string.IsNullOrEmpty(value) ? null : value;
            }
            set { SetValue("id", value); }
        }
    }
}