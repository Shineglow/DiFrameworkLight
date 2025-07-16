using System;

namespace com.shineglow.di.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter)]
    public sealed class InjectAttribute : Attribute
    {
        public string Id { get; set; }
    }
}
