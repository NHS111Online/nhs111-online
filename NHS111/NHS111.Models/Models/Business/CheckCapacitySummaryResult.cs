﻿using Newtonsoft.Json;
using FromExternalServices = NHS111.Models.Models.Web.FromExternalServices;

namespace NHS111.Models.Models.Business
{
    public class CheckCapacitySummaryResult : FromExternalServices.CheckCapacitySummaryResult
    {
        [JsonProperty(PropertyName = "callbackEnabled")]
        public bool CallbackEnabled { get; set; }
    }
}