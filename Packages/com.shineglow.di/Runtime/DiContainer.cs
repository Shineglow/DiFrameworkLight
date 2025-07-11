using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace com.shineglow.di.Runtime
{
    public class DiContainer
    {
        private readonly Dictionary<(Type, string), DiBindingCache> _typeToCaches = new();

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
                    var constructorInfo = cache.Properties.ConstructorInfo ??= GetConstructor(cache);
                    if (constructorInfo == null)
                    {
                        throw new
                            TypeCannotBeResolvedException($"Unable to create an object of type {cache.ResolvingType}. There is no empty constructor or it is impossible to get copies of the required types. Try changing the resolved object or registering types in the container.");
                    }

                    cache.Properties.ConstructorProperties ??= GetParametersArray(constructorInfo);
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
                        var propertyField = typeLoc.GetField($"<{memberInfo.Name}>k__BackingField");
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
            cache.Properties.MethodInfosToInject ??= GetMethodInfoToInject(cache.ResolvingType);
            
            foreach (var (methodInfo, propertiesWithIds) in cache.Properties.MethodInfosToInject)
            {
                object[] resolved = new object[propertiesWithIds.Count];
                for (var index = 0; index < propertiesWithIds.Count; index++)
                {
                    var (parameterInfo, id) = propertiesWithIds[index];
                    resolved[index] = Resolve(parameterInfo.ParameterType, id);
                }
                methodInfo.Invoke(instance, resolved);
            }

            List<(MethodInfo, List<(ParameterInfo, string)>)> GetMethodInfoToInject(Type typeLoc)
            {
                List<(MethodInfo, List<(ParameterInfo, string)>)> result = new();
                foreach (var methodInfo in typeLoc.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = methodInfo.GetCustomAttribute<InjectAttribute>();
                    if (attribute != null)
                    {
                        List<(ParameterInfo, string)> parametersInfo = new();
                        result.Add((methodInfo, parametersInfo));
                        var parameters = methodInfo.GetParameters();
                        parametersInfo.AddRange(parameters.Select(parameter => (parameter, parameter.GetCustomAttribute<InjectAttribute>()?.Id)));
                    }
                }

                return result;
            }
        }

        #endregion

        #region MassiveReflectionFunctions

        private (Type resolveType, string id)[] GetParametersArray(ConstructorInfo constructorInfo)
        {
            var constructorParameters = constructorInfo.GetParameters();
            (Type, string)[] parametersList = new (Type, string)[constructorParameters.Length];

            for (var i = 0; i < constructorParameters.Length; i++)
            {
                var parameterInfo = constructorParameters[i];
                parametersList[i] = (parameterInfo.ParameterType,
                                     parameterInfo.GetCustomAttribute<InjectAttribute>()?.Id);
            }

            return parametersList;
        }

        private object[] GetResolvedObjectsFromArray((Type, string)[] parameters)
        {
            object[] resolvedParameters = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterInfo = parameters[i];
                resolvedParameters[i] = Resolve(parameterInfo.Item1, parameterInfo.Item2);
            }

            return resolvedParameters;
        }

        private ConstructorInfo GetConstructor(DiBindingCache cache)
        {
            ConstructorInfo constructorInfo = null;
            var constructorInfos = cache.ResolvingType.GetConstructors();

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