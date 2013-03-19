using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Play.Studio.Core.Utility
{
    /// <summary>
    /// 字符串帮助者
    /// </summary>
    public static class StringHelper
    {
        public static IEnumerable<Capture> Parse(string source, string regx)
        {
            foreach (Capture r in new Regex(regx).Match(source).Captures)
                yield return r;
        }

        public static string Replace(string source, Dictionary<string, string> replaceParams) 
        {
            return Replace(source, "@+{[^}]+}", replaceParams);
        }

        public static string Replace(string source, string regx, Dictionary<string, string> replaceParams) 
        {
            Parallel.ForEach<Capture>(StringHelper.Parse(source, "@+{[^}]+}").GroupBy(X => X.Value).Where(g => g.Count() == 1)
                .Select(g => g.ElementAt(0)), (o, s) =>
            {
                if (replaceParams.ContainsKey(o.Value))
                    source = source.Replace(o.Value, replaceParams[o.Value]);
            });

            return source;
        }
    }
}
