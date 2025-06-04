using System;

namespace DI
{
    public class DiBuilderRecord
    {
        public Type BindingType { get; set; }
        public Type ResolvingType { get; set; }
        public object Instance { get; set; }
        public bool IsCachingInstance { get; set; }
    }
}