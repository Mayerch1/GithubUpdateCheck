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
    /// Specifies the algorithm for comparing the local and the remote version number
    /// </summary>
    public enum CompareType
    {
        /// <summary>
        /// Compares versions of the pattern 1.2.3.4 (e.g. 2.0.0 is higher than 1.9999.0)
        /// </summary>
        Incremental = 0,
        /// <summary>
        /// If the remote string is different, it will assume an update
        /// </summary>
        Boolean = 1
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
        private CompareType cmpType;

        /// <summary>
        /// Get the set compare type. (Getter only)
        /// </summary>
        public CompareType CompareType
        {
            get => cmpType;
        }



        /// <summary>
        /// Assumes version numbering with the pattern 1.2.3.4
        /// </summary>
        /// <param name="Username">Username of Repository owner</param>
        /// <param name="Repository">Name of Github Repository</param>
        public GithubUpdateCheck(string Username, string Repository)
        {
            this.Username = Username;
            this.Repository = Repository;
            cmpType = CompareType.Incremental;
        }


        /// <summary>
        /// Choose the compare system based on <see cref="CompareType"/>
        /// </summary>
        /// <param name="Username">Username of Repository owner</param>
        /// <param name="Repository">Name of Github Repository</param>
        /// <param name="compareType">Method to compare the local with the remote version</param>
        public GithubUpdateCheck(string Username, string Repository, CompareType compareType)
        {
            this.Username = Username;
            this.Repository = Repository;
            cmpType = compareType;
        }


        /// <summary>
        /// Compare two instances. If all (private) members are equal, the object is considered equal
        /// </summary>
        /// <param name="a">first instance</param>
        /// <param name="b">second instance</param>
        /// <returns>true if all members are equal</returns>
        public static bool operator ==(GithubUpdateCheck a, GithubUpdateCheck b)
        {
            // all members must be the same
            return (a.Username == b.Username) && (a.Repository == b.Repository) && (a.cmpType == b.cmpType);
        }

        /// <summary>
        /// Compare two instances. If at least one (private) members is different, the object is considered not-equal
        /// </summary>
        /// <param name="a">first instance</param>
        /// <param name="b">second instance</param>
        /// <returns>true if at least one member is different</returns>
        public static bool operator != (GithubUpdateCheck a, GithubUpdateCheck b)
        {
            return !(a == b);
        }


        /// <summary>
        /// Tests if inputted version is valid based on the allowed/specified patterns
        /// </summary>
        /// <param name="version">Current software version, is compared against the github version</param>
        /// <returns></returns>
        private bool isValidInput(string version)
        {
            switch (cmpType)
            {
                case CompareType.Incremental:
                    return isValidInputIncremental(version);

                // boolean is fallback, because it accepts the widest range of input
                case CompareType.Boolean:
                default:
                    return isValidInputBoolean(version);
            }
        }

        /// <summary>
        /// Tests if inputted version is valid based on the allowed/specified patterns
        /// </summary>
        /// <param name="version">Current software version, is compared against the github version</param>
        /// <returns></returns>
        private bool isValidInputIncremental(string version)
        {
            if (version != null)
            {
                // 1.0.0.0
                // 1.0.0
                // v.1.0.0
                // v1.0.0
                // and any combination of those
                string pattern = @"^([^0-9]\.{0,1}){0,1}\d+\.\d+\.\d+(\.\d+){0,1}$";
                Match match = Regex.Match(version, pattern);

                return match.Success;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Tests if inputted version is not null
        /// </summary>
        /// <param name="version">Current software version, is compared against the github version</param>
        /// <returns></returns>
        private bool isValidInputBoolean(string version)
        {
            return (version != null);
        }


        /// <summary>
        /// Removes leading v. and v from the string
        /// </summary>
        /// <param name="version">string must pass <see cref="isValidInput(string)"/></param>
        /// <returns>normalized string</returns>
        private string normalizeVersionString(string version)
        {
            switch (cmpType)
            {
                case CompareType.Incremental:
                    return normalizeVersionStringIncremental(version);

                // boolean is fallback, because it accepts the widest range of input
                case CompareType.Boolean:
                default:
                    return normalizeVersionStringBoolean(version);
            }
        }


        /// <summary>
        /// Removes leading v. and v from the string
        /// </summary>
        /// <param name="version">string which is compliant to the allowed pattern(s)</param>
        /// <returns>normalized string</returns>
        private string normalizeVersionStringIncremental(string version)
        {
            // leading (v. or v ) are allowed
            // therefore remove those from the version number
            string pattern = @"\d+\.\d+\.\d+(\.\d+){0,1}$";
            Match match = Regex.Match(version, pattern);

            return match.Value;
        }

        /// <summary>
        /// returns version, only to keep consistency
        /// </summary>
        /// <param name="version">string which is compliant to the allowed pattern(s)</param>
        /// <returns>normalized string</returns>
        private string normalizeVersionStringBoolean(string version)
        {
            // the boolean compare takes the version as is
            return version;
        }


        /// <summary>
        /// <para>Compares the current software version to the latest release on github. Asynchronous web request</para>
        /// If the webservice is not available this function will assume no updates available
        /// </summary>
        /// <param name="CurrentVersion">The version of the software wich is compared to the github version</param>
        /// <param name="VersionChange">The granularity of the comparison. Any version change smaller than this will be ignored. (e.g. Minor will check for a change in the first 2 digits groups). Does not apply to <see cref="CompareType.Boolean"/></param>
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
        /// <param name="VersionChange">The granularity of the comparison. Any version change smaller than this will be ignored. (e.g. Minor will check for a change in the first 2 digits groups). Does not apply to <see cref="CompareType.Boolean"/></param>
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
        /// Respects the selected compareType
        /// </summary>
        /// <param name="current">Local software version, must be checket for complinance with the allowed pattern(s)</param>
        /// <param name="github">Url of the latest github release</param>
        /// <param name="changeLevel">The level for comparison. Does not apply to Boolean compare</param>
        /// /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns>true if a newer version is available</returns>
        private bool compareVersions(string current, string github, VersionChange changeLevel)
        {
            // extract the tag from the url and validate/normalize input
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



            // choose CompareType specific compare method
            switch (cmpType)
            {
                case CompareType.Incremental:
                    return compareVersionsIncremental(current, github, changeLevel);

                // boolean is fallback, because it accepts the widest range of input
                case CompareType.Boolean:
                default:
                    return compareVersionsBoolean(current, github);
            }

        }


        /// <summary>
        /// Compares two version numbers. Extract version number of github release url
        /// Assumes update is available, if remote version is different to "current"
        /// </summary>
        /// <param name="current">Local software version</param>
        /// <param name="github">extracted, validated and normalized version string of the remote repo</param>        
        /// /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns></returns>
        private bool compareVersionsBoolean(string current, string github)
        {
            // if they are different, an update is available
            return current != github;
        }

        /// <summary>
        /// Compares two version numbers. Extract version number of github release url
        /// "Current" must comply with pattern, aswell as github version
        /// </summary>
        /// <param name="current">Local software version, must be checket for complinance with the allowed pattern(s)</param>
        /// <param name="github">extracted, validated and normalized version string of the remote repo</param>
        /// <param name="changeLevel">The level for comparison</param>
        /// /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns></returns>
        private bool compareVersionsIncremental(string current, string github, VersionChange changeLevel)
        {            
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
                [smaller]      [equals]       [greater]
                 true            |               false
                                 V
                       localMinor <> gitMinor
                [smaller]      [equals]       [greater]
                 true            |               false
                                 V
                       localBuild <> gitBuild
                [smaller]      [equals]       [greater]
                 true            |               false
                                 V
                        localRev. <> gitRev.
                [smaller]      [equals]       [greater]
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