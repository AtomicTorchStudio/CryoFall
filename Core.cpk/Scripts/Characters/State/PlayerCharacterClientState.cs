namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PlayerCharacterClientState : BaseCharacterClientState
    {
        private ComponentPlayerInputSender componentPlayerInputSender;

        public ComponentPlayerInputSender ComponentPlayerInputSender
        {
            get
            {
                if (this.componentPlayerInputSender == null)
                {
                    // create and attach input sender component to the player scene object
                    var sceneObject = Api.Client.Scene.GetSceneObject((IWorldObject)this.GameObject);
                    this.componentPlayerInputSender = sceneObject.AddComponent<ComponentPlayerInputSender>();
                    this.componentPlayerInputSender.Setup((PlayerCharacter)this.GameObject.ProtoGameObject);
                }

                return this.componentPlayerInputSender;
            }
        }

        public BasePublicActionState CurrentPublicActionState { get; set; }
    }
}