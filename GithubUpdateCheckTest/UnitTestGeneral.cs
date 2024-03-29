﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mayerch1.GithubUpdateCheck;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace GithubUpdateCheckTest
{
    /// <summary>
    /// Unit test of the GithubUpdateCheck
    /// As the update checker needs a connection to github for almost all funcionality, this is closer to an integration test than a unit test
    /// This unit test can take more than 20s runtime
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UnitTestGeneral
    {

        [TestMethod]
        public void TestDefaultConstructorIsIncremental()
        {
            // compare type is private, therefore use compare
            GithubUpdateCheck defaultCon = new GithubUpdateCheck("", "");

            Assert.AreEqual(defaultCon.CompareType, CompareType.Incremental);
        }


        [TestMethod]
        public void TestAreEqualFalse()
        {
            // change one member at a time
            bool isEqual;

            GithubUpdateCheck aCmpType = new GithubUpdateCheck("Mayerch1", "DIFFERENT", CompareType.Incremental);
            GithubUpdateCheck bCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Incremental);

            isEqual = (aCmpType == bCmpType);
            Assert.IsFalse(isEqual);

            GithubUpdateCheck aAuthor = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);
            GithubUpdateCheck bAuthor = new GithubUpdateCheck("DIFFERENT", "GithubUpdateCheck", CompareType.Boolean);

            isEqual = (aAuthor == bAuthor);
            Assert.IsFalse(isEqual);

            GithubUpdateCheck aRepo = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);
            GithubUpdateCheck bRepo = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Incremental);

            isEqual = (aRepo == bRepo);
            Assert.IsFalse(isEqual);
        }


        [TestMethod]
        public void TestAreEqualTrue()
        {
            // only if all are the same, it should return true
            GithubUpdateCheck aCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);
            GithubUpdateCheck bCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);

            bool isEqual = (aCmpType == bCmpType);
            Assert.IsTrue(isEqual);
        }

        [TestMethod]
        public void TestAreNotEqualTrue()
        {
            // assuming == is working, it is enough to test a single !=, as it returns !(==)

            GithubUpdateCheck aCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);
            GithubUpdateCheck bCmpType = new GithubUpdateCheck("DIFFERENT", "GithubUpdateCheck", CompareType.Boolean);

            bool isDifferent = (aCmpType != bCmpType);

            Assert.IsTrue(isDifferent);
        }


        [TestMethod]
        public void TestNullInvertTrue()
        {
            GithubUpdateCheck obj = null;
            Assert.IsTrue(null == obj);
        }

        [TestMethod]
        public void TestNullInvertFalse()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);

            Assert.IsFalse(null == obj);
        }

        [TestMethod]
        public void TestEqualNullTrue()
        {
            GithubUpdateCheck obj = null;

            Assert.IsTrue(obj == null);
        }

        [TestMethod]
        public void TestEqualNullFalse()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);

            Assert.IsFalse(obj == null);
        }


        [TestMethod]
        public void TestNotEqualNullTrue()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);

            Assert.IsTrue(obj != null);
        }

        [TestMethod]
        public void TestNotEqualNullFalse()
        {
            GithubUpdateCheck obj = null;

            Assert.IsFalse(obj != null);
        }

        [TestMethod]
        public void TestEqualsMethodFalse()
        {
            GithubUpdateCheck aCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);
            GithubUpdateCheck bCmpType = new GithubUpdateCheck("DIFFERENT", "GithubUpdateCheck", CompareType.Boolean);

            Assert.IsFalse(aCmpType.Equals(bCmpType));
        }


        [TestMethod]
        public void TestEqualsMethodTrue()
        {
            GithubUpdateCheck aCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Incremental);
            GithubUpdateCheck bCmpType = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Incremental);
                        
            Assert.IsTrue(aCmpType.Equals(bCmpType));
        }

        [TestMethod]
        public void TestNullConstructor()
        {
            // if the repo is invalid, 
            GithubUpdateCheck obj = new GithubUpdateCheck(null, null);
            Assert.IsFalse(obj.IsUpdateAvailable("1.0.0"));

        }

        [TestMethod]
        public void TestNoRelease()
        {
            // this repo doesn't have releases
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "BunnyVisual");
            Assert.IsFalse(obj.IsUpdateAvailable("1.0.0.0"));
        }

        [TestMethod]
        public void TestInvalidRepository()
        {
            // random uuid, high change for non-existent repo
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "cb91fb82-9621-479a-9c5a-23e565e6390d");
            obj.IsUpdateAvailable("1.0.0.0");
        }

        [TestMethod]
        public void TestInvalidUser()
        {
            // random uuid, high change for non-existent repo
            GithubUpdateCheck obj = new GithubUpdateCheck("5fe54fd3-2fd8-48ff-8f63-1b8575348b5f", "GithubUpdateCheck");
            obj.IsUpdateAvailable("1.0.0.0");
        }


        [TestMethod]
        public async Task TestAsyncRequest()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "TheDiscordSoundboard");

            // it cannot be guaranteed, that the major version of TDS will stay at 2
            // more specific compares cannot be made
            Assert.IsTrue(await obj.IsUpdateAvailableAsync("1.0.0", VersionChange.Major));
        }

        [TestMethod]
        public async Task TestInvalidRepositoryAsync()
        {
            // random uuid, high change for non-existent repo
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "cb91fb82-9621-479a-9c5a-23e565e6390d");
            Assert.IsFalse(await obj.IsUpdateAvailableAsync("1.0.0.0"));
        }

        [TestMethod]
        public async Task TestInvalidUserAsync()
        {
            // random uuid, high change for non-existent repo
            GithubUpdateCheck obj = new GithubUpdateCheck("5fe54fd3-2fd8-48ff-8f63-1b8575348b5f", "GithubUpdateCheck");
            Assert.IsFalse(await obj.IsUpdateAvailableAsync("1.0.0.0"));
        }

        [TestMethod]
        public void TestGetVersionIncremental()
        {
            GithubUpdateCheck getV = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest", CompareType.Incremental);
            var version = getV.Version();

            Assert.IsTrue(version.Equals("2.4.1.5"));
        }

        [TestMethod]
        public void TestGetVersionBoolean()
        {
            GithubUpdateCheck getV = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest", CompareType.Boolean);
            var version = getV.Version();

            Assert.IsTrue(version.Equals("v.2.4.1.5"));
        }

        [TestMethod]
        public void TestGetVersionNoVersion()
        {
            // repo doesn't have releases
            GithubUpdateCheck getV = new GithubUpdateCheck("Mayerch1", "BunnyVisual", CompareType.Incremental);
            Assert.ThrowsException<Mayerch1.GithubUpdateCheck.NoVersionException>(() => getV.Version());
        }

    }
}
