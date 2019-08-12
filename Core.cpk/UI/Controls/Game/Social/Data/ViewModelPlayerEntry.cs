namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelPlayerEntry : BaseViewModel, IComparable<ViewModelPlayerEntry>, IComparable
    {
        public ViewModelPlayerEntry(string name)
            : base(isAutoDisposeFields: false)
        {
            this.Name = name;
        }

        public BaseCommand CommandToggleBlock
            => new ActionCommand(() => ClientChatBlockList.SetBlockStatus(this.Name,
                                                                          block: !this.IsBlocked,
                                                                          askConfirmation: true));

        public bool IsBlocked => ClientChatBlockList.IsBlocked(this.Name);

        public string Name { get; }

        public int CompareTo(ViewModelPlayerEntry other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return 1;
            }

            if (ReferenceEquals(this, obj))
            {
                return 0;
            }

            return obj is ViewModelPlayerEntry other
                       ? this.CompareTo(other)
                       : throw new ArgumentException($"Object must be of type {nameof(ViewModelPlayerEntry)}");
        }

        public void RefreshBlockedStatus()
        {
            this.NotifyPropertyChanged(nameof(this.IsBlocked));
        }
    }
}