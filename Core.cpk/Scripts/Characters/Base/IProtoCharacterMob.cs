﻿namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoCharacterMob : IProtoCharacterCore, IProtoSpawnableObject
    {
        double BiomaterialValueMultiplier { get; }

        double CorpseInteractionAreaScale { get; }

        ITextureResource Icon { get; }

        bool IsAvailableInCompletionist { get; }

        bool IsBoss { get; }

        IReadOnlyDropItemsList LootDroplist { get; }

        double MobKillExperienceMultiplier { get; }

        void ServerAggroNotification(
            ICharacter currentCharacter,
            ICharacter sourceCharacterMob,
            ICharacter characterToAggro);

        void ServerOnDeath(ICharacter character);

        void ServerPlaySound(ICharacter character, CharacterSound characterSound);

        void ServerSetSpawnState(ICharacter character, MobSpawnState state);
    }
}