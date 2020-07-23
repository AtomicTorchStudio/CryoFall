namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.FishingSystem;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class CompletionistSystem : ProtoSystem<CompletionistSystem>
    {
        public static readonly SoundResource EntryUnlockedSoundResource
            = new SoundResource("UI/Completionist/EntryUnlocked");

        public static readonly SoundResource RewardClaimedSoundResource
            = new SoundResource("UI/Completionist/RewardClaimed");

        public static IReadOnlyCollection<IProtoItemFish> CompletionistAllFish { get; private set; }

        public static IReadOnlyCollection<IProtoItemFood> CompletionistAllFood { get; private set; }

        public static IReadOnlyCollection<IProtoObjectLoot> CompletionistAllLoot { get; private set; }

        public static IReadOnlyCollection<IProtoCharacterMob> CompletionistAllMobs { get; private set; }

        public override string Name => "Completionist system";

        public static void ClientClaimReward(IProtoEntity prototype)
        {
            Client.Audio.PlayOneShot(RewardClaimedSoundResource, 0.5f);
            Instance.CallServer(_ => _.ServerRemote_ClaimReward(prototype));
        }

        protected override void PrepareSystem()
        {
            CompletionistAllFood = new HashSet<IProtoItemFood>(
                Api.FindProtoEntities<IProtoItemFood>()
                   .Where(p => p.IsAvailableInCompletionist));

            CompletionistAllMobs = new HashSet<IProtoCharacterMob>(
                Api.FindProtoEntities<IProtoCharacterMob>()
                   .Where(p => p.IsAvailableInCompletionist));

            CompletionistAllLoot = new HashSet<IProtoObjectLoot>(
                Api.FindProtoEntities<IProtoObjectLoot>()
                   .Where(p => p.IsAvailableInCompletionist));

            CompletionistAllFish = new HashSet<IProtoItemFish>(
                Api.FindProtoEntities<IProtoItemFish>());

            if (IsServer)
            {
                ServerItemUseObserver.ItemUsed += ServerItemUsedHandler;
                ServerCharacterDeathMechanic.CharacterKilled += ServerCharacterKilledHandler;
                ServerLootEventHelper.LootReceived += ServerLootReceivedHandler;
                FishingSystem.ServerFishCaught += ServerFishCaughtHandler;
            }
        }

        private static void ServerCharacterKilledHandler(ICharacter attackerCharacter, ICharacter targetCharacter)
        {
            if (attackerCharacter.IsNpc
                || !targetCharacter.IsNpc)
            {
                return;
            }

            var protoCharacter = (IProtoCharacterMob)targetCharacter.ProtoCharacter;
            if (CompletionistAllMobs.Contains(protoCharacter))
            {
                SharedGetCompletionistData(attackerCharacter)
                    .ServerOnMobKilled(protoCharacter);
            }
        }

        private static void ServerFishCaughtHandler(ICharacter character, IItem itemFish, float sizeValue)
        {
            if (character.IsNpc)
            {
                return;
            }

            SharedGetCompletionistData(character)
                .ServerOnFishCaught((IProtoItemFish)itemFish.ProtoItem, sizeValue);
        }

        private static void ServerItemUsedHandler(ICharacter character, IItem item)
        {
            if (!(item.ProtoGameObject is IProtoItemFood protoItemFood)
                || !CompletionistAllFood.Contains(protoItemFood))
            {
                return;
            }

            SharedGetCompletionistData(character)
                .ServerOnItemUsed(protoItemFood);
        }

        private static void ServerLootReceivedHandler(ICharacter character, IStaticWorldObject staticWorldObject)
        {
            if (!(staticWorldObject.ProtoGameObject is IProtoObjectLoot protoObjectLoot)
                || !CompletionistAllLoot.Contains(protoObjectLoot))
            {
                return;
            }

            SharedGetCompletionistData(character)
                .ServerOnLootReceived(protoObjectLoot);
        }

        private static PlayerCharacterCompletionistData SharedGetCompletionistData(ICharacter character)
        {
            return PlayerCharacter.GetPrivateState(character).CompletionistData;
        }

        private void ServerRemote_ClaimReward(IProtoEntity prototype)
        {
            var completionistData = SharedGetCompletionistData(ServerRemoteContext.Character);
            completionistData.ServerTryClaimReward(prototype);
        }
    }
}