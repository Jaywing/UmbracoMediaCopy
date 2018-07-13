using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Models;

namespace MediaCopy.Extensions
{ 
    public static class MediaTypeExtensions
    {
        public static bool IsAllowedUnderMediaType(this IMediaType target, IMediaType test) =>
            target?.AllowedContentTypes?.FirstOrDefault(x => x.Id.Value == test.Id) != null;
    }
}