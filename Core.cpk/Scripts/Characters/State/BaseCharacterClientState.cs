namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class BaseCharacterClientState : BaseClientState, IClientStateWithShadowRenderer
    {
        [SubscribableProperty]
        public ProtoCharacterSkeleton CurrentProtoSkeleton { get; set; }

        public double CurrentProtoSkeletonScaleMultiplier { get; set; }

        public bool HasWeaponAnimationAssigned { get; set; }

        public bool? IsDead { get; set; }

        public bool IsWeaponFiringAnimationActive { get; set; }

        public float LastAimCoef { get; set; }

        [SubscribableProperty]
        public ushort? LastEquipmentContainerHash { get; set; }

        public double? LastInterpolatedRotationAngleRad { get; set; }

        public IItem LastSelectedItem { get; set; }

        public ViewOrientation LastViewOrientation { get; set; }

        public IComponentSpriteRenderer RendererShadow { get; set; }

        public List<IClientComponent> SkeletonComponents { get; } = new List<IClientComponent>();

        public IComponentSkeleton SkeletonRenderer { get; set; }

        public IComponentSoundEmitter SoundEmitterLoopCharacter { get; set; }

        public IComponentSoundEmitter SoundEmitterLoopMovemement { get; set; }
    }
}