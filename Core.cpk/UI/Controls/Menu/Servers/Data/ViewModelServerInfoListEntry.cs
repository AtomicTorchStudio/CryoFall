namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelServerInfoListEntry : BaseViewModel
    {
        private BaseCommand commandEdit;

        private BaseCommand commandRemove;

        public ViewModelServerInfoListEntry(ViewModelServerInfo viewModelServerInfo)
            : base(isAutoDisposeFields: false)
        {
            this.ViewModelServerInfo = viewModelServerInfo;
        }

#if !GAME

        public ViewModelServerInfoListEntry() : this(new ViewModelServerInfo())
        {
        }

#endif

        public BaseCommand CommandEdit
        {
            get => this.commandEdit;
            set
            {
                if (this.commandEdit == value)
                {
                    return;
                }

                this.commandEdit = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.EditButtonVisibility));
            }
        }

        public BaseCommand CommandRemove
        {
            get => this.commandRemove;
            set
            {
                if (this.commandRemove == value)
                {
                    return;
                }

                this.commandRemove = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.RemoveButtonVisibility));
            }
        }

        public Visibility EditButtonVisibility => IsDesignTime || this.CommandEdit != null
                                                      ? Visibility.Visible
                                                      : Visibility.Collapsed;

        public Visibility RemoveButtonVisibility => IsDesignTime || this.CommandRemove != null
                                                        ? Visibility.Visible
                                                        : Visibility.Collapsed;

        public ViewModelServerInfo ViewModelServerInfo { get; }
    }
}