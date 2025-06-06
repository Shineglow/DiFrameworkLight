using System;

namespace DiFramework.Runtime
{
    public class TypeCannotBeResolvedException : Exception
    {
        public TypeCannotBeResolvedException(){}
        public TypeCannotBeResolvedException(string text) : base(text){}
    }
    
    public class MultipleBindingsException : Exception
    {
        public MultipleBindingsException(){}
        public MultipleBindingsException(string text) : base(text){}
    }
}