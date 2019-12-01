using System;
using System.Net;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace Mayerch1.GithubUpdateCheck
{
    /// <summary>
    /// Enumeration of version steps (Major, Minor,...)
    /// </summary>
    public enum VersionChange
    {
        /// <summary>
        /// Major software update (X.0.0.0)
        /// </summary>
        Major = 1,

        /// <summary>
        /// Minor software update (0.X.0.0)
        /// </summary>
        Minor = 2,

        /// <summary>
        /// Build change (0.0.X.0)
        /// </summary>
        Build = 3,

        /// <summary>
        /// Revision changed (0.0.0.X)
        /// </summary>
        Revision = 4
    }

    /// <summary>
    /// The exception that is thrown if the version argument does not match the required pattern
    /// </summary>
    public class InvalidVersionException : ArgumentException {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVersionException"/> class with a specified error message
        /// </summary>        
        public InvalidVersionException(String message): base(message)
        {
        }
    };
    

    /// <summary>
    /// Checks if your github repo is on a newer version or not
    /// </summary>
    public class GithubUpdateCheck
    {
        private const string githubUrl = "https://github.com/";
        private const string latestVersionString = "/releases/latest";

        private string Username;
        private string Repository;

        /// <summary>
        /// Assumes version numbering with the pattern 1.2.3.4
        /// </summary>
        /// <param name="Username">Username of Repository owner</param>
        /// <param name="Repository">Name of Github Repository</param>
        public GithubUpdateCheck(string Username, string Repository)
        {
            this.Username = Username;
            this.Repository = Repository;
        }


        /// <summary>
        /// Tests if inputted version is valid based on the allowed/specified patterns
        /// </summary>
        /// <param name="version">Current software version, is compared against the github version</param>
        /// <returns></returns>
        private bool isValidInput(string version)
        {
            // currently accepeted pattern:
            // 1.0.0.0
            // 1.0.0
            // v.1.0.0
            // v1.0.0
            // and any combination of those
            string pattern = @"^([^0-9]\.{0,1}){0,1}\d+\.\d+\.\d+(\.\d+){0,1}$";
            Match match = Regex.Match(version, pattern);

            return match.Success;
        }


        /// <summary>
        /// Removes leading v. and v from the string
        /// </summary>
        /// <param name="version">string which is compliant to the allowed pattern(s)</param>
        /// <returns>normalized string</returns>
        private string normalizeVersionString(string version)
        {
            // leading (v. or v ) are allowed
            // therefore remove those from the version number
            string pattern = @"\d+\.\d+\.\d+(\.\d+){0,1}$";
            Match match = Regex.Match(version, pattern);

            return match.Value;
        }



        /// <summary>
        /// <para>Compares the current software version to the latest release on github. Asynchronous web request</para>
        /// If the webservice is not available this function will assume no updates available
        /// </summary>
        /// <param name="CurrentVersion">The version of the software wich is compared to the github version</param>
        /// <param name="VersionChange">The granularity of the comparison. Any version change smaller than this will be ignored. (e.g. Minor will check for a change in the first 2 digits groups)</param>
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns>bool - true if a newer version is available, false - if no newer version is available or if no connection to github is available</returns>
        public async Task<bool> IsUpdateAvailableAsync(string CurrentVersion, VersionChange VersionChange = VersionChange.Minor)
        {
            if (!isValidInput(CurrentVersion))
            {
                throw new InvalidVersionException(CurrentVersion + " does not follow the specified version pattern [CurrentVersion]");                
            }
            string resolved = await getResponseUrlAsync(githubUrl + Username + "/" + Repository + latestVersionString);

            if (resolved != null)
                return compareVersions(normalizeVersionString(CurrentVersion), resolved, VersionChange);
            else
                return false;
           

        }

        /// <summary>
        /// <para>Compares the current software version to the latest release on github. Synchronous (blocking) web request</para>
        /// If the webservice is not available this function will assume no updates available
        /// </summary>
        /// <param name="CurrentVersion">The version of the software wich is compared to the github version</param>
        /// <param name="VersionChange">The granularity of the comparison. Any version change smaller than this will be ignored. (e.g. Minor will check for a change in the first 2 digits groups)</param>
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns>bool - true if a newer version is available, false - if no newer version is available or if no connection to github is available</returns>
        public bool IsUpdateAvailable(string CurrentVersion, VersionChange VersionChange = VersionChange.Minor)
        {
            if (!isValidInput(CurrentVersion))
            {
                throw new InvalidVersionException(CurrentVersion + " does not follow the specified version pattern [CurrentVersion]");
            }


            string resolved = getResponseUrl(githubUrl + Username + "/" + Repository + latestVersionString);

            if (resolved != null)
                return compareVersions(normalizeVersionString(CurrentVersion), resolved, VersionChange);
            else
                return false;
        }



        /// <summary>
        /// Compares two version numbers. Extract version number of github release url
        /// "Current" must comply with pattern, aswell as github version
        /// </summary>
        /// <param name="current">Local software version, must be checket for complinance with the allowed pattern(s)</param>
        /// <param name="github">Url of the latest github release</param>
        /// <param name="changeLevel">The level for comparison</param>
        /// /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns></returns>
        private bool compareVersions(string current, string github, VersionChange changeLevel)
        {
            //no releases yet
            if (!github.Contains("/tag/"))
                return false;

            //get everything after last /tag/
            github = github.Substring(github.LastIndexOf("/tag/") + "/tag/".Length);


            if (!isValidInput(github))
            {
                throw new InvalidVersionException(github + " the github version number does not follow the specified version pattern [Remote error]");
            }

            github = normalizeVersionString(github);

            // separate version into VersionChange
            // input is tested for numbers only between the seperators ('.')
            Int64[] currentArr = Array.ConvertAll(current.Split('.'), s => Int64.Parse(s));
            Int64[] gitArr = Array.ConvertAll(github.Split('.'), s => Int64.Parse(s));


            // set the comparison depth
            // take the minimum depth, in case one specified number is smaller than another one
            int cmpDepth = (int)changeLevel;
            cmpDepth = Math.Min(currentArr.Length, cmpDepth);
            cmpDepth = Math.Min(gitArr.Length, cmpDepth);


            /* comparison of version numbers as follows

                       localMajor <> gitMajor
                [smalle]      [equals]       [greater]
                 true            |               false
                                 V
                       localMinor <> gitMinor
                [smalle]      [equals]       [greater]
                 true            |               false
                                 V
                       localBuild <> gitBuild
                [smalle]      [equals]       [greater]
                 true            |               false
                                 V
                        localRev. <> gitRev.
                [smalle]      [equals]       [greater]
                 true          false            false

                // if cmpDepth is reached at any time
                // the return is false aswell


            */

            // implicitie cmpDepth == 1
            // 1. major is checked on every version
            // 2. input is guaranteed to have cmpDepth >= 1

            //===============MAJOR==================
            if (currentArr[0] < gitArr[0])
            {
                return true;
            }
            else if (currentArr[0] > gitArr[0])
            {
                return false;
            }
            //===============MAJOR==================
            else
            {
                // first check if Depth was reached
                if(cmpDepth <= 1)
                {
                    return false;
                }
                //===============MINOR==================
                // repeat same procedure for minor
                if (currentArr[1] < gitArr[1])
                {
                    return true;
                }
                else if (currentArr[1] > gitArr[1])
                {
                    return false;
                }
                //===============MINOR==================
                else
                {
                    // check if Depth was reached
                    if (cmpDepth <= 2)
                    {
                        return false;
                    }
                    //===============BUILD==================
                    // repeat same procedure for build
                    if (currentArr[2] < gitArr[2])
                    {
                        return true;
                    }
                    else if (currentArr[2] > gitArr[2])
                    {
                        return false;
                    }
                    //===============BUILD==================
                    else
                    {
                        // check if Depth was reached
                        if (cmpDepth <= 3)
                        {
                            return false;
                        }

                        //===============REVSIION==================
                        // repeat same procedure for Revision
                        if (currentArr[3] < gitArr[3])
                        {
                            return true;
                        }
                        // no smaller compare possible
                        else
                        {
                            return false;
                        }
                        //===============REVISION==================
                    }
                }


            }
        }

        private string getResponseUrl(string request)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(request);
            WebResponse wResp;
            try
            {
                wResp = req.GetResponse();
            }
            catch
            {
                //if not available
                return null;
            }

            return wResp.ResponseUri.ToString();
        }

        private async Task<string> getResponseUrlAsync(string request)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(request);
            WebResponse wResp;
            try
            {
                wResp = await req.GetResponseAsync();
            }
            catch
            {
                return null;
                //if not available
            }

            return wResp.ResponseUri.ToString();
        }
    }
}