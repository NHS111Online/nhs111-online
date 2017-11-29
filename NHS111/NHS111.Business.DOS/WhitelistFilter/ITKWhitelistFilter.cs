﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NHS111.Business.DOS.Configuration;
using NHS111.Models.Models.Web.CCG;
using RestSharp;
using BusinessModels = NHS111.Models.Models.Business;

namespace NHS111.Business.DOS.WhitelistFilter
{
    public class ITKWhitelistFilter : IITKWhitelistFilter
    {
        private readonly IRestClient _restCCGApi;
        private readonly IConfiguration _configuration;

        public ITKWhitelistFilter(IRestClient restCCGApi, IConfiguration configuration)
        {
            _restCCGApi = restCCGApi;
            _configuration = configuration;
        }

        public async Task<List<BusinessModels.DosService>> Filter(List<BusinessModels.DosService> resultsToFilter, string postCode)
        {
            var localWhiteList = await PopulateLocalCCGITKServiceIdWhitelist(postCode);

            foreach (var service in resultsToFilter)
            {
                service.CallbackEnabled = localWhiteList.Contains(service.Id.ToString());
            }

            return resultsToFilter;
        }

        private async Task<ServiceListModel> PopulateLocalCCGITKServiceIdWhitelist(string postCode)
        {
            var response = await _restCCGApi.ExecuteTaskAsync<CCGDetailsModel> (
                new RestRequest(string.Format(_configuration.CCGApiGetCCGByPostcode, postCode), Method.GET));

            if (response.Data != null && response.Data.ItkServiceIdWhitelist != null)
                return response.Data.ItkServiceIdWhitelist;

            return new ServiceListModel();
        }
    }

    public interface IITKWhitelistFilter : IWhitelistFilter { }
}
