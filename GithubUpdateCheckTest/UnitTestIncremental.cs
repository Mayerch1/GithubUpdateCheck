using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mayerch1.GithubUpdateCheck;
using System.Threading.Tasks;

namespace GithubUpdateCheckTest
{
    /// <summary>
    /// Unit test of the GithubUpdateCheck. Tests CompareType.Inrcemental specific functionality
    /// As the update checker needs a connection to github for almost all funcionality, this is closer to an integration test than a unit test
    /// This unit test can take more than 20s runtime
    /// </summary>
    [TestClass]
    public class UnitTestIncremental
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
        public void TestNullPattern()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("", "");

            Assert.ThrowsException<Mayerch1.GithubUpdateCheck.InvalidVersionException>(() => obj.IsUpdateAvailable(null));
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
            

            // invalid prefix
            TestInvalidVersionPattern_Wrapper(obj, "vv.1.0.0.0.0");
            TestInvalidVersionPattern_Wrapper(obj, "vv1.0.0.0.0");

            // not enough groups
            TestInvalidVersionPattern_Wrapper(obj, "");
            TestInvalidVersionPattern_Wrapper(obj, " ");
            TestInvalidVersionPattern_Wrapper(obj, null);

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


        [TestMethod]
        public void TestExceedEnumDepth()
        {
            // current release is 2.4.1.5
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");

            // version must be same as obj (otherwise compare will stop before limit is reached) 
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.5.99.4.6", (VersionChange)7));
        }

        [TestMethod]
        public void TestExceedLocalDepth()
        {
            // current release is 2.4.1.5
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");

            // passed version must be same as obj's version (otherwise compare will stop before the limit is reached)
            Assert.IsFalse(obj.IsUpdateAvailable("2.4", VersionChange.Minor));
        }

        [TestMethod]
        public void TestExceedRemoteDepth()
        {
            // current release is 2.4.1.5
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest");

            // version must be same as obj (otherwise compare will stop before limit is reached)
            // this tests TesExceedEnumDepth at the same time (creating repo only for this test is too much overhead)
            Assert.IsFalse(obj.IsUpdateAvailable("2.4.1.5.10", (VersionChange)5));
        }


    }
}
