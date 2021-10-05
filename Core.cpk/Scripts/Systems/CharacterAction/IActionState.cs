namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IActionState
    {
        event Action<double> ProgressPercentsChanged;

        bool IsBlockingMovement { get; }

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