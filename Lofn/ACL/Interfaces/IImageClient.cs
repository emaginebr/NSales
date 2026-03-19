using Lofn.DTO.Product;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Lofn.ACL.Interfaces
{
    public interface IImageClient
    {
        Task<IList<ProductImageInfo>> ListAsync(long productId);
        Task<ProductImageInfo> UploadAsync(long productId, Stream fileStream, string fileName, int sortOrder = 0);
        Task DeleteAsync(long imageId);
    }
}
