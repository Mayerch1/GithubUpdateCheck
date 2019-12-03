using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mayerch1.GithubUpdateCheck;
using System.Threading.Tasks;

namespace GithubUpdateCheckTest
{
    /// <summary>
    /// Unit test of the GithubUpdateCheck. Tests CompareType.Boolean specific functionality
    /// As the update checker needs a connection to github for almost all funcionality, this is closer to an integration test than a unit test
    /// This unit test can take more than 20s runtime
    /// </summary>
    [TestClass]
    public class UnitTestBoolean
    {
        [TestMethod]
        public void TestInvalidPattern()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest", CompareType.Boolean);

            // this is the only invalid input
            Assert.ThrowsException<InvalidVersionException>(() => obj.IsUpdateAvailable(null));
        }


        [TestMethod]
        public void TestUpdateAvailable()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest", CompareType.Boolean);

            // the repo is guaranteed to have this version as latest patch ("v.2.4.1.5")
            // the slightest difference shall assume an update, the strings are not normalized
            Assert.IsTrue(obj.IsUpdateAvailable("2.4.1.5"));
        }


        [TestMethod]
        public void TestNoUpdateAvailable()
        {
            GithubUpdateCheck obj = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheckUnitTest", CompareType.Boolean);

            // the repo is guaranteed to have this version as latest patch ("v.2.4.1.5")
            Assert.IsFalse(obj.IsUpdateAvailable("v.2.4.1.5"));
        }

    }
}
