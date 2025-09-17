using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSales.DTO.Domain;
using NSales.DTO.Product;
using NSales.DTO.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSales.ACL.Core
{
    public abstract class BaseClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly IOptions<NSalesSetting> _nsalesSetting;

        public BaseClient(IOptions<NSalesSetting> nsalesSetting)
        {
            _httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
            _nsalesSetting = nsalesSetting;
        }

        protected ProductInfo GetProductInfoFromJson(string json)
        {
            var result = JsonConvert.DeserializeObject<ProductResult>(json);
            if (result == null)
            {
                throw new NullReferenceException("ProductResult is null");
            }
            if (!result.Sucesso)
            {
                throw new Exception(result.Mensagem);
            }
            return result.Product;
        }

        protected bool GetBoolFromJson(string json)
        {
            var result = JsonConvert.DeserializeObject<StatusResult>(json);
            if (result == null)
            {
                throw new NullReferenceException("StatusResult is null");
            }
            if (!result.Sucesso)
            {
                throw new Exception(result.Mensagem);
            }
            return result.Sucesso;
        }

        protected string GetStringFromJson(string json)
        {
            var result = JsonConvert.DeserializeObject<StringResult>(json);
            if (result == null)
            {
                throw new NullReferenceException("StatusResult is null");
            }
            if (!result.Sucesso)
            {
                throw new Exception(result.Mensagem);
            }
            return result.Value;
        }
    }
}
