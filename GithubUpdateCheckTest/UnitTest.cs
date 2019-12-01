using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mayerch1.GithubUpdateCheck;
using System.Threading.Tasks;

namespace GithubUpdateCheckTest
{
    /// <summary>
    /// Unit test of the GithubUpdateCheck
    /// As the update checker needs a connection to github for almost all funcionality, this is closer to an integration test than a unit test
    /// This unit test can take more than 20s runtime
    /// </summary>
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestValidVersionPattern()
        {
            // invalid repo will return false
            // this makes web request faster, as the request returns null without downloading resources
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "d5a4426e-8e8e-421e-b8d0-129e826153e3");

            obj.IsUpdateAvailable("1.0.0.0");
            obj.IsUpdateAvailable("1.0.0");

            obj.IsUpdateAvailable("v.1.0.0.0");
            obj.IsUpdateAvailable("v.1.0.0");

            obj.IsUpdateAvailable("v1.0.0.0");
            obj.IsUpdateAvailable("v1.0.0");

            obj.IsUpdateAvailable("158.080.098900.55400");
            obj.IsUpdateAvailable("v.0.054.4480.9499849840");

            obj.IsUpdateAvailable("x1.0.0.0");
            obj.IsUpdateAvailable("x.1.0.0");


        }

        [TestMethod]
        // Assert.Throws is not available for async methods
        [ExpectedException(typeof(Mayerch1.GithubUpdateCheck.InvalidVersionException))]
        public async Task TestInvalidAsyncPattern()
        {
            // it is enough to test if the fail-path is working in generall
            // detailed testing is done for the Sync. method, as both call the same function
            GithubUpdateCheck obj = new GithubUpdateCheck("", "");
            await obj.IsUpdateAvailableAsync("invalid1..0.0.00..");
        }

        [TestMethod]
        public void TestInvalidVersionPattern()
        {
            // should throw before web request is made
            GithubUpdateCheck obj = new GithubUpdateCheck("", "");

            // too many groups            
            TestInvalidVersionPattern_Wrapper(obj, "1.0.0.0.0");

            // invalid prefix
            TestInvalidVersionPattern_Wrapper(obj, "vv.1.0.0.0.0");
            TestInvalidVersionPattern_Wrapper(obj, "vv1.0.0.0.0");

            // not enough groups
            TestInvalidVersionPattern_Wrapper(obj, "1.0");
            TestInvalidVersionPattern_Wrapper(obj, "1");
            TestInvalidVersionPattern_Wrapper(obj, "");
            TestInvalidVersionPattern_Wrapper(obj, " ");

            // invalid character
            TestInvalidVersionPattern_Wrapper(obj, "1.a0.0.0.0");
            TestInvalidVersionPattern_Wrapper(obj, "1.0.0a.0.0");
            TestInvalidVersionPattern_Wrapper(obj, "1.a0.0.0.0a");

            // invalid ending
            TestInvalidVersionPattern_Wrapper(obj, "1.0.0.0.0.");
            TestInvalidVersionPattern_Wrapper(obj, "1.0.0.0.");
            TestInvalidVersionPattern_Wrapper(obj, "1.0.0.");

            // multiple/invalid seperatiors
            TestInvalidVersionPattern_Wrapper(obj, "v..1.0.0.");
            TestInvalidVersionPattern_Wrapper(obj, "1..0.0.");
            TestInvalidVersionPattern_Wrapper(obj, "1.0....0");
            TestInvalidVersionPattern_Wrapper(obj, "1.0,0.0");
            TestInvalidVersionPattern_Wrapper(obj, "1.0.0.0;");
        }

        private void TestInvalidVersionPattern_Wrapper(GithubUpdateCheck obj, string version)
        {
            Assert.ThrowsException<Mayerch1.GithubUpdateCheck.InvalidVersionException>(() => obj.IsUpdateAvailable(version));            
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
            await obj.IsUpdateAvailableAsync("1.0.0.0");
        }

        [TestMethod]
        public async Task TestInvalidUserAsync()
        {
            // random uuid, high change for non-existent repo
            GithubUpdateCheck obj = new GithubUpdateCheck("5fe54fd3-2fd8-48ff-8f63-1b8575348b5f", "GithubUpdateCheck");
            await obj.IsUpdateAvailableAsync("1.0.0.0");
        }


        [TestMethod]
        public void TestUpdateMajorAvailable()
        {
            // current release is 2.4.1.5
            // it is guarantees that those releases are not deleted
            // => every releaes can be used to check for updates
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");


            // it is guaranteed that this version will not change (2.4.1.5
            Assert.IsTrue(obj.IsUpdateAvailable("1.0.0", VersionChange.Major));
            // higher minor,... versions
            Assert.IsTrue(obj.IsUpdateAvailable("1.9.9", VersionChange.Major));


            Assert.IsFalse(obj.IsUpdateAvailable("2.0.0", VersionChange.Major));
            // equal version
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.5", VersionChange.Major));
            // newer local version
            Assert.IsFalse(obj.IsUpdateAvailable("3.0.0.0", VersionChange.Major));
        }

        [TestMethod]
        public void TestUpdateMinorAvailable()
        {
            // current release is 2.4.1.5
            // it is guarantees that those releases are not deleted
            // => every releaes can be used to check for updates
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");


            // it is guaranteed that this version will not change (2.4.1.5
            Assert.IsTrue(obj.IsUpdateAvailable("2.3.0", VersionChange.Minor));
            // smaller minor and higher build version
            Assert.IsTrue(obj.IsUpdateAvailable("2.3.9", VersionChange.Minor));


            // euqal minor but smaller build
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.0", VersionChange.Minor));
            // equal version
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.5", VersionChange.Minor));
            // newer local version
            Assert.IsFalse(obj.IsUpdateAvailable("2.9.1.5", VersionChange.Minor));
        }

        [TestMethod]
        public void TestUpdateBuildAvailable()
        {
            // current release is 2.4.1.5
            // it is guarantees that those releases are not deleted
            // => every releaes can be used to check for updates
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");


            // it is guaranteed that this version will not change (2.4.1.5
            Assert.IsTrue(obj.IsUpdateAvailable("2.4.0", VersionChange.Build));
            // smaller minor and higher build version
            Assert.IsTrue(obj.IsUpdateAvailable("2.4.0.6", VersionChange.Build));


            // euqal minor but smaller build
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.0", VersionChange.Build));
            // equal version
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.5", VersionChange.Build));
            // newer local version
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.90.0", VersionChange.Build));
        }

        [TestMethod]
        public void TestUpdateRevisionAvailable()
        {
            // current release is 2.4.1.5
            // it is guarantees that those releases are not deleted
            // => every releaes can be used to check for updates
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");


            // it is guaranteed that this version will not change (2.4.1.5
            Assert.IsTrue(obj.IsUpdateAvailable("2.4.1.1", VersionChange.Revision));
            
            // equal version
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.5", VersionChange.Revision));
            // newer local version
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.99", VersionChange.Revision));
        }

        [TestMethod]
        public void TestInvalidRemoteVersionPattern()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest2");
            Assert.ThrowsException<Mayerch1.GithubUpdateCheck.InvalidVersionException>(() => obj.IsUpdateAvailable("1.0.0"));
        }

    }
}
