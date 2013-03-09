using System.Collections.Generic;

namespace Play.Studio.Model.Templates
{
    /// <summary>
    /// 模板控制文件
    /// </summary>
    public struct TemplateConfig
    {
        Dictionary<string, string> m_keyValues;

        public string this[string key] 
        {
            get 
            {
                return m_keyValues[key];
            }
        }

        public bool Contains(string key) 
        {
            return m_keyValues.ContainsKey(key);
        }

        public TemplateConfig(string[] keys, string[] values) 
        {
            m_keyValues = new Dictionary<string, string>(keys.Length);
            for (int i = 0; i < keys.Length; i++)
                m_keyValues[keys[i]] = values[i];
        }
    }
}
