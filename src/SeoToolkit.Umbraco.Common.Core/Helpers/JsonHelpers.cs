using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SeoToolkit.Umbraco.Common.Core.Helpers
{
    public static class JsonHelpers
    {
        public static T[]? DeserializeArray<T>(object value)
        {
            if (value is null) return null;

            if (value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<T[]>(jsonElement);
            }
            else if (value is JArray jArray)
            {
                return jArray.ToObject<T[]>();
            }
            return null;
        }
    }
}
