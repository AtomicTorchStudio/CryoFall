namespace AtomicTorch.CBND.CoreMod.Systems.InteractionChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterUnstuck;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using JetBrains.Annotations;

    public class InteractionCheckerSystem : ProtoSystem<InteractionCheckerSystem>
    {
        private const double CheckTimeIntervalSeconds = 1 / 5d;

        private static readonly Dictionary<Pair, PairAction> RegisteredActions = new Dictionary<Pair, PairAction>();

        public delegate void DelegateFinishAction(bool isAbort);

        public override string Name => "Interaction checker system";

        public static void CancelCurrentInteraction(ICharacter character)
        {
            foreach (var action in RegisteredActions)
            {
                if (action.Key.Character != character)
                {
                    continue;
                }

                // found current interaction
                SharedUnregister(character, action.Key.WorldObject, isAbort: true);
                return;
            }
        }

        public static IEnumerable<ICharacter> SharedEnumerateCurrentInteractionCharacters(IWorldObject worldObject)
        {
            foreach (var pair in RegisteredActions.Keys)
            {
                if (pair.WorldObject == worldObject)
                {
                    yield return pair.Character;
                }
            }
        }

        public static IWorldObject SharedGetCurrentInteraction(ICharacter character)
        {
            foreach (var pair in RegisteredActions.Keys)
            {
                if (pair.Character != character)
                {
                    continue;
                }

                // found current interaction
                return pair.WorldObject;
            }

            return null;
        }

        [CanBeNull]
        public static ITempList<TestResult> SharedGetTempObjectsInCharacterInteractionArea(
            ICharacter character,
            bool writeToLog = false,
            CollisionGroup requiredCollisionGroup = null)
        {
            var characterPhysicsBody = character.PhysicsBody;
            if (!characterPhysicsBody.IsEnabled)
            {
                return null;
            }

            if (requiredCollisionGroup is null)
            {
                // default group to check interaction is click area
                requiredCollisionGroup = CollisionGroups.ClickArea;
            }

            // find character interaction area
            var characterInteractionArea = characterPhysicsBody.Shapes.FirstOrDefault(
                s => s.CollisionGroup
                     == CollisionGroups.CharacterInteractionArea);

            if (characterInteractionArea is null)
            {
                //Logger.Warning("No character interaction area defined: " + character);
                return null;
            }

            var physicsSpace = characterPhysicsBody.PhysicsSpace;
            var tempObjectsInCharacterInteractionArea = physicsSpace.TestShape(
                characterPhysicsBody.Position,
                characterInteractionArea,
                requiredCollisionGroup,
                sendDebugEvent: writeToLog);
            return tempObjectsInCharacterInteractionArea;
        }

        public static bool SharedHasInteraction(
            ICharacter character,
            IWorldObject worldObject,
            bool requirePrivateScope)
        {
            var pair = new Pair(character, worldObject);
            if (!RegisteredActions.ContainsKey(pair))
            {
                return false;
            }

            if (requirePrivateScope)
            {
                if (IsServer && !Server.Core.IsInPrivateScope(character, worldObject)
                    || IsClient && !Client.Core.IsInPrivateScope(worldObject))
                {
                    return false;
                }
            }

            return true;
        }

        public static void SharedRegister(
            ICharacter character,
            IWorldObject worldObject,
            DelegateFinishAction finishAction)
        {
            CancelCurrentInteraction(character);
            RegisteredActions[new Pair(character, worldObject)] = new PairAction(finishAction);
        }

        public static bool SharedUnregister(ICharacter character, IWorldObject worldObject, bool isAbort)
        {
            var pair = new Pair(character, worldObject);
            if (!RegisteredActions.TryGetValue(pair, out var action))
            {
                return false;
            }

            try
            {
                action.FinishAction?.Invoke(isAbort);
            }
            finally
            {
                RegisteredActions.Remove(pair);
            }

            return true;
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                // configure time interval trigger
                TriggerTimeInterval.ServerConfigureAndRegister(
                    interval: TimeSpan.FromSeconds(CheckTimeIntervalSeconds),
                    callback: this.TimerTickCallback,
                    name: "System." + this.ShortId);
            }
            else // if (IsClient)
            {
                ClientTimersSystem.AddAction(CheckTimeIntervalSeconds, this.TimerTickCallback);
            }
        }

        // check if character can interact
        private static bool SharedIsValidInteraction(Pair key)
        {
            var character = key.Character;
            if (!character.ServerIsOnline)
            {
                // character is offline - cannot interact
                return false;
            }

            var worldObject = key.WorldObject;
            if (worldObject.IsDestroyed)
            {
                // world object is destroyed
                return false;
            }

            var canInteract = worldObject
                              .ProtoWorldObject.SharedCanInteract(
                                  character,
                                  worldObject,
                                  writeToLog: true);

            return canInteract;
        }

        private void TimerTickCallback()
        {
            if (IsClient)
            {
                ClientTimersSystem.AddAction(CheckTimeIntervalSeconds, this.TimerTickCallback);
            }

            RegisteredActions.ProcessAndRemove(
                // remove if cannot interact
                removeCondition: pair => !SharedIsValidInteraction(pair),
                // abort action when pair removed due to the interaction check failed
                removeCallback: pair => pair.Value.FinishAction?.Invoke(isAbort: true));
        }

        private struct Pair : IEquatable<Pair>
        {
            public readonly ICharacter Character;

            public readonly IWorldObject WorldObject;

            public Pair(ICharacter character, IWorldObject worldObject)
            {
                this.Character = character;
                this.WorldObject = worldObject;
            }

            public static bool operator ==(Pair left, Pair right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Pair left, Pair right)
            {
                return !left.Equals(right);
            }

            public bool Equals(Pair other)
            {
                return ReferenceEquals(this.Character,      other.Character)
                       && ReferenceEquals(this.WorldObject, other.WorldObject);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is Pair && this.Equals((Pair)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.Character is not null ? this.Character.GetHashCode() : 0) * 397)
                           ^ (this.WorldObject is not null ? this.WorldObject.GetHashCode() : 0);
                }
            }
        }

        private struct PairAction
        {
            public readonly DelegateFinishAction FinishAction;

            public PairAction(DelegateFinishAction finishAction)
            {
                this.FinishAction = finishAction;
            }
        }
    }
}