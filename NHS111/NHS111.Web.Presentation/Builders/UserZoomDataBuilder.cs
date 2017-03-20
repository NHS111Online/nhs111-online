﻿using System.Web;
using System.Web.Mvc;
using NHS111.Models.Models.Web;

namespace NHS111.Web.Presentation.Builders
{
    public class UserZoomDataBuilder : IUserZoomDataBuilder
    {
        public void SetFieldsForQuestion(JourneyViewModel model)
        {
            var title = model.TitleWithoutBullets;

            SetUserZoomFields(title, GetQuestioNUrl(model), model);
        }

        public void SetFieldsForOutcome(JourneyViewModel model)
        {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

            var outcomeGroup = model.OutcomeGroup == null ? "NoGroup" : urlHelper.Encode(model.OutcomeGroup.Text);
            var url = string.Format("outcome/{0}/{1}/{2}/disposition/", urlHelper.Encode(model.PathwayNo), outcomeGroup, urlHelper.Encode(model.Id));

            SetUserZoomFields("title", url, model);
        }

        public void SetFieldsForSearch(SearchJourneyViewModel model)
        {
            SetUserZoomFields("Search", "Search", model);
        }

        public void SetFieldsForSearchResults(SearchJourneyViewModel model)
        {
            SetUserZoomFields("Search Results", "SearchResults", model);
        }

        public void SetFieldsForCareAdvice(JourneyViewModel model)
        {
            SetUserZoomFields(model.QuestionNo, GetQuestioNUrl(model), model);
        }

        private static string GetQuestioNUrl(JourneyViewModel model)
        {
            UrlHelper urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

            return string.Format("{0}/{1}/{2}/", urlHelper.Encode(model.PathwayId), urlHelper.Encode(model.PathwayTitle), urlHelper.Encode(model.QuestionNo));
        }

        private static void SetUserZoomFields(string title, string url, JourneyViewModel model)
        {
            model.UserZoomTitle = title;
            model.UserZoomUrl = url;
        }
    }

    public interface IUserZoomDataBuilder
    {
        void SetFieldsForQuestion(JourneyViewModel model);
        void SetFieldsForOutcome(JourneyViewModel model);
        void SetFieldsForSearch(SearchJourneyViewModel model);
        void SetFieldsForSearchResults(SearchJourneyViewModel model);
        void SetFieldsForCareAdvice(JourneyViewModel model);
    }
}
