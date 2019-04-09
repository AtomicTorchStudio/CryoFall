namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Special class for caching UI controls.
    /// </summary>
    /// <typeparam name="TControl">Type of control - must implement <see cref="ICacheableControl" />.</typeparam>
    public class ControlsCache<TControl> : IDisposable
        where TControl : FrameworkElement, ICacheableControl, new()
    {
        private const bool IsLoggingEnabled = false;

        private static readonly Dictionary<object, ControlsCache<TControl>> SpecialInstancesCache =
            new Dictionary<object, ControlsCache<TControl>>();

        private static ControlsCache<TControl> instance;

        private readonly Stack<TControl> controls;

        private bool isDisposed;

        private ControlsCache(int defaultCount = 16)
        {
            this.controls = new Stack<TControl>(defaultCount);
            Api.OnShutdown += this.ApiOnShutdownHandler;
        }

        public static ControlsCache<TControl> Instance
            => instance
               ?? (instance = new ControlsCache<TControl>());

        public static ControlsCache<TControl> InstanceFor(object key)
        {
            if (!SpecialInstancesCache.TryGetValue(key, out var foundInstance))
            {
                SpecialInstancesCache[key] = foundInstance = new ControlsCache<TControl>();
            }

            return foundInstance;
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;

            if (this.controls.Count > 0)
            {
                // there is no actual need to dispose them explicitly on scripts shutdown
                //foreach (var control in this.controls.ToList())
                //{
                //    control.Dispose();
                //}

                this.controls.Clear();
            }

            if (instance == this)
            {
                // remove from static default instance
                instance = null;
            }
            else
            {
                // remove from special instances cache
                foreach (var pair in SpecialInstancesCache)
                {
                    if (pair.Value == this)
                    {
                        SpecialInstancesCache.Remove(pair.Key);
                        break;
                    }
                }
            }
        }

        public TControl Pop()
        {
            if (this.controls.Count > 0)
            {
                var control = this.controls.Pop();
                if (IsLoggingEnabled)
                {
                    Api.Logger.Important(
                        $"{this.GetType()}: popped control. Total available controls count: {this.controls.Count}");
                }

                return control;
            }

            if (IsLoggingEnabled)
            {
                Api.Logger.Important(
                    $"{this.GetType()}: new control created. Total available controls count: {this.controls.Count}");
            }

            return new TControl();
        }

        public void Push(TControl control)
        {
            if (control.Parent != null
                || control.TemplatedParent != null)
            {
                Api.Logger.Error(
                    "Cannot push control "
                    + control
                    + " to ControlsCache - control parent not null! Please remove it from the parent children collection before pushing it to ControlsCache");
                return;
            }

            control.ResetControlForCache();
            this.controls.Push(control);

            if (IsLoggingEnabled)
            {
                Api.Logger.Important(
                    $"{this.GetType()}: pushed control. Total available controls count: {this.controls.Count}");
            }
        }

        private void ApiOnShutdownHandler()
        {
            Api.OnShutdown -= this.ApiOnShutdownHandler;
            this.Dispose();
        }
    }
}