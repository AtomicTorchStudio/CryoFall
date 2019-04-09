namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Animation;

    public class AnimationHelper
    {
        public static Storyboard CreateCanvasMoveStoryboard(
            DependencyObject element,
            double durationSeconds,
            double fromX,
            double toX,
            double fromY,
            double toY,
            EasingMode mode,
            Action onCompleted = null)
        {
            var storyboard = new Storyboard();

            var children = storyboard.Children;

            if (Math.Abs(fromX - toX) > double.Epsilon)
            {
                var anim1 = new DoubleAnimation();
                anim1.From = fromX;
                anim1.To = toX;
                anim1.Duration = new Duration(TimeSpan.FromSeconds(durationSeconds));
                anim1.EasingFunction = new ExponentialEase() { EasingMode = mode };
                Storyboard.SetTarget(anim1, element);
                Storyboard.SetTargetProperty(anim1, new PropertyPath("(Canvas.Left)"));
                children.Add(anim1);
            }

            if (Math.Abs(fromY - toY) > double.Epsilon)
            {
                var anim2 = new DoubleAnimation();
                anim2 = new DoubleAnimation();
                anim2.From = fromY;
                anim2.To = toY;
                anim2.Duration = new Duration(TimeSpan.FromSeconds(durationSeconds));
                anim2.EasingFunction = new ExponentialEase() { EasingMode = mode };
                Storyboard.SetTarget(anim2, element);
                Storyboard.SetTargetProperty(anim2, new PropertyPath("(Canvas.Top)"));
                children.Add(anim2);
            }

            if (onCompleted != null)
            {
                storyboard.Completed += delegate { onCompleted(); };
            }

            return storyboard;
        }

        /// <summary>
        /// Please be sure the return Storyboard is written into field, otherwise the onCompleted action will not be called
        /// </summary>
        public static Storyboard CreateScaleStoryboard(
            ScaleTransform element,
            double durationSeconds,
            double from,
            double to,
            Action onCompleted = null)
        {
            var storyboard = new Storyboard();

            var properties = new List<string>(2)
            {
                ScaleTransform.ScaleXProperty.Name,
                ScaleTransform.ScaleYProperty.Name
            };

            foreach (var property in properties)
            {
                var anim = new DoubleAnimation();
                anim.From = from;
                anim.To = to;
                anim.Duration = new Duration(TimeSpan.FromSeconds(durationSeconds));
                Storyboard.SetTarget(anim, element);
                Storyboard.SetTargetProperty(anim, new PropertyPath(property));
                storyboard.Children.Add(anim);
            }

            if (onCompleted != null)
            {
                storyboard.Completed += delegate { onCompleted(); };
            }

            return storyboard;
        }

        public static Storyboard CreateStoryboard(
            DependencyObject element,
            double durationSeconds,
            double from,
            double to,
            string propertyName,
            Action onCompleted = null)
        {
            var anim = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(durationSeconds))
            };

            Storyboard.SetTarget(anim, element);
            Storyboard.SetTargetProperty(anim, new PropertyPath(propertyName));
            var storyboard = new Storyboard();
            storyboard.Children.Add(anim);

            if (onCompleted != null)
            {
                storyboard.Completed += delegate { onCompleted(); };
            }

            return storyboard;
        }

        public static Storyboard CreateTransformMoveStoryboard(
            DependencyObject element,
            double durationSeconds,
            double fromX,
            double toX,
            double fromY,
            double toY,
            Action onCompleted = null)
        {
            var storyboard = new Storyboard();
            DoubleAnimation anim;

            var children = storyboard.Children;

            if (Math.Abs(fromX - toX) > double.Epsilon)
            {
                anim = new DoubleAnimation();
                anim.From = fromX;
                anim.To = toX;
                anim.Duration = new Duration(TimeSpan.FromSeconds(durationSeconds));
                Storyboard.SetTarget(anim, element);
                Storyboard.SetTargetProperty(anim, new PropertyPath(TranslateTransform.XProperty.Name));
                children.Add(anim);
            }

            if (Math.Abs(fromY - toY) > double.Epsilon)
            {
                anim = new DoubleAnimation();
                anim.From = fromY;
                anim.To = toY;
                anim.Duration = new Duration(TimeSpan.FromSeconds(durationSeconds));
                Storyboard.SetTarget(anim, element);
                Storyboard.SetTargetProperty(anim, new PropertyPath(TranslateTransform.YProperty.Name));
                children.Add(anim);
            }

            if (onCompleted != null)
            {
                storyboard.Completed += delegate { onCompleted(); };
            }

            return storyboard;
        }
    }
}