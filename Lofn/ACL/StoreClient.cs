using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Lofn.ACL.Core;
using Lofn.ACL.Interfaces;
using Lofn.DTO.Store;
using Lofn.DTO.Settings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lofn.ACL
{
    public class StoreClient : BaseClient, IStoreClient
    {
        public StoreClient(IOptions<LofnSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<IList<StoreInfo>> ListAsync()
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Store/list");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<StoreInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<StoreInfo>> ListActiveAsync()
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Store/listActive");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<StoreInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<StoreInfo> GetBySlugAsync(string storeSlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Store/getBySlug/{storeSlug}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<StoreInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<StoreInfo> GetByIdAsync(long storeId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Store/getById/{storeId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<StoreInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<StoreInfo> InsertAsync(StoreInsertInfo store)
        {
            var content = new StringContent(JsonConvert.SerializeObject(store), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Store/insert", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<StoreInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<StoreInfo> UpdateAsync(StoreUpdateInfo store)
        {
            var content = new StringContent(JsonConvert.SerializeObject(store), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Store/update", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<StoreInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(long storeId)
        {
            var response = await _httpClient.DeleteAsync($"{_nsalesSetting.Value.ApiUrl}/Store/delete/{storeId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
