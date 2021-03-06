﻿

using log4net;
using NHS111.Features.IoC;
using NHS111.Utils.RestTools;
using RestSharp;

namespace NHS111.Web.IoC
{
    using Models.IoC;
    using Presentation.Configuration;
    using Presentation.IoC;
    using StructureMap;
    using Utils.Cache;
    using Utils.IoC;

    public class WebRegistry : Registry
    {
        public WebRegistry()
        {
            Configure();
        }

        public WebRegistry(IConfiguration configuration)
        {
            // Init Redis only if connection string is set
            if (!string.IsNullOrEmpty(configuration.RedisConnectionString))
            {
                For<ICacheManager<string, string>>().Singleton().Use(new RedisManager(configuration.RedisConnectionString));
            }
            For<ILoggingRestClient>().Singleton().Use(
                    new LoggingRestClient(
                        configuration.BusinessApiProtocolandDomain,
                        LogManager.GetLogger("log"),
                        configuration.ServicePointManagerDefaultConnectionLimit,
                        configuration.RestClientTimeoutMs,
                        configuration.APIMSubscriptionKey
                    )).Named("restClientBusinessApi");
            Configure();
        }

        private void Configure()
        {
            IncludeRegistry<FeatureRegistry>();
            IncludeRegistry<UtilsRegistry>();
            IncludeRegistry<ModelsRegistry>();
            IncludeRegistry<WebPresentationRegistry>();

            Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.WithDefaultConventions();
            });
        }

    }
}
