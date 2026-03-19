using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Lofn.ACL.Core;
using Lofn.ACL.Interfaces;
using Lofn.DTO.Category;
using Lofn.DTO.Settings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lofn.ACL
{
    public class CategoryClient : BaseClient, ICategoryClient
    {
        public CategoryClient(IOptions<LofnSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<IList<CategoryInfo>> ListAsync(string storeSlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/list");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<CategoryInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<CategoryInfo>> ListActiveAsync(string storeSlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/listActive");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<CategoryInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CategoryInfo> GetBySlugAsync(string storeSlug, string categorySlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/getBySlug/{categorySlug}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<CategoryInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CategoryInfo> GetByIdAsync(string storeSlug, long categoryId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/getById/{categoryId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<CategoryInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CategoryInfo> InsertAsync(string storeSlug, CategoryInsertInfo category)
        {
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/insert", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<CategoryInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<CategoryInfo> UpdateAsync(string storeSlug, CategoryUpdateInfo category)
        {
            var content = new StringContent(JsonConvert.SerializeObject(category), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/update", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<CategoryInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(string storeSlug, long categoryId)
        {
            var response = await _httpClient.DeleteAsync($"{_nsalesSetting.Value.ApiUrl}/Category/{storeSlug}/delete/{categoryId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
