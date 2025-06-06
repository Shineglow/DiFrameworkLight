using System;

namespace DiFramework.Runtime
{
    public class DiBuilder<T> : DiBuilder
    {
        private DiBuilderRecord _bindingInfo = new();
        
        public DiBuilder()
        {
            _bindingInfo.BindingType = typeof(T);
        }

        public DiBuilder<T> To<T1>() where T1 : T
        {
            _bindingInfo.ResolvingType = typeof(T1);
            return this;
        }

        public DiBuilder<T> IsCachingInstance()
        {
            _bindingInfo.IsCachingInstance = true;
            return this;
        }

        public DiBuilder<T> AsInstance<T1>(T1 instance) where T1 : T
        {
            if (instance == null)
            {
                throw new ArgumentNullException($"{nameof(instance)} parameter is null");
            }
            _bindingInfo.Instance = instance;
            return this;
        } 

        public override DiBuilderRecord GetRecord()
        {
            DiBuilderRecord result;
            (_bindingInfo, result) = (null, _bindingInfo);
            return result;
        }
    }
    
    public abstract class DiBuilder
    {
        public abstract DiBuilderRecord GetRecord();
    }
}