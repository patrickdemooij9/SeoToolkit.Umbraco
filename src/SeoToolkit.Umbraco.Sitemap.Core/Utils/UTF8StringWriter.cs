﻿using System.IO;
using System.Text;

namespace SeoToolkit.Umbraco.Sitemap.Core.Utils
{
    internal class UTF8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
