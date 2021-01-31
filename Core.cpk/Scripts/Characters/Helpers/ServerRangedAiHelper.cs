namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public static class ServerRangedAiHelper
    {
        private static readonly IWorldServerService ServerWorldService
            = Api.IsServer
                  ? Api.Server.World
                  : null;

        private static readonly List<ICharacter> TempListCharactersInView
            = new();

        public static ICharacter GetClosestTargetPlayer(ICharacter characterNpc, CharacterMobPrivateState privateState)
        {
            try
            {
                var charactersInView = TempListCharactersInView;
                ServerWorldService.GetCharactersInView(characterNpc,
                                                       charactersInView,
                                                       onlyPlayerCharacters: true);
                if (charactersInView.Count == 0)
                {
                    return null;
                }

                ICharacter enemy = null;
                foreach (var targetCharacter in charactersInView)
                {
                    if (!IsValidTargetInternal(targetCharacter)
                        || !CanHit(characterNpc, targetCharacter, privateState))
                    {
                        continue;
                    }

                    enemy = targetCharacter;
                    break;
                }

                return enemy;
            }
            finally
            {
                TempListCharactersInView.Clear();
            }
        }

        private static bool CanHit(
            ICharacter characterNpc,
            ICharacter target,
            CharacterMobPrivateState characterNpcPrivateState)
        {
            double rotationAngleRad = 0;
            ServerCharacterAiHelper.CalculateDistanceAndDirectionToEnemy(characterNpc,
                                                                         target,
                                                                         isRangedWeapon: true,
                                                                         out _,
                                                                         out _,
                                                                         out var directionToEnemyHitbox);

            ServerCharacterAiHelper.LookOnEnemy(directionToEnemyHitbox, ref rotationAngleRad);

            return ServerCharacterAiHelper.CanHitAnyTargetWithRangedWeapon(
                characterNpc,
                rotationAngleRad,
                characterNpcPrivateState,
                isValidTargetCallback: IsValidTargetCallback);

            static bool IsValidTargetCallback(IWorldObject worldObject)
                => worldObject is ICharacter { IsNpc: false }
                   || worldObject.ProtoGameObject is IProtoVehicle;
        }

        private static bool IsValidTargetInternal(ICharacter targetCharacter)
        {
            if (targetCharacter.GetPublicState<ICharacterPublicState>().IsDead)
            {
                // do not pay attention to dead characters
                return false;
            }

            if (targetCharacter.IsNpc)
            {
                return false;
            }

            if (targetCharacter.ProtoGameObject.GetType() != typeof(PlayerCharacter))
            {
                // don't react on spectator and other special player character prototypes
                return false;
            }

            return true;
        }
    }
}