﻿using System;
using System.Text;
using NHS111.SmokeTest.Utils;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace NHS111.SmokeTests
{
    [TestFixture]
    public class RegressionTests
    {
        private IWebDriver _driver;

        [TestFixtureSetUp]
        public void InitTests()
        {
            _driver = new ChromeDriver();
        }

        [TestFixtureTearDown]
        public void TeardownTest()
        {
            try
            {
                _driver.Quit();
                _driver.Dispose();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
        }

         
        [Test]
        public void PT8JumpToDx35()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(_driver, "Tiredness (Fatigue)", TestScenerioGender.Male, TestScenerioAgeGroups.Adult);

            questionPage.ValidateQuestion("Have you got a raised temperature now or have you had one at any time since the tiredness started?");
            var outcomePage = questionPage
                .AnswerSuccessiveByOrder(3, 4)
                .Answer(4)
                .Answer(4)
                .Answer(3)
                .Answer(4)
                .Answer(3)
                .AnswerSuccessiveByOrder(5, 2)
                .AnswerSuccessiveByOrder(3, 4)
                .AnswerForDispostion("Alcohol");

            outcomePage.VerifyOutcome("A nurse from 111 will phone you");
        }
        [Test]
        public void PathwayNotFound()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(_driver, "Wound Problems, Plaster Casts, Tubes and Metal Appliances", TestScenerioGender.Male, TestScenerioAgeGroups.Adult);

            questionPage.ValidateQuestion("Is the problem to do with any of these?");
            var outcomePage = questionPage
 
             .AnswerForDispostion("A tube or drain");
        
            outcomePage.VerifyPathwayNotFound();
        }


        [Test]
        public void SplitQuestionNavigateBackDisplaysCorrectCareAdvice()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(_driver, "Headache", "Female", 49);

            var outcomePage = questionPage.ValidateQuestion("Is there a chance you are pregnant?")

           .AnswerSuccessiveByOrder(3, 4)
            .Answer(1)
            .Answer(3)
            .Answer(5)
            .Answer(3)
            .Answer(4)
            .Answer(2)
            .Answer(3)
            .Answer(3)
            .Answer(3)
            .Answer(4)
            .Answer(1)
            .Answer(3)
            .Answer(4)
            .AnswerForDispostion(1);

            var newOutcome = outcomePage.NavigateBack()
            .Answer(3, false)
            .Answer(1)


             .AnswerForDispostion("Within the next 6 hours");

            newOutcome.EnterPostCodeAndSubmit("LS17 7NZ");

            newOutcome.VerifyOutcome("You should speak to your GP practice within the next 6 hours");
            newOutcome.VerifyCareAdvice(new[] { "Medication, next dose", "Medication, pain and/or fever", "Headache" });

            
        }

        [Test]
        public void SplitQuestionJourneyThroughEachRoute()
        {
            var questionPage = TestScenerios.LaunchTriageScenerio(_driver, "Headache", TestScenerioGender.Male, TestScenerioAgeGroups.Adult);

            questionPage.ValidateQuestion("Have you hurt or banged your head in the last 7 days?");
            var outcomePage = questionPage
                .Answer(3)
                .Answer(3)
                .Answer(1)
             .AnswerForDispostion("Yes - I have a rash that doesn't disappear if I press it");

            outcomePage.VerifyOutcome("Your answers suggest you should dial 999 now for an ambulance");

            TestScenerios.LaunchTriageScenerio(_driver, "Headache", "Female", 49);

            questionPage.ValidateQuestion("Is there a chance you are pregnant?")
            
           .AnswerSuccessiveByOrder(3, 4)
            .Answer(1)
            .Answer(3)
            .Answer(5)
            .Answer(3)
            .Answer(4)
            .Answer(2)
            .Answer(3)
            .Answer(3)
            .Answer(3)
            .Answer(4)
            .Answer(1)
            .Answer(3)
            .Answer(4)
                //.Answer(1)
                //.NavigateBack()
            .Answer(3)
            .Answer(1)
              

             .AnswerForDispostion("Within the next 6 hours");

            outcomePage.EnterPostCodeAndSubmit("LS17 7NZ");

            outcomePage.VerifyOutcome("You should speak to your GP practice within the next 6 hours");


            TestScenerios.LaunchTriageScenerio(_driver, "Headache", "Female", 50);

            questionPage.ValidateQuestion("Is there a chance you are pregnant?")
            
               .AnswerSuccessiveByOrder(3, 5)
                .Answer(5)
                .Answer(3)
                .Answer(4)
                .Answer(2)
                .AnswerSuccessiveByOrder(3, 3)

             .AnswerForDispostion("Yes");

            outcomePage.EnterPostCodeAndSubmit("LS17 7NZ");


            outcomePage.VerifyOutcome("You should speak to your GP practice within the next 2 hours");
        }

        [Test]
        //alias and id check for categories age 5 Female with mental health
        public void CategoriesPresentForFemaleChild()
        {
            var categoryPage = TestScenerios.LaunchCategoryScenerio(_driver, TestScenerioGender.Female, 5);

            categoryPage.VerifyPathwayInCategoryList("Abdomen injury - skin not broken", "PW500FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body - skin not broken", "PW500FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Abdomen injury with broken skin", "PW508FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body with broken skin", "PW508FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Abdominal pain", "PW517FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Stomach pain", "PW517FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Absent periods", "PW1676FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Missed periods", "PW1676FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Accidental poisoning", "PW881FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breathed in something poisonous", "PW881FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swallowed something poisonous", "PW881FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Drunk too much alcohol", "PW1552FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Alcohol intoxication", "PW1552FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury - skin not broken", "PW1512FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Foot injury - skin not broken", "PW1512FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury with broken skin", "PW1518FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Foot injury with broken skin", "PW1518FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Arm injury - skin not broken", "PW895FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Arm injury with broken skin", "PW902FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful arm", "PW1165FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen arm", "PW1165FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Cold arm", "PW1733FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Arm changing colour", "PW1733FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Change in behaviour", "PW1749FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bites and stings", "PW1575FemaleChild");
            categoryPage.VerifyPathwayNotInCategoryList("Bleeding from stoma","PT507FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blood in urine", "PW961FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breast problems", "PW1605FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breastfeeding problems", "PW1114FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breathing problems", "PW557FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Upper back pain", "PW557FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest and upper back pain", "PW557FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest pain", "PW557FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wheezing", "PW557FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Coughing up blood", "PW1652FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bringing up blood", "PW1652FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Burns from chemicals", "PW564FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sunburn", "PW987FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Burns from heat", "PW580FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Catheter problems", "PW1567FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest injury - skin not broken", "PW588FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury - skin not broken", "PW588FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest injury with broken skin", "PW596FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury with broken skin", "PW596FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Colds and flu", "PW1041FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Constipation", "PW1161FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Cough", "PW978FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Dental injury", "PW620FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toothache and other dental problems", "PW1611FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Diabetes - blood sugar problems", "PW1746FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea - no vomiting", "PW629FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea and vomiting", "PW1554FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Difficulty passing urine", "PW886FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Difficulty swallowing", "PW1496FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Dizziness", "PW637FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Vertigo", "PW637FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Ear discharge", "PW1702FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Earwax", "PW1702FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Earache", "PW656FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eye injury - no damage to surface of eye", "PW660FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eye injury with damage to surface of eye", "PW668FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eye problems", "PW1628FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eyelid problems", "PW1628FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the eye", "PW1098FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen face", "PW1548FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful face", "PW1548FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen neck", "PW1548FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful neck", "PW1548FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Falling", "PW700FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fainting", "PW700FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Passing out", "PW700FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fever", "PW709FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("High temperature", "PW709FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Finger injury - skin not broken", "PW1270FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury - skin not broken", "PW1270FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Finger injury with broken skin", "PW1264FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury with broken skin", "PW1264FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fingernail injury", "PW1570FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Pain in the side of the body", "PW717FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the ear", "PW1528FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the nose", "PW1529FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the bottom/back passage", "PW1531FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the vagina", "PW1532FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Genital injury - skin not broken", "PW1116FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Genital injury with broken skin", "PW1010FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen groin", "PW729FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful groin", "PW729FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hair loss", "PW1678FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hand injury - skin not broken", "PW1267FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury - skin not broken", "PW1267FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hand injury with broken skin", "PW1260FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury with broken skin", "PW1260FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Head injury - skin not broken", "PW684FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Face injury - skin not broken", "PW684FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Neck injury - skin not broken", "PW684FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Head injury with broken skin", "PW692FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Face injury with broken skin", "PW692FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Neck injury with broken skin", "PW692FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Headache", "PW753FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hearing problems", "PW1762FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blocked ear", "PW1762FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Heatstroke", "PW998FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Heat exhaustion", "PW998FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hiccups", "PW1775FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Leg injury - skin not broken", "PW1591FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Leg injury with broken skin", "PW1234FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Leg changing colour", "PW1734FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Cold leg", "PW1734FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Locked jaw", "PW1712FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Loss of bowel control", "PW1759FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bowel incontinence", "PW1759FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury - skin not broken", "PW1596FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury with broken skin", "PW798FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Lower back pain", "PW783FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful legs", "PW775FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen legs", "PW775FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Mental health problems", "PW1751FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Low mood", "PW1751FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Depression", "PW1751FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Anxiety", "PW1751FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Mouth ulcers", "PW1323FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blocked nose", "PW984FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed after an injury", "PW1713FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed", "PW819FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Numbness", "PW1683FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Tingling", "PW1683FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swallowed an object", "PW1034FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breathed in an object", "PW1034FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Other symptoms", "PW1348FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Palpitations", "PW1029FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Pounding heart", "PW1029FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fluttering heart", "PW1029FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Urinary problems", "PW645FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bleeding from the bottom/back passage", "PW846FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen bottom/back passage", "PW1091FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful bottom/back passage", "PW1091FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Itchy bottom/back passage", "PW1091FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Can't feel baby moving as much", "PW1763FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Grazes", "PW1590FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Minor wounds", "PW1590FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Scratches", "PW1590FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sexual concerns - female", "PW1699FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Period concerns", "PW1699FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Shoulder pain", "PW1141FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sinusitis", "PW1050FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Rashes, itching, spots, moles and other skin problems", "PW1772FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Glue on the skin", "PW1301FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something under the skin", "PW1259FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Splinter", "PW1259FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sleep problems", "PW1697FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sore throat", "PW854FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hoarse voice", "PW854FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Stoma problems", "PW1719FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Stroke symptoms", "PA171FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Tiredness", "PW1071FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fatigue", "PW1071FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toe injury - skin not broken", "PW1282FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toe injury with broken skin", "PW1526FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toenail injury", "PW1593FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toothache after an injury", "PW572FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blisters", "PW1625FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Trembling", "PW1764FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Shaking", "PW1764FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bleeding from the vagina", "PW911FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Discharge from the vagina", "PW916FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Itchy vagina", "PW1560FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sore vagina", "PW1560FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swelling in or around the vagina", "PW1103FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Vomiting and nausea - no diarrhoea", "PW937FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems", "PW1776FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - plaster casts", "PW1709FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - drainage tubes", "PW1709FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - metal attachments", "PW1709FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen finger", "PW1616FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful finger", "PW1616FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen hand", "PW1616FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful hand", "PW1616FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen wrist", "PW1616FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful wrist", "PW1616FemaleChild");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543FemaleChild");

            categoryPage.SelectCategory("bowel-and-urinary-problems");
            categoryPage.SelectCategory("bowel-and-urinary-problems-bowel-problems");
            categoryPage.SelectPathway("Something in the bottom/back passage");

            _driver.FindElement(By.XPath("//input[@value = 'PW1531FemaleChild']"));
        }

        [Test]
        //alias and id check for categories age 5 Male with mental health
        public void CategoriesPresentForFemaleAdult()
        {
            var categoryPage = TestScenerios.LaunchCategoryScenerio(_driver, TestScenerioGender.Female, 40);

            categoryPage.VerifyPathwayInCategoryList("Can't feel baby moving as much", "PW1763FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Abdomen injury - skin not broken", "PW500FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body - skin not broken", "PW500FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Abdomen injury with broken skin", "PW508FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body with broken skin", "PW508FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Abdominal pain", "PW516FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Stomach pain", "PW516FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Absent periods", "PW1676FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Missed periods", "PW1676FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Accidental poisoning", "PW881FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breathed in something poisonous", "PW881FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swallowed something poisonous", "PW881FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Drunk too much alcohol", "PW1551FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Alcohol intoxication", "PW1551FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury - skin not broken", "PW1512FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Foot injury - skin not broken", "PW1512FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury with broken skin", "PW1518FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Foot injury with broken skin", "PW1518FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Arm injury - skin not broken", "PW894FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Arm injury with broken skin", "PW902FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful arm", "PW1164FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen arm", "PW1164FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Cold arm", "PW1733FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Arm changing colour", "PW1733FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Change in behaviour", "PW1749FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bites and stings", "PW1575FemaleAdult");
            categoryPage.VerifyPathwayNotInCategoryList("Bleeding from stoma", "PT507FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blood in urine", "PW961FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breast problems", "PW1603FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breastfeeding problems", "PW1114FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breathing problems", "PW556FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Upper back pain", "PW556FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest and upper back pain", "PW556FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest pain", "PW556FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wheezing", "PW556FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Coughing up blood", "PW1651FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bringing up blood", "PW1651FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Burns from chemicals", "PW564FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sunburn", "PW987FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Burns from heat", "PW580FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Catheter problems", "PW1567FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest injury - skin not broken", "PW588FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury - skin not broken", "PW588FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest injury with broken skin", "PW596FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury with broken skin", "PW596FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Colds and flu", "PW1040FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Constipation", "PW1158FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Cough", "PW975FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Dental injury", "PW620FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toothache and other dental problems", "PW1610FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Diabetes - blood sugar problems", "PW1746FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea - no vomiting", "PW628FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea and vomiting", "PW1553FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Difficulty passing urine", "PW886FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Difficulty swallowing", "PW1496FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Dizziness", "PW636FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Vertigo", "PW636FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Ear discharge", "PW1702FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Earwax", "PW1702FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Earache", "PW655FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eye injury - no damage to surface of eye", "PW660FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eye injury with damage to surface of eye", "PW668FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eye problems", "PW1627FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eyelid problems", "PW1627FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the eye", "PW1098FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen face", "PW1544FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful face", "PW1544FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen neck", "PW1544FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful neck", "PW1544FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Falling", "PW700FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fainting", "PW700FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Passing out", "PW700FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fever", "PW708FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("High temperature", "PW708FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Finger injury - skin not broken", "PW1270FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury - skin not broken", "PW1270FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Finger injury with broken skin", "PW1264FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury with broken skin", "PW1264FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fingernail injury", "PW1570FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Pain in the side of the body", "PW716FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the ear", "PW1528FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the nose", "PW1529FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the bottom/back passage", "PW1531FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the vagina", "PW1532FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Genital injury - skin not broken", "PW1116FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Genital injury with broken skin", "PW1010FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen groin", "PW728FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful groin", "PW728FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hair loss", "PW1678FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hand injury - skin not broken", "PW1267FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury - skin not broken", "PW1267FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hand injury with broken skin", "PW1260FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury with broken skin", "PW1260FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Head injury - skin not broken", "PW684FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Face injury - skin not broken", "PW684FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Neck injury - skin not broken", "PW684FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Head injury with broken skin", "PW692FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Face injury with broken skin", "PW692FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Neck injury with broken skin", "PW692FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Headache", "PW752FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hearing problems", "PW1762FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blocked ear", "PW1762FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Heatstroke", "PW998FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Heat exhaustion", "PW998FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hiccups", "PW1775FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Leg injury - skin not broken", "PW1240FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Leg injury with broken skin", "PW1234FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Leg changing colour", "PW1734FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Cold leg", "PW1734FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Locked jaw", "PW1712FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Loss of bowel control", "PW1759FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bowel incontinence", "PW1759FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury - skin not broken", "PW790FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury with broken skin", "PW798FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Lower back pain", "PW782FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful legs", "PW774FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen legs", "PW774FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Mental health problems", "PW1751FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Low mood", "PW1751FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Depression", "PW1751FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Anxiety", "PW1751FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Mouth ulcers", "PW1323FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blocked nose", "PW981FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed after an injury", "PW1713FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed", "PW818FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Numbness", "PW1683FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Tingling", "PW1683FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swallowed an object", "PW1034FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breathed in an object", "PW1034FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Other symptoms", "PW1345FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Palpitations", "PW1028FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Pounding heart", "PW1028FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fluttering heart", "PW1028FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Urinary problems", "PW644FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bleeding from the bottom/back passage", "PW846FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen bottom/back passage", "PW1091FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful bottom/back passage", "PW1091FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Itchy bottom/back passage", "PW1091FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Grazes", "PW1590FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Minor wounds", "PW1590FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Scratches", "PW1590FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sexual concerns - female", "PW1684FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Period concerns", "PW1684FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Shoulder pain", "PW1140FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sinusitis", "PW1046FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Rashes, itching, spots, moles and other skin problems", "PW1771FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Glue on the skin", "PW1301FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something under the skin", "PW1259FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Splinter", "PW1259FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sleep problems", "PW1686FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sore throat", "PW854FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hoarse voice", "PW854FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Stoma problems", "PW1719FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Tiredness", "PW1070FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fatigue", "PW1070FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toe injury - skin not broken", "PW1282FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toe injury with broken skin", "PW1526FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toenail injury", "PW1593FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toothache after an injury", "PW572FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blisters", "PW1625FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Trembling", "PW1764FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Shaking", "PW1764FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bleeding from the vagina", "PW910FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Discharge from the vagina", "PW915FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Itchy vagina", "PW1559FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sore vagina", "PW1559FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swelling in or around the vagina", "PW1102FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Vomiting and nausea - no diarrhoea", "PW936FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems", "PW1776FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - plaster casts", "PW1709FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - drainage tubes", "PW1709FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - metal attachments", "PW1709FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen finger", "PW1614FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful finger", "PW1614FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen hand", "PW1614FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful hand", "PW1614FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen wrist", "PW1614FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful wrist", "PW1614FemaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543FemaleAdult");


            categoryPage.SelectCategory("bowel-and-urinary-problems");
            categoryPage.SelectCategory("bowel-and-urinary-problems-bowel-problems");
            categoryPage.SelectPathway("Something in the bottom/back passage");

            _driver.FindElement(By.XPath("//input[@value = 'PW1531FemaleAdult']"));
        }

        [Test]
        //alias and id check for categories age 5 Male with mental health
        public void CategoriespresentForMaleChild()
        {
            var categoryPage = TestScenerios.LaunchCategoryScenerio(_driver, TestScenerioGender.Male, 5);

            categoryPage.VerifyPathwayInCategoryList("Abdomen injury - skin not broken", "PW503MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body - skin not broken", "PW503MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Abdomen injury with broken skin", "PW511MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body with broken skin", "PW511MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Abdominal pain", "PW520MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Stomach pain", "PW520MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Accidental poisoning", "PW881MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breathed in something poisonous", "PW881MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swallowed something poisonous", "PW881MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Drunk too much alcohol", "PW1552MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Alcohol intoxication", "PW1552MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury - skin not broken", "PW1512MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Foot injury - skin not broken", "PW1512MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury with broken skin", "PW1518MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Foot injury with broken skin", "PW1518MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Arm injury - skin not broken", "PW895MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Arm injury with broken skin", "PW902MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful arm", "PW1167MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen arm", "PW1167MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Cold arm", "PW1733MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Arm changing colour", "PW1733MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Change in behaviour", "PW1749MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bites and stings", "PW1575MaleChild");
            categoryPage.VerifyPathwayNotInCategoryList("Bleeding from stoma", "PT507MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blood in urine", "PW962MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breast problems", "PW1606MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breathing problems", "PW560MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Upper back pain", "PW560MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest and upper back pain", "PW560MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest pain", "PW560MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wheezing", "PW560MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Coughing up blood", "PW1654MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bringing up blood", "PW1654MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Burns from chemicals", "PW564MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sunburn", "PW987MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Burns from heat", "PW580MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Catheter problems", "PW1567MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest injury - skin not broken", "PW588MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury - skin not broken", "PW588MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Chest injury with broken skin", "PW596MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury with broken skin", "PW596MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Colds and flu", "PW1043MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Constipation", "PW1162MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Cough", "PW979MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Dental injury", "PW620MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toothache and other dental problems", "PW1611MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Diabetes - blood sugar problems", "PW1746MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea - no vomiting", "PW632MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea and vomiting", "PW1556MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Difficulty passing urine", "PW886MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Difficulty swallowing", "PW1496MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Dizziness", "PW640MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Vertigo", "PW640MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Ear discharge", "PW1702MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Earwax", "PW1702MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Earache", "PW656MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eye injury - no damage to surface of eye", "PW660MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eye injury with damage to surface of eye", "PW668MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eye problems", "PW1630MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Eyelid problems", "PW1630MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the eye", "PW1098MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen face", "PW1549MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful face", "PW1549MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen neck", "PW1549MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful neck", "PW1549MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Falling", "PW700MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fainting", "PW700MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Passing out", "PW700MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fever", "PW712MaleChild");
            categoryPage.VerifyPathwayInCategoryList("High temperature", "PW712MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Finger injury - skin not broken", "PW1270MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury - skin not broken", "PW1270MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Finger injury with broken skin", "PW1264MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury with broken skin", "PW1264MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fingernail injury", "PW1570MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Pain in the side of the body", "PW720MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the ear", "PW1528MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the nose", "PW1529MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the penis", "PW1530MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something in the bottom/back passage", "PW1531MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Genital injury - skin not broken", "PW1118MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Genital injury with broken skin", "PW1012MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Genital problems", "PW1565MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen groin", "PW732MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful groin", "PW732MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hair loss", "PW1678MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hand injury - skin not broken", "PW1267MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury - skin not broken", "PW1267MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hand injury with broken skin", "PW1260MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury with broken skin", "PW1260MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Head injury - skin not broken", "PW684MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Face injury - skin not broken", "PW684MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Neck injury - skin not broken", "PW684MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Head injury with broken skin", "PW692MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Face injury with broken skin", "PW692MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Neck injury with broken skin", "PW692MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Headache", "PW756MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hearing problems", "PW1762MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blocked ear", "PW1762MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Heatstroke", "PW998MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Heat exhaustion", "PW998MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hiccups", "PW1775MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Leg injury - skin not broken", "PW1591MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Leg injury with broken skin", "PW1234MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Leg changing colour", "PW1734MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Cold leg", "PW1734MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Locked jaw", "PW1712MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Loss of bowel control", "PW1759MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bowel incontinence", "PW1759MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury - skin not broken", "PW1597MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury with broken skin", "PW801MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Lower back pain", "PW786MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful legs", "PW778MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen legs", "PW778MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Mental health problems", "PW1751MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Low mood", "PW1751MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Depression", "PW1751MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Anxiety", "PW1751MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Mouth ulcers", "PW1323MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blocked nose", "PW984MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed after an injury", "PW1713MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed", "PW819MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Numbness", "PW1683MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Tingling", "PW1683MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swallowed an object", "PW1034MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Breathed in an object", "PW1034MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Other symptoms", "PW1349MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Palpitations", "PW1031MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Pounding heart", "PW1031MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fluttering heart", "PW1031MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Urinary problems", "PW648MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Bleeding from the bottom/back passage", "PW846MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen bottom/back passage", "PW1091MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful bottom/back passage", "PW1091MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Itchy bottom/back passage", "PW1091MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Grazes", "PW1590MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Minor wounds", "PW1590MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Scratches", "PW1590MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sexual concerns - male", "PW1698MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Shoulder pain", "PW1143MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sinusitis", "PW1602MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Rashes, itching, spots, moles and other skin problems", "PW1772MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Glue on the skin", "PW1301MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Something under the skin", "PW1754MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Splinter", "PW1754MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sleep problems", "PW1697MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Sore throat", "PW854MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Hoarse voice", "PW854MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Stoma problems", "PW1719MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Stroke symptoms", "PA171MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Tiredness", "PW1073MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Fatigue", "PW1073MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toe injury - skin not broken", "PW1282MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toe injury with broken skin", "PW1526MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toenail injury", "PW1593MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Toothache after an injury", "PW572MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Blisters", "PW1625MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Trembling", "PW1764MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Shaking", "PW1764MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Vomiting and nausea - no diarrhoea", "PW940MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems", "PW1776MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - plaster casts", "PW1709MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - drainage tubes", "PW1709MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - metal attachments", "PW1709MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen finger", "PW1617MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful finger", "PW1617MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen hand", "PW1617MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful hand", "PW1617MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Swollen wrist", "PW1617MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Painful wrist", "PW1617MaleChild");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543MaleChild");

            categoryPage.SelectCategory("bowel-and-urinary-problems");
            categoryPage.SelectCategory("bowel-and-urinary-problems-bowel-problems");
            categoryPage.SelectPathway("Something in the bottom/back passage");

            _driver.FindElement(By.XPath("//input[@value = 'PW1531MaleChild']"));

        }

        [Test]
        //alias and id check for categories age 40 Male with mental health
        public void CategoriesPresentForMaleAdult()
        {
            var categoryPage = TestScenerios.LaunchCategoryScenerio(_driver, TestScenerioGender.Male, 40);

            categoryPage.VerifyPathwayInCategoryList("Abdomen injury - skin not broken", "PW503MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body - skin not broken", "PW503MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Abdomen injury with broken skin", "PW511MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Injury to the side of the body with broken skin", "PW511MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Abdominal pain", "PW519MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Stomach pain", "PW519MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Accidental poisoning", "PW881MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breathed in something poisonous", "PW881MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swallowed something poisonous", "PW881MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Drunk too much alcohol", "PW1551MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Alcohol intoxication", "PW1551MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury - skin not broken", "PW1512MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Foot injury - skin not broken", "PW1512MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Ankle injury with broken skin", "PW1518MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Foot injury with broken skin", "PW1518MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Arm injury - skin not broken", "PW897MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Arm injury with broken skin", "PW902MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful arm", "PW1166MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen arm", "PW1166MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Cold arm", "PW1733MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Arm changing colour", "PW1733MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Change in behaviour", "PW1749MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bites and stings", "PW1575MaleAdult");
            categoryPage.VerifyPathwayNotInCategoryList("Bleeding from stoma", "PT507MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blood in urine", "PW962MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breast problems", "PW1604MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breathing problems", "PW559MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Upper back pain", "PW559MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest and upper back pain", "PW559MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest pain", "PW559MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wheezing", "PW559MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Coughing up blood", "PW1653MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bringing up blood", "PW1653MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Burns from chemicals", "PW564MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sunburn", "PW987MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Burns from heat", "PW580MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Catheter problems", "PW1567MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest injury - skin not broken", "PW588MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury - skin not broken", "PW588MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Chest injury with broken skin", "PW596MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Upper back injury with broken skin", "PW596MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Colds and flu", "PW1042MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Constipation", "PW1159MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Cough", "PW976MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Dental injury", "PW620MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toothache and other dental problems", "PW1610MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Diabetes - blood sugar problems", "PW1746MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea - no vomiting", "PW631MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Diarrhoea and vomiting", "PW1555MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Difficulty passing urine", "PW886MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Difficulty swallowing", "PW1496MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Dizziness", "PW639MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Vertigo", "PW639MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Ear discharge", "PW1702MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Earwax", "PW1702MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Earache", "PW655MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eye injury - no damage to surface of eye", "PW660MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eye injury with damage to surface of eye", "PW668MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eye problems", "PW1629MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Eyelid problems", "PW1629MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the eye", "PW1098MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen face", "PW1545MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful face", "PW1545MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen neck", "PW1545MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful neck", "PW1545MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Falling", "PW700MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fainting", "PW700MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Passing out", "PW700MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fever", "PW711MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("High temperature", "PW711MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Finger injury - skin not broken", "PW1270MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury - skin not broken", "PW1270MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Finger injury with broken skin", "PW1264MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Thumb injury with broken skin", "PW1264MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fingernail injury", "PW1570MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Pain in the side of the body", "PW719MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the ear", "PW1528MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the nose", "PW1529MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the penis", "PW1530MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something in the bottom/back passage", "PW1531MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Genital injury - skin not broken", "PW1118MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Genital injury with broken skin", "PW1012MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Genital problems", "PW1564MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen groin", "PW731MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful groin", "PW731MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hair loss", "PW1678MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hand injury - skin not broken", "PW1267MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury - skin not broken", "PW1267MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hand injury with broken skin", "PW1260MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wrist injury with broken skin", "PW1260MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Head injury - skin not broken", "PW684MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Face injury - skin not broken", "PW684MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Neck injury - skin not broken", "PW684MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Head injury with broken skin", "PW692MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Face injury with broken skin", "PW692MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Neck injury with broken skin", "PW692MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Headache", "PW755MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hearing problems", "PW1762MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blocked ear", "PW1762MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Heatstroke", "PW998MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Heat exhaustion", "PW998MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hiccups", "PW1775MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Leg injury - skin not broken", "PW1241MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Leg injury with broken skin", "PW1234MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Leg changing colour", "PW1734MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Cold leg", "PW1734MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Locked jaw", "PW1712MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Loss of bowel control", "PW1759MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bowel incontinence", "PW1759MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury - skin not broken", "PW793MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Lower back injury with broken skin", "PW801MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Lower back pain", "PW785MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful legs", "PW777MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen legs", "PW777MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Mental health problems", "PW1751MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Low mood", "PW1751MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Depression", "PW1751MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Anxiety", "PW1751MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Mouth ulcers", "PW1323MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blocked nose", "PW981MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed after an injury", "PW1713MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Nosebleed", "PW818MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Numbness", "PW1683MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Tingling", "PW1683MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swallowed an object", "PW1034MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Breathed in an object", "PW1034MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Other symptoms", "PW1346MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Palpitations", "PW1030MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Pounding heart", "PW1030MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fluttering heart", "PW1030MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Urinary problems", "PW647MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Bleeding from the bottom/back passage", "PW846MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen bottom/back passage", "PW1091MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful bottom/back passage", "PW1091MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Itchy bottom/back passage", "PW1091MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Grazes", "PW1590MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Minor wounds", "PW1590MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Scratches", "PW1590MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sexual concerns - male", "PW1685MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Shoulder pain", "PW1140MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sinusitis", "PW1048MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Rashes, itching, spots, moles and other skin problems", "PW1771MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Glue on the skin", "PW1301MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Something under the skin", "PW1754MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Splinter", "PW1754MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sleep problems", "PW1686MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Sore throat", "PW854MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Hoarse voice", "PW854MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Stoma problems", "PW1719MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Tiredness", "PW1072MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Fatigue", "PW1072MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toe injury - skin not broken", "PW1282MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toe injury with broken skin", "PW1526MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toenail injury", "PW1593MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Toothache after an injury", "PW572MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Blisters", "PW1625MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Trembling", "PW1764MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Shaking", "PW1764MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Vomiting and nausea - no diarrhoea", "PW939MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems", "PW1776MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - plaster casts", "PW1709MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - drainage tubes", "PW1709MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Wound problems - metal attachments", "PW1709MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen finger", "PW1615MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful finger", "PW1615MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen hand", "PW1615MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful hand", "PW1615MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Swollen wrist", "PW1615MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Painful wrist", "PW1615MaleAdult");
            categoryPage.VerifyPathwayInCategoryList("Self-harm", "PW1543MaleAdult");

            categoryPage.SelectCategory("bowel-and-urinary-problems");
            categoryPage.SelectCategory("bowel-and-urinary-problems-bowel-problems");
            categoryPage.SelectPathway("Something in the bottom/back passage");

            //var wait = new WebDriverWait(_driver, new TimeSpan(0, 0, 5));
            //wait.Until(ExpectedConditions.ElementExists(By.XPath("//input[@value = 'PW1531MaleAdult']")));
            _driver.FindElement(By.XPath("//input[@value = 'PW1531MaleAdult']"));
        }
    }
}

