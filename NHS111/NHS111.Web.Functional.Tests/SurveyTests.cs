﻿using NHS111.Web.Functional.Utils;
using NUnit.Framework;
using OpenQA.Selenium;
using System.Linq;

namespace NHS111.Web.Functional.Tests
{
    [TestFixture]
    public class SurveyTests : BaseTests
    {

        /*
         * These tests ensure the model is correct for passing through to the survey.
         * They ensure we don't have issues such as the Pathways Title being passed
         * through instead of the Digital Title.
         */

        [Test]
        public void DigitalTitleThroughSearch()
        {
            var questionPage = TestScenarios.LaunchTriageScenario(Driver, "Arm Injury, Penetrating",
                TestScenarioSex.Male,
                TestScenarioAgeGroups.Adult);

            questionPage.VerifyHiddenField("PathwayTitle", "Arm Injury, Penetrating");
            questionPage.VerifyHiddenField("DigitalTitle", "Arm or shoulder injury with a cut or wound");
        }

        [Test]
        public void DigitalTitleThroughCategoryAllTopics()
        {
            var categoryPage = TestScenarios.LaunchCategoryScenario(Driver, TestScenarioSex.Female, 64);
            var questionPage = TestScenarioPart.Question(categoryPage.SelectPathway("Arm or shoulder injury with a cut or wound"));

            questionPage.VerifyHiddenField("PathwayTitle", "Arm Injury, Penetrating");
            questionPage.VerifyHiddenField("DigitalTitle", "Arm or shoulder injury with a cut or wound");
        }

        [Test]
        public void DigitalTitleThroughDeeplink()
        {
            var questionPage = TestScenarios.LaunchDeeplinkScenario(Driver, TestScenarioSex.Male, TestScenarioAgeGroups.Adult, "L1 2SA");

            questionPage.VerifyHiddenField("PathwayTitle", "Emergency Prescription 111 online");
            questionPage.VerifyHiddenField("DigitalTitle", "Emergency prescription");
        }



        [Test]
        public void InterstitialPageHasSurveyUrl()
        {
            // Run dead end journey as short/quick to get to the survey link
            var questionPage = TestScenarios.LaunchTriageScenario(Driver, "Trauma Blisters", TestScenarioSex.Male, TestScenarioAgeGroups.Adult);

            var outcomePage = questionPage
                .Answer<DeadEndPage>(1);

            outcomePage.CompareAndVerify("1");  // Captures screenshot of disposition

            var surveyUrlElement = Driver.FindElement(By.CssSelector(".survey-banner [name='SurveyUrl']"));
            var surveyUrl = surveyUrlElement.GetAttribute("value");
            Assert.IsNotEmpty(surveyUrl);

            var surveyButton = Driver.FindElement(By.CssSelector(".survey-banner button"));
            surveyButton.Click();

            Driver.SwitchTo().Window(Driver.WindowHandles.Last()); // Handle new tab 
            var surveyInterstitialPage = new SurveyInterstitial(Driver);
            surveyInterstitialPage.VerifyHeading("Thanks for agreeing to take our survey");
            surveyInterstitialPage.VerifyUrl(surveyUrl);
            surveyInterstitialPage.CompareAndVerify("2"); // Captures screenshot of survey interstitial


        }

        [Test]
        public void InterstitialPageHasSurveyUrlViaEP()
        {
            var questionPage = TestScenarios.LaunchRecommendedServiceScenario(Driver, "Emergency Prescription 111 online", TestScenarioSex.Male, TestScenarioAgeGroups.Adult, "L1 2SA");

            questionPage.VerifyQuestion("Can you contact your GP or usual pharmacy?");
            questionPage
                .Answer(2)
                .Answer<PreOutcomePage>(1)
                .ClickShowServices();

            var surveyUrlElement = Driver.FindElement(By.CssSelector(".survey-banner [name='SurveyUrl']"));
            var surveyUrl = surveyUrlElement.GetAttribute("value");
            Assert.IsNotEmpty(surveyUrl);
            var surveyButton = Driver.FindElement(By.CssSelector(".survey-banner button"));
            surveyButton.Click();

            Driver.SwitchTo().Window(Driver.WindowHandles.Last()); // Handle new tab 
            var surveyInterstitialPage = new SurveyInterstitial(Driver);
            surveyInterstitialPage.VerifyHeading("Thanks for agreeing to take our survey");
            surveyInterstitialPage.VerifyUrl(surveyUrl);
            surveyInterstitialPage.CompareAndVerify("2"); // Captures screenshot of survey interstitial


        }
    }
}
