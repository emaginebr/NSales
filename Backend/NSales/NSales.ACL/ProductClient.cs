using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSales.ACL.Core;
using NSales.ACL.Interfaces;
using NSales.DTO.Product;
using NSales.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.ACL
{
    public class ProductClient : BaseClient, IProductClient
    {
        public ProductClient(IOptions<NSalesSetting> nsalesSetting) : base(nsalesSetting )
        {
        }

        public async Task<ProductListPagedInfo> SearchAsync(ProductSearchParam param)
        {
            var content = new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Product/search", content);
            response.EnsureSuccessStatusCode();
            //return GetProductInfoFromJson(await response.Content.ReadAsStringAsync());
            return null;
        }

        public async Task<ProductInfo> GetByIdAsync(long productId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Product/getById/{productId}");
            response.EnsureSuccessStatusCode();
            return GetProductInfoFromJson(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> GetBySlugAsync(string productSlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Product/getBySlug/{productSlug}");
            response.EnsureSuccessStatusCode();
            return GetProductInfoFromJson(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> InsertAsync(ProductInfo product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Product/insert", content);
            response.EnsureSuccessStatusCode();
            return GetProductInfoFromJson(await response.Content.ReadAsStringAsync());
        }

        public async Task<ProductInfo> UpdateAsync(ProductInfo product)
        {
            var content = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Product/update", content);
            response.EnsureSuccessStatusCode();
            return GetProductInfoFromJson(await response.Content.ReadAsStringAsync());
        }
    }
}
