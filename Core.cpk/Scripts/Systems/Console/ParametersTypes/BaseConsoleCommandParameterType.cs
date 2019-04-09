namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data;

    public abstract class BaseConsoleCommandParameterType : ProtoEntity
    {
        public override string Name => this.Id;

        public abstract Type ParameterType { get; }

        public abstract string ShortDescription { get; }

        public abstract IEnumerable<string> GetSuggestions();

        /// <summary>
        /// Return null if cannot parse.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryParse(string value, out object result);

        protected override void PrepareProto()
        {
            base.PrepareProto();
            ConsoleCommandsParametersHelper.Register(this);
        }
    }
}