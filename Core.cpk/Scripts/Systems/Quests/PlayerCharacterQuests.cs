namespace AtomicTorch.CBND.CoreMod.Systems.Quests
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PlayerCharacterQuests : BaseNetObject, IPlayerActiveTasksHolder
    {
        private readonly List<CharacterQuestEntry> serverLockedQuests
            = new();

        static PlayerCharacterQuests()
        {
            PartySystem.ServerCharacterJoinedOrLeftParty += ServerCharacterJoinedOrLeftPartyHandler;
        }

        public ICharacter Character => (ICharacter)this.GameObject;

        [SyncToClient]
        public NetworkSyncList<CharacterQuestEntry> Quests { get; }
            = new();

        public void OnActiveTaskCompletedStateChanged(ServerPlayerActiveTask activeTask)
        {
            var quest = (IProtoQuest)activeTask.PlayerTask.TaskTarget;
            var questEntry = this.SharedFindQuestEntry(quest, out _);
            questEntry.OnActiveTaskCompletedStateChanged(activeTask);
        }

        /// <summary>
        /// Please note - it will throw an exception if the reward cannot be claimed or the quest is already completed.
        /// </summary>
        public void ServerClaimReward(IProtoQuest quest, bool ignoreTasks)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out bool questEntryIsUnlocked);
            if (questEntry is null)
            {
                throw new Exception("No quest entry found for: " + quest);
            }

            if (!questEntryIsUnlocked)
            {
                throw new Exception("The quest is not unlocked " + quest);
            }

            if (!ignoreTasks
                && !questEntry.AreAllTasksCompleted)
            {
                throw new Exception("The quest tasks are not completed " + quest);
            }

            if (questEntry.IsCompleted)
            {
                throw new Exception("The quest is already completed and the reward is already claimed " + quest);
            }

            questEntry.ServerSetCompleted();

            var rewardLearningPoints = quest.RewardLearningPoints;
            rewardLearningPoints = (ushort)Math.Round(
                rewardLearningPoints * TechConstants.ServerLearningPointsGainMultiplier,
                MidpointRounding.AwayFromZero);
            this.Character.SharedGetTechnologies()
                .ServerAddLearningPoints(rewardLearningPoints, allowModifyingByStatsAndRates: false);

            Api.Logger.Important(
                $"Quest completed and reward claimed: {quest.ShortId}. Learning points added: {rewardLearningPoints}",
                this.Character);
        }

        public void ServerInit()
        {
            Process(this.Quests);
            Process(this.serverLockedQuests);

            QuestsSystem.ServerInitCharacterQuests(this);

            void Process(IList<CharacterQuestEntry> list)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var entry = list[index];
                    if (entry.Quest is null)
                    {
                        // invalid quest entry
                        list.RemoveAt(index);
                        index--;
                        continue;
                    }

                    if (entry.IsCompleted)
                    {
                        continue;
                    }

                    entry.ServerEnsureTasksStateIsValid();
                    entry.ServerCreateActiveTasks(this.Character, this);
                }
            }
        }

        public void ServerRemoveNewFlag(IProtoQuest quest)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out var isUnlocked);
            if (questEntry is null)
            {
                throw new Exception("No quest entry for: " + quest);
            }

            if (!isUnlocked)
            {
                throw new Exception("Cannot remove the new flag for the locked quest entry: " + quest);
            }

            questEntry.IsNew = false;
        }

        public void ServerReset()
        {
            foreach (var questEntry in this.Quests)
            {
                questEntry.ServerDestroy();
            }

            this.Quests.Clear();

            foreach (var questEntry in this.serverLockedQuests)
            {
                questEntry.ServerDestroy();
            }

            this.serverLockedQuests.Clear();

            Api.Logger.Important("Quests reset", this.Character);
            this.ServerInit();
        }

        public void ServerTryAddQuest(IProtoQuest quest, bool isUnlocked)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out _);
            if (questEntry is null)
            {
                questEntry = new CharacterQuestEntry(quest);
            }
            else
            {
                // already has an entry
                if (isUnlocked
                    && this.serverLockedQuests.Remove(questEntry))
                {
                    // this is a locked entry and it was removed - it will be added to the unlocked quests list
                }
                else
                {
                    // no need to add the quest entry
                    return;
                }
            }

            if (isUnlocked)
            {
                this.Quests.Add(questEntry);
            }
            else
            {
                this.serverLockedQuests.Add(questEntry);
            }

            //Api.Logger.Important($"Quest added: {quest.ShortId} as {(isUnlocked ? "unlocked" : "locked")}",
            //                     this.Character);

            questEntry.ServerCreateActiveTasks(this.Character, this);
        }

        public void ServerTryRemoveQuest(IProtoQuest quest)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out _);
            if (questEntry is null)
            {
                // no such entry
                return;
            }

            questEntry.ServerDestroy();
            this.Quests.Remove(questEntry);
            this.serverLockedQuests.Remove(questEntry);
            Api.Logger.Important("Quest removed: " + quest.ShortId, this.Character);
        }

        public CharacterQuestEntry SharedFindQuestEntry(IProtoQuest quest, out bool isUnlocked)
        {
            foreach (var entry in this.Quests)
            {
                if (entry.Quest == quest)
                {
                    isUnlocked = true;
                    return entry;
                }
            }

            if (Api.IsServer)
            {
                foreach (var entry in this.serverLockedQuests)
                {
                    if (entry.Quest == quest)
                    {
                        isUnlocked = false;
                        return entry;
                    }
                }
            }

            isUnlocked = false;
            return null;
        }

        public bool SharedHasCompletedQuest(IProtoQuest quest)
        {
            var questEntry = this.SharedFindQuestEntry(quest, out var isUnlocked);
            if (questEntry is null
                || !isUnlocked)
            {
                // no such entry
                return false;
            }

            return questEntry.IsCompleted;
        }

        public bool SharedHasCompletedTask(IPlayerTask task)
        {
            if (!(task.TaskTarget is IProtoQuest quest))
            {
                throw new Exception("Task must have an associated quest");
            }

            var questEntry = this.SharedFindQuestEntry(quest, out _);
            if (questEntry is null)
            {
                // no such entry
                return false;
            }

            if (questEntry.IsCompleted
                || questEntry.AreAllTasksCompleted)
            {
                // all tasks are/were completed
                return true;
            }

            // find a state for this task and check whether it's completed
            var indexOfTask = -1;
            var tasks = quest.Tasks;
            if (tasks is null)
            {
                return false;
            }

            for (var index = 0; index < tasks.Count; index++)
            {
                var r = tasks[index];
                if (r != task)
                {
                    continue;
                }

                indexOfTask = index;
                break;
            }

            Api.Assert(indexOfTask >= 0, "Task not exist in quest");
            var taskState = questEntry.TaskStates[indexOfTask];
            return taskState.IsCompleted;
        }

        private static void ServerCharacterJoinedOrLeftPartyHandler(
            ICharacter character,
            ILogicObject party,
            bool isJoined)
        {
            var playerCharacterQuests = character.SharedGetQuests();
            foreach (var quest in playerCharacterQuests.Quests)
            {
                quest.ServerRefreshAllTasksCompleted(character);
            }
        }

        public class CharacterQuestEntry : BaseNetObject
        {
            [NonSerialized]
            private List<ServerPlayerActiveTask> serverActiveTasks;

            public CharacterQuestEntry(IProtoQuest quest)
            {
                this.Quest = quest;
                this.ServerSetupTaskStates();
            }

            /// <summary>
            /// When all tasks are completed, player can claim the reward and then <see cref="IsCompleted" /> should be set to
            /// true.
            /// </summary>
            [SyncToClient]
            public bool AreAllTasksCompleted { get; private set; }

            [SyncToClient]
            public bool IsCompleted { get; private set; }

            [SyncToClient(isSendChanges: false)]
            public bool IsNew { get; set; } = true;

            [SyncToClient]
            public IProtoQuest Quest { get; }

            [SyncToClient]
            public IReadOnlyList<PlayerTaskState> TaskStates { get; private set; }

            public void OnActiveTaskCompletedStateChanged(ServerPlayerActiveTask activeTask)
            {
                var isCompleted = activeTask.PlayerTaskState.IsCompleted;
                for (var index = 0; index < this.serverActiveTasks.Count; index++)
                {
                    var entry = this.serverActiveTasks[index];
                    if (entry != activeTask)
                    {
                        continue;
                    }

                    // found entry
                    if (isCompleted
                        && !activeTask.PlayerTask.IsReversible)
                    {
                        // remove this active task entry as it's completed and cannot be reversed
                        activeTask.IsActive = false;
                        this.serverActiveTasks.RemoveAt(index);
                    }

                    this.ServerRefreshAllTasksCompleted(activeTask.Character);
                    return;
                }

                Api.Logger.Error("Unknown task completed: " + activeTask);
            }

            public void ServerCreateActiveTasks(
                ICharacter character,
                IPlayerActiveTasksHolder activeTasksHolder)
            {
                this.ServerResetActiveTasks();
                if (this.IsCompleted)
                {
                    // the quest is completed - no active tasks required in that case
                    return;
                }

                var tasks = this.Quest.Tasks;

                this.serverActiveTasks = new List<ServerPlayerActiveTask>(tasks.Count);

                // refresh the task states
                for (byte index = 0; index < tasks.Count; index++)
                {
                    var task = tasks[index];
                    var taskState = this.TaskStates[index];
                    task.ServerRefreshIsCompleted(character, taskState);

                    if (taskState.IsCompleted
                        && !task.IsReversible)
                    {
                        continue;
                    }

                    // create active task entry
                    var questTask = new ServerPlayerActiveTask(task,
                                                               taskState,
                                                               activeTasksHolder,
                                                               character);
                    this.serverActiveTasks.Add(questTask);
                    questTask.IsActive = true;
                }

                this.ServerRefreshAllTasksCompleted(character);
            }

            public void ServerDestroy()
            {
                this.ServerResetActiveTasks();
            }

            public void ServerEnsureTasksStateIsValid()
            {
                var tasks = this.Quest.Tasks;
                var isValid = this.TaskStates.Count == tasks.Count;
                try
                {
                    if (!isValid)
                    {
                        return;
                    }

                    // check that the task state types are matching
                    for (byte index = 0; index < tasks.Count; index++)
                    {
                        var task = tasks[index];
                        var taskState = this.TaskStates[index];
                        if (task.IsStateTypeMatch(taskState))
                        {
                            continue;
                        }

                        isValid = false;
                        return;
                    }
                }
                finally
                {
                    if (!isValid)
                    {
                        this.ServerSetupTaskStates();
                    }
                }
            }

            public void ServerRefreshAllTasksCompleted(ICharacter character)
            {
                if (this.IsCompleted)
                {
                    // the quest is already completed
                    return;
                }

                var completed = false;
                try
                {
                    foreach (var state in this.TaskStates)
                    {
                        if (!state.IsCompleted)
                        {
                            // not all tasks are completed
                            return;
                        }
                    }

                    // ensure that all task states are updated and really completed
                    var tasks = this.Quest.Tasks;
                    for (byte index = 0; index < tasks.Count; index++)
                    {
                        var task = tasks[index];
                        var taskState = this.TaskStates[index];
                        task.ServerRefreshIsCompleted(character, taskState);
                    }

                    foreach (var state in this.TaskStates)
                    {
                        if (!state.IsCompleted)
                        {
                            // not all tasks are completed
                            return;
                        }
                    }

                    completed = true;
                }
                finally
                {
                    if (completed
                        && !this.AreAllTasksCompleted)
                    {
                        //Api.Logger.Important("Quest tasks became completed: " + this.Quest.ShortId,
                        //                     character);
                        this.AreAllTasksCompleted = true;
                    }
                    else if (!completed
                             && this.AreAllTasksCompleted)
                    {
                        //Api.Logger.Important("Quest tasks became incomplete: " + this.Quest.ShortId,
                        //                     character);
                        this.AreAllTasksCompleted = false;
                    }
                }
            }

            public void ServerSetCompleted()
            {
                this.ServerResetActiveTasks();
                this.IsCompleted = true;
                this.AreAllTasksCompleted = true;
                this.IsNew = false;
                this.TaskStates = null;
            }

            private void ServerResetActiveTasks()
            {
                if (this.serverActiveTasks is null)
                {
                    return;
                }

                // deactivate all the active tasks
                foreach (var characterActiveQuestTask in this.serverActiveTasks)
                {
                    characterActiveQuestTask.IsActive = false;
                }

                this.serverActiveTasks = null;
            }

            private void ServerSetupTaskStates()
            {
                Api.Assert(!this.IsCompleted, "Cannot create task states for completed quest");

                // ensure active tasks are reset
                this.ServerResetActiveTasks();

                var tasks = this.Quest.Tasks;
                // we never modify this list but NetworkSyncList is required in order to bind its elements (QuestTaskState) to the state owner
                var taskStates = new NetworkSyncList<PlayerTaskState>(tasks.Count);

                foreach (var task in tasks)
                {
                    var state = task.CreateState();
                    taskStates.Add(state);
                }

                this.TaskStates = taskStates;
            }
        }
    }
}