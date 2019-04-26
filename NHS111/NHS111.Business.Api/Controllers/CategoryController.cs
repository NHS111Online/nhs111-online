﻿
using System;
using System.Collections.Generic;
using System.Web.Http.Results;
using Newtonsoft.Json;
using NHS111.Models.Models.Domain;
using NHS111.Utils.Cache;

namespace NHS111.Business.Api.Controllers {
    using System.Threading.Tasks;
    using System.Web.Http;
    using Services;

    public class CategoryController : ApiController {
        private readonly ICategoryService _categoryService;
        private readonly ICacheManager<string, string> _cacheManager;

        public CategoryController(ICategoryService categoryService, ICacheManager<string, string> cacheManager)
        {
            _categoryService = categoryService;
            _cacheManager = cacheManager;
        }

        [HttpGet]
        [Route("categories/pathways")]
        public async Task<JsonResult<IEnumerable<CategoryWithPathways>>> GetCategoriesWithPathways()
        {
            var categoriesWithPathways = await _categoryService.GetCategoriesWithPathways();
            return Json(categoriesWithPathways);
        }

        [HttpGet]
        [Route("categories/pathways/{gender}/{age}")]
        public async Task<JsonResult<IEnumerable<CategoryWithPathways>>> GetCategoriesWithPathways(string gender, int age)
        {
            var cacheKey = String.Format("GetCategoriesWithPathways-{0}-{1}", gender, age);
#if !DEBUG
                var cacheValue = await _cacheManager.Read(cacheKey);
                if (!string.IsNullOrEmpty(cacheValue))
                {
                    return Json(JsonConvert.DeserializeObject<IEnumerable<CategoryWithPathways>>(cacheValue));
                }
#endif

            var categoriesWithPathways = await _categoryService.GetCategoriesWithPathways(gender, age);
#if !DEBUG
              _cacheManager.Set(cacheKey, JsonConvert.SerializeObject(categoriesWithPathways));
#endif
            return Json(categoriesWithPathways);
        }
    }
}