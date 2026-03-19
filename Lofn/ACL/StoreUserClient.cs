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
    public class StoreUserClient : BaseClient, IStoreUserClient
    {
        public StoreUserClient(IOptions<LofnSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<IList<StoreUserInfo>> ListAsync(string storeSlug)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/StoreUser/{storeSlug}/list");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<StoreUserInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<StoreUserInfo> InsertAsync(string storeSlug, StoreUserInsertInfo storeUser)
        {
            var content = new StringContent(JsonConvert.SerializeObject(storeUser), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/StoreUser/{storeSlug}/insert", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<StoreUserInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(string storeSlug, long storeUserId)
        {
            var response = await _httpClient.DeleteAsync($"{_nsalesSetting.Value.ApiUrl}/StoreUser/{storeSlug}/delete/{storeUserId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
