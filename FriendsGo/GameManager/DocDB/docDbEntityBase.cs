using System;
using GoogleApi.Entities.Common;
using Newtonsoft.Json;

namespace GameManager
{
    public class DocDbEntityBase
    {
        [JsonProperty(PropertyName = "id")]
        public string TelegramId;
    }
}