using System;

namespace Play.Studio.Core
{
    /// <summary>
    /// AOP基类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class AopAttribute : Attribute
    {
        /// <summary>
        /// Aop类型
        /// </summary>
        public AopTypes AopType
        {
            get;
            private set;
        }

        public AopAttribute(AopTypes aopType) 
        {
            AopType = aopType;
        }

        /// <summary>
        /// 执行
        /// </summary>
        public abstract void Invoke();
    }

    /// <summary>
    /// Aop定义（实现该基类才可以执行Aop操作）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public sealed class AopDefineAttribute : AopAttribute
    {
        public AopDefineAttribute() 
            : base(AopTypes.None)
        {
        }

        public override void Invoke()
        {
            throw new NotImplementedException();
        }
  
    }

    /// <summary>
    /// Aop程序集（实现该基类才可以执行Aop操作）
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AopAssembly : Attribute
    {
    }
}
