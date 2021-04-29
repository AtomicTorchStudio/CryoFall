namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterInvincibility;
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

        public void ServerReduceHealth(double damage, IProtoGameObject damageSource)
        {
            if (damage <= 0)
            {
                return;
            }

            if (this.HealthCurrent <= 0)
            {
                return;
            }

            if (damageSource is not null)
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

            if (this.HealthCurrent <= 0)
            {
                return;
            }

            var damagedCharacter = (ICharacter)this.GameObject;
            if (damageSource is not null)
            {
                // it's important to register the damage source before the damage is applied
                // (to use it in case of the subsequent death)
                CharacterDamageTrackingSystem.ServerRegisterDamage(damage,
                                                                   damagedCharacter,
                                                                   new ServerDamageSourceEntry(damageSource));
            }

            var newHealth = this.HealthCurrent - damage;

            if (newHealth <= 0
                && damagedCharacter.IsNpc
                && damageSource?.ProtoGameObject is IProtoStatusEffect)
            {
                var attackerCharacter = GetAttackerCharacter(damageSource, out _);
                if (attackerCharacter is null
                    || attackerCharacter.IsNpc)
                {
                    // don't allow killing a mob by a status effect which is NOT added by a player character
                    newHealth = float.Epsilon;
                }
            }

            this.ServerSetHealthCurrent((float)newHealth);

            if (newHealth <= 0)
            {
                var attackerCharacter = GetAttackerCharacter(damageSource, out var weaponSkill);
                ServerCharacterDeathMechanic.OnCharacterKilled(
                    attackerCharacter,
                    targetCharacter: damagedCharacter,
                    weaponSkill);
            }
        }

        public virtual void ServerSetCurrentValuesToMaxValues()
        {
            this.ServerSetHealthCurrent(this.HealthMax);
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
            if (this.HealthMax == maxHealth)
            {
                return;
            }

            var healthCurrent = this.HealthCurrent;
            if (this.HealthMax > 0
                && maxHealth > 0
                && healthCurrent > 0)
            {
                // recalculate current health to keep the ratio
                healthCurrent = maxHealth * (healthCurrent / this.HealthMax);
            }

            this.HealthMax = maxHealth;
            // reapply current health
            this.ServerSetHealthCurrent(healthCurrent);
        }

        protected void SharedTryRefreshFinalCache()
        {
            var character = (ICharacter)this.GameObject;
            character.ProtoCharacter.SharedRefreshFinalCacheIfNecessary(character);
        }

        private static ICharacter GetAttackerCharacter(IGameObjectWithProto damageSource, out IProtoSkill protoSkill)
        {
            switch (damageSource)
            {
                case ICharacter character:
                    protoSkill = !character.IsNpc
                                     ? character.GetPrivateState<PlayerCharacterPrivateState>().WeaponState
                                                .ProtoWeapon?.WeaponSkillProto
                                     : null;
                    return character;

                case { } when damageSource.ProtoGameObject is IProtoStatusEffect:
                    var statusEffectPublicState = damageSource.GetPublicState<StatusEffectPublicState>();
                    protoSkill = statusEffectPublicState.ServerStatusEffectWasAddedByCharacterWeaponSkill;
                    return statusEffectPublicState.ServerStatusEffectWasAddedByCharacter;

                default:
                    protoSkill = null;
                    return null;
            }
        }
    }
}