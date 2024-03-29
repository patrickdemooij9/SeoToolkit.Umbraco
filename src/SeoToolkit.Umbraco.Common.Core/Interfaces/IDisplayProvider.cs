﻿using SeoToolkit.Umbraco.Common.Core.Models.ViewModels;

namespace SeoToolkit.Umbraco.Common.Core.Interfaces
{
    public interface IDisplayProvider
    {
        SeoDisplayViewModel Get(int contentTypeId);
    }
}
