﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using Newtonsoft.Json;
using NHS111.Models.Models.Domain;
using NHS111.Models.Models.Web;
using NHS111.Models.Models.Web.Enums;
using NHS111.Models.Models.Web.FromExternalServices;
using NHS111.Utils.Helpers;
using IConfiguration = NHS111.Web.Presentation.Configuration.IConfiguration;

namespace NHS111.Web.Presentation.Builders
{
    public class QuestionViewModelBuilder : IQuestionViewModelBuilder
    {
        private readonly IOutcomeViewModelBuilder _outcomeViewModelBuilder;
        private readonly IJustToBeSafeFirstViewModelBuilder _justToBeSafeFirstViewModelBuilder;
        private readonly IRestfulHelper _restfulHelper;
        private readonly IConfiguration _configuration;
        private readonly IMappingEngine _mappingEngine;
        private readonly ISymptomDicriminatorCollector _symptomDicriminatorCollector;
        private readonly IKeywordCollector _keywordCollector;

        public QuestionViewModelBuilder(IOutcomeViewModelBuilder outcomeViewModelBuilder, IJustToBeSafeFirstViewModelBuilder justToBeSafeFirstViewModelBuilder, IRestfulHelper restfulHelper,
            IConfiguration configuration, IMappingEngine mappingEngine, ISymptomDicriminatorCollector symptomDicriminatorCollector, IKeywordCollector keywordCollector)
        {
            _outcomeViewModelBuilder = outcomeViewModelBuilder;
            _justToBeSafeFirstViewModelBuilder = justToBeSafeFirstViewModelBuilder;
            _restfulHelper = restfulHelper;
            _configuration = configuration;
            _mappingEngine = mappingEngine;
            _symptomDicriminatorCollector = symptomDicriminatorCollector;
            _keywordCollector = keywordCollector;
        }

        public JourneyViewModel BuildGender(JourneyViewModel model)
        {
            return model;
        }

        public async Task<JourneyViewModel> BuildGender(string pathwayTitle)
        {
            // do we have a symptom that is in the white list
            // return a list of pathways numbers
            var pathwayNo = await _restfulHelper.GetAsync(_configuration.GetBusinessApiPathwayNumbersUrl(pathwayTitle));
            return new JourneyViewModel() { PathwayNo = pathwayNo };
        }

        public async Task<Tuple<string, JourneyViewModel>> BuildQuestion(JourneyViewModel model)
        {
            model.PreviousTitle = model.Title;
            model.PreviousStateJson = model.StateJson;
            var answer = JsonConvert.DeserializeObject<Answer>(model.SelectedAnswer);
            var journey = JsonConvert.DeserializeObject<Journey>(model.JourneyJson);
            journey.Steps.Add(new JourneyStep { QuestionNo = model.QuestionNo, QuestionTitle = model.Title, Answer = answer, QuestionId = model.Id });
            model.JourneyJson = JsonConvert.SerializeObject(journey);
            model.State = JsonConvert.DeserializeObject<Dictionary<string, string>>(model.StateJson);

            var request = new HttpRequestMessage { Content = new StringContent(JsonConvert.SerializeObject(answer.Title), Encoding.UTF8, "application/json") };
            var response = await(await _restfulHelper.PostAsync(_configuration.GetBusinessApiNextNodeUrl(model.PathwayId, model.Id, HttpUtility.UrlEncode(JsonConvert.SerializeObject(model.State))), request)).Content.ReadAsStringAsync();
            
            model = _symptomDicriminatorCollector.Collect(JsonConvert.DeserializeObject<QuestionWithAnswers>(response),model);
            model = _keywordCollector.Collect(answer, model);
            model = _mappingEngine.Mapper.Map(response, model);
            //model = _mappingEngine.Map(answer, model);

            return await ActionSelection(model);
        }

        public async Task<JourneyViewModel> BuildPreviousQuestion(JourneyViewModel model)
        {
            var journey = JsonConvert.DeserializeObject<Journey>(model.JourneyJson);
            var url = _configuration.GetBusinessApiQuestionByIdUrl(model.PathwayId, journey.Steps.Last().QuestionId);
            var response = await _restfulHelper.GetAsync(url);
            if (journey.Steps.Count == 1)
            {
                model.PreviousTitle = string.Empty;
                journey.Steps.RemoveAt(journey.Steps.Count - 1);
            }
            else
            {
                journey.Steps.RemoveAt(journey.Steps.Count - 1);
                model.PreviousTitle = journey.Steps.Last().IsJustToBeSafe == false ? journey.Steps.Last().QuestionTitle : string.Empty;
            }

            model.CollectedKeywords = _keywordCollector.CollectKeywordsFromPreviousQuestion(model.CollectedKeywords, journey.Steps);
            model.StateJson = model.PreviousStateJson;
            model.JourneyJson = JsonConvert.SerializeObject(journey);
            model.State = JsonConvert.DeserializeObject<Dictionary<string, string>>(model.StateJson);
            return _mappingEngine.Mapper.Map(response, model);
        }

