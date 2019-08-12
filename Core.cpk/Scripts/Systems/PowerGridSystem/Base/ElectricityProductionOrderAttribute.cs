namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Extensions;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ElectricityProductionOrderAttribute : Attribute
    {
        public readonly Type AfterType;

        public ElectricityProductionOrderAttribute(Type afterType)
        {
            this.AfterType = afterType;
        }

        public static IEnumerable<T> GetDependencies<T>(T entity, IReadOnlyList<T> allObjects)
            where T : class
        {
            var attributes = entity.GetType()
                                   .GetCustomAttributes<ElectricityProductionOrderAttribute>(inherit: true);

            foreach (var attribute in attributes)
            {
                var dependsOnType = attribute.AfterType;
                foreach (var obj in allObjects)
                {
                    if (dependsOnType.IsInstanceOfType(obj))
                    {
                        yield return obj;
                    }
                }
            }
        }
    }
}