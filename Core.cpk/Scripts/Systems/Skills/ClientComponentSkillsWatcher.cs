namespace AtomicTorch.CBND.CoreMod.Systems.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentSkillsWatcher : ClientComponent
    {
        // {0} is the discovered skill title
        public const string NotificationSkillDiscovered = "{0} skill discovered.";

        // {0} is the skill title, {1} is the current skill level.
        public const string NotificationSkillReachedLevel = "{0} level {1}.";

        public static readonly SoundResource SoundResourceSkillDiscovered
            = new SoundResource("UI/Skills/SkillDiscovered");

        public static readonly SoundResource SoundResourceSkillLevelUp
            = new SoundResource("UI/Skills/SkillLevelUp");

        private PlayerCharacterPrivateState privateState;

        private NetworkSyncDictionary<IProtoSkill, SkillLevelData> skillsDictionary;

        public delegate void SkillLevelChangedDelegate(
            IProtoSkill skill,
            SkillLevelData skillLevelData);

        public static event SkillLevelChangedDelegate SkillLevelChanged;

        public void OnSkillLevelChanged(IProtoSkill skill, SkillLevelData data)
        {
            this.OnSkillsChanged();

            Api.SafeInvoke(() => SkillLevelChanged?.Invoke(skill, data));

            var level = data.Level;
            if (level == 0)
            {
                return;
            }

            // show notification
            if (level == 1)
            {
                Client.Audio.PlayOneShot(SoundResourceSkillDiscovered, volume: 0.5f);
                NotificationSystem.ClientShowNotification(
                    string.Format(NotificationSkillDiscovered, skill.Name),
                    message: null,
                    color: NotificationColor.Good,
                    icon: skill.Icon,
                    onClick: OnNotificationClick,
                    playSound: false);
            }
            else
            {
                Client.Audio.PlayOneShot(SoundResourceSkillLevelUp, volume: 0.5f);
                NotificationSystem.ClientShowNotification(
                    string.Format(NotificationSkillReachedLevel, skill.Name, level),
                    color: NotificationColor.Good,
                    icon: skill.Icon,
                    onClick: OnNotificationClick,
                    playSound: false);
            }

            void OnNotificationClick()
            {
                WindowSkills.OpenAndSelectSkill(skill);
            }
        }

        public void Setup(PlayerCharacterPrivateState privateState)
        {
            this.privateState = privateState;
            this.skillsDictionary = privateState.Skills.Skills;
            this.skillsDictionary.ClientPairSet += this.SkillsDictionaryPairSetHandler;
            this.skillsDictionary.ClientPairRemoved += this.SkillsDictionaryClientPairRemoved;

            foreach (var pair in this.skillsDictionary)
            {
                this.RegisterSkill(pair.Key, pair.Value, isInitial: true);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.skillsDictionary.ClientPairSet -= this.SkillsDictionaryPairSetHandler;
            this.skillsDictionary.ClientPairRemoved -= this.SkillsDictionaryClientPairRemoved;
        }

        private void OnSkillsChanged()
        {
            this.privateState.SetFinalStatsCacheIsDirty();
        }

        private void RegisterSkill(IProtoSkill skill, SkillLevelData data, bool isInitial)
        {
            data.ClientSubscribe(
                _ => _.Level,
                _ => this.OnSkillLevelChanged(skill, data),
                this);

            if (!isInitial)
            {
                this.OnSkillLevelChanged(skill, data);
            }
        }

        private void SkillsDictionaryClientPairRemoved(
            NetworkSyncDictionary<IProtoSkill, SkillLevelData> source,
            IProtoSkill key)
        {
            // this method might be called only in the editor mode or when the character is wiped
            // during normal play it's not possible as skill entries only added (and their exp gained)
            this.OnSkillsChanged();

            Api.SafeInvoke(() => SkillLevelChanged?.Invoke(key, default));
        }

        private void SkillsDictionaryPairSetHandler(
            NetworkSyncDictionary<IProtoSkill, SkillLevelData> source,
            IProtoSkill key,
            SkillLevelData value)
        {
            // new skill added
            this.RegisterSkill(key, value, isInitial: false);
            this.OnSkillsChanged();
        }
    }
}