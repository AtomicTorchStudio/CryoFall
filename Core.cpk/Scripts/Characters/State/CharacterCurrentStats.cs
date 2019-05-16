namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterInvincibility;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterStamina;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public class CharacterCurrentStats : BaseNetObject
    {
        protected const byte NetworkMaxStatUpdatesPerSecond = ScriptingConstants.NetworkDefaultMaxUpdatesPerSecond;

        [SyncToClient(
            receivers: SyncToClientReceivers.Everyone,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: NetworkMaxStatUpdatesPerSecond)]
        public float HealthCurrent { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Everyone,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: NetworkMaxStatUpdatesPerSecond)]
        public float HealthMax { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            // Don't send changes - stamina system is completely simulated on Client-side.
            // Otherwise it will be impossible to make client movement to match Server-side when a player is out of stamina.
            isSendChanges: false)]
        public float StaminaCurrent { get; private set; } = 100;

        [SyncToClient(
            receivers: SyncToClientReceivers.Owner,
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: NetworkMaxStatUpdatesPerSecond)]
        public float StaminaMax { get; private set; } = 100;

        public void ServerReduceHealth(double damage, IProtoGameObject damageSource)
        {
            if (damage <= 0)
            {
                return;
            }

            if (damageSource != null)
            {
                // it's important to register the damage source before the damage is applied
                // (to use it in case of the subsequent death)
                CharacterDamageTrackingSystem.ServerRegisterDamage(damage,
                                                                   (ICharacter)this.GameObject,
                                                                   new ServerDamageSourceEntry(damageSource));
            }

            var newHealth = this.HealthCurrent - damage;
            if (newHealth <= 0 
                && ((ICharacter)this.GameObject).IsNpc
                && damageSource is IProtoStatusEffect)
            {
                // Don't allow killing mob by a status effect.
                // This is a workaround to kill quests which cannot be finished
                // as the final damage is done by a status effect.
                // TODO: Should be removed when we enable the damage tracking for mobs damage.
                newHealth = float.Epsilon;
            }

            this.ServerSetHealthCurrent((float)newHealth);
        }

        public void ServerReduceHealth(double damage, IGameObjectWithProto damageSource)
        {
            if (damage <= 0)
            {
                return;
            }

            if (damageSource != null)
            {
                // it's important to register the damage source before the damage is applied
                // (to use it in case of the subsequent death)
                CharacterDamageTrackingSystem.ServerRegisterDamage(damage,
                                                                   (ICharacter)this.GameObject,
                                                                   new ServerDamageSourceEntry(damageSource));
            }

            var newHealth = this.HealthCurrent - damage;
            this.ServerSetHealthCurrent((float)newHealth);
        }

        public virtual void ServerSetCurrentValuesToMaxValues()
        {
            this.ServerSetHealthCurrent(this.HealthMax);
            this.SharedSetStaminaCurrent(this.StaminaMax);
        }

        /// <summary>
        /// Set health - it will be clamped automatically.
        /// </summary>
        public void ServerSetHealthCurrent(float health, bool overrideDeath = false)
        {
            this.SharedTryRefreshFinalCache();

            var character = (ICharacter)this.GameObject;
            var characterPublicState = character.GetPublicState<ICharacterPublicState>();
            if (characterPublicState.IsDead
                && !overrideDeath)
            {
                // cannot change health this way for the dead character
                return;
            }

            health = MathHelper.Clamp(health, min: 0, max: this.HealthMax);
            if (this.HealthCurrent == health
                && !overrideDeath)
            {
                return;
            }

            if (health < this.HealthCurrent)
            {
                if (!character.IsNpc
                    && CharacterInvincibilitySystem.ServerIsInvincible(character))
                {
                    // don't apply damage - character is invincible
                    //Api.Logger.Important(
                    //    $"Cannot reduce character health - {character}: character is in invincible mode");
                    health = MathHelper.Clamp(this.HealthCurrent, min: 0, max: this.HealthMax);
                }
            }

            this.HealthCurrent = health;

            if (health <= 0)
            {
                characterPublicState.IsDead = true;
                Api.Logger.Important("Character dead: " + character);
                ServerCharacterDeathMechanic.OnCharacterDeath(character);
            }
            else if (characterPublicState.IsDead)
            {
                characterPublicState.IsDead = false;
                Api.Logger.Important("Character is not dead anymore: " + character);
            }
        }

        public void ServerSetHealthMax(float maxHealth)
        {
            this.HealthMax = maxHealth;
            this.ServerSetHealthCurrent(this.HealthCurrent);
        }

        public void ServerSetStaminaMax(float staminaMax)
        {
            this.StaminaMax = staminaMax;
            this.SharedSetStaminaCurrent(this.StaminaCurrent);
        }

        /// <summary>
        /// Set stamina - it will be clamped automatically.
        /// </summary>
        public void SharedSetStaminaCurrent(float stamina, bool notifyClient = true)
        {
            this.SharedTryRefreshFinalCache();

            stamina = MathHelper.Clamp(stamina, min: 0, max: this.StaminaMax);
            var deltaStamina = stamina - this.StaminaCurrent;
            if (deltaStamina == 0)
            {
                return;
            }

            if (notifyClient
                && Api.IsServer)
            {
                var character = (ICharacter)this.GameObject;
                if (!character.IsNpc)
                {
                    CharacterStaminaSystem.ServerNotifyClientStaminaChange(character, deltaStamina);
                }
            }

            this.StaminaCurrent = stamina;
        }

        protected void SharedTryRefreshFinalCache()
        {
            var character = (ICharacter)this.GameObject;
            character.ProtoCharacter.SharedRefreshFinalCacheIfNecessary(character);
        }
    }
}