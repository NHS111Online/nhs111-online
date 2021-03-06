﻿
using NHS111.Utils.RestTools;
using RestSharp;

namespace NHS111.Web.Controllers
{
    using Models.Models.Domain;
    using Models.Models.Web;
    using Presentation.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    public class TopicController : Controller
    {

        public TopicController(ILoggingRestClient restClientBusinessApi, IConfiguration configuration)
        {
            _restClientBusinessApi = restClientBusinessApi;
            _configuration = configuration;
        }

        public async Task<ActionResult> Search(string q, string gender, int age)
        {
            var ageGroup = new AgeCategory(age);
            var url = _configuration.GetBusinessApiPathwaySearchUrl(gender, ageGroup.Value);
            var response = await _restClientBusinessApi.ExecuteAsync<List<SearchResultViewModel>>(new RestRequest(url, Method.GET)).ConfigureAwait(false);

            return View(new SearchJourneyViewModel { Results = response.Data });
        }

        private readonly ILoggingRestClient _restClientBusinessApi;
        private readonly IConfiguration _configuration;
    }

}