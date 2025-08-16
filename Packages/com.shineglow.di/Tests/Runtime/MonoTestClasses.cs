using com.shineglow.di.Runtime;
using UnityEngine;

namespace com.shineglow.di.Tests.Runtime
{
    public class MonoTestClasses
    {
        public class TestMono1 : MonoBehaviour, ITestMono
        {
            [Inject] public Foo1_1 Foo1;
            public Foo1_1 foo1 => Foo1;
        } 
        
        public class TestMono2 : MonoBehaviour, ITestMono
        {
            [Inject] public Foo1_1 Foo1 { get; private set; }
            public Foo1_1 foo1 => Foo1;
        } 
        
        public class TestMono3 : MonoBehaviour, ITestMono
        {
            public Foo1_1 Foo1;
            [Inject]
            private void Construct(Foo1_1 foo) => Foo1 = foo;
            public Foo1_1 foo1 => Foo1;
        } 
    
        public interface ITestMono
        {
            public Foo1_1 foo1 { get; }
        }
    }
}