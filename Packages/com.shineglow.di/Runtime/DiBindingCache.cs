using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.shineglow.di.Runtime
{
    public class DiBindingCache
    {
        public string BindingId { get; internal set; }
        public Type BindingType { get; internal set; }
        public Type ResolvingType { get; internal set; }
        public object Instance { get; internal set; }
        public CacheProperties Properties { get; internal set; }

        public class CacheProperties
        {
            public bool IsCachingInstance { get; internal set; }
            public ConstructorInfo ConstructorInfo { get; set; }
            public IReadOnlyList<(Type resolveType, string id)> ConstructorProperties { get; set; }
            public IReadOnlyList<(FieldInfo, string)> FieldsToInject { get; set; }
            public IReadOnlyList<(FieldInfo, string)> GeneratedFieldsToInject { get; set; }
            public IReadOnlyList<(MethodInfo, IReadOnlyList<(Type, string)>)> MethodInfosToInject { get; set; }
            public bool HasNonConstructResolves => (FieldsToInject == null || FieldsToInject.Count > 0) && 
                                                   (GeneratedFieldsToInject == null || GeneratedFieldsToInject.Count > 0) &&
                                                   (MethodInfosToInject == null || MethodInfosToInject.Count > 0);
        }
    }
    
    
}