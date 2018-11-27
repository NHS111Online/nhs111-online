﻿namespace NHS111.DOS.Functional.Tests {
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    public class BaseTests {
        public IWebDriver Driver;

        public BaseTests() {
            Driver = new ChromeDriver();
        }

        ~BaseTests() {
            try {
                Driver.Quit();
                Driver.Dispose();
            }
            catch (Exception) {
                // Ignore errors if unable to close the browser
            }
        }

        public void SaveScreenAsPNG(string filename) {
            var screenshotDriver = Driver as ITakesScreenshot;
            var screenshot = screenshotDriver.GetScreenshot();
            screenshot.SaveAsFile(Path.Combine("C:\\", filename + ".png"));
        }
    }
}