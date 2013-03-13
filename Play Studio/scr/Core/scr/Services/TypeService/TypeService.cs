using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;

namespace Play.Studio.Core.Services
{
    /// <summary>
    /// Type service.
    /// </summary>
    public static class TypeService
    {
        public const string DynamicAssemblyName = "DdynamicAssembly";
        public const string DynamicModuleName = "DynamicModule";

        private static Type[] typeOneArray = new Type[] { typeof(string) };
        private static Dictionary<Type[], Type> combinePropertiesType 
                                        = new Dictionary<Type[], Type>();

        private static Dictionary<Type, HashSet<Type>> searchReferenceTypes 
                                        = new Dictionary<Type, HashSet<Type>>();


        /// <summary>
        /// 创建某类型实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CreateInstance(Type type, params object[] args)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                Type[] argsTypes = Type.EmptyTypes;
                if (args != null && args.Length > 0)
                {
                    argsTypes = new Type[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        argsTypes[i] = args[i].GetType();
                    }
                }

                ReflectionAccessor ra = ReflectionAccessor.InitCreateAccessor(type, argsTypes);

                if (ra == null)
                    return null;
                else
                    return ra.Create(args);
            }
        }

        public static T CreateInstance<T>(params object[] args) 
        {
            return (T)CreateInstance(typeof(T), args);
        }

        public static PropertyInfo GetIndexProperty(Type itemType)
        {
            typeOneArray[0] = typeof(string);
            return GetIndexProperty(itemType, typeOneArray);
        }

        public static PropertyInfo GetIndexProperty(Type itemType, Type indexType)
        {
            typeOneArray[0] = indexType;
            return GetIndexProperty(itemType, typeOneArray);
        }

        public static PropertyInfo GetIndexProperty(Type itemType, Type[] parameters)
        {
            if (itemType == null)
                return null;
            return itemType.GetProperty("Item", parameters);
        }

        public static bool ContainsAttribute<TAttribute>(Type type) where TAttribute : Attribute 
        {
            object[] dscArray = type.GetCustomAttributes(typeof(TAttribute), false);
            if (dscArray.Length == 0)
                return false;
            else
                return true;
        }

        public static TAttribute GetAttribute<TAttribute>(Type type) where TAttribute : Attribute 
        {
            object[] dscArray = type.GetCustomAttributes(typeof(TAttribute), false);
            if (dscArray.Length == 0)
                return null;
            else
                return dscArray[0] as TAttribute;
        }

        public static TAttribute GetAttribute<TAttribute>(Type type, bool inherit) where TAttribute : Attribute
        {
            object[] dscArray = type.GetCustomAttributes(typeof(TAttribute), inherit);
            if (dscArray.Length == 0)
                return null;
            else
                return dscArray[0] as TAttribute;
        }

        public static bool ContainsAttribute<TAttribute>(MemberInfo member) where TAttribute : Attribute
        {
            object[] dscArray = member.GetCustomAttributes(typeof(TAttribute), false);
            if (dscArray.Length == 0)
                return false;
            else
                return true;
        }

        public static TAttribute GetAttribute<TAttribute>(MemberInfo member) where TAttribute : Attribute
        {
            object[] dscArray = member.GetCustomAttributes(typeof(TAttribute), false);
            if (dscArray.Length == 0)
                return null;
            else
                return dscArray[0] as TAttribute;
        }

        public static Attribute GetAttribute(Type attributeType, MemberInfo member)
        {
            object[] dscArray = member.GetCustomAttributes(attributeType, false);
            if (dscArray.Length == 0)
                return null;
            else
                return dscArray[0] as Attribute;
        }

        public static bool IsInterface(Type type, string interfaceName)
        {
            return (type.GetInterface(interfaceName) != null);
        }

        public static bool IsDictionary(Type type)
        {
            return (type.GetInterface("IDictionary") != null && type != typeof(byte[]));
        }

        public static bool IsList(Type type)
        {
            return (type.GetInterface("IList") != null && type != typeof(byte[]));
        }

        public static bool IsArray(Type type) 
        {
            return (type.GetInterface("IList") != null && type != typeof(byte[]));
        }

        public static bool IsCollection(Type type)
        {
            return (type.GetInterface("ICollection") != null && type != typeof(byte[]));
        }

        public static bool IsFSerialize(Type type)
        {
            Type t = type.GetInterface("IFileSerialize");
            return t == null ? false : true;
        }

        public static bool IsNonSerialized(FieldInfo field)
        {
            if (field.FieldType.BaseType == typeof(Delegate))
                return true;
            object[] dscArray = field.GetCustomAttributes(typeof(NonSerializedAttribute), false);
            return (dscArray.Length > 0) ? true : false;
        }

        public static bool CheckDefault(FieldInfo field, object value)
        {
            object[] dscArray = field.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (dscArray.Length == 0)
                return false;
            return ((DefaultValueAttribute)dscArray[0]).Value.Equals(value);
        }

        public static FieldInfo GetField(Type type, string Name, bool nonPublic)
        {
            BindingFlags flag = BindingFlags.Instance;
            if (nonPublic)
                flag |= BindingFlags.NonPublic;
            return type.GetField(Name, flag);
        }

        public static FieldInfo[] GetFields(Type type, bool nonPublic)
        {
            BindingFlags flag = BindingFlags.Instance;
            if (nonPublic)
                flag |= BindingFlags.NonPublic;
            FieldInfo[] buf = type.GetFields(flag);
            return buf;
        }

        public static bool IsXmlAttribute(Type type)
        {
            if (type == typeof(Type))
                //type == typeof(Font) ||
                //type == typeof(StringFormat))
                return true;
            else if (!type.IsValueType &&
                type != typeof(CultureInfo) &&
                type != typeof(string) &&
                type != typeof(byte[]) &&
                //type != typeof(Image) &&
                //type != typeof(Bitmap) ||
                type == typeof(DictionaryEntry))
                return false;
            else
                return true;
        }

        public static bool IsXmlAttribute(FieldInfo field)
        {
            //if(field.)
            object[] dscArray = field.GetCustomAttributes(typeof(XmlAttribute), false);
            return (dscArray.Length > 0) ? true : false;
        }

        public static string GetDisplayName(Type type)
        {
            object[] dscArray = type.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (dscArray.Length == 0)
                return type.Name;
            return ((DisplayNameAttribute)dscArray[0]).DisplayName;
        }

        public static string GetDescription(Type type)
        {
            object[] dscArray = type.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (dscArray.Length == 0)
                return type.ToString();
            return ((DescriptionAttribute)dscArray[0]).Description;
        }

        public static string GetCategory(Type type)
        {
            object[] dscArray = type.GetCustomAttributes(typeof(CategoryAttribute), false);
            if (dscArray.Length == 0)
                return "Misc";
            return ((CategoryAttribute)dscArray[0]).Category;
        }

        public static bool GetBrowsable(Type type)
        {
            object[] dscArray = type.GetCustomAttributes(typeof(BrowsableAttribute), false);
            if (dscArray.Length == 0)
                return false;
            return ((BrowsableAttribute)dscArray[0]).Browsable;
        }

        public static string GetDisplayName(MemberInfo member)
        {
            object[] dscArray = member.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (dscArray.Length == 0)
                return member.Name;
            return ((DisplayNameAttribute)dscArray[0]).DisplayName;
        }

        public static string GetDescription(MemberInfo member) 
        {
            object[] dscArray = member.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (dscArray.Length == 0)
                return member.Name;
            return ((DescriptionAttribute)dscArray[0]).Description;
        }

        public static string GetCategory(MemberInfo member)
        {
            object[] dscArray = member.GetCustomAttributes(typeof(CategoryAttribute), false);
            if (dscArray.Length == 0)
                return "";
            return ((CategoryAttribute)dscArray[0]).Category;
        }

        public static bool GetBrowsable(MemberInfo member)
        {
            object[] dscArray = member.GetCustomAttributes(typeof(BrowsableAttribute), false);
            if (dscArray.Length == 0)
                return true;
            return ((BrowsableAttribute)dscArray[0]).Browsable;
        }

        public static bool GetReadOnly(MemberInfo member) 
        {
            if (member is PropertyInfo)
            {
                return GetReadOnly(member as PropertyInfo);
            }
            else 
            {
                object[] dscArray = member.GetCustomAttributes(typeof(ReadOnlyAttribute), false);
                if (dscArray.Length == 0)
                    return false;
                return ((ReadOnlyAttribute)dscArray[0]).IsReadOnly;
            }
        }

        public static bool GetReadOnly(PropertyInfo property)
        {
            object[] dscArray = property.GetCustomAttributes(typeof(ReadOnlyAttribute), false);
            if (dscArray.Length == 0)
                return !property.CanWrite;
            else
                return ((ReadOnlyAttribute)dscArray[0]).IsReadOnly;

        }

        /// <summary>
        /// 获取包含某标签类型的成员类型
        /// </summary>
        public static IEnumerable<Type> GetMemberTypes(Type type, Type attributeType) 
        {
            return GetMembers(type, attributeType).Select(X => X.ReflectedType);
        }

        /// <summary>
        /// 获取包含某标签类型的成员类型
        /// </summary>
        public static IEnumerable<Type> GetMemberTypes(Type type, params Type[] attributeTypes)
        {
            return GetMembers(type, attributeTypes).Select(X => X.ReflectedType);
        }

        /// <summary>
        /// 获取包含某标签类型的成员
        /// </summary>
        public static IEnumerable<T> GetMembers<T>(Type type, Type attributeType) where T : MemberInfo 
        {
            foreach (var member in GetMembers(type, attributeType)) 
            {
                if (member is T)
                    yield return (T)member;
            }
        }

        /// <summary>
        /// 获取包含某标签类型的成员
        /// </summary>
        public static IEnumerable<T> GetMembers<T>(Type type, params Type[] attributeTypes) where T : MemberInfo 
        {
            foreach (var member in GetMembers(type, attributeTypes)) 
            {
                if (member is T)
                    yield return (T)member;
            }
        }

        /// <summary>
        /// 获取包含某标签类型的成员
        /// </summary>
        public static IEnumerable<MemberInfo> GetMembers(Type type, Type attributeType)
        {
            if (typeof(Attribute).IsAssignableFrom(attributeType))
            {
                foreach (var member in type.GetMembers())
                {
                    object[] dscArray = member.GetCustomAttributes(attributeType, false);
                    if (dscArray.Length > 0)
                        yield return member;
                }
            }
        }

        /// <summary>
        /// 获取包含某标签类型的成员
        /// </summary>
        public static IEnumerable<MemberInfo> GetMembers(Type type, params Type[] attributeTypes)
        {
            Type at = typeof(Attribute);

            if (attributeTypes.Length > 0)
            {
                foreach (var member in type.GetMembers())
                {
                    bool isMatching = true;

                    for (int i = 0; i < attributeTypes.Length; i++)
                    {
                        if (at.IsAssignableFrom(attributeTypes[i]))
                        {
                            object[] dscArray = member.GetCustomAttributes(attributeTypes[i], false);
                            if (dscArray.Length < 1)
                            {
                                isMatching = false;
                                break;
                            }
                        }
                        else
                        {
                            isMatching = false;
                        }
                    }

                    if (isMatching)
                        yield return member;
                }
            }
            else
            {
                foreach (var member in type.GetMembers())
                    yield return member;
            }
        }

		public static Type MakeConstructionWarpper(ConstructorInfo ctorInfo)    
		{
			return MakeCombinProperiesType(ctorInfo.GetParameters().Select(X => X.ParameterType).ToArray());
		}
		
		public static Type MakeCombinProperiesType(params Type[] types)         
		{
			if(combinePropertiesType.ContainsKey(types))
			{
				return combinePropertiesType[types];
			}
			
			
			TypeBuilder tb = CreateTypeBuilder(DynamicAssemblyName, 
                                               DynamicModuleName, 
                                               Guid.NewGuid().ToString());

			tb.DefineDefaultConstructor(MethodAttributes.Public);
			
			foreach(var type in types)
			{
				tb.CreateAutoImplementedProperty(type.Name, type);
			}
			
			combinePropertiesType[types] = tb.CreateType();
			
			return combinePropertiesType[types];
		}

		public static TypeBuilder CreateTypeBuilder(
			string assemblyName, string moduleName, string typeName)            
		{
			TypeBuilder typeBuilder = AppDomain
				.CurrentDomain
				.DefineDynamicAssembly(new AssemblyName(assemblyName),
				                       AssemblyBuilderAccess.Run)
				.DefineDynamicModule(moduleName)
				.DefineType(typeName, TypeAttributes.Public);

			return typeBuilder;
		}
		
		public static void CreateAutoImplementedProperty(
		 	this TypeBuilder builder, string propertyName, Type propertyType)   
		{
			const string PrivateFieldPrefix = "m_";
			const string GetterPrefix = "get_";
			const string SetterPrefix = "set_";
			
			// Generate the field.
			FieldBuilder fieldBuilder = builder.DefineField(
				string.Concat(PrivateFieldPrefix, propertyName),
				propertyType, FieldAttributes.Private);
			
			// Generate the property
			PropertyBuilder propertyBuilder = builder.DefineProperty(
				propertyName, PropertyAttributes.HasDefault, propertyType, null);
			
			// Property getter and setter attributes.
			MethodAttributes propertyMethodAttributes =
				MethodAttributes.Public | MethodAttributes.SpecialName |
				MethodAttributes.HideBySig;
			
			// Define the getter method.
			MethodBuilder getterMethod = builder.DefineMethod(
				string.Concat(GetterPrefix, propertyName),
				propertyMethodAttributes, propertyType, Type.EmptyTypes);
			
			// Emit the IL code.
			// ldarg.0
			// ldfld,_field
			// ret
			ILGenerator getterILCode = getterMethod.GetILGenerator();
			getterILCode.Emit(OpCodes.Ldarg_0);
			getterILCode.Emit(OpCodes.Ldfld, fieldBuilder);
			getterILCode.Emit(OpCodes.Ret);
			
			// Define the setter method.
			MethodBuilder setterMethod = builder.DefineMethod(
				string.Concat(SetterPrefix, propertyName),
				propertyMethodAttributes, null, new Type[] { propertyType });
			
			// Emit the IL code.
			// ldarg.0
			// ldarg.1
			// stfld,_field
			// ret
			ILGenerator setterILCode = setterMethod.GetILGenerator();
			setterILCode.Emit(OpCodes.Ldarg_0);
			setterILCode.Emit(OpCodes.Ldarg_1);
			setterILCode.Emit(OpCodes.Stfld, fieldBuilder);
			setterILCode.Emit(OpCodes.Ret);
			
			propertyBuilder.SetGetMethod(getterMethod);
			propertyBuilder.SetSetMethod(setterMethod);
		}

        public static IEnumerable<Type> FindProxyClasses<T>(Assembly assembly)      
        {
            Type baseType = typeof(T);
            return assembly.GetExportedTypes().Where(X => baseType.IsAssignableFrom(X));
        }
    }
}