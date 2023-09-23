using System;
using Umbraco.Cms.Core.Models.Blocks;

namespace SeoToolkit.Umbraco.MetaFields.Core.Common.Converters.SeoValueConverters
{
    public class BlockListSeoValueConverter : BaseGridSeoValueConverter<BlockListItem>
    {
        public override Type FromValue => typeof(BlockListModel);
        public override Type ToValue => typeof(string);
    }
}
