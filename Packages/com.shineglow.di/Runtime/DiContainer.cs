using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DiFramework.Runtime
{
    public class DiContainer
    {
        private readonly Dictionary<Type, DiBindingCache> _typeToCaches = new();
        private readonly Dictionary<Type, IFactory> _typeToFactories = new();

        private DiBuilder _typeBuilder;

        public DiBuilder<T> Bind<T>()
        {
            EndBinding();

            var bindingType = typeof(T);
            if (_typeToCaches.ContainsKey(bindingType))
            {
                throw new
                    MultipleBindingsException($"The container already contains binding for type {bindingType.Name}");
            }

            DiBuilder<T> result = new DiBuilder<T>();
            _typeBuilder = result;
            return result;
        }

        public T Resolve<T>()
        {
            EndBinding();
            return (T)ResolveFromConstructor(typeof(T));
        }

        public void RegisterFactory<T>(IFactory<T> factory) where T : IFactoryItem
        {
            if (factory == null)
            {
                throw new ArgumentNullException($"{nameof(factory)} parameter is null.");
            }

            if (_typeToFactories.TryGetValue(typeof(T), out var cache))
            {
                throw new KeyNotFoundException($"Factory already exist for type {typeof(T)}");
            }
        }
        
        private void EndBinding()
        {
            if (_typeBuilder == null)
            {
                return;
            }

            var record = _typeBuilder.GetRecord();
            record.ResolvingType ??= record.BindingType;
            var bindingCache =
                new DiBindingCache()
                {
                    BindingType = record.BindingType,
                    ResolvingType = record.ResolvingType,
                    Instance = record.Instance,
                    Properties =
                        new DiBindingCache.CacheProperties()
                        {
                            IsCachingInstance = record.IsCachingInstance,
                        }
                };
            _typeToCaches.Add(record.BindingType, bindingCache);
            _typeBuilder = null;
        }

        private object ResolveFromConstructor(Type type)
        {
            EndBinding();
            object result = null;

            if (type == null)
            {
                throw new ArgumentNullException($"{nameof(type)} parameter is null.");
            }

            if (!_typeToCaches.TryGetValue(type, out var cache))
            {
                throw new KeyNotFoundException($"No bindings for type {type.Name}");
            }

            if (cache.Instance != null)
            {
                result = cache.Instance;
            }
            else
            {
                result = CreateInstanceFromConstructor(cache);

                if (cache.Properties.IsCachingInstance)
                {
                    cache.Instance = result;
                }
            }

            return result;
        }

        private object CreateInstanceFromConstructor(DiBindingCache cache)
        {
            object result;
            var constructorInfo = GetConstructor(cache);
            if (constructorInfo == null)
            {
                throw new
                    TypeCannotBeResolvedException($"Unable to create an object of type {cache.ResolvingType}. There is no empty constructor or it is impossible to get copies of the required types. Try changing the resolved object or registering types in the container.");
            }

            var resolvedParameters = GetResolveParameters(constructorInfo);
            result = constructorInfo.Invoke(resolvedParameters);
            return result;
        }

        private object[] GetResolveParameters(ConstructorInfo constructorInfo)
        {
            var constructorParameters = constructorInfo.GetParameters();
            object[] resolvedParameters = new object[constructorParameters.Length];

            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var parameterInfo = constructorParameters[i];
                resolvedParameters[i] = ResolveFromConstructor(parameterInfo.ParameterType);
            }

            return resolvedParameters;
        }

        private ConstructorInfo GetConstructor(DiBindingCache cache)
        {
            ConstructorInfo constructorInfo = null;
            var constructorInfos = cache.ResolvingType.GetConstructors();
            foreach (var info in constructorInfos)
            {
                var parameters = info.GetParameters();
                if (parameters.Length != 0)
                {
                    if (parameters.Any(i =>
                                           IsBasicType(i.ParameterType)
                                        || !CanGetInstanceOfType(i.ParameterType)
                                        || IsParameterTypeEqualTypeOfResolvingObject(cache.ResolvingType,
                                               cache.BindingType, i.ParameterType))
                       )
                    {
                        continue;
                    }
                }

                constructorInfo = info;
            }

            return constructorInfo;
        }

        private static bool IsParameterTypeEqualTypeOfResolvingObject(Type resolvingType, Type bindingType,
                                                                      Type parameterType)
        {
            return (parameterType == resolvingType || parameterType == bindingType);
        }

        public bool CanGetInstanceOfType(Type type)
        {
            bool result = false;
            result = _typeToCaches.ContainsKey(type);
            return result;
        }

        public static bool IsBasicType(Type type)
        {
            return type.IsPrimitive
                || type == typeof(string)
                || type == typeof(decimal);
        }
    }
}