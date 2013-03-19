using System.Collections.Generic;
using System.ComponentModel;

namespace Play.Studio.Core
{
    public class Property
    {
        public static Dictionary<string, Property> m_keyValue = new Dictionary<string, Property>();

        public object Value
        {
            get;
            private set;
        }

        private Property(object value) 
        {
            Value = value;
        }

        public static T Get<T>(string key) 
        {
            return (T)m_keyValue[key].Value;
        }

        public static T Get<T>(string key, T defaultValue) 
        {
            if (m_keyValue.ContainsKey(key))
                return (T)m_keyValue[key].Value;
            else
                return defaultValue;
        }

        public static void Set<T>(string key, T value) 
        {
            if (!m_keyValue.ContainsKey(key) || (m_keyValue.ContainsKey(key) && m_keyValue[key].Value != (object)value))
            {
                m_keyValue[key] = new Property(value);
                if (PropertyChanged != null)
                    PropertyChanged(m_keyValue[key], new PropertyChangedEventArgs(key));
            }
        }

        public static event PropertyChangedEventHandler PropertyChanged;
    }
}
