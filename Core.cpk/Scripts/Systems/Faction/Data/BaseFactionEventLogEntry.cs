namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Text;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    [Serializable]
    public abstract class BaseFactionEventLogEntry
    {
        public const string Text_Date_Format = "[b]Date:[/b] {0}";

        public const string Text_Name_Format = "[b]By:[/b] {0}";

        [NonSerialized]
        private TextureResource cachedIconResource;

        protected BaseFactionEventLogEntry(ICharacter byMember)
        {
            this.ByMemberName = byMember?.Name;
            this.Date = DateTime.Now;
        }

        [CanBeNull]
        public string ByMemberName { get; }

        public string ClientDate => WelcomeMessageSystem.FormatDate(this.Date);

        public virtual bool ClientIsLongNotification => true;

        public virtual NotificationColor ClientNotificationColor => NotificationColor.Neutral;

        /// <summary>
        /// Determines whether the client should display a notification for this event.
        /// </summary>
        public virtual bool ClientShowNotification => false;

        public abstract string ClientText { get; }

        public virtual string ClientTooltipText
        {
            get
            {
                var sb = new StringBuilder()
                    .AppendFormat(Text_Date_Format, this.ClientDate);

                if (!string.IsNullOrEmpty(this.ByMemberName))
                {
                    sb.Append("[br]")
                      .AppendFormat(Text_Name_Format, this.ByMemberName);
                }

                return sb.ToString();
            }
        }

        public DateTime Date { get; }

        public Brush Icon
            => Api.Client.UI.GetTextureBrush(this.IconResource);

        public ITextureResource IconResource
            => this.cachedIconResource
                   ??= GetEventTextureResource(this.GetType());

        public static TextureResource GetEventTextureResource(Type type)
        {
            return new("Icons/FactionEventsLog/"
                       + type.Name.Replace("FactionLogEntry", string.Empty));
        }

        /// <summary>
        /// Called when the log entry is received in the recent events list.
        /// </summary>
        public virtual void ClientOnReceived()
        {
        }
    }
}