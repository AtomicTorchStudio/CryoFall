namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public abstract class BaseConsoleCommandParameterType
    {
        public abstract Type ParameterType { get; }

        /// <summary>
        /// Gets the ClientScriptingApi - use only on Client-side.
        /// </summary>
        protected static IClientApi Client => Api.Client;

        /// <summary>
        /// Returns true if current code is executing on Client-side.
        /// </summary>
        protected static bool IsClient => Api.IsClient;

        /// <summary>
        /// Returns true if current code is executing on Server-side.
        /// </summary>
        protected static bool IsServer => Api.IsServer;

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected static ILogger Logger => Api.Logger;

        /// <summary>
        /// Returns ServerApi - use only on Server-side.
        /// </summary>
        protected static IServerApi Server => Api.Server;

        public abstract IEnumerable<string> GetSuggestions();

        /// <summary>
        /// Return null if cannot parse.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryParse(string value, out object result);

        /// <summary>
        /// Gets the instances of proto-classes of the specified type. For example, use IItemType as type parameter to get all
        /// proto-classes of IItemType.
        /// </summary>
        /// <typeparam name="TProtoEntity">Type of proto entity.</typeparam>
        /// <returns>Collection of instances which implements specified prototype.</returns>
        protected static List<TProtoEntity> FindProtoEntities<TProtoEntity>()
            where TProtoEntity : class, IProtoEntity
        {
            return Api.FindProtoEntities<TProtoEntity>();
        }

        /// <summary>
        /// Gets the instance of proto-class by its type.
        /// </summary>
        /// <typeparam name="TProtoEntity">Type of proto entity.</typeparam>
        /// <returns>Instance of proto-class.</returns>
        protected static TProtoEntity GetProtoEntity<TProtoEntity>()
            where TProtoEntity : class, IProtoEntity, new()
        {
            return Api.GetProtoEntity<TProtoEntity>();
        }
    }
}