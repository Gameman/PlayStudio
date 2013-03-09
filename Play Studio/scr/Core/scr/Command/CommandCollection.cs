using System;
using System.Collections.ObjectModel;

namespace Play.Studio.Core.Command
{
    public class CommandCollection : Collection<ICommand>
    {
        public event EventHandler<CommandCollectionEventArgs> CommandAdded;

        public event EventHandler<CommandCollectionEventArgs> CommandRemoved;

        protected override void ClearItems()
        {
            for (int i = 0; i < base.Count; i++) {
                this.OnCommandRemoved(new CommandCollectionEventArgs(base[i]));
                this.GetType();
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, ICommand item)
        {
            if (base.IndexOf(item) != -1) {
                throw new ArgumentException("Cannot Add Same Component Multiple Times");
            }
            base.InsertItem(index, item);
            if (item != null) {
                this.OnCommandAdded(new CommandCollectionEventArgs(item));
            }
        }

        private void OnCommandAdded(CommandCollectionEventArgs eventArgs)
        {
            if (this.CommandAdded != null) {
                this.CommandAdded(this, eventArgs);
            }
        }

        private void OnCommandRemoved(CommandCollectionEventArgs eventArgs)
        {
            if (this.CommandRemoved != null) {
                this.CommandRemoved(this, eventArgs);
            }
        }

        protected override void RemoveItem(int index)
        {
            ICommand item = base[index];
            base.RemoveItem(index);
            if (item != null) {
                this.OnCommandRemoved(new CommandCollectionEventArgs(item));
            }
        }

        protected override void SetItem(int index, ICommand item)
        {
            throw new NotSupportedException();
        }
    }
}
