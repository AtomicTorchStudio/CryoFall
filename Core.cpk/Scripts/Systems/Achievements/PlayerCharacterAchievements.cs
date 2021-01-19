namespace AtomicTorch.CBND.CoreMod.Systems.Achievements
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Achievements;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class PlayerCharacterAchievements : BaseNetObject, IPlayerActiveTasksHolder
    {
        private readonly List<CharacterAchievementEntry> serverLockedAchievements
            = new();

        public ICharacter Character => (ICharacter)this.GameObject;

        [SyncToClient]
        public NetworkSyncList<CharacterAchievementEntry> UnlockedAchievements { get; }
            = new();

        public void OnActiveTaskCompletedStateChanged(ServerPlayerActiveTask activeTask)
        {
            var quest = (IProtoAchievement)activeTask.PlayerTask.TaskTarget;
            var achievementEntry = this.SharedFindAchievementEntry(quest, out _);
            achievementEntry.OnActiveTaskCompletedStateChanged(activeTask);
        }

        public void ServerInit()
        {
            Process(this.UnlockedAchievements,     isUnlocked: true);
            Process(this.serverLockedAchievements, isUnlocked: false);

            AchievementsSystem.ServerInitCharacterAchievements(this);

            void Process(IList<CharacterAchievementEntry> list, bool isUnlocked)
            {
                for (var index = 0; index < list.Count; index++)
                {
                    var entry = list[index];
                    if (entry.Achievement is null)
                    {
                        // invalid achievement entry
                        list.RemoveAt(index);
                        index--;
                        continue;
                    }

                    if (isUnlocked)
                    {
                        continue;
                    }

                    entry.ServerEnsureTasksStateIsValid();
                    entry.ServerCreateActiveTasks(this.Character, this);
                }
            }
        }

        public void ServerReset()
        {
            foreach (var achievementEntry in this.UnlockedAchievements)
            {
                achievementEntry.ServerClearStatesAndActiveTasks();
            }

            this.UnlockedAchievements.Clear();

            foreach (var achievementEntry in this.serverLockedAchievements)
            {
                achievementEntry.ServerClearStatesAndActiveTasks();
            }

            this.serverLockedAchievements.Clear();

            Api.Logger.Important("Achievements reset", this.Character);
            this.ServerInit();
        }

        public void ServerTryAddAchievement(IProtoAchievement achievement, bool isUnlocked)
        {
            var achievementEntry = this.SharedFindAchievementEntry(achievement, out _);
            if (achievementEntry is null)
            {
                achievementEntry = new CharacterAchievementEntry(achievement);
            }
            else
            {
                // already has an entry
                if (isUnlocked
                    && this.serverLockedAchievements.Remove(achievementEntry))
                {
                    // this is a locked entry and it was removed
                    achievementEntry.ServerClearStatesAndActiveTasks();
                    // create and add new entry
                    achievementEntry = new CharacterAchievementEntry(achievement);
                }
                else
                {
                    // no need to add the achievement entry
                    return;
                }
            }

            if (isUnlocked)
            {
                this.UnlockedAchievements.Add(achievementEntry);
            }
            else
            {
                this.serverLockedAchievements.Add(achievementEntry);
            }

            if (isUnlocked)
            {
                Api.Logger.Important($"Achievement unlocked: {achievement.ShortId}",
                                     this.Character);
            }

            if (isUnlocked)
            {
                // it will clear the state
                achievementEntry.ServerClearStatesAndActiveTasks();
            }
            else
            {
                achievementEntry.ServerCreateActiveTasks(this.Character, this);
            }
        }

        public void ServerTryRemoveAchievement(IProtoAchievement achievement)
        {
            CharacterAchievementEntry foundAchievementEntry = null;
            foreach (var entry in this.UnlockedAchievements)
            {
                if (entry.Achievement == achievement)
                {
                    foundAchievementEntry = entry;
                }
            }

            if (foundAchievementEntry is null)
            {
                return;
            }

            foundAchievementEntry.ServerClearStatesAndActiveTasks();
            this.UnlockedAchievements.Remove(foundAchievementEntry);
            this.serverLockedAchievements.Remove(foundAchievementEntry);
            Api.Logger.Info("Achievement removed: " + achievement.ShortId, this.Character);

            // add locked achievement
            this.ServerTryAddAchievement(achievement, isUnlocked: false);
        }

        public CharacterAchievementEntry SharedFindAchievementEntry(IProtoAchievement achievement, out bool isUnlocked)
        {
            foreach (var entry in this.UnlockedAchievements)
            {
                if (entry.Achievement == achievement)
                {
                    isUnlocked = true;
                    return entry;
                }
            }

            if (Api.IsServer)
            {
                foreach (var entry in this.serverLockedAchievements)
                {
                    if (entry.Achievement == achievement)
                    {
                        isUnlocked = false;
                        return entry;
                    }
                }
            }

            isUnlocked = false;
            return null;
        }

        public bool SharedHasCompletedAchievement(IProtoAchievement achievement)
        {
            var achievementEntry = this.SharedFindAchievementEntry(achievement, out var isUnlocked);
            if (achievementEntry is null)
            {
                // no such entry
                return false;
            }

            return isUnlocked;
        }

        public class CharacterAchievementEntry : BaseNetObject
        {
            [NonSerialized]
            private List<ServerPlayerActiveTask> serverActiveTasks;

            public CharacterAchievementEntry(IProtoAchievement achievement)
            {
                this.Achievement = achievement;
                this.ServerSetupTaskStates();
            }

            [SyncToClient]
            public IProtoAchievement Achievement { get; }

            private IReadOnlyList<PlayerTaskState> TaskStates { get; set; }

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

            public void ServerClearStatesAndActiveTasks()
            {
                this.ServerResetActiveTasks();
                this.TaskStates = null;
            }

            public void ServerCreateActiveTasks(
                ICharacter character,
                IPlayerActiveTasksHolder activeTasksHolder)
            {
                this.ServerResetActiveTasks();

                var tasks = this.Achievement.Tasks;

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
                    var achievementTask = new ServerPlayerActiveTask(task,
                                                                     taskState,
                                                                     activeTasksHolder,
                                                                     character);
                    this.serverActiveTasks.Add(achievementTask);
                    achievementTask.IsActive = true;
                }

                this.ServerRefreshAllTasksCompleted(character);
            }

            public void ServerEnsureTasksStateIsValid()
            {
                var tasks = this.Achievement.Tasks;
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
                if (this.TaskStates is null)
                {
                    // the achievement is already finished
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
                    var tasks = this.Achievement.Tasks;
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
                    if (completed)
                    {
                        this.ServerSetCompleted(character);
                    }
                }
            }

            public void ServerSetCompleted(ICharacter character)
            {
                this.ServerClearStatesAndActiveTasks();

                // move this achievement from locked to unlocked achievements list
                character.SharedGetAchievements()
                         .ServerTryAddAchievement(this.Achievement, isUnlocked: true);
            }

            private void ServerResetActiveTasks()
            {
                if (this.serverActiveTasks is null)
                {
                    return;
                }

                // deactivate all the active tasks
                foreach (var characterActiveAchievementTask in this.serverActiveTasks)
                {
                    characterActiveAchievementTask.IsActive = false;
                }

                this.serverActiveTasks = null;
            }

            private void ServerSetupTaskStates()
            {
                // ensure active tasks are reset
                this.ServerResetActiveTasks();

                var tasks = this.Achievement.Tasks;
                // we never modify this list but NetworkSyncList is required in order to bind its elements (AchievementTaskState) to the state owner
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