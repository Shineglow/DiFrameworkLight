using DiFramework.Runtime;
using NUnit.Framework;
using Tests;

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
        
        Assert.Catch<MultipleBindingsException>(() => container.Bind<IFoo1>().To<Foo1_5>());
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
}
