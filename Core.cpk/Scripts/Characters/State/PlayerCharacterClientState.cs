namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class PlayerCharacterClientState : BaseCharacterClientState
    {
        private ComponentPlayerInputSender componentPlayerInputSender;

        public ComponentPlayerInputSender ComponentPlayerInputSender
        {
            get
            {
                if (this.componentPlayerInputSender is null)
                {
                    // create and attach input sender component to the player scene object
                    var sceneObject = ((IWorldObject)this.GameObject).ClientSceneObject;
                    this.componentPlayerInputSender = sceneObject.AddComponent<ComponentPlayerInputSender>();
                    this.componentPlayerInputSender.Setup((PlayerCharacter)this.GameObject.ProtoGameObject);
                }

                return this.componentPlayerInputSender;
            }
        }

        public BasePublicActionState CurrentPublicActionState { get; set; }
    }
}