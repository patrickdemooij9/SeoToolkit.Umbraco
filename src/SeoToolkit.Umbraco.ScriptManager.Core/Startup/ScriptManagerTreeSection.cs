using SeoToolkit.Umbraco.Common.Core.Collections;
using System;

namespace SeoToolkit.Umbraco.ScriptManager.Core.Startup
{
    public class ScriptManagerTreeSection : ISeoTreeSection
    {
        public Guid Id => new("94E95F4A-2ECB-4038-BCFD-8357B7C41F1A");

        public string Name => "Script Manager"; 
        
        public bool CanBeDomainSpecific => false;
    }
}