        public async Task<string> BuildSearch(string input)
        {
            var response = await _restfulHelper.GetAsync(_configuration.GetBusinessApiGroupedPathwaysUrl(input));
            var pathways = JsonConvert.DeserializeObject<List<GroupedPathways>>(response);
            return JsonConvert.SerializeObject(pathways.Select(pathway => new { label = pathway.Group, value = pathway.PathwayNumbers }));
        }

        public OutcomeViewModel AddressSearchViewBuilder(OutcomeViewModel model)
        {
            model.AddressSearchViewModel = new AddressSearchViewModel()
            {
                PostcodeApiAddress = _configuration.PostcodeSearchByIdApiUrl,
                PostcodeApiSubscriptionKey = _configuration.PostcodeSubscriptionKey
            };
            return model;
        }

        public async Task<Tuple<string, JourneyViewModel>> ActionSelection(JourneyViewModel model)
        {
            switch (model.NodeType)
            {
                case NodeType.Outcome:
                {
                    var newModel = _mappingEngine.Mapper.Map<OutcomeViewModel>(model);
                    if (newModel.OutcomeGroup != null && newModel.OutcomeGroup.Id != "Call_999")
                    {
                        newModel = AddressSearchViewBuilder(newModel);
                        newModel.CareAdviceMarkers = newModel.State.Keys.Where(key => key.StartsWith("Cx"));
                    }
                    return await DetermineOutcomeView(newModel);
                }

                case NodeType.Pathway:
                    {
                        var pathway = JsonConvert.DeserializeObject<Pathway>(await _restfulHelper.GetAsync(_configuration.GetBusinessApiPathwayUrl(model.PathwayId)));
                        if (pathway == null) return null;

                        var derivedAge = model.UserInfo.Age == -1 ? pathway.MinimumAgeInclusive : model.UserInfo.Age;
                        var newModel = new JustToBeSafeViewModel
                        {
                            PathwayId = pathway.Id,
                            PathwayNo = pathway.PathwayNo,
                            PathwayTitle = pathway.Title,
                            UserInfo = new UserInfo() { Age = derivedAge, Gender = pathway.Gender },
                            JourneyJson = model.JourneyJson,
                            SymptomDiscriminator = model.SymptomDiscriminator,
                            State = JourneyViewModelStateBuilder.BuildState(pathway.Gender, derivedAge),
                        };

                        newModel.StateJson = JourneyViewModelStateBuilder.BuildStateJson(newModel.State);

                        return await _justToBeSafeFirstViewModelBuilder.JustToBeSafeFirstBuilder(newModel);
                    }
                case NodeType.DeadEndJump:
                    return new Tuple<string, JourneyViewModel>("../Question/DeadEndJump", model);
                case NodeType.Question:
                default:
                    return new Tuple<string, JourneyViewModel>("../Question/Question", model);

            }
        }

        private async Task<Tuple<string, JourneyViewModel>> DetermineOutcomeView(OutcomeViewModel newModel)
        {
            if (newModel.OutcomeGroup != null && newModel.OutcomeGroup.Id == "Call_999")
                return new Tuple<string, JourneyViewModel>("../Outcome/Emergency",
                    await _outcomeViewModelBuilder.DispositionBuilder(newModel));

            if (newModel.OutcomeGroup != null && newModel.OutcomeGroup.Id == "Home_Care")
                return new Tuple<string, JourneyViewModel>("../Outcome/HomeCare",
                    await _outcomeViewModelBuilder.DispositionBuilder(newModel));


            return (newModel.OutcomeGroup != null && newModel.OutcomeGroup.Id == "SP_Accident_and_emergency")
                ? new Tuple<string, JourneyViewModel>("../Outcome/Disposition2",
                    await _outcomeViewModelBuilder.DispositionBuilder(newModel))
                : new Tuple<string, JourneyViewModel>("../Outcome/Disposition",
                    await _outcomeViewModelBuilder.DispositionBuilder(newModel));
        }
    }

    public interface IQuestionViewModelBuilder
    {
        JourneyViewModel BuildGender(JourneyViewModel model);
        Task<JourneyViewModel> BuildGender(string pathwayTitle);
        //Task<JourneyViewModel> BuildSlider(JourneyViewModel model);
        //Task<JourneyViewModel> BuildSlider(string pathwayTitle, string gender, int age);
        Task<Tuple<string, JourneyViewModel>> BuildQuestion(JourneyViewModel model);
        Task<JourneyViewModel> BuildPreviousQuestion(JourneyViewModel model);
        Task<string> BuildSearch(string input);
        Task<Tuple<string, JourneyViewModel>> ActionSelection(JourneyViewModel model);
    }
}