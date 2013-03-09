using System;

namespace Play.Studio.Core.Command
{
    public class CommandCollectionEventArgs : EventArgs
    {
        private ICommand _item;

        public CommandCollectionEventArgs(ICommand item)
        {
            _item = item;
        }

        public ICommand Item
        {
            get 
            {
                return _item;
            }
        }
    }
}