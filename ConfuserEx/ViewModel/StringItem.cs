namespace ConfuserEx.ViewModel
{
    public class StringItem : IViewModel<string>
    {
        public StringItem(string item)
        {
            Item = item;
        }

        public string Item
        {
            get;
        }

        string IViewModel<string>.Model => Item;

        public override string ToString()
        {
            return Item;
        }
    }
}