using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Play.Studio.Core.Utility
{
    /// <summary>
    /// 字符串解析器
    /// </summary>
    public static class StringParser
    {
        public static IEnumerable<Capture> Parse(string source, string regx)
        {
            foreach (Capture r in new Regex(regx).Match(source).Captures)
                yield return r;
        }
    }
}
