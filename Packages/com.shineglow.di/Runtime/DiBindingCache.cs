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
            public (Type resolveType, string id)[] ConstructorProperties { get; set; }
            public List<(FieldInfo, string)> FieldsToInject { get; set; }
            public List<(FieldInfo, string)> GeneratedFieldsToInject { get; set; }
            public List<(MethodInfo, List<(ParameterInfo, string)>)> MethodInfosToInject { get; set; }
        }
    }
    
    
}