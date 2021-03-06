﻿using NHS111.Utils.Parser;
using NHS111.Utils.RestTools;
using RestSharp;

namespace NHS111.Web.Presentation.Builders.Tests
{
    using AutoMapper;
    using Logging;
    using Moq;
    using NHS111.Models.Models.Domain;
    using NHS111.Models.Models.Web;
    using NHS111.Models.Models.Web.DosRequests;
    using NHS111.Models.Models.Web.Enums;
    using NHS111.Models.Models.Web.FromExternalServices;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;

    [TestFixture()]
    public class OutcomeViewModelBuilderTests
    {

        private Mock<IMappingEngine> _mappingEngine;
        private Mock<ICareAdviceBuilder> _mockCareAdviceBuilder;
        private Mock<ILoggingRestClient> _mockRestClient;
        private Mock<ILoggingRestClient> _mockRestPostcodeApiClient;
        private Mock<ILoggingRestClient> _mockRestClientItkDispatcherApi;
        private Mock<ILoggingRestClient> _resetClientCaseDataCaptureApi;
        private Mock<Presentation.Configuration.IConfiguration> _mockConfiguration;
        private Mock<IKeywordCollector> _mockKeywordCollector;
        private Mock<IJourneyHistoryWrangler> _mockJourneyHistoryWrangler;
        private OutcomeViewModelBuilder _outcomeViewModelBuilder;
        private Mock<ISurveyLinkViewModelBuilder> _mockSurveyLinkViewModelBuilder;
        private Mock<IAuditLogger> _mockAuditLogger;
        private Mock<IDOSBuilder> _mockDosBuilder;
        private Mock<IRecommendedServiceBuilder> _mockRecommendedServiceBuilder;
        private OutcomeViewModel _model;

        private string _mockPathwayURL = "PW755";

        private string _expectedBusinessApiPathwaySymptomGroupUrl;
        private List<int> _sentDispositions;

        [SetUp()]
        public void Setup()
        {
            _mappingEngine = new Mock<IMappingEngine>();
            _mockCareAdviceBuilder = new Mock<ICareAdviceBuilder>();
            _mockRestClient = new Mock<ILoggingRestClient>();
            _mockRestPostcodeApiClient = new Mock<ILoggingRestClient>();
            _mockRestClientItkDispatcherApi = new Mock<ILoggingRestClient>();
            _resetClientCaseDataCaptureApi = new Mock<ILoggingRestClient>();
            _mockConfiguration = new Mock<Presentation.Configuration.IConfiguration>();
            _mockJourneyHistoryWrangler = new Mock<IJourneyHistoryWrangler>();
            _mockKeywordCollector = new Mock<IKeywordCollector>();
            _mockSurveyLinkViewModelBuilder = new Mock<ISurveyLinkViewModelBuilder>();
            _mockAuditLogger = new Mock<IAuditLogger>();
            _mockDosBuilder = new Mock<IDOSBuilder>();
            _mockRecommendedServiceBuilder = new Mock<IRecommendedServiceBuilder>();
            SetupMockFillCareAdviceBuilder();

            SetupMockConfiguration();

            _outcomeViewModelBuilder = new OutcomeViewModelBuilder(_mockCareAdviceBuilder.Object,
                _mockRestClient.Object,
                _mockRestPostcodeApiClient.Object,
                _mockRestClientItkDispatcherApi.Object,
                _resetClientCaseDataCaptureApi.Object,
                _mockConfiguration.Object,
                _mappingEngine.Object,
                _mockKeywordCollector.Object,
                _mockJourneyHistoryWrangler.Object,
                _mockSurveyLinkViewModelBuilder.Object,
                _mockAuditLogger.Object,
                _mockDosBuilder.Object,
                _mockRecommendedServiceBuilder.Object);

            _sentDispositions = new List<int>();

            _mockDosBuilder.Setup(d => d.BuildDosViewModel(It.IsAny<OutcomeViewModel>(), It.IsAny<DateTime?>()))
                .Returns(new DosViewModel { Disposition = 1111 });
            _mockDosBuilder.Setup(d => d.FillCheckCapacitySummaryResult(It.IsAny<DosViewModel>(), It.IsAny<bool>(), It.IsAny<DosEndpoint?>()))
                .Callback<DosViewModel, bool, DosEndpoint?>((m, f, e) => _sentDispositions.Add(m.Disposition))
                .Returns(Task.FromResult(new DosCheckCapacitySummaryResult()));

            ConfigurationManager.AppSettings["ValidationDxRemap"] = "Dx02:Dx333";
            ConfigurationManager.AppSettings["ValidationDxRetry"] = "Dx02";
            _model = new OutcomeViewModel
            {
                Id = "Dx02",
                SymptomDiscriminatorCode = "1",
                UserInfo = new UserInfo
                {
                    Demography = new AgeGenderViewModel
                    {
                        Gender = "Male"
                    }
                },
                OutcomeGroup = OutcomeGroup.AccidentAndEmergency
            };

        }

