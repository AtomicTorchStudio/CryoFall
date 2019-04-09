namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentObjectMineralStageWatcher : ClientComponent
    {
        private byte lastDamageStage;

        private IStaticWorldObject mineralObject;

        private Action<IStaticWorldObject> onMineralDestroyStageChanged;

        private IProtoObjectMineral protoObjectMineral;

        private StaticObjectPublicState staticObjectServerPublicState;

        protected StaticObjectPublicState StaticObjectPublicState
        {
            get => this.staticObjectServerPublicState;
            private set
            {
                if (this.staticObjectServerPublicState == value)
                {
                    return;
                }

                if (this.staticObjectServerPublicState != null)
                {
                    this.ReleaseSubscriptions();
                }

                this.staticObjectServerPublicState = value;
                this.staticObjectServerPublicState?.ClientSubscribe(
                    _ => _.StructurePointsCurrent,
                    this.StructurePointsCurrentChanged,
                    this);
            }
        }

        public void Setup(
            IStaticWorldObject mineralObject,
            StaticObjectPublicState serverPublicState,
            Action<IStaticWorldObject> onMineralDestroyStageChanged)
        {
            this.mineralObject = mineralObject;
            this.StaticObjectPublicState = serverPublicState;
            this.protoObjectMineral = (IProtoObjectMineral)mineralObject.ProtoWorldObject;
            this.onMineralDestroyStageChanged = onMineralDestroyStageChanged;

            // update last damage stage
            var damageStage = this.protoObjectMineral.SharedCalculateDamageStage(
                this.StaticObjectPublicState.StructurePointsCurrent);
            this.lastDamageStage = damageStage;
        }

        private void StructurePointsCurrentChanged(float newValue)
        {
            var currentDamageStage = this.protoObjectMineral.SharedCalculateDamageStage(newValue);
            if (this.lastDamageStage == currentDamageStage)
            {
                return;
            }

            // damage stage changed
            this.lastDamageStage = currentDamageStage;
            this.onMineralDestroyStageChanged?.Invoke(this.mineralObject);
        }
    }
}