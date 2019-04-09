namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// A special helper class to allow only one interaction per time.
    /// </summary>
    public static class ClientSingleSimultaneousInteractionLimiter
    {
        private static readonly HashSet<IGameObject> CurrentlyInteractingWith
            = new HashSet<IGameObject>();

        public static async void InvokeForGameObject(IGameObject gameObject, Func<Task> func)
        {
            if (CurrentlyInteractingWith.Contains(gameObject))
            {
                Api.Logger.Warning(
                    $"Already interacting with {gameObject}: cannot send the new interaction request");
                return;
            }

            Api.ValidateIsClient();

            try
            {
                CurrentlyInteractingWith.Add(gameObject);
                await func();
            }
            finally
            {
                CurrentlyInteractingWith.Remove(gameObject);
            }
        }
    }
}