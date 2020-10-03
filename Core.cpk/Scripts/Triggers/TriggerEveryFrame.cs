namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using System;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TriggerEveryFrame : ProtoTrigger
    {
        private static BaseTriggerConfig config;

        [NotLocalizable]
        public override string Name => "Trigger every frame";

        public static void ServerRegister(
            Action callback,
            string name)
        {
            Api.ValidateIsServer();
            EnsureConfigCreated();
            config.ServerRegister(callback, name);
        }

        public override void ServerUpdate()
        {
            this.ServerUpdateConfigurations();
        }

        protected override void PrepareProto()
        {
            EnsureConfigCreated();
        }

        private static void EnsureConfigCreated()
        {
            if (IsClient
                || config is not null)
            {
                return;
            }

            config = new EmptyTriggerConfig(GetProtoEntity<TriggerEveryFrame>());
        }

        private class EmptyTriggerConfig : BaseTriggerConfig
        {
            public EmptyTriggerConfig(ProtoTrigger trigger) : base(trigger)
            {
            }

            public override void ServerUpdateConfiguration()
            {
                // simply invoke the trigger
                this.ServerInvokeTrigger();
            }
        }
    }
}