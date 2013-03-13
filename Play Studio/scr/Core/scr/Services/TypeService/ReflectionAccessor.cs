using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Play.Studio.Core.Services
{
    public delegate object FastInvokeHandler(object target, object[] paramters);

    public delegate object FastConstructorHandler(object[] paramters);

    /// <summary>
    /// Reflection accessor.
    /// Dynamic method + delegate container 
    /// for System.Reflection property, method and constructor.
    /// <see href="http://www.codeproject.com/KB/cs/FastMethodInvoker.aspx"/>
    /// <see href="http://www.codeproject.com/KB/cs/FastInvokerWrapper.aspx"/>
    /// </summary>
    public class ReflectionAccessor
    {
        /// <summary>
        /// The caching handlers.
        /// </summary>
        static Hashtable cacheHandlers = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        /// <summary>
        /// The caching accessors.
        /// </summary>
        static Hashtable cacheAccessors = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
        //temp parameter arrays.
        private object[] getParam = new object[1];
        private object[] setParam = new object[2];
        private ReflectionAccessor mOwner;
        //private PropertyInfo mProperty;
        private object mIndex;
        private FastInvokeHandler getHandler;
        private FastInvokeHandler setHandler;
        private FastConstructorHandler constructorHandler;
        private Type type;

        public Type ValueType
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        /// <summary>
        /// Inits the accessor with ctor.
        /// </summary>
        /// <returns>
        /// The create accessor.
        /// </returns>
        /// <param name='type'>
        /// Type.
        /// </param>
        /// <param name='param'>
        /// Parameter.
        /// </param>
        public static ReflectionAccessor InitCreateAccessor(Type type, Type[] param)
        {
            if (cacheAccessors.Contains(type))
                return (ReflectionAccessor)cacheAccessors[type];
            ConstructorInfo ci = type.GetConstructor(param);
            if (ci == null)
                return null;
            ReflectionAccessor pa = new ReflectionAccessor(ci);
            cacheAccessors.Add(type, pa);
            return pa;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ais.tool.ReflectionAccessor"/> class.
        /// </summary>
        /// <param name='constructorInfo'>
        /// Constructor info. only create method usable
        /// </param>
        public ReflectionAccessor(ConstructorInfo constructorInfo)
        {
            string name = constructorInfo.DeclaringType.FullName + ".ctor(" + constructorInfo.GetParameters().Length + ")";
            if (cacheHandlers.Contains(name))
                constructorHandler = (FastConstructorHandler)cacheHandlers[name];
            else
            {
                constructorHandler = GetConstructorInvoker(constructorInfo);
                cacheHandlers[name] = constructorHandler;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ais.tool.ReflectionAccessor"/> class.
        /// </summary>
        /// <param name='methodInfo'>
        /// Method info. only get method usable 
        /// </param>
        public ReflectionAccessor(MethodInfo methodInfo)
            : this(methodInfo, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ais.tool.ReflectionAccessor"/> class.
        /// </summary>
        /// <param name='propertyInfo'>
        /// Property info.
        /// </param>
        /// <param name='Index'>
        /// Some object if indexed property 
        /// Like Row["column_name"] index is "column_name") 
        /// that mean you can create accessor for each column or specify index in get set methods
        /// or null otherwise.
        /// </param>
        public ReflectionAccessor(PropertyInfo propertyInfo, object Index) :
            this(propertyInfo.GetGetMethod(false), propertyInfo.GetSetMethod(false))
        {
            //this.mProperty = propertyInfo;
            this.mIndex = Index;
        }
        ///
        public ReflectionAccessor(MethodInfo getMethod, MethodInfo setMethod)
        {
            string name = null;
            if (getMethod != null)
            {
                type = getMethod.ReturnType;
                name = getMethod.DeclaringType.FullName + "." + getMethod.Name;
                if (cacheHandlers.Contains(name))
                    getHandler = (FastInvokeHandler)cacheHandlers[name];
                else
                {
                    getHandler = GetMethodInvoker(getMethod);
                    cacheHandlers[name] = getHandler;
                }
            }
            if (setMethod != null)
            {
                name = setMethod.DeclaringType.FullName + "." + setMethod.Name;
                if (cacheHandlers.Contains(name))
                    setHandler = (FastInvokeHandler)cacheHandlers[name];
                else
                {
                    setHandler = GetMethodInvoker(setMethod);
                    cacheHandlers[name] = setHandler;
                }
            }
        }
        /// <summary>
        /// Gets or sets the owner(for complex property).
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public ReflectionAccessor Owner
        {
            get { return mOwner; }
            set { mOwner = value; }
        }

        /// <summary>
        /// Gets the top owner.
        /// </summary>
        /// <value>
        /// The top owner.
        /// </value>
        public ReflectionAccessor TopOwner
        {
            get
            {
                if (mOwner == null)
                    return this;
                else if (mOwner.Owner == null)
                    return mOwner;
                else
                    return mOwner.TopOwner;
            }
        }

        /// <summary>
        /// Create the new instance of specified in constructor object by specified parameters.
        /// </summary>
        /// <param name='parameters'>
        /// Parameters.
        /// </param>
        public object Create(object[] parameters)
        {
            if (constructorHandler == null)
                return null;
            return constructorHandler(parameters);
        }

        /// <summary>
        /// Get the property(or method result) of specified target by parameters.
        /// </summary>
        /// <param name='target'>
        /// Target.
        /// </param>
        /// <param name='parameters'>
        /// Parameters.
        /// </param>
        public object Get(object target, object[] parameters)
        {
            if (getHandler == null)
                return null;
            if (mOwner != null)
            {
                target = mOwner.Get(target);
            }
            if (target == null)
                return null;
            return getHandler(target, parameters);
        }

        /// <summary>
        /// Get the property(or method result) of specified target.
        /// </summary>
        /// <param name='target'>
        /// Target.
        /// </param>
        public object Get(object target)
        {
            getParam[0] = mIndex;
            return Get(target, getParam);
        }

        /// <summary>
        /// Set the specified value to target object.
        /// </summary>
        /// <param name='target'>
        /// Target.
        /// </param>
        /// <param name='value'>
        /// Value.
        /// </param>
        public void Set(object target, object value)
        {
            Set(target, value, mIndex);
        }
        /// <summary>
        /// Set the value (index) to target object.
        /// </summary>
        /// <param name='target'>
        /// Target.
        /// </param>
        /// <param name='value'>
        /// Value.
        /// </param>
        /// <param name='index'>
        /// Index.
        /// </param>
        public void Set(object target, object value, object index)
        {
            if (setHandler == null)
                return;
            if (mOwner != null)
            {
                target = mOwner.Get(target);
            }
            if (target == null)
                return;
            if (index != null)
            {
                setParam[0] = index;
                setParam[1] = value;
            }
            else
                setParam[0] = value;
            setHandler(target, setParam);
        }
        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        //public PropertyInfo Property {
        //get { return this.mProperty; }
        //}
        /// <summary>
        /// Gets or sets the index if property is indexed.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        public object Index
        {
            get { return mIndex; }
            set { mIndex = value; }
        }
        /// <summary>
        /// Gets the constructor invoker.
        /// </summary>
        /// <returns>
        /// The constructor invoker.
        /// </returns>
        /// <param name='methodInfo'>
        /// Method info.
        /// </param>
        private static FastConstructorHandler GetConstructorInvoker(ConstructorInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                            typeof(object),
                            new Type[] { typeof(object[]) },
                            methodInfo.DeclaringType.Module, true);

            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_0);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            //il.Emit(OpCodes.Ldarg_0);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldloc, locals[i]);
            }
            il.Emit(OpCodes.Newobj, methodInfo);


            //EmitBoxIfNeeded(il, methodInfo.DeclaringType);

            il.Emit(OpCodes.Ret);
            FastConstructorHandler invoder = (FastConstructorHandler)dynamicMethod.CreateDelegate(typeof(FastConstructorHandler));
            return invoder;
        }
        /// <summary>
        /// Create delegate to new dynamic method <see href="http://www.codeproject.com/KB/cs/FastMethodInvoker.aspx"/>
        /// </summary>
        /// <returns>
        /// The method invoker.
        /// </returns>
        /// <param name='methodInfo'>
        /// Method info.
        /// </param>
        private static FastInvokeHandler GetMethodInvoker(MethodInfo methodInfo)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty,
                typeof(object),
                new Type[] { typeof(object), typeof(object[]) },
                methodInfo.DeclaringType.Module, true);

            ILGenerator il = dynamicMethod.GetILGenerator();
            ParameterInfo[] ps = methodInfo.GetParameters();
            Type[] paramTypes = new Type[ps.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    paramTypes[i] = ps[i].ParameterType.GetElementType();
                else
                    paramTypes[i] = ps[i].ParameterType;
            }
            LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

            for (int i = 0; i < paramTypes.Length; i++)
            {
                locals[i] = il.DeclareLocal(paramTypes[i], true);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                il.Emit(OpCodes.Ldarg_1);
                EmitFastInt(il, i);
                il.Emit(OpCodes.Ldelem_Ref);
                EmitCastToReference(il, paramTypes[i]);
                il.Emit(OpCodes.Stloc, locals[i]);
            }
            if (!methodInfo.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                    il.Emit(OpCodes.Ldloca_S, locals[i]);
                else
                    il.Emit(OpCodes.Ldloc, locals[i]);
            }
            if (methodInfo.IsStatic)
                il.EmitCall(OpCodes.Call, methodInfo, null);
            else
                il.EmitCall(OpCodes.Callvirt, methodInfo, null);
            if (methodInfo.ReturnType == typeof(void))
                il.Emit(OpCodes.Ldnull);
            else
                EmitBoxIfNeeded(il, methodInfo.ReturnType);

            for (int i = 0; i < paramTypes.Length; i++)
            {
                if (ps[i].ParameterType.IsByRef)
                {
                    il.Emit(OpCodes.Ldarg_1);
                    EmitFastInt(il, i);
                    il.Emit(OpCodes.Ldloc, locals[i]);
                    if (locals[i].LocalType.IsValueType)
                        il.Emit(OpCodes.Box, locals[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
            il.Emit(OpCodes.Ret);
            FastInvokeHandler invoder = (FastInvokeHandler)dynamicMethod.CreateDelegate(typeof(FastInvokeHandler));
            return invoder;
        }

        private static void EmitCastToReference(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }

        private static void EmitBoxIfNeeded(ILGenerator il, System.Type type)
        {
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }
        }

        private static void EmitFastInt(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            if (value > -129 && value < 128)
            {
                il.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, value);
            }
        }


    }
}
