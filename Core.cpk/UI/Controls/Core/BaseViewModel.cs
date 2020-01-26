namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public abstract class BaseViewModel : BaseDataObject, IStateSubscriptionOwner
    {
        private readonly bool isAutoDisposeFields;

        private StateSubscriptionStorage stateSubscriptionStorage;

        protected BaseViewModel(bool isAutoDisposeFields = true)
        {
            this.isAutoDisposeFields = isAutoDisposeFields;
        }

        ~BaseViewModel()
        {
            if (ApiState.IsShutdown)
            {
                return;
            }

            var typeDescription = $"* {this.GetType().Name} ({this})";
            var message = "Found a not disposed view model:"
                          + Environment.NewLine
                          + typeDescription
                          + Environment.NewLine
                          + "  - please ensure that Dispose() method is called when the view model is no longer used.";

            //if (Api.IsEditor)
            //{
            //    Api.Logger.WriteError(message);
            //}
            //else
            //{
            Api.Logger.Warning(message);
            //}
        }

        internal static IClientApi Client => Api.Client;

        internal static ILogger Logger => Api.Logger;

        public void RegisterSubscription(StateSubscriptionToken stateSubscriptionToken)
        {
            if (this.stateSubscriptionStorage == null)
            {
                this.stateSubscriptionStorage = new StateSubscriptionStorage();
            }

            this.stateSubscriptionStorage.RegisterSubscription(stateSubscriptionToken);
        }

        public void ReleaseSubscriptions()
        {
            this.stateSubscriptionStorage?.ReleaseSubscriptions();
        }

        protected void DisposeCollection<TItem>(IReadOnlyCollection<TItem> collection)
            where TItem : BaseViewModel
        {
            if (collection == null
                || collection.Count == 0)
            {
                return;
            }

            foreach (var element in Api.Shared.WrapInTempList(collection).EnumerateAndDispose())
            {
                try
                {
                    element.Dispose();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(
                        ex,
                        "Exception during disposing view model in list");
                }
            }
        }

        protected virtual void DisposeViewModel()
        {
        }

        protected sealed override void OnDispose()
        {
            this.ReleaseSubscriptions();

            ClientContainersExchangeManager.Unregister(this);

            try
            {
                this.DisposeViewModel();
            }
            finally
            {
                // please note - this is too dangerous as this way we can dispose something we don't want to dispose automatically!
                if (this.isAutoDisposeFields)
                {
                    this.DisposeDisposableFields();
                }
            }
        }

        private void DisposeDisposableFields()
        {
            // use reflection to dispose all the disposable fields
            var fieldInfos = this.GetType()
                                 .GetDeclaredFieldsInSelfAndParent();

            foreach (var fieldInfo in fieldInfos)
            {
                if (fieldInfo.FieldType.IsValueType)
                {
                    continue;
                }

                var fieldValue = fieldInfo.GetValue(this);
                if (fieldValue == null)
                {
                    continue;
                }

                if (fieldInfo.GetCustomAttribute<ViewModelNotAutoDisposeField>() != null)
                {
                    continue;
                }

                if (fieldValue is Visual)
                {
                    // don't dispose UI controls this way
                    continue;
                }

                if (fieldValue is IDisposable disposableFieldValue
                    && !(fieldValue is BaseState))
                {
                    try
                    {
                        if (!(fieldValue is BaseViewModel)
                            && !(fieldValue is StateSubscriptionStorage))
                        {
                            Logger.Warning(
                                $"Auto-disposing view model field (IDisposable): {fieldInfo.Name} in type {this.GetType().FullName} - value: {fieldValue}");
                        }

                        disposableFieldValue.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Api.Logger.Exception(ex, "Exception during disposing view model field value");
                    }

                    continue;
                }

                if (fieldValue is IReadOnlyCollection<BaseViewModel> enumerable)
                {
                    this.DisposeCollection(enumerable);
                    continue;
                }
            }
        }
    }
}