namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStyle;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowCharacterStyleCustomization : BaseUserControlWithWindow
    {
        protected override void InitControlWithWindow()
        {
            this.Window.IsCached = false;

            var characterCustomizationControl = this.GetByName<CharacterCustomizationControl>(
                "CharacterCustomizationControl");

            characterCustomizationControl.CallbackClose
                = result =>
                  {
                      if (!result.Equals(default))
                      {
                          CharacterStyleSystem.ClientChangeStyle(result.style, result.isMale);
                      }

                      this.Window.Close(DialogResult.OK);
                  };
        }
    }
}