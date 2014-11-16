﻿using System;
using Newtonsoft.Json;

namespace Warehouse.Silverlight.Data.Auth
{
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(".expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}