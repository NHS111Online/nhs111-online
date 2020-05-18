﻿using NHS111.Features;
using NHS111.Web.Functional.Utils.ScreenShot;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using System;
using System.Configuration;

namespace NHS111.Web.Functional.Utils
{
    public class LayoutPage : IScreenShotPage
    {
        public static string _baseUrl = ConfigurationManager.AppSettings["TestWebsiteUrl"];
        public readonly IWebDriver Driver;
        private readonly IVisualRegressionTestingFeature _visualRegressionTestingFeature;
        private readonly EmergencyAlertFeature _emergencyAlertFeature;
        internal const string _headerLogoTitle = "Go to the NHS 111 homepage";
        internal const string _headerText = "1 1 1 online";

        [FindsBy(How = How.CssSelector, Using = ".global-header")]
        internal IWebElement Header { get; set; }

        [FindsBy(How = How.CssSelector, Using = ".global-header__logo")]
        internal IWebElement HeaderLogo { get; set; }

        [FindsBy(How = How.CssSelector, Using = ".global-footer")]
        internal IWebElement Footer { get; set; }


        private bool _screenShotsEqual = false;

        public LayoutPage(IWebDriver driver) : this(driver, new VisualRegressionTestingFeature()) { }

        public LayoutPage(IWebDriver driver, IVisualRegressionTestingFeature visualRegressionTestingFeature)
        {
            Driver = driver;
            PageFactory.InitElements(Driver, this);
            _visualRegressionTestingFeature = visualRegressionTestingFeature;
            _emergencyAlertFeature = new EmergencyAlertFeature();
        }

        public void VerifyLayoutPagePresent()
        {
            Assert.IsTrue(Header.Displayed);
            Assert.IsTrue(HeaderLogo.Displayed);
            Assert.AreEqual(_headerLogoTitle, HeaderLogo.GetAttribute("title"));
            Assert.IsTrue(Footer.Displayed);
        }

        public void VerifyHeaderBannerDisplayed()
        {
            Assert.IsTrue(Header.Displayed);
        }

        public IWebElement TabToFirstPageBodyElement()
        {
            var header = HeaderLogo.Tab(Driver);
            if (IsElementPresent(By.CssSelector(".nhsuk-global-alert a"))) return header.Tab(Driver);

            return header;
        }

        public void VerifyHeaderBannerHidden()
        {
            Assert.IsFalse(Header.Displayed);
        }

        public void VerifyHiddenField(string id, string value)
        {
            var field = Driver.FindElement(By.Id(id));
            Assert.AreEqual(value, field.GetAttribute("value"));
        }

        public void VerifyHasButton(string buttonName, string buttonValue)
        {
            var field = Driver.FindElement(By.Name(buttonName));
            Assert.AreEqual(buttonValue, field.GetAttribute("value"));
        }

        public string GetUrlWithoutCredentials()
        {
            if (UrlContainsCredentials())
            {
                return Driver.Url.Remove(_baseUrl.IndexOf("://") + 3,
                    _baseUrl.LastIndexOf("@") - (_baseUrl.IndexOf("://") + 3));
            }

            return _baseUrl;
        }

        public static bool UrlContainsCredentials()
        {
            return _baseUrl.Contains("@");
        }

        public void WaitForElement(IWebElement element, int timeoutInSeconds)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutInSeconds));
            wait.Until(drv => element.Displayed);
        }
        public void WaitForElement(IWebElement element)
        {
            WaitForElement(element, 5);
        }

        public IScreenShotMaker ScreenShotMaker
        {
            get { return new ScreenShotMaker(Driver); }
        }

        public bool GetScreenShotsEqual()
        {
            return _screenShotsEqual;
        }

        public void CompareScreenShot(string uniqueId)
        {
            _screenShotsEqual = ScreenShotComparer.Compare(ScreenShotMaker.GetScreenShotFilename(uniqueId), ScreenShotMaker.BaselineScreenShotDir, ScreenShotMaker.ScreenShotDir);
        }

        public T CompareScreenShot<T>(T page, string uniqueId) where T : IScreenShotPage
        {
            CompareScreenShot(uniqueId);
            return page;
        }

        public bool IsElementPresent(By by)
        {
            return Driver.FindElements(by).Count > 0;
        }

        public T CompareAndVerify<T>(T page, string uniqueId) where T : IScreenShotPage
        {
            if (!_visualRegressionTestingFeature.IsEnabled) return page;

            ScreenShotMaker.MakeScreenShot(uniqueId);
            if (_visualRegressionTestingFeature.MakeBaselineScreenShotsOnly) return page;

            if (!ScreenShotMaker.CheckBaselineExists(uniqueId))
                Assert.Fail("Screenshot comparison baseline missing at step " + ScreenShotMaker.GetScreenShotFilename(uniqueId));

            page = CompareScreenShot(page, uniqueId);
            if (!page.GetScreenShotsEqual())
            {
                ScreenShotMaker.CopyBaseline(ScreenShotMaker.GetScreenShotFilename(uniqueId));
                Console.WriteLine("##teamcity[testMetadata testName='{0}' name='Differences'  type='image' value='{1}']", TestContext.CurrentContext.Test.FullName, "diff/" + ScreenShotMaker.GetScreenShotFilename(uniqueId));
                Console.WriteLine("##teamcity[testMetadata testName='{0}' name='Test Baseline'  type='image' value='{1}']", TestContext.CurrentContext.Test.FullName, "baselines/" + ScreenShotMaker.GetScreenShotFilename(uniqueId));
                Assert.Fail("Screenshot comparison shows not equal to baseline at step " + ScreenShotMaker.GetScreenShotFilename(uniqueId));
            }
            return page;
        }

        public bool IsDisplayed(IWebElement element)
        {
            try
            {
                return element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
}
