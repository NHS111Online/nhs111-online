﻿using FluentValidation.Mvc;
using log4net;
using NHS111.Features;
using NHS111.Utils.RestTools;
using NHS111.Web.Presentation.Filters;
using NHS111.Web.Presentation.Logging;

namespace NHS111.Web
{
    using Authentication;
    using Models.Models.Web;
    using Presentation.ModelBinders;
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Authentication;
    using Models.Models.Web;
    using NHS111.Web.IoC;
    using Presentation.ModelBinders;
    using Utils.Logging;

    public class MvcApplication
        : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;
            
            // Enable TLS 1.2 for outbound connections so that TLS 1.2 on the downstream App Services can be enabled
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            // Register Web APIs
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // Register MVC Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            ModelBinders.Binders[typeof(JourneyViewModel)] = new JourneyViewModelBinder();
            ModelBinders.Binders[typeof(OutcomeViewModel)] = new JourneyViewModelBinder();
            ModelBinders.Binders[typeof(PersonalDetailViewModel)] = new JourneyViewModelBinder();
            ModelBinders.Binders[typeof(QuestionViewModel)] = new JourneyViewModelBinder();

            var basicAuthFeature = new BasicAuthFeature();
            if (basicAuthFeature.IsEnabled)
            {
                GlobalFilters.Filters.Add(new BasicAuthenticationAttribute(
                    ConfigurationManager.AppSettings["login_credential_user"],
                    ConfigurationManager.AppSettings["login_credential_password"]));
            }

            // Get AuditLogger using dependency resolution via IoC
            DependencyResolver.SetResolver(StructuremapMvc.StructureMapDependencyScope);
            var auditLogger = DependencyResolver.Current.GetService<IAuditLogger>();

            GlobalFilters.Filters.Add(new LogJourneyFilterAttribute(auditLogger));

            // StartSession requires logger so it can capture browser info at start of user's session.
            // It can be used on globally as it doesn't set a new session ID if one already exists.
            GlobalFilters.Filters.Add(new StartSessionFilterAttribute(auditLogger));

            FluentValidationModelValidatorProvider.Configure();

            var razorEngine = ViewEngines.Engines.OfType<RazorViewEngine>().FirstOrDefault();
            if (razorEngine == null) return;
            var newPartialViewFormats = new[]
            {
                "~/Views/{1}/Elements/{0}.cshtml",
                "~/Views/Shared/Elements/{0}.cshtml",
                "~/Views/Shared/Repeat_Prescription/{0}.cshtml",
                "~/Views/Shared/Service_First/{0}.cshtml",
                "~/Views/Shared/Service_First/service_types/{0}.cshtml",
                "~/Views/Outcome/Confirmation/Service_First/Emergency_Prescription/{0}.cshtml",
                "~/Views/Shared/Help/{0}.cshtml"
            };

            razorEngine.PartialViewLocationFormats = razorEngine.PartialViewLocationFormats.Union(newPartialViewFormats).ToArray();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                var currentUrl = "";

                var context = HttpContext.Current;
                if (context == null)
                    currentUrl = context.Request.RawUrl;

                var lastException = GetException();
                if (lastException == null)
                    return; //nothing to log

                string data = "";
                foreach (DictionaryEntry pair in lastException.Data)
                    data += string.Format("{0}: {1}{2}", pair.Key, pair.Value, Environment.NewLine);

                Log4Net.Error(string.Format("{0} occured executing '{1}'{5}{2}{5}{3}{5}Exception.Data:{5}{4}",
                    lastException.GetType().FullName, currentUrl, lastException.Message,
                    lastException.StackTrace, data, Environment.NewLine));
            }
            catch (Exception ex)
            {
                Log4Net.Error(string.Format("Exception occured in OnError: [{0}]", ex.Message));
            }
        }

        private Exception GetException()
        {
            var lastException = Server.GetLastError();
            if (lastException == null && HttpContext.Current.AllErrors.Any())
                lastException = HttpContext.Current.AllErrors.First();

            return lastException;
        }
    }
}
