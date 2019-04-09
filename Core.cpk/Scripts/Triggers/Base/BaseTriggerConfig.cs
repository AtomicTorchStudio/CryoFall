namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseTriggerConfig
    {
        private readonly List<TriggerCallback> invocationList
            = new List<TriggerCallback>();

        private readonly ProtoTrigger trigger;

        private bool isConfigurationRegistered;

        protected BaseTriggerConfig(ProtoTrigger trigger)
        {
            this.trigger = trigger;
        }

        public string ShortId => this.trigger.ShortId;

        public ProtoTrigger Trigger => this.trigger;

        public void ServerRegister(Action callback, string name)
        {
            // should not register server trigger on the Client-side
            Api.ValidateIsServer();

            if (!this.isConfigurationRegistered)
            {
                this.trigger.ServerRegisterConfiguration(this);
                this.isConfigurationRegistered = true;
            }

            var profilerKey = this.trigger.Id + "." + name;
            this.invocationList.Add(new TriggerCallback(callback, profilerKey));
        }

        public void ServerUnregister()
        {
            this.trigger.ServerUnregisterConfiguration(this);
            this.isConfigurationRegistered = false;
            this.invocationList.Clear();
        }

        public abstract void ServerUpdateConfiguration();

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return "Trigger config for " + this.trigger;
        }

        protected void ServerInvokeTrigger()
        {
            // the invocationList might change during the enumeration so let's use for instead of foreach
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var index = 0; index < this.invocationList.Count; index++)
            {
                var callback = this.invocationList[index];
                try
                {
                    using (new ScriptPerformanceMeasurementSection(callback.ProfilerKey))
                    {
                        callback.Action();
                    }
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Exception during the trigger callback: " + this.trigger.ShortId);
                }
            }
        }

        private readonly struct TriggerCallback
        {
            public readonly Action Action;

            public readonly ScriptPerformanceMeasurementKey ProfilerKey;

            public TriggerCallback(Action action, string profilerKey)
            {
                this.Action = action;
                this.ProfilerKey = ScriptPerformanceMeasurementSection.CreateKey(profilerKey);
            }
        }
    }
}