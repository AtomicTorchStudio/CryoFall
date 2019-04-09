namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.CooldownIndicator
{
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentCooldownIndicator : ClientComponent
    {
        public ViewModelCooldownIndicatorControl ViewModelToUpdate { get; set; }

        public override void Update(double deltaTime)
        {
            this.ViewModelToUpdate.Update(deltaTime);
        }
    }
}