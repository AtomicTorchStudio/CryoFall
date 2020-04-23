namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
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
        public NetworkSyncList<DataEntryCompletionist> ListFood { get; }
            = new NetworkSyncList<DataEntryCompletionist>();

        [SyncToClient]
        public NetworkSyncList<DataEntryCompletionist> ListLoot { get; }
            = new NetworkSyncList<DataEntryCompletionist>();

        [SyncToClient]
        public NetworkSyncList<DataEntryCompletionist> ListMobs { get; }
            = new NetworkSyncList<DataEntryCompletionist>();

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
                                                      reward);
                    break;

                case IProtoCharacterMob protoCharacterMob:
                    this.ServerTryClaimRewardInternal(this.ListMobs,
                                                      protoCharacterMob,
                                                      reward);
                    break;

                case IProtoObjectLoot protoObjectLoot:
                    this.ServerTryClaimRewardInternal(this.ListLoot,
                                                      protoObjectLoot,
                                                      reward);
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

        private void ServerTryClaimRewardInternal(
            NetworkSyncList<DataEntryCompletionist> list,
            IProtoEntity prototype,
            ushort rewardLearningPoints)
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
                list.Insert(index, new DataEntryCompletionist(isRewardClaimed: true, prototype));
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