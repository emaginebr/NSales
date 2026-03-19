using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Lofn.ACL.Core;
using Lofn.ACL.Interfaces;
using Lofn.DTO.Product;
using Lofn.DTO.Settings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lofn.ACL
{
    public class ProductClient : BaseClient, IProductClient
    {
        public ProductClient(IOptions<LofnSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<ProductListPagedInfo> SearchAsync(ProductSearchParam param)
        {
            var content = new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Product/search", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ProductListPagedInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> GetByIdAsync(string storeSlug, long productId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Product/{storeSlug}/getById/{productId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ProductInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> GetBySlugAsync(string productSlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Product/getBySlug/{productSlug}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ProductInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<ProductInfo>> ListActiveByCategoryAsync(string storeSlug, string categorySlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Product/{storeSlug}/category/{categorySlug}/listActive");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<ProductInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> InsertAsync(string storeSlug, ProductInsertInfo product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Product/{storeSlug}/insert", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ProductInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> UpdateAsync(string storeSlug, ProductUpdateInfo product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Product/{storeSlug}/update", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<ProductInfo>(await response.Content.ReadAsStringAsync());
        }
    }
}
