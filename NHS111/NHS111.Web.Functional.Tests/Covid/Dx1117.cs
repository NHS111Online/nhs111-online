﻿using NHS111.Web.Functional.Utils;
using NUnit.Framework;


namespace NHS111.Web.Functional.Tests.Covid
{
    [TestFixture]
    class Dx1117 : BaseTests
    {
        private QuestionPage LaunchWithCovidLink(string sex, int age, string guidedSelection)
        {
            var homepage = TestScenarioPart.HomePage(Driver);
            var covidHomePage = homepage.ClickCovidLink();
            var locationPage = covidHomePage.ClickOnStartNow();
            var moduleZeroPage = TestScenarioPart.ModuleZero(locationPage);
            var demographicsPage = TestScenarioPart.Demographics(moduleZeroPage);
            var guidedSelectionPage = TestScenarioPart.Question(demographicsPage, sex, age);
            var weirdQuestionPage = guidedSelectionPage.guidedSelection(guidedSelection);

            return weirdQuestionPage.AnswerWeirdQuestion();
        }

        [Test]
        public void NavigateToDispositionDx01213_ColdAndFluSymptoms()
        {
            //0,2,0,2,2,2,2,2,2,2,2,2,2,2,3,1
            var questionPage = LaunchWithCovidLink(TestScenerioSex.Female, TestScenerioAgeGroups.Adult, "Coldandflusymptoms");

            var outcomePage = questionPage.Answer(3) // No - breathless
                .Answer(1) // Continuous Cough
                .Answer(3) // No - breathing harder
                .Answer(3) // No - so ill
                .Answer(3) // No - sharp pain
                .Answer(3) // No - choked
                .Answer(3) // No - breathed toxic
                .Answer(3) // No - coughed blood
                .Answer(3) // No - confused
                .Answer(3) // No - doctor told you
                .Answer(3) // No - NHS letter
                .Answer(3) // No - diabetes
                .Answer(3) // Not Sure - either of these
                .Answer(4) // No - temperature
                .Answer<OutcomePage>(2); // Asthma
            outcomePage.VerifyHiddenField("Id", "Dx1117");
        }


    }
}
