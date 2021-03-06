﻿using NHS111.Business.Services;
using NHS111.Models.Models.Domain;
using NHS111.Utils.Cache;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Newtonsoft.Json;

namespace NHS111.Business.Api.Controllers
{
    public class SymptomDiscriminatorController : ApiController
    {
        private readonly ISymptomDisciminatorService _symptomDisciminatorService;

        public SymptomDiscriminatorController(ISymptomDisciminatorService symptomDisciminatorService)
        {
            _symptomDisciminatorService = symptomDisciminatorService;
        }

        [System.Web.Http.Route("symptomdiscriminator/{symptomDisciminatorCode}")]
        public async Task<JsonResult<SymptomDiscriminator>> GetSymptomDisciminator(string symptomDisciminatorCode, string cacheKey = null)
        {
            var symptomDiscriminators = await _symptomDisciminatorService.GetSymptomDisciminator(symptomDisciminatorCode);
            return Json(symptomDiscriminators);
        }
    }
}
