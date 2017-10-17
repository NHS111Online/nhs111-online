﻿using NUnit.Framework;
using NHS111.SmokeTest.Utils;

namespace NHS111.SmokeTests
{
    [TestFixture]
    public class HomePageTests : BaseTests
    {
        [Test]
        public void HomePage_Displays()
        {
            var homePage = new HomePage(Driver);
            homePage.Load();
            homePage.Verify();
        }
    }
}
