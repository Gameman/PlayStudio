using System;

namespace Play.Studio.Core.Messaging
{
    /// <summary>
    /// 日志标签(AOP)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessagingAttribute : AopAttribute
    {
        private string m_message;
        private object[] m_args;

        public MessagingAttribute(AopTypes aopType, string message, params object[] args)
            : base(aopType)
        {
            m_message = message;
            m_args = args;
        }

        public override void Invoke()
        {
        }
    }
}
