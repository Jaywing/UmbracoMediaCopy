using MediaCopy.Models;
using Umbraco.Core.Models;

namespace MediaCopy.Services
{
    public interface IMediaCopyService
    {
        CopyResponse Copy(int id, int destinationId);
        void CopyFileStream(IMedia mediaToCopy, IMedia copiedMedia);
    }
}
