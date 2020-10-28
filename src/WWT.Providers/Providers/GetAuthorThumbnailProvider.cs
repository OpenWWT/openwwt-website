using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WWT.Tours;

namespace WWT.Providers
{
    public class GetAuthorThumbnailProvider : GetTourProviderBase
    {
        private readonly ITourAccessor _tourAccessor;

        public GetAuthorThumbnailProvider(ITourAccessor tourAccessor)
        {
            _tourAccessor = tourAccessor;
        }

        protected override Task<Stream> GetStreamAsync(string id, CancellationToken token)
            => _tourAccessor.GetAuthorThumbnailAsync(id, token);
    }
}
