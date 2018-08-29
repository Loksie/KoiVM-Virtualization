namespace ConfuserEx.ViewModel
{
    public abstract class TabViewModel : ViewModelBase
    {
        protected TabViewModel(AppVM app, string header)
        {
            App = app;
            Header = header;
        }

        public AppVM App
        {
            get;
        }

        public string Header
        {
            get;
        }
    }
}