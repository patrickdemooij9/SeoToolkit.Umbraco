using System;
using Umbraco.Cms.Core.Models.Blocks;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class BlockGridSeoValueConverter : BaseGridSeoValueConverter<BlockGridItem>
    {
        public override Type FromValue => typeof(BlockGridModel);
        public override Type ToValue => typeof(string);
    }
}
