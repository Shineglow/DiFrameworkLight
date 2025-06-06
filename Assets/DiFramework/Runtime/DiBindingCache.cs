using System;

namespace DiFramework.Runtime
{
    public class DiBindingCache
    {
        public Type BindingType { get; internal set; }
        public Type ResolvingType { get; internal set; }
        public object Instance { get; internal set; }
        public CacheProperties Properties { get; internal set; }

        public class CacheProperties
        {
            public bool IsCachingInstance { get; internal set; }
        }
    }
    
    
}