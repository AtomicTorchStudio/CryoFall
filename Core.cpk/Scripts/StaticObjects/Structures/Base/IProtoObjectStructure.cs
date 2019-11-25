namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectStructure : IProtoStaticWorldObject, IDamageableProtoWorldObject
    {
        ProtoStructureCategory Category { get; }

        IConstructionStageConfigReadOnly ConfigBuild { get; }

        IConstructionStageConfigReadOnly ConfigRepair { get; }

        IConstructionUpgradeConfigReadOnly ConfigUpgrade { get; }

        string Description { get; }

        string DescriptionUpgrade { get; }

        bool IsAutoUnlocked { get; }

        bool IsListedInTechNodes { get; }

        bool IsRepeatPlacement { get; }

        IReadOnlyList<TechNode> ListedInTechNodes { get; }

        float StructurePointsMaxForConstructionSite { get; }

        /// <summary>
        /// Returns actual config - for constructed object this is repair config (because it's constructed),
        /// for a construction site it's a build config of the building structure!
        /// </summary>
        /// <param name="staticWorldObject"></param>
        /// <returns></returns>
        IConstructionStageConfigReadOnly GetStructureActiveConfig(IStaticWorldObject staticWorldObject);

        void PrepareProtoSetLinkWithTechNode(TechNode techNode);

        void ServerApplyDamage(IStaticWorldObject worldObject, double damage);

        void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime);

        void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter);

        void SharedCreatePhysicsConstructionBlueprint(IPhysicsBody physicsBody);

        float SharedGetStructurePointsMax(IStaticWorldObject worldObject);

        bool SharedIsTechUnlocked(ICharacter character, bool allowIfAdmin = true);

        void SharedOnDeconstructionStage(
            IStaticWorldObject worldObject,
            ICharacter byCharacter,
            float oldStructurePoints,
            float newStructurePoints);
    }
}