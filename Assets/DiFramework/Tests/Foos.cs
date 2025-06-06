namespace Tests
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
}