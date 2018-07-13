using MediaCopy.Models;
using MediaCopy.Services;
using System;
using System.Web.Http;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace MediaCopy.Controllers
{
    [PluginController("MediaCopy")]
    public partial class MediaCopyController : UmbracoAuthorizedJsonController
    {
        private readonly ILocalizedTextService _textService;
        private readonly IMediaCopyService _mediaCopyService;

        public MediaCopyController(ILocalizedTextService textService, IMediaCopyService mediaCopyService)
        {
            _textService = textService;
            _mediaCopyService = mediaCopyService;
        }
         
        [HttpPost]
        public CopyResponse PostCopy(int id, int destinationId = -1)
        {
            try
            { 
                return _mediaCopyService.Copy(id, destinationId);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MediaCopyService>("CopyMedia", ex);
                return new CopyResponse { Message = _textService?.Localize("mediaCopy/weEncountedAProblem") };
            }
        }
    }
}