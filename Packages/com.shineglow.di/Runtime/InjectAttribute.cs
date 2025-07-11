using System;

namespace com.shineglow.di.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
    internal sealed class InjectAttribute : Attribute
    {
        internal string Id { get; set; }
    }
}
