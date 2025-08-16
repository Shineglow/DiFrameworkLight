using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace com.shineglow.di.Runtime
{
    public class DiContainer
    {
        private readonly Dictionary<(Type, string), DiBindingCache> _typeToCaches = new();
        private readonly Dictionary<(Type, string), DiBindingCache> _instanceInjectCache = new();

        private DiBuilder _typeBuilder;

        public DiBuilder<T> Bind<T>()
        {
            EndBinding();
            DiBuilder<T> result = new DiBuilder<T>();
            _typeBuilder = result;
            return result;
        }

        public T Resolve<T>(string id = null)
        {
            var instance = (T)Resolve(typeof(T), id);
            return instance;
        }

        public T Resolve<T>(Type type, string id)
        {
            var instance = (T)Resolve(type, id);
            return instance;
        }

        public void InjectIntoGameObject(GameObject gameObject, string id = null)
        {
            foreach (var component in gameObject.GetComponents<MonoBehaviour>())
            {
                InjectIntoInstance(component, id);
            }
        }

        public void InjectIntoInstance(object instance, string id = null)
        {
            EndBinding();
            Type type = instance.GetType();
            if (_instanceInjectCache.TryGetValue((type, id), out var cache))
            {
                if(!cache.Properties.HasNonConstructResolves)
                    return;
            }
            else
            {
                cache = new DiBindingCache()
                {
                    BindingId = id,
                    BindingType = type,
                    ResolvingType = type,
                    Instance = null,
                    Properties = new DiBindingCache.CacheProperties()
                };
                _instanceInjectCache.Add((type, id), cache);
            }
            
            InjectFieldsByAttributes(instance, cache);
            InjectPropertiesByAttributes(instance, cache);
            InjectMethodsByAttributes(instance, cache);
        }

        private object Resolve(Type type, string id = null)
        {
            EndBinding();
            if (!_typeToCaches.TryGetValue((type, id), out var cache))
            {
                throw new KeyNotFoundException($"No bindings for type {type.Name}");
            }
            
            object result = ResolveFromConstructor(type, cache);
            InjectFieldsByAttributes(result, cache);
            InjectPropertiesByAttributes(result, cache);
            InjectMethodsByAttributes(result, cache);
            return result;
            
            object ResolveFromConstructor(Type typeLoc, DiBindingCache cacheLoc)
            {
                object result = null;

                if (typeLoc == null)
                {
                    throw new ArgumentNullException($"{nameof(typeLoc)} parameter is null.");
                }

                if (cacheLoc.Instance != null)
                {
                    result = cacheLoc.Instance;
                }
                else
                {
                    result = CreateInstanceFromConstructor(cacheLoc);

                    if (cacheLoc.Properties.IsCachingInstance)
                    {
                        cacheLoc.Instance = result;
                    }
                }

                return result;
            
                object CreateInstanceFromConstructor(DiBindingCache cache)
                {
                    var constructorInfo = cache.Properties.ConstructorInfo ??= GetTheHighestPriorityConstructor(cache);
                    if (constructorInfo == null)
                    {
                        throw new
                            TypeCannotBeResolvedException($"Unable to create an object of type {cache.ResolvingType}. There is no empty constructor or it is impossible to get copies of the required types. Try changing the resolved object or registering types in the container.");
                    }

                    cache.Properties.ConstructorProperties ??= GetMethodBaseParametersList(constructorInfo);
                    var resolvedParameters = GetResolvedObjectsFromArray(cache.Properties.ConstructorProperties);
                    var result = constructorInfo.Invoke(resolvedParameters);
                    return result;
                }
            }
        }

        private void EndBinding()
        {
            if (_typeBuilder == null)
            {
                return;
            }
            var record = _typeBuilder.GetRecord();
            var resolvingKey = (record.BindingType, record.Id);
            if (_typeToCaches.ContainsKey(resolvingKey))
            {
                throw new
                    MultipleBindingsException($"The container already contains binding for type {record.BindingType.Name}");
            }
           
            record.ResolvingType ??= record.BindingType;
            var bindingCache =
                new DiBindingCache()
                {
                    BindingId = record.Id,
                    BindingType = record.BindingType,
                    ResolvingType = record.ResolvingType,
                    Instance = record.Instance,
                    Properties =
                        new DiBindingCache.CacheProperties()
                        {
                            IsCachingInstance = record.IsCachingInstance,
                        }
                };

            _typeToCaches.Add(resolvingKey, bindingCache);
            _typeBuilder = null;
        }

        #region InjectByAttributeMethods

        private void InjectFieldsByAttributes(object instance, DiBindingCache cache)
        {
            cache.Properties.FieldsToInject ??= GetFieldsToInject(cache.ResolvingType);
            
            foreach (var (fieldInfo, id) in cache.Properties.FieldsToInject)
            {
                fieldInfo.SetValue(instance, Resolve(fieldInfo.FieldType, id));
            }

            List<(FieldInfo, string)> GetFieldsToInject(Type typeLoc)
            {
                List<(FieldInfo, string)> result = new();
                foreach (var memberInfo in typeLoc.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = memberInfo.GetCustomAttribute<InjectAttribute>();
                    if (attribute != null)
                    {
                        result.Add((memberInfo, attribute.Id));
                    }
                }

                return result;
            }
        }

        private void InjectPropertiesByAttributes(object instance, DiBindingCache cache)
        {
            cache.Properties.GeneratedFieldsToInject ??= GetGeneratedFieldsToInject(cache.ResolvingType);
            
            foreach (var (propertyField, id) in cache.Properties.GeneratedFieldsToInject)
            {
                propertyField.SetValue(instance, Resolve(propertyField.FieldType, id));
            }
            
            List<(FieldInfo, string)> GetGeneratedFieldsToInject(Type typeLoc)
            {
                List<(FieldInfo, string)> result = new();
                foreach (var memberInfo in typeLoc.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = memberInfo.GetCustomAttribute<InjectAttribute>();
                    if (attribute != null)
                    {
                        var propertyField = typeLoc.GetField($"<{memberInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (propertyField == null)
                        {
                            throw new GeneratedMembersAccessException($"Can not find BackingField of {memberInfo.Name} property of {typeLoc.Name} type");
                        }
                        result.Add((propertyField, attribute.Id));
                    }
                }

                return result;
            }
        }

        private void InjectMethodsByAttributes(object instance, DiBindingCache cache)
        {
            cache.Properties.MethodInfosToInject ??= GetMethodInfosToInject(cache.ResolvingType);
            
            foreach (var (methodInfo, propertiesWithIds) in cache.Properties.MethodInfosToInject)
            {
                object[] resolved = new object[propertiesWithIds.Count];
                for (var index = 0; index < propertiesWithIds.Count; index++)
                {
                    var (parameterType, id) = propertiesWithIds[index];
                    resolved[index] = Resolve(parameterType, id);
                }
                methodInfo.Invoke(instance, resolved);
            }

            IReadOnlyList<(MethodInfo, IReadOnlyList<(Type, string)>)> GetMethodInfosToInject(Type typeLoc)
            {
                List<(MethodInfo, IReadOnlyList<(Type, string)>)> result = new();
                foreach (var methodInfo in typeLoc.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = methodInfo.GetCustomAttribute<InjectAttribute>();
                    if (attribute != null)
                    {
                        result.Add((methodInfo, GetMethodBaseParametersList(methodInfo)));
                    }
                }

                return result;
            }
        }

        #endregion

        #region MassiveReflectionFunctions

        private IReadOnlyList<(Type resolveType, string id)> GetMethodBaseParametersList(MethodBase constructorInfo)
        {
            var parametersArray = constructorInfo.GetParameters();
            (Type, string)[] parametersList = new (Type, string)[parametersArray.Length];

            for (var i = 0; i < parametersArray.Length; i++)
            {
                var parameterInfo = parametersArray[i];
                parametersList[i] = (parameterInfo.ParameterType,
                                     parameterInfo.GetCustomAttribute<InjectAttribute>()?.Id);
            }

            return parametersList;
        }

        private object[] GetResolvedObjectsFromArray(IReadOnlyList<(Type, string)> parameters)
        {
            object[] resolvedParameters = new object[parameters.Count];

            for (var i = 0; i < parameters.Count; i++)
            {
                var parameterInfo = parameters[i];
                resolvedParameters[i] = Resolve(parameterInfo.Item1, parameterInfo.Item2);
            }

            return resolvedParameters;
        }

        private ConstructorInfo GetTheHighestPriorityConstructor(DiBindingCache cache)
        {
            ConstructorInfo constructorInfo = null;
            var constructorInfos = cache.ResolvingType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            var infos = constructorInfos.OrderBy(i => i.GetCustomAttribute<InjectAttribute>() != null);
            
            foreach (var info in infos)
            {
                var attribute = info.GetCustomAttribute<InjectAttribute>();
                if (attribute == null || cache.BindingId == null || cache.BindingId.Equals(attribute.Id))
                {
                    if (IsConstructorInfoValid(info))
                    {
                        constructorInfo = info;
                        break;
                    }
                }
            }
            
            return constructorInfo;

            #region ParametersValidationFunctions
            bool IsConstructorInfoValid(ConstructorInfo info)
            {
                var parameters = info.GetParameters();
                if (parameters.Length != 0)
                {
                    if (parameters.Any(i =>
                                           IsBasicType(i.ParameterType)
                                           || !CanGetInstanceOfType(i.ParameterType, i.GetCustomAttribute<InjectAttribute>()?.Id)
                                           || IsTypesHaveNotCyclicDependencies(cache.ResolvingType,
                                                                               cache.BindingType, i.ParameterType))
                       )
                    {
                        return false;
                    }
                }

                return true;

                bool IsTypesHaveNotCyclicDependencies(Type resolvingType, Type bindingType, Type parameterType)
                {
                    return CompareTypes(parameterType, resolvingType) || CompareTypes(parameterType, bindingType);

                    bool CompareTypes(Type a, Type b)
                    {
                        if (a != b) return false;
                        
                        var parameterResolveId = a.GetCustomAttribute<InjectAttribute>()?.Id;
                        var resolvingTypeResolveId = b.GetCustomAttribute<InjectAttribute>()?.Id;
                        return parameterResolveId == null || resolvingTypeResolveId == null
                                   ? ReferenceEquals(parameterResolveId, resolvingTypeResolveId)
                                   : parameterResolveId.Equals(resolvingTypeResolveId);
                    }
                }

                bool CanGetInstanceOfType(Type type, string id)
                {
                    var result = _typeToCaches.ContainsKey((type, id));
                    return result;
                }

                bool IsBasicType(Type type)
                {
                    return type.IsPrimitive
                           || type == typeof(string)
                           || type == typeof(decimal);
                }
                
            }
            #endregion
        }

        #endregion
    }
}