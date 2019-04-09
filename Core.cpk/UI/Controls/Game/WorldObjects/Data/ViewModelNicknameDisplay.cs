namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelNicknameDisplay : BaseViewModel
    {
        private static readonly Brush BrushPartyMember
            = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFA, 0x33));

        private static readonly Brush BrushStranger
            = new SolidColorBrush(Color.FromArgb(0xFF, 0xF1, 0xF1, 0xF1));

        private readonly bool isOnline;

        private bool isPartyMember;

        public ViewModelNicknameDisplay(string name, bool isOnline)
        {
            this.Name = name;
            this.isOnline = isOnline;
        }

        public Brush Brush => this.isPartyMember
                                  ? BrushPartyMember
                                  : BrushStranger;

        public bool IsPartyMember
        {
            get => this.isPartyMember;
            set
            {
                if (this.isPartyMember == value)
                {
                    return;
                }

                this.isPartyMember = value;
                this.NotifyThisPropertyChanged();

                this.NotifyPropertyChanged(nameof(this.Brush));
            }
        }

        public string Name { get; }

        public string Text
        {
            get
            {
                if (string.IsNullOrEmpty(this.Name))
                {
                    return "Nickname";
                }

                var result = this.Name;
                if (DevelopersListHelper.IsDeveloper(this.Name))
                {
                    result = "[Developer]\n" + result;
                }

                if (!this.isOnline)
                {
                    result = "Offline Zzz...\n" + result;
                }

                return result;
            }
        }
    }
}