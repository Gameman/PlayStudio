using System.Globalization;
using System.IO;
using Play.Studio.Module.Resource;

namespace Play.Studio.Module.Language
{
    class LanguageResource : Resource<LanguagePackage, LanguageResource>
    {
        protected internal override object OnRead(Stream stream)
        {
            StreamReader reader = new StreamReader(stream, true);

            string line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                return new LanguagePackage(null);
            }
            else
            {
                LanguagePackage package = new LanguagePackage(CultureInfo.GetCultureInfo(line));
                if (package.IsValid)
                {
                    int currentIndex = 1;
                    while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                    {
                        var lineElements = line.Split(',');
                        if (lineElements.Length != 2) 
                        {
                            throw new LanguageReadException();
                        }

                        var key     = lineElements[0];
                        var value   = lineElements[1];

                        package[key] = value;

                        currentIndex++;
                    }
                }

                return package;
            }
        }
    }
}
