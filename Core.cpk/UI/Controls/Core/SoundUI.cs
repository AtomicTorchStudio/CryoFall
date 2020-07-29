namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    // Bug - doesn't work. Reported http://bugs.noesisengine.com/view.php?id=1020
    //[TypeConverter(typeof(SoundUITypeConverter))]
    public class SoundUI : IEquatable<SoundUI>
    {
        public static readonly DependencyProperty ClickSoundProperty = DependencyProperty.RegisterAttached(
            "ClickSound",
            propertyType: typeof(SoundUI),
            ownerType: typeof(SoundUI),
            defaultMetadata: new PropertyMetadata(default(SoundUI), ClickSoundPropertyChanged));

        public static readonly DependencyProperty EnterSoundProperty = DependencyProperty.RegisterAttached(
            "EnterSound",
            propertyType: typeof(SoundUI),
            ownerType: typeof(SoundUI),
            defaultMetadata: new PropertyMetadata(default(SoundUI), MouseEnterSoundPropertyChanged));

        public static readonly SoundUI NoSound = new SoundUI() { Path = null };

        public string Path { get; set; }

        public static SoundUI GetClickSound(DependencyObject element)
        {
            return (SoundUI)element.GetValue(ClickSoundProperty);
        }

        public static SoundUI GetEnterSound(DependencyObject element)
        {
            return (SoundUI)element.GetValue(EnterSoundProperty);
        }

        public static bool operator ==(SoundUI left, SoundUI right)
        {
            return (left ?? NoSound).Equals(right);
        }

        public static bool operator !=(SoundUI left, SoundUI right)
        {
            return !(left ?? NoSound).Equals(right);
        }

        public static void PlaySound(SoundUI soundUI)
        {
            if (soundUI == NoSound)
            {
                return;
            }

            Api.Client.Audio.PlayOneShot(
                new SoundResource(soundUI.Path));
        }

        public static void SetClickSound(DependencyObject element, SoundUI value)
        {
            element.SetValue(ClickSoundProperty, value);
        }

        public static void SetEnterSound(DependencyObject element, SoundUI value)
        {
            element.SetValue(EnterSoundProperty, value);
        }

        public bool Equals(SoundUI other)
        {
            return string.Equals(this.Path, other?.Path, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                if (this.Path == null)
                {
                    // no sound
                    return true;
                }

                return false;
            }

            var sound = obj as SoundUI;
            return sound != null && this.Equals(sound);
        }

        public override int GetHashCode()
        {
            return this.Path?.GetHashCode() ?? 0;
        }

        private static void ClickSoundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((SoundUI)e.NewValue == NoSound)
            {
                return;
            }

            // TODO: we might subscribe more than one time if the property is changed again
            if (d is ButtonBase button)
            {
                button.Click += ControlMouseClickHandler;
                return;
            }

            var fe = (FrameworkElement)d;
            fe.PreviewMouseLeftButtonUp += ControlMouseLeftButtonUpHandler;
        }

        private static void ControlMouseClickHandler(object sender, RoutedEventArgs e)
        {
            var clickSound = GetClickSound((DependencyObject)sender);
            PlaySound(clickSound);
        }

        private static void ControlMouseEnterHandler(object sender, MouseEventArgs e)
        {
            var clickSound = GetEnterSound((DependencyObject)sender);
            PlaySound(clickSound);
        }

        private static void ControlMouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            var clickSound = GetClickSound((DependencyObject)sender);
            PlaySound(clickSound);
        }

        private static void MouseEnterSoundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = (FrameworkElement)d;
            if ((SoundUI)e.NewValue == NoSound)
            {
                return;
            }

            // TODO: this might cause a memory leak
            fe.MouseEnter += ControlMouseEnterHandler;
        }
    }
}