using System;

namespace com.shineglow.di.Runtime
{
    public interface IFactory<T> : IFactory where T : IFactoryItem
    {
        T GetInstance();
        void FreeInstance(T instance);
    }
    
    public interface IFactory
    {
        T GetInstance<T>() where T : IFactoryItem;
    }

    public interface IFactoryItem
    {
        event Action OnItemFree;
        protected internal void FreeForce();
    }
}