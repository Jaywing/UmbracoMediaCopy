using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace MediaCopy.Extensions
{
    public static class MediaExtensions
    {
        public static string GetUrl(this IMedia media, string propertyAlias = "umbracoFile") => media.GetMediaUrlWithTypeChecking(propertyAlias);

        public static string GetMediaUrlWithTypeChecking(this IMedia media, string propertyAlias)
        {
            PropertyType propertyType = media.PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(propertyAlias));
            if (propertyType == null) return string.Empty;

            Property val = media.Properties[propertyType.Alias];
            var jsonString = val?.Value as string;
            if (jsonString == null) return string.Empty;

            switch (propertyType.PropertyEditorAlias)
            {
                case Constants.PropertyEditors.ImageCropperAlias:
                    if (jsonString.DetectIsJson())
                    {
                        try
                        {
                            var json = JsonConvert.DeserializeObject<JObject>(jsonString);
                            if (json["src"] != null)
                                return json["src"].Value<string>();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error<ImageCropperValueConverter>("Could not parse the string " + jsonString + " to a json object", ex);
                            return string.Empty;
                        }
                    }
                    else
                        return jsonString;
                    break;
                case Constants.PropertyEditors.UploadFieldAlias:
                    return jsonString;
            }
            return string.Empty;
        }
    }
}