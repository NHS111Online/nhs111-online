﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NHS111.Business.Builders;
using NHS111.Business.Configuration;
using NHS111.Business.Transformers;
using NHS111.Models.Models.Configuration;
using NHS111.Models.Models.Domain;
using NHS111.Models.Models.Web;
using NHS111.Models.Models.Web.FromExternalServices;
using NHS111.Utils.Helpers;
using NHS111.Utils.Parser;

namespace NHS111.Business.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IConfiguration _configuration;
        private readonly IRestfulHelper _restfulHelper;
        private readonly IAnswersForNodeBuilder _answersForNodeBuilder;
        private readonly IModZeroJourneyStepsBuilder _modZeroJourneyStepsBuilder;
        private readonly IKeywordCollector _keywordCollector;
        private readonly ICareAdviceService _careAdviceService;
        private readonly ICareAdviceTransformer _careAdviceTransformer;
        public QuestionService(IConfiguration configuration, IRestfulHelper restfulHelper, IAnswersForNodeBuilder answersForNodeBuilder, IModZeroJourneyStepsBuilder modZeroJourneyStepsBuilder, IKeywordCollector keywordcollector, ICareAdviceService careAdviceService, ICareAdviceTransformer careAdviceTransformer)
        {
            _configuration = configuration;
            _restfulHelper = restfulHelper;
            _answersForNodeBuilder = answersForNodeBuilder;
            _modZeroJourneyStepsBuilder = modZeroJourneyStepsBuilder;
            _keywordCollector = keywordcollector;
            _careAdviceService = careAdviceService;
            _careAdviceTransformer = careAdviceTransformer;
        }

        public async Task<string> GetQuestion(string id)
        {
            return await _restfulHelper.GetAsync(_configuration.GetDomainApiQuestionUrl(id));
        }

        public async Task<string> GetAnswersForQuestion(string id)
        {
            return await _restfulHelper.GetAsync(_configuration.GetDomainApiAnswersForQuestionUrl(id));
        }

        public async Task<HttpResponseMessage> GetNextQuestion(string id, string nodeLabel, string answer)
        {
            var request = new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(answer), Encoding.UTF8, "application/json") };
            return (await _restfulHelper.PostAsync(_configuration.GetDomainApiNextQuestionUrl(id, nodeLabel), request));
        }

        public async Task<string> GetFirstQuestion(string pathwayId)
        {
            return await _restfulHelper.GetAsync(_configuration.GetDomainApiFirstQuestionUrl(pathwayId));
        }

        public async Task<string> GetJustToBeSafeQuestionsFirst(string pathwayId)
        {
            return await _restfulHelper.GetAsync(_configuration.GetDomainApiJustToBeSafeQuestionsFirstUrl(pathwayId));
        }

        public async Task<HttpResponseMessage> GetFullPathwayJourney(string traumaType, JourneyStep[] steps, string startingPathwayId, string dispositionCode, IDictionary<string, string> state)
        {
            var age = GetAgeFromState(state);
            var gender = GetGenderFromState(state);
            var moduleZeroJourney = await GetModuleZeroJourney(gender, age, traumaType);
            
            var pathwaysJourney = await GetPathwayJourney(steps, startingPathwayId, dispositionCode, gender, age);
            var filteredJourneySteps = NavigateReadNodeLogic(steps, pathwaysJourney.ToList(), state);

            //keywords from pathways
            var pathwayKeywords = filteredJourneySteps.Where(q => q.Labels.Contains("Pathway")).Select(q => q.Question.Keywords);
            var pathwayExcludeKeywords = filteredJourneySteps.Where(q => q.Labels.Contains("Pathway")).Select(q => q.Question.ExcludeKeywords);
            var keywords = _keywordCollector.CollectKeywords(pathwayKeywords, pathwayExcludeKeywords);
            
            // keywords from answers
            var journeySteps = filteredJourneySteps.Where(q => q.Answered != null).Select(q => new JourneyStep {Answer = q.Answered}).ToList();
            keywords = _keywordCollector.CollectKeywordsFromPreviousQuestion(keywords, journeySteps);
            
            var careAdvice = await _careAdviceService.GetCareAdvice(new AgeCategory(age).Value, gender,
                _keywordCollector.ConsolidateKeywords(keywords).Aggregate((i, j) => i + '|' + j), dispositionCode);

            var careAdviceAsQuestionWithAnswersListString = _careAdviceTransformer.AsQuestionWithAnswersList(careAdvice);
            var careAdviceAsQuestionWithAnswers = JsonConvert.DeserializeObject<List<QuestionWithAnswers>>(careAdviceAsQuestionWithAnswersListString);

            var content = new StringContent(JsonConvert.SerializeObject(moduleZeroJourney.Concat(filteredJourneySteps).Concat(careAdviceAsQuestionWithAnswers)), Encoding.UTF8, "application/json");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        }

        private int GetAgeFromState(IDictionary<string, string> state)
        {
            int age;
            if (!int.TryParse(FindStateValue(state, "PATIENT_AGE"), out age))
                throw new ArgumentException("State value for key 'PATIENT_AGE' must be an integer.");
            return age;
        }

        private string GetGenderFromState(IDictionary<string, string> state)
        {
            var genderStateValue = FindStateValue(state, "PATIENT_GENDER");

            if (!(genderStateValue == "\"F\"" || genderStateValue == "\"M\""))
                throw new ArgumentException("State value for key 'PATIENT_GENDER' must be of value \"F\" or \"M\"");

            var gender = genderStateValue == "\"F\"" ? "Female" : "Male";
            return gender;
        }

        private IEnumerable<QuestionWithAnswers> NavigateReadNodeLogic(JourneyStep[] answeredQuestions, IEnumerable<QuestionWithAnswers> journey, IDictionary<string, string> state)
        {
            var filteredJourney = new List<QuestionWithAnswers>();

            var groupledRead = journey.Where(s => s.Labels.Contains("Read")).GroupBy(s => s.Question.Id, s => s.Answered,
                (key, g) => new { Node = key, Answers = g.ToList().Distinct() });

            var pathNavigationAnswers = new List<Answer>();
            foreach (var step in journey)
            {
                if (!step.Labels.Contains("Read"))
                {
                    if(!step.Labels.Contains("Question") || (step.Labels.Contains("Question") && answeredQuestions.Any(q => q.QuestionId == step.Question.Id)))
                        filteredJourney.Add(step);
                }
                else
                {
                    var answers = groupledRead.First(s => s.Node == step.Question.Id).Answers;
                    var value = FindStateValue(state, step);
                    var answer = _answersForNodeBuilder.SelectAnswer(answers, value);
                    if (answer != default(Answer) && step.Answered.Title == answer.Title)
                    {
                        filteredJourney.Add(step);
                        pathNavigationAnswers.Add(answer);
                    }
                }
            }

            return filteredJourney;
        }

        private static IDictionary<string, string> BuildState(string gender, int age, IDictionary<string, string> state)
        {
            var ageCategory = new AgeCategory(age);

            state.Add("PATIENT_AGE", age.ToString());
            state.Add("PATIENT_GENDER", string.Format("\"{0}\"", gender.First().ToString().ToUpper()));
            state.Add("PATIENT_PARTY", "1");
            state.Add("PATIENT_AGEGROUP", ageCategory.Value);

            return state;
        }

        private string FindStateValue(IDictionary<string, string> state, QuestionWithAnswers step)
        {
            return FindStateValue(state, step.Question.Title);
        }

        private string FindStateValue(IDictionary<string, string> state, string key)
        {
            return state.ContainsKey(key)
                ? state[key]
                : null;
        }

        private async Task<IEnumerable<QuestionWithAnswers>> GetModuleZeroJourney(string gender, int age, string traumaType)
        {
            var pathwayJourney = _modZeroJourneyStepsBuilder.GetModZeroJourney(gender, age, traumaType);
            var steps = pathwayJourney.Steps;

            var request = new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(steps), Encoding.UTF8, "application/json") };
            var response = await _restfulHelper.PostAsync(_configuration.GetDomainApiPathwayJourneyUrl(pathwayJourney.PathwayId, pathwayJourney.DispositionId, gender, age), request).ConfigureAwait(false);

            var moduleZeroJourney = JsonConvert.DeserializeObject<IEnumerable<QuestionWithAnswers>>(await response.Content.ReadAsStringAsync());
            var state = BuildState(gender, age, pathwayJourney.State);
            var filteredModZeroJourney = NavigateReadNodeLogic(steps.ToArray(), moduleZeroJourney.ToList(), state);

            return filteredModZeroJourney;
        }

        public async Task<IEnumerable<QuestionWithAnswers>> GetPathwayJourney(JourneyStep[] steps, string startingPathwayId, string dispositionCode, string gender, int age)
        {
            var request = new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(steps), Encoding.UTF8, "application/json") };
            var response = await _restfulHelper.PostAsync(_configuration.GetDomainApiPathwayJourneyUrl(startingPathwayId, dispositionCode, gender, age), request).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<IEnumerable<QuestionWithAnswers>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<string> GetJustToBeSafeQuestionsNext(string pathwayId, IEnumerable<string> answeredQuestionIds, bool multipleChoice, string selectedQuestionId)
        {
            return await _restfulHelper.GetAsync(_configuration.GetDomainApiJustToBeSafeQuestionsNextUrl(pathwayId, answeredQuestionIds, multipleChoice, selectedQuestionId));
        }
    }

    public interface IQuestionService
    {
        Task<IEnumerable<QuestionWithAnswers>> GetPathwayJourney(JourneyStep[] steps, string startingPathwayId,
            string dispositionCode, string gender, int age);
        Task<HttpResponseMessage> GetFullPathwayJourney(string traumaType, JourneyStep[] steps, string startingPathwayId, string dispositionCode, IDictionary<string, string> state);
        Task<string> GetQuestion(string id);
        Task<string> GetAnswersForQuestion(string id);
        Task<HttpResponseMessage> GetNextQuestion(string id, string nodeLabel,  string answer);
        Task<string> GetFirstQuestion(string pathwayId);
        Task<string> GetJustToBeSafeQuestionsFirst(string pathwayId);
        Task<string> GetJustToBeSafeQuestionsNext(string pathwayId, IEnumerable<string> answeredQuestionIds, bool multipleChoice, string selectedQuestionId);
    }
}