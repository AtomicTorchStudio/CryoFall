namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IActionState
    {
        event Action<double> ProgressPercentsChanged;

        bool IsBlockingActions { get; }

        bool IsBlockingMovement { get; }

        // For mods compatibility
        [Obsolete("Use IsBlockingMovement instead")]
        bool IsBlocksMovement { get; }

        bool IsCancelled { get; }

        bool IsCancelledByServer { get; set; }

        bool IsCompleted { get; }

        bool IsDisplayingProgress { get; }

        double ProgressPercents { get; }

        IWorldObject TargetWorldObject { get; }

        void Cancel();

        void OnStart();

        void SharedUpdate(double deltaTime);
    }
}