        private void SetupMockConfiguration()
        {
            _expectedBusinessApiPathwaySymptomGroupUrl = "http://Test.ApiPathwaySymptomGroupUrl.com/" + _mockPathwayURL;
            _mockConfiguration.Setup(c => c.GetBusinessApiPathwaySymptomGroupUrl(_mockPathwayURL)).Returns(_expectedBusinessApiPathwaySymptomGroupUrl);
            _mockConfiguration.Setup(c => c.DOSWhitelist).Returns("Service 1|Service 2");
        }

        private void SetupMockFillCareAdviceBuilder()
        {
            var mockCareAdvices = new List<CareAdvice>()
            {
                new CareAdvice() {Title = "TestAdvice", Id = "CA123", Items = new List<CareAdviceText>() {new CareAdviceText(){Text = "Test advice text"}}}
            };


            _mockCareAdviceBuilder.Setup(
                cb =>
                    cb.FillCareAdviceBuilder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<List<string>>())).ReturnsAsync(mockCareAdvices);
        }

        [Test]
        public async Task PopulateGroupedDosResults_WithNoCallbacks_RequeriesWithOriginalDxCode()
        {
            var result = await _outcomeViewModelBuilder.PopulateGroupedDosResults(_model, null, null, null);

            _mockDosBuilder.Verify(d =>
                d.FillCheckCapacitySummaryResult(It.IsAny<DosViewModel>(), It.IsAny<bool>(),
                    It.IsAny<DosEndpoint?>()), Times.Exactly(2));
            Assert.AreEqual(1111, _sentDispositions.First());
            Assert.AreEqual(1002, _sentDispositions.Last());
        }

        [Test]
        public async Task PopulateGroupedDosResults_WithWithCallbacks_DoesntRequeryDos()
        {

            _mockDosBuilder.Setup(d =>
                    d.FillCheckCapacitySummaryResult(It.IsAny<DosViewModel>(), It.IsAny<bool>(),
                        It.IsAny<DosEndpoint?>()))
                .Returns(Task.FromResult(new DosCheckCapacitySummaryResult
                {
                    Success = new SuccessObject<ServiceViewModel>
                    {
                        Services = new List<ServiceViewModel>
                            {new ServiceViewModel {OnlineDOSServiceType = OnlineDOSServiceType.Callback}}
                    }
                }));

            var result = await _outcomeViewModelBuilder.PopulateGroupedDosResults(_model, null, null, null);

            _mockDosBuilder.Verify(d => d.FillCheckCapacitySummaryResult(It.IsAny<DosViewModel>(), It.IsAny<bool>(),
                It.IsAny<DosEndpoint?>()), Times.Once);
        }

        [Test]
        public async Task PopulateGroupedDosResults_WithCallbackRejected_DoesntRequery()
        {
            _mockDosBuilder.Setup(d =>
                    d.FillCheckCapacitySummaryResult(It.IsAny<DosViewModel>(), It.IsAny<bool>(),
                        It.IsAny<DosEndpoint?>()))
                .Returns(Task.FromResult(new DosCheckCapacitySummaryResult
                {
                    Success = new SuccessObject<ServiceViewModel>
                    {
                        Services = new List<ServiceViewModel>
                            {new ServiceViewModel {OnlineDOSServiceType = OnlineDOSServiceType.Callback}}
                    }
                }));

            _model.HasAcceptedCallbackOffer = false;
            var result = await _outcomeViewModelBuilder.PopulateGroupedDosResults(_model, null, null, null);

            _mockDosBuilder.Verify(d => d.FillCheckCapacitySummaryResult(It.Is<DosViewModel>(x => x.Disposition == 1111), It.IsAny<bool>(),
                It.IsAny<DosEndpoint?>()), Times.Once);
        }

        [Test]
        public async Task ConfirmationSurveyLinkBuilder()
        {
            var model = new PersonalDetailViewModel()
            {
                Id = "Dx09",
                SymptomDiscriminatorCode = "1",
                UserInfo = new UserInfo
                {
                    Demography = new AgeGenderViewModel
                    {
                        Gender = "Male"
                    }
                },
                OutcomeGroup = OutcomeGroup.AccidentAndEmergency
            };

            _mockSurveyLinkViewModelBuilder.Setup(b => b.SurveyLinkBuilder(It.IsAny<OutcomeViewModel>()))
                .ReturnsAsync(new SurveyLinkViewModel());

            var result = await _outcomeViewModelBuilder.ConfirmationSurveyLinkBuilder(model);
            Assert.IsNotNull(result.SurveyLink);
        }
    }
}
