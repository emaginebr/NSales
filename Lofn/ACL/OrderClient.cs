using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Lofn.ACL.Core;
using Lofn.ACL.Interfaces;
using Lofn.DTO.Order;
using Lofn.DTO.Settings;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lofn.ACL
{
    public class OrderClient : BaseClient, IOrderClient
    {
        public OrderClient(IOptions<LofnSetting> nsalesSetting) : base(nsalesSetting)
        {
        }

        public async Task<OrderListPagedResult> SearchAsync(OrderSearchParam param)
        {
            var content = new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Order/search", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<OrderListPagedResult>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<OrderInfo>> ListAsync(OrderParam param)
        {
            var content = new StringContent(JsonConvert.SerializeObject(param), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Order/list", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<IList<OrderInfo>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<OrderInfo> GetByIdAsync(long orderId)
        {
            var response = await _httpClient.GetAsync($"{_nsalesSetting.Value.ApiUrl}/Order/getById/{orderId}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<OrderInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task<OrderInfo> UpdateAsync(OrderInfo order)
        {
            var content = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_nsalesSetting.Value.ApiUrl}/Order/update", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<OrderInfo>(await response.Content.ReadAsStringAsync());
        }
    }
}
