using NUnit.Framework;
using com.shineglow.di.Runtime;

namespace com.shineglow.di.Tests.Runtime
{
    public class DiTests
    {
        [Test]
        public void DiContainer_SimpleResolve_ResolvedNotNull()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>();
            IFoo1 resolved = container.Resolve<IFoo1>();
        
            Assert.IsTrue(resolved != null);
        }
    
        [Test]
        public void DiContainer_SimpleResolve_ResolvedOfBindedType()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>();
            IFoo1 resolved = container.Resolve<IFoo1>();
        
            Assert.IsTrue(resolved.GetType() == typeof(Foo1_1));
        }
    
        [Test]
        public void DiContainer_SimpleResolve_NotResolveFromPrivateConstructor()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_2>();
        
            Assert.Catch<TypeCannotBeResolvedException>(() => container.Resolve<IFoo1>());
        }
    
        [Test]
        public void DiContainer_SimpleResolve_NotResolveFromConstructorWithBaseTypeParameters()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_3>();
        
            Assert.Catch<TypeCannotBeResolvedException>(() => container.Resolve<IFoo1>());
        }
    
        [Test]
        public void DiContainer_SimpleResolve_NotResolveIfConstructorTypeIsResolvingType()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_4>();
        
            Assert.Catch<TypeCannotBeResolvedException>(() => container.Resolve<IFoo1>());
        }
    
        [Test]
        public void DiContainer_SimpleResolve_ResolveWithResolvingParameters()
        {
            DiContainer container = new DiContainer();
            container.Bind<Foo1_1>();
            container.Bind<IFoo1>().To<Foo1_5>();
            IFoo1 resolved = container.Resolve<IFoo1>();
        
            Assert.IsTrue(resolved != null);
        }
    
        [Test]
        public void DiContainer_SimpleResolve_CannotMultipleBindToOneType()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>();
            container.Bind<IFoo1>().To<Foo1_5>();
            // with InjectAttribute checking for multiple bindings moved to EndBinding method that called in Resolve and Bind methods
            Assert.Catch<MultipleBindingsException>(() => container.Resolve<IFoo1>());
        }
    
        [Test]
        public void DiContainer_SimpleResolve_CannotResolveWhenConstructorParameterTypeEqualsResolvedType()
        {
            DiContainer container = new DiContainer();
            container.Bind<Foo1_6>();
        
            Assert.Catch<TypeCannotBeResolvedException>(() => container.Resolve<Foo1_6>());
        }
    
        [Test]
        public void DiContainer_SimpleResolve_ResolveFromInstance()
        {
            DiContainer container = new DiContainer();
            IFoo1 foo = new Foo1_1();
            container.Bind<IFoo1>().AsInstance(foo);
            IFoo1 resolved = container.Resolve<IFoo1>();
        
            Assert.IsTrue(ReferenceEquals(resolved, foo));
        }
    
        [Test]
        public void DiContainer_SimpleResolve_ResolveCachedInstance()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            IFoo1 resolved1 = container.Resolve<IFoo1>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
        
            Assert.IsTrue(ReferenceEquals(resolved1, resolved2));
        }
        
        [Test]
        public void DiContainer_ResolveWithId_ResolveWillBeCompletedWithoutExceptions()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().WithId("1").IsCachingInstance();
            container.Bind<IFoo1>().To<Foo1_1>().WithId("2").IsCachingInstance();
            IFoo1 resolved1 = container.Resolve<IFoo1>("1");
            IFoo1 resolved2 = container.Resolve<IFoo1>("2");
        
            Assert.IsTrue(!ReferenceEquals(resolved1, resolved2));
        }
        
        [Test]
        public void DiContainer_InjectPublicField_ResolveWillBeCompletedWithoutExceptions()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_publicField>().To<Foo2_publicField>();
            Foo2_publicField resolved1 = container.Resolve<Foo2_publicField>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
        
            Assert.IsTrue(ReferenceEquals(resolved1._privateFooField, resolved2));
        }
        
        [Test]
        public void DiContainer_InjectPrivateField_ResolveWillBeCompletedWithoutExceptions()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_privateField>().To<Foo2_privateField>();
            Foo2_privateField resolved1 = container.Resolve<Foo2_privateField>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
        
            Assert.IsTrue(ReferenceEquals(resolved1.Getter, resolved2));
        }
        
        [Test]
        public void DiContainer_InjectPrivateFieldWithId_IdWillBeCountedOnResolvingAndCorrectInjected()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_privateField_WithId>().To<Foo2_privateField_WithId>();
            Foo2_privateField_WithId resolved1 = container.Resolve<Foo2_privateField_WithId>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
            IFoo1 resolved3 = container.Resolve<IFoo1>("1");
        
            Assert.IsTrue(!ReferenceEquals(resolved1.Getter, resolved2) && ReferenceEquals(resolved1.Getter, resolved3));
        }
        
        [Test]
        public void DiContainer_InjectPublicFieldWithId_IdWillBeCountedOnResolvingAndCorrectInjected()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_publicField_WithId>().To<Foo2_publicField_WithId>();
            Foo2_publicField_WithId resolved1 = container.Resolve<Foo2_publicField_WithId>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
            IFoo1 resolved3 = container.Resolve<IFoo1>("1");
        
            Assert.IsTrue(!ReferenceEquals(resolved1.FooField, resolved2) && ReferenceEquals(resolved1.FooField, resolved3));
        }
        
        [Test]
        public void DiContainer_InjectPublicProperty_ResolveWillBeCompletedWithoutExceptions()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_privateProperty>().To<Foo2_privateProperty>();
            Foo2_privateProperty resolved1 = container.Resolve<Foo2_privateProperty>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
        
            Assert.IsTrue(ReferenceEquals(resolved1.Getter, resolved2));
        }
        
        [Test]
        public void DiContainer_InjectPrivateProperty_ResolveWillBeCompletedWithoutExceptions()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_publicProperty>().To<Foo2_publicProperty>();
            Foo2_publicProperty resolved1 = container.Resolve<Foo2_publicProperty>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
        
            Assert.IsTrue(ReferenceEquals(resolved1.FooProperty, resolved2));
        }
        
        [Test]
        public void DiContainer_InjectPrivatePropertyWithId_IdWillBeCountedOnResolvingAndCorrectInjected()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_privateProperty_WithId>().To<Foo2_privateProperty_WithId>();
            Foo2_privateProperty_WithId resolved1 = container.Resolve<Foo2_privateProperty_WithId>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
            IFoo1 resolved3 = container.Resolve<IFoo1>("1");
        
            Assert.IsTrue(!ReferenceEquals(resolved1.Getter, resolved2) && ReferenceEquals(resolved1.Getter, resolved3));
        }
        
        [Test]
        public void DiContainer_InjectPublicPropertyWithId_IdWillBeCountedOnResolvingAndCorrectInjected()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo2_publicProperty_WithId>().To<Foo2_publicProperty_WithId>();
            Foo2_publicProperty_WithId resolved1 = container.Resolve<Foo2_publicProperty_WithId>();
            IFoo1 resolved2 = container.Resolve<IFoo1>();
            IFoo1 resolved3 = container.Resolve<IFoo1>("1");
        
            Assert.IsTrue(!ReferenceEquals(resolved1.FooProperty, resolved2) && ReferenceEquals(resolved1.FooProperty, resolved3));
        }

        [Test]
        public void DiContainer_InjectByPublicMethod_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PublicMethodInjection>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>();
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPrivateMethod_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PrivateMethodInjection>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>();
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPublicMethodWithId_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PublicMethodInjection_WithId>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>("1");
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPrivateMethodWithId_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PrivateMethodInjection_WithId>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>("1");
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPublicConstructor_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PublicConstructorInjection>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>();
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPrivateConstructor_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PrivateConstructorInjection>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>();
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPublicConstructorWithId_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PublicConstructorInjection_WithId>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>("1");
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
        
        [Test]
        public void DiContainer_InjectByPrivateConstructorWithId_CorrectInjection()
        {
            DiContainer container = new DiContainer();
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance().WithId("1");
            container.Bind<IFoo1>().To<Foo1_1>().IsCachingInstance();
            container.Bind<Foo3_MethodInjection_Base>().To<PrivateConstructorInjection_WithId>();
            var resolve1 = container.Resolve<Foo3_MethodInjection_Base>();
            var resolve2 = container.Resolve<IFoo1>("1");
            
            Assert.IsTrue(ReferenceEquals(resolve1.Getter, resolve2));
        }
    }
}
