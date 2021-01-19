namespace AtomicTorch.CBND.CoreMod.Editor.Scripts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class EditorClientActionsHistorySystem
    {
        private const int MaxHistoryEntries = 200;

        private static readonly List<EditorAction> Actions
            = new();

        public static void DoAction(
            string title,
            Action onDo,
            Action onUndo,
            bool canGroupWithPreviousAction)
        {
            Api.ValidateIsClient();

            var editorAction = new EditorAction(title, onDo, onUndo);
            // remove all non-done editor actions
            Actions.RemoveAll(a => !a.IsDone);
            var lastAction = Actions.LastOrDefault();
            if (lastAction is not null
                && canGroupWithPreviousAction
                && lastAction.TryGroupWith(editorAction))
            {
                // actions are grouped together. No need to call editorAction.Do().
                return;
            }

            Actions.Add(editorAction);

            if (Actions.Count > MaxHistoryEntries)
            {
                Actions.RemoveRange(0, Actions.Count - MaxHistoryEntries);
            }

            editorAction.Do();
        }

        /// <summary>
        /// Removes all actions in the buffer so it's no longer possible to invoke undo.
        /// Should be used only when a non-undoable action was performed (such as world create/load).
        /// </summary>
        public static void Purge()
        {
            Actions.Clear();
            Api.Logger.Important("Editor actions history purged.");
        }

        public static void Redo()
        {
            var action = Actions.FirstOrDefault(a => !a.IsDone);
            if (action is not null)
            {
                action.Do();
            }
            else
            {
                Api.Logger.Important("Nothing to redo");
            }
        }

        public static void Undo()
        {
            var action = Actions.LastOrDefault(a => a.IsDone);
            if (action is not null)
            {
                action.Undo();
            }
            else
            {
                Api.Logger.Important("Nothing to undo");
            }
        }

        public class EditorAction
        {
            private const double ActionsGroupingMaxTimeIntervalSeconds = 2.0;

            public readonly double TimestampSeconds = Api.Client.Core.ClientRealTime;

            public readonly string Title;

            private readonly List<Action> listDo = new(capacity: 1);

            private readonly List<Action> listUndo = new(capacity: 1);

            private bool isDone;

            public EditorAction(string title, Action onDo, Action onUndo)
            {
                this.Title = title;
                this.listDo.Add(onDo);
                this.listUndo.Add(onUndo);
            }

            public bool IsDone => this.isDone;

            public void Do()
            {
                if (this.isDone)
                {
                    throw new Exception(this + " is already done");
                }

                this.isDone = true;
                Api.Logger.Important("Do: " + this);
                foreach (var action in this.listDo)
                {
                    action.Invoke();
                }
            }

            public override string ToString()
            {
                return "Editor action: " + this.Title;
            }

            /// <summary>
            /// We allow grouping of the similar actions during the restricted time interval.
            /// </summary>
            /// <param name="otherAction"></param>
            /// <returns>True if groupped successfully.</returns>
            public bool TryGroupWith(EditorAction otherAction)
            {
                if (this.Title != otherAction.Title)
                {
                    return false;
                }

                var timeDelta = otherAction.TimestampSeconds - this.TimestampSeconds;
                if (timeDelta < 0
                    || timeDelta > ActionsGroupingMaxTimeIntervalSeconds)
                {
                    return false;
                }

                if (this.isDone)
                {
                    otherAction.Do();
                }

                this.listDo.AddRange(otherAction.listDo);
                this.listUndo.AddRange(otherAction.listUndo);
                otherAction.listDo.Clear();
                otherAction.listUndo.Clear();

                return true;
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            public void Undo()
            {
                if (!this.isDone)
                {
                    throw new Exception(this + " is not done");
                }

                this.isDone = false;
                Api.Logger.Important("Undo: " + this);

                // undo actions must be played in reverse order
                for (var index = this.listUndo.Count - 1; index >= 0; index--)
                {
                    var action = this.listUndo[index];
                    action.Invoke();
                }
            }
        }
    }
}