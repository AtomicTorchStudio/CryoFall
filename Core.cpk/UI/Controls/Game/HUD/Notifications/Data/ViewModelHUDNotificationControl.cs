﻿namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications.Data
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelHudNotificationControl : BaseViewModel
    {
        private Brush icon;

        private float requiredHeight;

        public ViewModelHudNotificationControl(
            string title,
            string message,
            Brush brushBackground,
            Brush brushBorder,
            Brush iconBrush,
            Action onClick)
        {
            if (!string.IsNullOrEmpty(message))
            {
                this.Message = message;
                this.MessageVisibility = Visibility.Visible;
            }

            this.Title = title;
            this.BrushBackground = brushBackground;
            this.BrushBorder = brushBorder;

            if (iconBrush is not null)
            {
                this.Icon = iconBrush;
            }

            if (onClick is not null)
            {
                this.CommandClick = new ActionCommand(onClick);
                this.Cursor = CursorId.InteractionPossible;
            }
        }

        public Brush BrushBackground { get; }

        public Brush BrushBorder { get; }

        public ActionCommand CommandClick { get; }

        public CursorId Cursor { get; }

        public Brush Icon
        {
            get => this.icon;
            set
            {
                if (Equals(this.icon, value))
                {
                    return;
                }

                this.icon = value;
                this.NotifyThisPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.IconVisibility));
            }
        }

        public Visibility IconVisibility => this.Icon is not null
                                                ? Visibility.Visible
                                                : Visibility.Collapsed;

        public string Message { get; set; }

        public Visibility MessageVisibility { get; } = Visibility.Collapsed;

        public float RequiredHeight
        {
            get => this.requiredHeight;
            set
            {
                this.requiredHeight = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public string Title { get; set; }

        public bool IsSame(ViewModelHudNotificationControl other)
        {
            return string.Equals(this.Title,      other.Title,   StringComparison.Ordinal)
                   && string.Equals(this.Message, other.Message, StringComparison.Ordinal)
                   && this.Icon == other.Icon
                   && this.BrushBackground == other.BrushBackground
                   && this.BrushBorder == other.BrushBorder
                   && this.Cursor == other.Cursor;

            // please note - we don't compare onClick delegate - but for user's point of view it's not visible either
        }
    }
}