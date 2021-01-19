namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;

    [Serializable]
    public readonly struct FactionAllianceRequest
    {
        public const double DefaultRequestCooldown = 20; //1 * 60 * 60; // 1 hour

        public const double DefaultRequestLifetime = 60; //2 * 24 * 60 * 60; // 2 days

        /// <summary>
        /// The alliance request has limited lifetime.
        /// </summary>
        public readonly double ExpirationDate;

        /// <summary>
        /// A rejected request remains in the list of requests until the cooldown finishes.
        /// </summary>
        public readonly bool IsRejected;

        /// <summary>
        /// To prevent current faction to spam other faction with alliance requests,
        /// a cooldown applies.
        /// </summary>
        public readonly double NewRequestCooldownDate;

        public FactionAllianceRequest(
            double expirationDate,
            double newRequestCooldownDate)
            : this(expirationDate,
                   newRequestCooldownDate,
                   isRejected: false)
        {
        }

        private FactionAllianceRequest(
            double expirationDate,
            double newRequestCooldownDate,
            bool isRejected)
        {
            this.IsRejected = isRejected;
            this.NewRequestCooldownDate = newRequestCooldownDate;
            this.ExpirationDate = expirationDate;
        }

        public FactionAllianceRequest AsRejected(double newRequestCooldownDate)
        {
            return new(expirationDate: this.ExpirationDate,
                       newRequestCooldownDate: newRequestCooldownDate,
                       isRejected: true);
        }
    }
}