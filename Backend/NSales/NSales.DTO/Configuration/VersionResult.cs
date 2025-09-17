using System;
using System.Text.Json.Serialization;
using NSales.DTO.Domain;

namespace NSales.DTO.Configuration
{
    public class VersionResult : StatusResult
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
