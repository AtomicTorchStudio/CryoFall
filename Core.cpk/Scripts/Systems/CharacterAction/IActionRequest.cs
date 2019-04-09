namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public interface IActionRequest : IRemoteCallParameter, IEquatable<IActionRequest>
    {
        [TempOnly]
        ICharacter Character { get; set; }
    }
}