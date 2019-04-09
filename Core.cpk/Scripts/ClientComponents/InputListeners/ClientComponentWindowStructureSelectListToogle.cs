namespace AtomicTorch.CBND.CoreMod.ClientComponents.InputListeners
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentWindowStructureSelectListToogle : ClientComponent
    {
        public override void Update(double deltaTime)
        {
            if (ClientInputManager.IsButtonDown(GameButton.ConstructionMenu))
            {
                ConstructionPlacementSystem.ClientToggleConstructionMenu();
            }
        }
    }
}