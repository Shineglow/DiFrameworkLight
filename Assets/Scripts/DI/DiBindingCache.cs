using System;

namespace DI
{
    public class DiBindingCache
    {
        public Type BindingType { get; set; }
        public Type ResolvingType { get; set; }
        public object Instance { get; set; }
        public CacheProperties Properties { get; set; }

        public class CacheProperties
        {
            public bool IsCachingInstance { get; set; }
        }
    }
    
    
}