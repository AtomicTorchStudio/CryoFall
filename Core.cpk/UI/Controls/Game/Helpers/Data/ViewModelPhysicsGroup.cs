namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Helpers.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelPhysicsGroup : BaseViewModel
    {
        private bool isEnabled;

        public ViewModelPhysicsGroup(
            string title,
            CollisionGroupId collisionGroupId,
            bool isEnabled,
            Brush foregroundBrush)
        {
            this.Title = title;
            this.CollisionGroupId = collisionGroupId;
            this.isEnabled = isEnabled;
            this.ForegroundBrush = foregroundBrush;
        }

        public event Action<ViewModelPhysicsGroup> IsEnabledChanged;

        public CollisionGroupId CollisionGroupId { get; }

        public Brush ForegroundBrush { get; }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;
                this.NotifyThisPropertyChanged();
                this.IsEnabledChanged?.Invoke(this);
            }
        }

        public string Title { get; }
    }
}