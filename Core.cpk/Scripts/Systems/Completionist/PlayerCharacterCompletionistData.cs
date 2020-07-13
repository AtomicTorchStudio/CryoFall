namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PlayerCharacterCompletionistData : BaseNetObject
    {
        [SyncToClient]
        public NetworkSyncList<DataEntryCompletionistFish> ListFish { get; }
            = new NetworkSyncList<DataEntryCompletionistFish>();

        [SyncToClient]
        public NetworkSyncList<DataEntryCompletionist> ListFood { get; }
            = new NetworkSyncList<DataEntryCompletionist>();

        [SyncToClient]
        public NetworkSyncList<DataEntryCompletionist> ListLoot { get; }
            = new NetworkSyncList<DataEntryCompletionist>();

        [SyncToClient]
        public NetworkSyncList<DataEntryCompletionist> ListMobs { get; }
            = new NetworkSyncList<DataEntryCompletionist>();

        public void ServerOnFishCaught(IProtoItemFish protoItemFish, float sizeValue)
        {
            Api.ValidateIsServer();

            for (var index = 0; index < this.ListFish.Count; index++)
            {
                var entry = this.ListFish[index];
                if (!ReferenceEquals(entry.Prototype, protoItemFish))
                {
                    continue;
                }

                // already have an entry
                if (entry.MaxSizeValue >= sizeValue)
                {
                    return;
                }

                // update the entry
                this.ListFish[index] = new DataEntryCompletionistFish(entry.IsRewardClaimed,
                                                                      protoItemFish,
                                                                      sizeValue);

                Api.Logger.Info($"Completionist entry updated: {protoItemFish.ShortId} with size value {sizeValue:F2}");
                return;
            }

            // add an entry
            this.ListFish.Add(new DataEntryCompletionistFish(isRewardClaimed: false,
                                                             protoItemFish,
                                                             sizeValue));
            Api.Logger.Info($"Completionist entry added: {protoItemFish.ShortId} with size value {sizeValue:F2}");
        }

        public void ServerOnItemUsed(IProtoItemFood protoItemFood)
        {
            Api.ValidateIsServer();
            AddIfNotContains(protoItemFood, this.ListFood);
        }

        public void ServerOnLootReceived(IProtoObjectLoot protoObjectLoot)
        {
            Api.ValidateIsServer();
            AddIfNotContains(protoObjectLoot, this.ListLoot);
        }

        public void ServerOnMobKilled(IProtoCharacterMob protoMob)
        {
            Api.ValidateIsServer();
            AddIfNotContains(protoMob, this.ListMobs);
        }

        public void ServerReset()
        {
            this.ListFood.Clear();
            this.ListMobs.Clear();
            this.ListLoot.Clear();
            this.ListFish.Clear();
        }

        public void ServerTryClaimReward(IProtoEntity prototype)
        {
            Api.ValidateIsServer();

            var reward = CompletionistSystemConstants.ServerRewardLearningPointsPerEntry;

            switch (prototype)
            {
                case IProtoItemFood protoItemFood:
                    this.ServerTryClaimRewardInternal(this.ListFood,
                                                      protoItemFood,
                                                      reward,
                                                      new DataEntryCompletionist(isRewardClaimed: true,
                                                                                 protoItemFood));
                    break;

                case IProtoCharacterMob protoCharacterMob:
                    this.ServerTryClaimRewardInternal(this.ListMobs,
                                                      protoCharacterMob,
                                                      reward,
                                                      new DataEntryCompletionist(isRewardClaimed: true,
                                                                                 protoCharacterMob));
                    break;

                case IProtoObjectLoot protoObjectLoot:
                    this.ServerTryClaimRewardInternal(this.ListLoot,
                                                      protoObjectLoot,
                                                      reward,
                                                      new DataEntryCompletionist(isRewardClaimed: true,
                                                                                 protoObjectLoot));
                    break;

                case IProtoItemFish protoItemFish:
                    var existingEntry = this.ListFish.FirstOrDefault(e => ReferenceEquals(protoItemFish, e.Prototype));
                    if (existingEntry.Prototype is null
                        || existingEntry.IsRewardClaimed)
                    {
                        // no entry found or entry already claimed
                        return;
                    }

                    var finishedEntry = new DataEntryCompletionistFish(isRewardClaimed: true,
                                                                       protoItemFish,
                                                                       maxSizeValue: existingEntry.MaxSizeValue);
                    this.ServerTryClaimRewardInternal(this.ListFish,
                                                      protoItemFish,
                                                      reward,
                                                      finishedEntry);
                    break;

                default:
                    throw new Exception("Unknown prototype: " + prototype);
            }
        }

        private static void AddIfNotContains(
            IProtoEntity prototype,
            NetworkSyncList<DataEntryCompletionist> list)
        {
            foreach (var entry in list)
            {
                if (ReferenceEquals(entry.Prototype, prototype))
                {
                    // already have an entry
                    return;
                }
            }

            // add an entry
            list.Add(new DataEntryCompletionist(isRewardClaimed: false, prototype));
            Api.Logger.Info("Completionist entry added: " + prototype.ShortId);
        }

        private void ServerTryClaimRewardInternal<TDataEntry>(
            NetworkSyncList<TDataEntry> list,
            IProtoEntity prototype,
            ushort rewardLearningPoints,
            TDataEntry dataEntryWithClaimedReward)
            where TDataEntry : struct, ICompletionistDataEntry
        {
            var character = (ICharacter)this.GameObject;

            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index];
                if (!ReferenceEquals(prototype, entry.Prototype))
                {
                    continue;
                }

                if (entry.IsRewardClaimed)
                {
                    Api.Logger.Warning("Completionist: the reward is already claimed: " + prototype,
                                       characterRelated: character);
                    return;
                }

                list.RemoveAt(index);
                list.Insert(index, dataEntryWithClaimedReward);
                Api.Logger.Info($"Completionist: the reward is claimed: {prototype}: +{rewardLearningPoints} LP",
                                characterRelated: character);

                character.SharedGetTechnologies()
                         .ServerAddLearningPoints(rewardLearningPoints, allowModifyingByStat: false);
                return;
            }

            Api.Logger.Warning(
                "Completionist: the reward cannot be claimed as the entry is not discovered: " + prototype,
                characterRelated: character);
        }
    }
}