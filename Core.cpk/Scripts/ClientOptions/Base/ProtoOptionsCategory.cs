namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    /// <summary>
    /// Base class for all the options categories.
    /// </summary>
    public abstract class ProtoOptionsCategory : ProtoEntity
    {
        private readonly IClientStorage clientStorage;

        private readonly List<IProtoOption> options = new();

        protected ProtoOptionsCategory()
        {
            var name = this.GetType().Name;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            name = name.Replace("OptionsCategory", string.Empty);

            this.ShortId = name;

            if (IsClient)
            {
                this.Icon = new TextureResource($"Options/{name}.png", qualityOffset: -100);
                this.OptionsStorageLocalFilePath = "Options/" + this.ShortId;
                this.clientStorage = Api.Client.Storage
                                        .GetStorage(this.OptionsStorageLocalFilePath);
            }
        }

        public event Action<ProtoOptionsCategory> Modified;

        public TextureResource Icon { get; }

        public virtual bool IsModified => this.Options.Any(o => o.IsModified);

        public IReadOnlyList<IProtoOption> Options => this.options;

        public virtual ProtoOptionsCategory OrderAfterCategory { get; }

        public override string ShortId { get; }

        protected string OptionsStorageLocalFilePath { get; }

        public void ApplyAndSave()
        {
            this.ProcessOptions(o => o.Apply());
            this.OnApply();

            this.SaveOptionsToStorage();

            this.NotifyModified();
        }

        public void Cancel()
        {
            if (!this.IsModified)
            {
                return;
            }

            this.OnCancel();
            this.ProcessOptions(o => o.Cancel());

            this.NotifyModified();
        }

        public virtual UIElement CreateControl()
        {
            var options = this.Options.Where(o => !o.IsHidden).ToList();
            if (options.Count <= 0)
            {
                return new TextBlock()
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    Text = "This category has no options",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }

            var tableControl = new TableControl()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            tableControl.Loaded
                += (_, e) =>
                   {
                       if (!tableControl.IsEmpty)
                       {
                           return;
                       }

                       // populate options
                       tableControl.Clear();

                       // local helper method for getting option order
                       IEnumerable<IProtoOption> GetOptionOrder(IProtoOption tab)
                       {
                           if (tab.OrderAfterOption is not null)
                           {
                               yield return tab.OrderAfterOption;
                           }
                       }

                       foreach (var option in options.OrderBy(o => o.ShortId)
                                                     .TopologicalSort(GetOptionOrder)
                                                     .Where(o => !o.IsHidden))
                       {
                           option.CreateControl(out var labelControl, out var optionControl);
                           optionControl.Focusable = false;

                           if (!string.IsNullOrEmpty(option.Description))
                           {
                               // wrap into a stackpanel with an info tooltip control
                               var controlInfoPoint = new Control
                               {
                                   Style = Api.Client.UI.GetApplicationResource<Style>(
                                       "ControlInfoQuestionPointStyle"),
                                   VerticalAlignment = VerticalAlignment.Center,
                                   Margin = new Thickness(0, 6, 0, -6)
                               };

                               var stackPanel = new StackPanel
                               {
                                   Orientation = Orientation.Horizontal,
                                   Background = Brushes.Transparent
                               };

                               stackPanel.Children.Add(optionControl);
                               stackPanel.Children.Add(controlInfoPoint);
                               optionControl = stackPanel;

                               var tooltipContent = new FormattedTextBlock()
                               {
                                   Content = option.Description,
                                   MaxWidth = 300
                               };

                               ToolTipServiceExtend.SetToolTip(stackPanel,   tooltipContent);
                               ToolTipServiceExtend.SetToolTip(labelControl, tooltipContent);
                           }

                           tableControl.Add(labelControl, optionControl);
                       }
                   };

            return new ScrollViewer()
            {
                Content = tableControl,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible
            };
        }

        public void LoadOptionsFromStorage()
        {
            if (this.options.Count == 0)
            {
                this.OnLoadedFromStorage();
                return;
            }

            if (!this.clientStorage.TryLoad<Dictionary<string, object>>(out var snapshot))
            {
                Logger.Important(
                    $"There are no options snapshot for {this.ShortId} or it cannot be deserialized - applying default values");
                this.Reset();
                return;
            }

            var optionsToProcess = this.options.ToList();
            foreach (var pair in snapshot)
            {
                for (var index = 0; index < optionsToProcess.Count; index++)
                {
                    var option = optionsToProcess[index];
                    if (option.Id != pair.Key)
                    {
                        continue;
                    }

                    // found a value of option
                    option.ApplyAbstractValue(pair.Value);
                    optionsToProcess.RemoveAt(index);
                    index--;
                }
            }

            // reset all the remaining options (don't found values from them in snapshot)
            foreach (var option in optionsToProcess)
            {
                option.Reset(apply: true);
            }

            this.OnLoadedFromStorage();
        }

        public void NotifyModified()
        {
            this.Modified?.Invoke(this);
        }

        public void OnOptionModified(IProtoOption protoOption)
        {
            this.NotifyModified();
        }

        public void Reset()
        {
            this.OnReset();
            this.ProcessOptions(o => o.Reset(apply: false));
            this.ApplyAndSave();
        }

        internal void RegisterOption(IProtoOption option)
        {
            this.options.Add(option);
            option.RegisterValueType(this.clientStorage);
        }

        protected abstract void OnApply();

        protected virtual void OnCancel()
        {
        }

        protected virtual void OnLoadedFromStorage()
        {
        }

        protected abstract void OnReset();

        protected override void PrepareProto()
        {
            if (IsServer)
            {
                return;
            }

            ClientOptionsManager.Initialize();
        }

        private void ProcessOptions(Action<IProtoOption> func)
        {
            foreach (var option in this.options)
            {
                func(option);
            }
        }

        private void SaveOptionsToStorage()
        {
            if (this.options.Count == 0)
            {
                return;
            }

            var snapshot = new Dictionary<string, object>();
            foreach (var option in this.options)
            {
                snapshot[option.Id] = option.GetAbstractValue();
            }

            this.clientStorage.Save(snapshot);

            //Logger.WriteDev(
            //    "Saved options tab: "
            //    + this.Id
            //    + Environment.NewLine
            //    + snapshot.Select(p => p.Key + "=" + p.Value).GetJoinedString(Environment.NewLine));
        }
    }
}