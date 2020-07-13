namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using System;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class FishingSession
        : ProtoGameObject<ILogicObject, EmptyPrivateState, FishingSession.PublicState, EmptyClientState>,
          IProtoLogicObject
    {
        private static readonly Interval<float> FishingDurationBeforeBiting
            = (8, 20);

        private static readonly Interval<float> FishingDurationBiting
            = (2, 2);

        public static double MaxDuration => FishingDurationBeforeBiting.Max
                                            + FishingDurationBiting.Max;

        public override double ClientUpdateIntervalSeconds => 0.1;

        [NotLocalizable]
        public override string Name => "Fishing session";

        public override double ServerUpdateIntervalSeconds => 0.1;

        public static void ServerSetup(ILogicObject currentFishingSession, ICharacter characterFishing)
        {
            GetPublicState(currentFishingSession).CharacterFishing = characterFishing;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            if (!data.IsFirstTimeInit)
            {
                return;
            }

            var publicState = data.PublicState;
            publicState.ServerTimeRemainsBeforeFishBiting = RandomHelper.Range(FishingDurationBeforeBiting.Min,
                                                                               FishingDurationBeforeBiting.Max);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var publicState = data.PublicState;

            if (publicState.ServerTimeRemainsBeforeFishBiting > 0)
            {
                // waiting to start biting
                publicState.ServerTimeRemainsBeforeFishBiting
                    = publicState.ServerTimeRemainsBeforeFishBiting - data.DeltaTime;

                if (publicState.ServerTimeRemainsBeforeFishBiting > 0)
                {
                    return;
                }

                publicState.ServerTimeRemainsBeforeFishBiting = 0;
                publicState.IsFishBiting = true;
                //Logger.Dev("Fish is biting now!", publicState.CharacterFishing);

                publicState.ServerTimeRemainsFishBiting = RandomHelper.Range(FishingDurationBiting.Min,
                                                                             FishingDurationBiting.Max);

                // extend on ping duration to help that player catch the fish
                try
                {
                    var connectionStats = Api.Server.Core.GetPlayerConnectionStats(data.PublicState.CharacterFishing);
                    publicState.ServerTimeRemainsFishBiting += Math.Min(0.5,
                                                                        connectionStats.LatencyRoundtripGameSeconds);
                }
                catch (Exception ex)
                {
                    // probably the player is offline?
                    Logger.Warning(ex.Message, publicState.CharacterFishing);
                }

                return;
            }

            // fish is biting now
            publicState.ServerTimeRemainsFishBiting -= data.DeltaTime;
            if (publicState.ServerTimeRemainsFishBiting > 0)
            {
                return;
            }

            // catch failed, time out!
            Server.World.DestroyObject(data.GameObject);
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient]
            public ICharacter CharacterFishing { get; set; }

            /// <summary>
            /// Required to display a floater down animation. Player has a limited time to react then.
            /// </summary>
            [SyncToClient]
            public bool IsFishBiting { get; set; }

            public double ServerTimeRemainsBeforeFishBiting { get; set; }

            public double ServerTimeRemainsFishBiting { get; set; }
        }
    }
}