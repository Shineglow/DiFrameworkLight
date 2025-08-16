using com.shineglow.di.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace com.shineglow.di.Tests.Runtime
{
    public class DiInstanceInjectTests
    {
        [Test]
        public void InjectIntoInstanceField()
        {
            InjectToObjectTemplate<MonoTestClasses.TestMono1>();
        }
        
        [Test]
        public void InjectIntoInstanceProperty()
        {
            InjectToObjectTemplate<MonoTestClasses.TestMono2>();
        }
        
        [Test]
        public void InjectIntoInstanceMethod()
        {
            InjectToObjectTemplate<MonoTestClasses.TestMono3>();
        }
        
        [Test]
        public void InjectIntoGameObjectComponentField()
        {
            InjectToObjectTemplate<MonoTestClasses.TestMono1>();
        }
        
        [Test]
        public void InjectIntoGameObjectComponentProperty()
        {
            InjectToObjectTemplate<MonoTestClasses.TestMono2>();
        }
        
        [Test]
        public void InjectIntoGameObjectComponentMethod()
        {
            InjectToObjectTemplate<MonoTestClasses.TestMono3>();
        }
        
        
        public void InjectToObjectTemplate<T>() where T : MonoBehaviour, MonoTestClasses.ITestMono
        {
            DiContainer container = new DiContainer();
            container.Bind<Foo1_1>().To<Foo1_1>().IsCachingInstance();
            
            MonoTestClasses.ITestMono obj = new GameObject().AddComponent<T>();
            container.InjectIntoInstance(obj);
            
            Assert.IsNotNull(obj.foo1);
        }
        
        public void InjectToGameObjectTemplate<T>() where T : MonoBehaviour, MonoTestClasses.ITestMono
        {
            DiContainer container = new DiContainer();
            container.Bind<Foo1_1>().To<Foo1_1>().IsCachingInstance();

            var gameObject = new GameObject("name", typeof(T));
            MonoTestClasses.ITestMono obj = gameObject.GetComponent<T>();
            container.InjectIntoGameObject(gameObject);
            
            Assert.IsNotNull(obj.foo1);
        }
    }
}