using MediaCopy.Extensions;
using MediaCopy.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System.Net;

namespace MediaCopy.Services
{
    public class MediaCopyService : IMediaCopyService
    {
        private readonly ILocalizedTextService _textService;
        private readonly IMediaService _mediaService;
        private readonly MediaFileSystem _mediaFileSystem;

        private string _nodePath;

        private const string FolderAlias = "Folder";
        private const string UmbracoFileAlias = "umbracoFile";

        public MediaCopyService(ILocalizedTextService textService, IMediaService mediaService)
        {
            _textService = textService;
            _mediaService = mediaService;
            _mediaFileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
        }

        public CopyResponse Copy(int id, int destinationId = -1)
        {
            if (id == destinationId)
                return new CopyResponse { Message = _textService?.Localize("mediaCopy/cannotCopyToSelf") };

            IMedia media = _mediaService.GetById(id);
            if (media == null)
                return new CopyResponse { Message = _textService?.Localize("mediaCopy/mediaItemCouldntBeFound") };

            IMedia destination = _mediaService.GetById(destinationId);
            if (destination == null && destinationId != -1)
                return new CopyResponse { Message = _textService?.Localize("mediaCopy/destinationCouldntBeFound") };

            if ((destinationId == -1 && !media.ContentType.AllowedAsRoot) ||
                (destinationId != -1 && !destination.ContentType.IsAllowedUnderMediaType(media.ContentType)))
            {
                return new CopyResponse
                {
                    Message = _textService?.Localize(
                        "mediaCopy/youreNotAllowedToCopyUnder",
                        new Dictionary<string, string>
                        {
                            { "mediaName", media.Name },
                            { "destination", destinationId == -1 ? _textService?.Localize("mediaCopy/media") : destination.Name }
                        })
                };
            }

            if (!CopyMedia(media, destinationId))
                return new CopyResponse { Message = _textService?.Localize("mediaCopy/weEncountedAProblem") };
            else
                return new CopyResponse { Success = true, Path = _nodePath };
        }

        private bool CopyMedia(IMedia media, int destinationId, bool copyChildren = false)
        {
            if (media == null) return false;

            var copiedMedia = _mediaService.CreateMediaWithIdentity(
                $"{media.Name}{_textService?.Localize("mediaCopy/copySuffix")}",
                destinationId,
                media.ContentType.Alias);

            if (copiedMedia == null) return false;

            for (int i = 0; i < media.Properties.Count; ++i)
                copiedMedia.Properties[i].Value = media.Properties[i]?.Value;

            if (media.ContentType.Alias != FolderAlias)
                CopyUploadedFile(media, copiedMedia);

            _nodePath = copiedMedia.Path;

            _mediaService.Save(copiedMedia);

            if (media.Children()?.Any() ?? false)
                foreach (IMedia childMedia in media.Children())
                    if (!CopyMedia(childMedia, copiedMedia.Id, copyChildren))
                        return false;

            return true;
        }

        private void CopyUploadedFile(IMedia media, IMedia copiedMedia)
        {
            if (media == null || copiedMedia == null) return;

            var propertyType = media.PropertyTypes.FirstOrDefault(x => x.Alias.InvariantEquals(UmbracoFileAlias));
            if (propertyType == null) return;

            var propertyValue = media.Properties[propertyType.Alias];
            if (!(propertyValue?.Value is string jsonString)) return;

            switch (propertyType.PropertyEditorAlias)
            {
                case Constants.PropertyEditors.ImageCropperAlias:
                    if (jsonString.DetectIsJson())
                    {
                        var copiedUmbFile = JsonConvert.DeserializeObject<JObject>(jsonString);
                        CopyFileStream(media, copiedMedia);
                        copiedUmbFile["src"] = copiedMedia.Properties[UmbracoFileAlias]?.Value?.ToString();
                        copiedMedia.SetValue(UmbracoFileAlias, copiedUmbFile?.ToString());
                    }
                    else
                        CopyFileStream(media, copiedMedia);
                    break;
                default:
                    CopyFileStream(media, copiedMedia);
                    break;
            }
        }

        public void CopyFileStream(IMedia mediaToCopy, IMedia copiedMedia)
        {
            string path = mediaToCopy?.GetUrl();
            if (string.IsNullOrEmpty(path)) return;

            string fullPath = _mediaFileSystem?.GetFullPath(path);
            if (string.IsNullOrEmpty(fullPath)) return;

            try
            {
                using (var inStream = _mediaFileSystem.OpenFile(WebUtility.UrlDecode(fullPath)))
                {
                    inStream.Position = 0;
                    copiedMedia.Properties[UmbracoFileAlias].Value = null;
                    copiedMedia.SetValue(UmbracoFileAlias, Path.GetFileName(path), inStream);
                }
            }
            catch { return; }
        }
    }
}
