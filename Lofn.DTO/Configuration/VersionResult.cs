using System;
using System.Text.Json.Serialization;
using Lofn.DTO.Domain;

namespace Lofn.DTO.Configuration
{
    public class VersionResult : StatusResult
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
    }
}
