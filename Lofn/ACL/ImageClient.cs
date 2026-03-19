using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Lofn.ACL.Core;
using Lofn.ACL.Interfaces;
using Lofn.DTO.Product;
using Lofn.DTO.Settings;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Lofn.ACL
{
    public class ImageClient : BaseClient, IImageClient
    {
        public ImageClient(IOptions<LofnSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<IList<ProductImageInfo>> ListAsync(long productId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Image/list/{productId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<ProductImageInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductImageInfo> UploadAsync(long productId, Stream fileStream, string fileName, int sortOrder = 0)
        {
            using var formData = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            formData.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync(
                $"{_nsalesSetting.Value.ApiUrl}/Image/upload/{productId}?sortOrder={sortOrder}",
                formData);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ProductImageInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(long imageId)
        {
            var response = await _httpClient.DeleteAsync($"{_nsalesSetting.Value.ApiUrl}/Image/delete/{imageId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
