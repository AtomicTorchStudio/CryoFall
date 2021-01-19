namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelRequiredItemControl : BaseViewModel
    {
        private const string StateNameAvailable = "Available";

        private const string StateNameDefault = "Default";

        private const string StateNameUnavailable = "Unavailable";

        private ushort countMultiplier = 1;

        private bool isAvailable;

        private ProtoItemWithCount protoItemWithCount;

        public ViewModelRequiredItemControl(ProtoItemWithCount protoItemWithCount)
        {
            this.ProtoItemWithCount = protoItemWithCount;
        }

        public ViewModelRequiredItemControl()
        {
        }

        public ushort CountMultiplier => this.countMultiplier;

        public string CountString { get; private set; }

        public Visibility CountVisibility { get; set; }

        public string Description => this.protoItemWithCount?.ProtoItem.Description;

        public Brush Icon { get; private set; }

        /// <summary>
        /// Returns actual value only if <see cref="IsChecksItemAvailability" /> is enabled. Otherwise always returns false.
        /// </summary>
        public bool IsAvailable => this.isAvailable;

        public bool IsChecksItemAvailability { get; set; } = true;

        public ProtoItemWithCount ProtoItemWithCount
        {
            get => this.protoItemWithCount;
            set
            {
                if (this.protoItemWithCount == value)
                {
                    return;
                }

                this.protoItemWithCount = value;
                this.Refresh();
            }
        }

        public string Title => this.protoItemWithCount?.ProtoItem.Name;

        public string VisualStateName { get; private set; } = StateNameDefault;

        public void Refresh()
        {
            this.RefreshCount();
            this.CountVisibility = this.protoItemWithCount is not null ? Visibility.Visible : Visibility.Hidden;
            this.Icon = Client.UI.GetTextureBrush(this.protoItemWithCount?.ProtoItem.Icon);
            this.NotifyPropertyChanged(nameof(this.Title));
        }

        public void RefreshCount()
        {
            if (this.protoItemWithCount is null)
            {
                this.VisualStateName = StateNameDefault;
                this.CountString = "--";
                this.isAvailable = false;
                this.VisualStateName = StateNameDefault;
                return;
            }

            var requiredCount = this.protoItemWithCount.Count * this.CountMultiplier;

            if (!this.IsChecksItemAvailability)
            {
                // simply display count (usually this is used for displaying output items for crafting recipe)
                this.VisualStateName = StateNameDefault;
                this.CountString = requiredCount.ToString();
                this.isAvailable = false;
                this.VisualStateName = StateNameAvailable;
                return;
            }

            var availableCount = this.CalculateAvailableCount();
            this.isAvailable = availableCount >= requiredCount;
            this.VisualStateName = this.isAvailable ? StateNameAvailable : StateNameUnavailable;
            this.CountString = $"{availableCount}/{requiredCount}";
        }

        public void SetCountMultiplier(ushort setCountMultiplier, bool refreshCount)
        {
            this.countMultiplier = setCountMultiplier;
            if (refreshCount)
            {
                this.RefreshCount();
            }
        }

        private int CalculateAvailableCount()
        {
            return Client.Characters.CurrentPlayerCharacter
                         .CountItemsOfType(this.protoItemWithCount.ProtoItem);
        }
    }
}