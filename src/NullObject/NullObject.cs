using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace NullObject
{
    public class NullObject
    {
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly IDictionary<Type, Type> CachedTypes = new ConcurrentDictionary<Type, Type>();

        static NullObject()
        {
            var assemblyName = new AssemblyName(string.Concat(nameof(NullObject), "Types"));
            var assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule(string.Concat(nameof(NullObject), "Module"));
        }

        public static TInterfaceType Of<TInterfaceType>() where TInterfaceType : class
        {
            var type = typeof(TInterfaceType);

            if (!type.IsInterface)
                throw new NotSupportedException("NullObject only works with interfaces.");

            if (!CachedTypes.ContainsKey(type))
                CreateType(type);

            return (TInterfaceType) Activator.CreateInstance(CachedTypes[type]);
        }

        private static void CreateType(Type type)
        {
            var typeBuilder =
                ModuleBuilder.DefineType("ImplOf" + type.Name, TypeAttributes.Class | TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(type);
            CreateConstructor(typeBuilder, out var iLGenerator);
            ImplementProperties(type, typeBuilder, iLGenerator, out var propertyAccessors);
            ImplementMethods(type, typeBuilder, propertyAccessors);
            Type createType = typeBuilder.CreateTypeInfo();
            CachedTypes[type] = createType;
        }

        private static void CreateConstructor(TypeBuilder typeBuilder, out ILGenerator iLGenerator)
        {
            var constructorInfo = typeof(object).GetConstructor(new Type[0]);
            var constructorBuilder =
                typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            iLGenerator = constructorBuilder.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Call, constructorInfo);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private static void ImplementMethods(Type type, TypeBuilder typeBuilder, IEnumerable<MethodInfo> propertyAccessors)
        {
            var methodInfos = new List<MethodInfo>();
            GetMethodsRecursively(type, ref methodInfos);
            foreach (var propertyAccessor in propertyAccessors)
                methodInfos.Remove(propertyAccessor);
            ImplementDefaultMethods(methodInfos, typeBuilder);
        }

        private static void GetMethodsRecursively(Type type, ref List<MethodInfo> methodInfos)
        {
            methodInfos.AddRange(type.GetMethods());
            foreach (var subInterface in type.GetInterfaces())
                GetMethodsRecursively(subInterface, ref methodInfos);
        }

        private static void ImplementProperties(Type type, TypeBuilder typeBuilder, ILGenerator iLGenerator,
            out List<MethodInfo> propertyAccessorMethodInfos)
        {
            var propertyInfos = new List<PropertyInfo>();
            GetPropertiesRecursively(type, ref propertyInfos);
            ImplementAccessorsOfProperties(typeBuilder, iLGenerator, propertyInfos, out propertyAccessorMethodInfos);
        }

        private static void GetPropertiesRecursively(Type type, ref List<PropertyInfo> propertyInfos)
        {
            propertyInfos.AddRange(type.GetProperties());
            foreach (var subInterface in type.GetInterfaces())
                GetPropertiesRecursively(subInterface, ref propertyInfos);
        }

        private static void ImplementAccessorsOfProperties(TypeBuilder typeBuilder, ILGenerator iLGenerator,
            IEnumerable<PropertyInfo> propertyInfos, out List<MethodInfo> propertyAccessorMethodInfos)
        {
            propertyAccessorMethodInfos = new List<MethodInfo>();
            foreach (var propertyInfo in propertyInfos)
            {
                var piName = propertyInfo.Name;
                var propertyType = propertyInfo.PropertyType;

                var field = typeBuilder.DefineField(
                    "_" + piName, propertyType, FieldAttributes.Private);

                var getMethod = propertyInfo.GetGetMethod();
                if (getMethod != null)
                {
                    propertyAccessorMethodInfos.Add(getMethod);
                    var methodBuilder = typeBuilder.DefineMethod(
                        getMethod.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        propertyType,
                        Type.EmptyTypes);

                    iLGenerator = methodBuilder.GetILGenerator();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, field);
                    iLGenerator.Emit(OpCodes.Ret);
                    typeBuilder.DefineMethodOverride(methodBuilder, getMethod);
                }

                var setMethod = propertyInfo.GetSetMethod();
                if (setMethod != null)
                {
                    propertyAccessorMethodInfos.Add(setMethod);
                    var methodBuilder = typeBuilder.DefineMethod(
                        setMethod.Name,
                        MethodAttributes.Public | MethodAttributes.Virtual,
                        typeof(void),
                        new[] {propertyInfo.PropertyType});

                    iLGenerator = methodBuilder.GetILGenerator();
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    iLGenerator.Emit(OpCodes.Stfld, field);
                    iLGenerator.Emit(OpCodes.Ret);
                    typeBuilder.DefineMethodOverride(methodBuilder, setMethod);
                }
            }
        }

        private static void ImplementDefaultMethods(IEnumerable<MethodInfo> methodInfos, TypeBuilder typeBuilder)
        {
            foreach (var methodInfo in methodInfos)
            {
                var returnType = methodInfo.ReturnType;
                var argumentTypes = new List<Type>();
                foreach (var parameterInfo in methodInfo.GetParameters())
                    argumentTypes.Add(parameterInfo.ParameterType);

                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    returnType,
                    argumentTypes.ToArray());

                var iLGenerator = methodBuilder.GetILGenerator();

                if (returnType != typeof(void))
                {
                    var localBuilder =
                        iLGenerator.DeclareLocal(returnType);

                    iLGenerator.Emit(
                        OpCodes.Ldloc, localBuilder);
                }

                iLGenerator.Emit(OpCodes.Ret);
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }
        }
    }
}