using System;

namespace Play.Studio.Core
{
    /// <summary>
    /// 异常快捷方式标签
    /// </summary>
    public class ExceptionAttribute : Attribute 
    {
        public ExceptionAttribute() 
            : this(true, true)
        {

        }

        public ExceptionAttribute(bool @catch) 
            : this(@catch, true)
        {
        }

        public ExceptionAttribute(bool @catch, bool logging) 
        {
        }
    }
}
