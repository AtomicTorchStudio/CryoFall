namespace AtomicTorch.CBND.CoreMod.UI.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data;

    public static class ProtoSearchHelper
    {
        public static IEnumerable<T> SearchProto<T>(
            IEnumerable<T> items,
            string searchText)
            where T : IProtoGameObject
        {
            var camelCaseSegments = ConsoleCommandsParametersHelper.GetCamelCaseSegments(searchText);

            return from s in items
                   let indexA = ConsoleCommandsParametersHelper.GetSuggestionMatchIndex(
                       searchText,
                       camelCaseSegments,
                       s.ShortId)
                   let indexB = ConsoleCommandsParametersHelper.GetSuggestionMatchIndex(
                       searchText,
                       camelCaseSegments,
                       s.Name)
                   where indexA >= 0
                         || indexB >= 0
                   orderby Math.Max(indexA, indexB)
                   select s;
        }
    }
}