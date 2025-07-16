using System;

namespace com.shineglow.di.Runtime
{
    public class DiBuilderRecord
    {
        public Type BindingType { get; internal set; }
        public Type ResolvingType { get; internal set; }
        public object Instance { get; internal set; }
        public bool IsCachingInstance { get; internal set; }
        public string Id { get; internal set; }
    }
}