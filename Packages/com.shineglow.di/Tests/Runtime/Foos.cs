using com.shineglow.di.Runtime;

namespace com.shineglow.di.Tests.Runtime
{
    public interface IFoo1{}

    public class Foo1_1 : IFoo1
    {
        public Foo1_1(){}
    }

    public class Foo1_2 : IFoo1
    {
        private Foo1_2(){}
    }
    
    public class Foo1_3 : IFoo1
    {
        public Foo1_3(int a){}
    }

    public class Foo1_4 : IFoo1
    {
        public Foo1_4(IFoo1 foo){}
    }

    public class Foo1_5 : IFoo1
    {
        public Foo1_5(Foo1_1 foo){}
    }
    
    public class Foo1_6 : IFoo1
    {
        public Foo1_6(Foo1_6 foo){}
    }

    public class Foo2_privateField
    {
        [Inject] private IFoo1 _privateFooField;
        public IFoo1 Getter => _privateFooField;
    }
    
    public class Foo2_publicField
    {
        [Inject] public IFoo1 _privateFooField;
    }
    
    public class Foo2_privateField_WithId
    {
        [Inject(Id = "1")] private IFoo1 _privateFooField;
        public IFoo1 Getter => _privateFooField;
    }
    
    public class Foo2_publicField_WithId
    {
        [Inject(Id = "1")] public IFoo1 FooField;
    }
    
    public class Foo2_privateProperty
    {
        [Inject] private IFoo1 _privateFooProperty{get;set;}
        public IFoo1 Getter => _privateFooProperty;
    }
    
    public class Foo2_publicProperty
    {
        [Inject] public IFoo1 FooProperty{get;set;}
    }
    
    public class Foo2_privateProperty_WithId
    {
        [Inject(Id = "1")] private IFoo1 _privateFooProperty{get;set;}
        public IFoo1 Getter => _privateFooProperty;
    }
    
    public class Foo2_publicProperty_WithId
    {
        [Inject(Id = "1")] public IFoo1 FooProperty{get;set;}
    }

    public abstract class Foo3_MethodInjection_Base
    {
        protected IFoo1 _fooField;
        public IFoo1 Getter => _fooField;
    }

    public class PublicMethodInjection : Foo3_MethodInjection_Base
    {
        [Inject]
        public void Init(IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PrivateMethodInjection : Foo3_MethodInjection_Base
    {
        [Inject]
        private void Init(IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PublicMethodInjection_WithId : Foo3_MethodInjection_Base
    {
        [Inject]
        public void Init([Inject(Id="1")]IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PrivateMethodInjection_WithId : Foo3_MethodInjection_Base
    {
        [Inject]
        private void Init([Inject(Id="1")]IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PublicConstructorInjection : Foo3_MethodInjection_Base
    {
        [Inject]
        public PublicConstructorInjection(IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PrivateConstructorInjection : Foo3_MethodInjection_Base
    {
        [Inject]
        private PrivateConstructorInjection(IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PublicConstructorInjection_WithId : Foo3_MethodInjection_Base
    {
        [Inject]
        public PublicConstructorInjection_WithId([Inject(Id="1")]IFoo1 foo)
        {
            _fooField = foo;
        }
    }
    
    public class PrivateConstructorInjection_WithId : Foo3_MethodInjection_Base
    {
        [Inject]
        private PrivateConstructorInjection_WithId([Inject(Id="1")]IFoo1 foo)
        {
            _fooField = foo;
        }
    }
}