using System.IO;
using System.Text;

namespace uSeoToolkit.Umbraco.Sitemap.Core.Utils
{
    internal class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
