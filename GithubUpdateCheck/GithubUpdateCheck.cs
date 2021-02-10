using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Mayerch1.GithubUpdateCheck
{
    /// <summary>
    /// Enumeration of version steps (Major, Minor,...)
    /// Value equals to 1-based index of the version.
    /// May be replaced by int, casted to <see cref="VersionChange"/>
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
        /// Initializes a new instance of the <see cref="InvalidVersionException"/> class
        /// </summary>
        [ExcludeFromCodeCoverage]
        public InvalidVersionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVersionException"/> class with a specified error message
        /// </summary>        
        public InvalidVersionException(String message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidVersionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        [ExcludeFromCodeCoverage]
        public InvalidVersionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    };


    /// <summary>
    /// Checks if your github repo is on a newer version or not
    /// </summary>
#pragma warning disable CA1724 // Do not warn about namespace/classname conflict
    public class GithubUpdateCheck : IEquatable<GithubUpdateCheck>
    {

        private const string githubUrl = "https://github.com/";
        private const string latestVersionString = "/releases/latest";

        private readonly string Username;
        private readonly string Repository;
        private readonly CompareType cmpType;

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
            // check both sides for null
            if ((b is null) || (a is null))
            {
                // if both are null, return true
                if ((a is null) && (b is null))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // all members must be the same
            return (a.Username == b.Username) && (a.Repository == b.Repository) && (a.cmpType == b.cmpType);
        }

        /// <summary>
        /// Compare two instances. If at least one (private) members is different, the object is considered not-equal
        /// </summary>
        /// <param name="a">first instance</param>
        /// <param name="b">second instance</param>
        /// <returns>true if at least one member is different</returns>
        public static bool operator !=(GithubUpdateCheck a, GithubUpdateCheck b)
        {

            return !(a == b);
        }


        /// <summary>
        /// Compare two instances. If all (private) members are equal, the object is considered equal
        /// </summary>
        /// <param name="obj">other object</param>
        /// <returns>False if objects are not the same type. Otherwise behaves like ==</returns>
        [ExcludeFromCodeCoverage]
        public override bool Equals(object obj)
        {
            return Equals(obj as GithubUpdateCheck);
        }

        /// <summary>
        /// Compare two instances. If all (private) members are equal, the object is considered equal
        /// </summary>
        /// <param name="other">the other object</param>
        /// <returns>true - if all members are equal</returns>
        public bool Equals(GithubUpdateCheck other)
        {
            return other != null &&
                   Username == other.Username &&
                   Repository == other.Repository &&
                   cmpType == other.cmpType;
        }

        /// <summary>
        /// Get the hash code of the object (generated by VS refactoring)
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        public override int GetHashCode()
        {
            var hashCode = -1134592763;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Username);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Repository);
            hashCode = hashCode * -1521134295 + cmpType.GetHashCode();
            return hashCode;
        }



        /// <summary>
        /// Tests if inputted version is valid based on the allowed/specified patterns
        /// </summary>
        /// <param name="version">Current software version, is compared against the github version</param>
        /// <returns></returns>
        private bool IsValidInput(string version)
        {
            switch (cmpType)
            {
                case CompareType.Incremental:
                    return IsValidInputIncremental(version);

                // boolean is fallback, because it accepts the widest range of input
                case CompareType.Boolean:
                default:
                    return IsValidInputBoolean(version);
            }
        }

        /// <summary>
        /// Tests if inputted version is valid based on the allowed/specified patterns
        /// </summary>
        /// <param name="version">Current software version, is compared against the github version</param>
        /// <returns></returns>
        private static bool IsValidInputIncremental(string version)
        {
            if (version != null)
            {
                // 1.0.0.0
                // 1.0.0
                // v.1.0.0
                // v1.0.0
                // and any combination of those
                string pattern = @"^([^0-9]\.?)?(\d+\.)*\d+$";
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
        private static bool IsValidInputBoolean(string version)
        {
            return (version != null);
        }


        /// <summary>
        /// Removes leading v. and v from the string
        /// </summary>
        /// <param name="version">string must pass <see cref="IsValidInput(string)"/></param>
        /// <returns>normalized string</returns>
        private string NormalizeVersionString(string version)
        {
            switch (cmpType)
            {
                case CompareType.Incremental:
                    return NormalizeVersionStringIncremental(version);

                // boolean is fallback, because it accepts the widest range of input
                case CompareType.Boolean:
                default:
                    return NormalizeVersionStringBoolean(version);
            }
        }


        /// <summary>
        /// Removes leading v. and v from the string
        /// </summary>
        /// <param name="version">string which is compliant to the allowed pattern(s)</param>
        /// <returns>normalized string</returns>
        private static string NormalizeVersionStringIncremental(string version)
        {
            // leading (v. or v ) are allowed
            // therefore remove those from the version number
            string pattern = @"(\d+\.)*\d+$";
            Match match = Regex.Match(version, pattern);

            return match.Value;
        }

        /// <summary>
        /// returns version, only to keep consistency
        /// </summary>
        /// <param name="version">string which is compliant to the allowed pattern(s)</param>
        /// <returns>normalized string</returns>
        private static string NormalizeVersionStringBoolean(string version)
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
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version or the remote version does not match the allowed version pattern</exception>
        /// <returns>bool - true if a newer version is available, false - if no newer version is available or if no connection to github is available</returns>
        public async Task<bool> IsUpdateAvailableAsync(string CurrentVersion, VersionChange VersionChange = VersionChange.Minor)
        {
            if (!IsValidInput(CurrentVersion))
            {
                throw new InvalidVersionException(CurrentVersion + " does not follow the specified version pattern [CurrentVersion]");                
            }
            string resolved = await GetResponseUrlAsync(githubUrl + Username + "/" + Repository + latestVersionString).ConfigureAwait(false);

            if (resolved != null)
                return CompareVersions(NormalizeVersionString(CurrentVersion), resolved, VersionChange);
            else
                return false;
           

        }

        /// <summary>
        /// <para>Compares the current software version to the latest release on github. Asynchronous web request</para>
        /// If the webservice is not available this function will assume no updates available
        /// </summary>
        /// <param name="CurrentVersion">The version of the software which is compared to the github version</param>
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version or the remote version does not match the allowed version pattern</exception>
        /// <returns>bool - true if a newer version is available, false - if no newer version is available or if no connection to github is available</returns>
        public async Task<bool> IsUpdateAvailableAsync(string CurrentVersion)
        {
            if (!IsValidInput(CurrentVersion))
            {
                throw new InvalidVersionException(CurrentVersion + " does not follow the specified version pattern [CurrentVersion]");
            }
            string resolved = await GetResponseUrlAsync(githubUrl + Username + "/" + Repository + latestVersionString).ConfigureAwait(false);

            if (resolved != null)
                return CompareVersions(NormalizeVersionString(CurrentVersion), resolved);
            else
                return false;


        }

        /// <summary>
        /// <para>Compares the current software version to the latest release on github. Synchronous (blocking) web request</para>
        /// If the webservice is not available this function will assume no updates available
        /// </summary>
        /// <param name="CurrentVersion">The version of the software wich is compared to the github version</param>
        /// <param name="VersionChange">The granularity of the comparison. Any version change smaller than this will be ignored. (e.g. Minor will check for a change in the first 2 digits groups). Does not apply to <see cref="CompareType.Boolean"/></param>
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version or the remote version does not match the allowed version pattern</exception>
        /// <returns>bool - true if a newer version is available, false - if no newer version is available or if no connection to github is available</returns>
        public bool IsUpdateAvailable(string CurrentVersion, VersionChange VersionChange = VersionChange.Minor)
        {
            if (!IsValidInput(CurrentVersion))
            {
                throw new InvalidVersionException(CurrentVersion + " does not follow the specified version pattern [CurrentVersion]");
            }


            string resolved = GetResponseUrl(githubUrl + Username + "/" + Repository + latestVersionString);

            if (resolved != null)
                return CompareVersions(NormalizeVersionString(CurrentVersion), resolved, VersionChange);
            else
                return false;
        }

        /// <summary>
        /// <para>Compares the current software version to the latest release on github. Synchronous (blocking) web request</para>
        /// If the webservice is not available this function will assume no updates available
        /// </summary>
        /// <param name="CurrentVersion">The version of the software which is compared to the github version</param>
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version or the remote version does not match the allowed version pattern</exception>
        /// <returns>bool - true if a newer version is available, false - if no newer version is available or if no connection to github is available</returns>
        public bool IsUpdateAvailable(string CurrentVersion)
        {
            if (!IsValidInput(CurrentVersion))
            {
                throw new InvalidVersionException(CurrentVersion + " does not follow the specified version pattern [CurrentVersion]");
            }


            string resolved = GetResponseUrl(githubUrl + Username + "/" + Repository + latestVersionString);

            if (resolved != null)
                return CompareVersions(NormalizeVersionString(CurrentVersion), resolved);
            else
                return false;
        }

        /// <summary>
        /// Compares two version numbers. Extract version number of github release url
        /// "Current" must comply with pattern, as well as github version
        /// </summary>
        /// <param name="current">Local software version, must be checked for compliance with the allowed pattern(s)</param>
        /// <param name="github">Url of the latest github release</param>
        /// <exception cref="InvalidVersionException">Is thrown if the supplied version does not match the allowed version pattern</exception>
        /// <returns>true if a newer version is available</returns>
        private bool CompareVersions(string current, string github)
        {
            // extract the tag from the url and validate/normalize input
            //no releases yet
            if (!github.Contains("/tag/"))
                return false;


            //get everything after last /tag/
            int indexOfTag = CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(github, "/tag/");
            github = github.Substring(indexOfTag + "/tag/".Length);

            if (!IsValidInput(github))
            {
                throw new InvalidVersionException(github + " the github version number does not follow the specified version pattern [Remote error]");
            }

            github = NormalizeVersionString(github);


            var currentValid = Version.TryParse(current, out var currentVersion);
            var githubValid = Version.TryParse(github, out var githubVersion);

            if (currentValid && githubValid)
            {
                return currentVersion < githubVersion;
            }
            else
            {
                return false;
            }
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
        private bool CompareVersions(string current, string github, VersionChange changeLevel)
        {
            // extract the tag from the url and validate/normalize input
            //no releases yet
            if (!github.Contains("/tag/"))
                return false;


            //get everything after last /tag/
            int indexOfTag = CultureInfo.InvariantCulture.CompareInfo.LastIndexOf(github, "/tag/");            
            github = github.Substring(indexOfTag + "/tag/".Length);

            if (!IsValidInput(github))
            {
                throw new InvalidVersionException(github + " the github version number does not follow the specified version pattern [Remote error]");
            }

            github = NormalizeVersionString(github);



            // choose CompareType specific compare method
            switch (cmpType)
            {
                case CompareType.Incremental:
                    return CompareVersionsIncremental(current, github, changeLevel);

                // boolean is fallback, because it accepts the widest range of input
                case CompareType.Boolean:
                default:
                    return CompareVersionsBoolean(current, github);
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
        private static bool CompareVersionsBoolean(string current, string github)
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
        private bool CompareVersionsIncremental(string current, string github, VersionChange changeLevel)
        {            
            // input is tested for numbers only between the seperators ('.')
            Int64[] currentArr = Array.ConvertAll(current.Split('.'), s => Int64.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture));
            Int64[] gitArr = Array.ConvertAll(github.Split('.'), s => Int64.Parse(s, NumberStyles.Integer, CultureInfo.InvariantCulture));


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
                                 .
                                 .
                                 .
                // if cmpDepth is reached, the return is false


            */
            // implicitie cmpDepth == 1
            // 1. major is checked on every version
            // 2. input is guaranteed to have cmpDepth >= 1

            return CompareVersionIncrementalRecursive(cmpDepth, currentArr, gitArr);

        }

        /// <summary>
        /// Compare one element of the version array with the same element of the remoteVersion. Increments index on every call.
        /// Aborts if maxDepth is reached. 
        /// maxDepth must not exceed the length of localVersion nor the remoteVersion
        /// </summary>
        /// <param name="maxDepth">Maximum compare depth, must be greater 0. Must not exceed localVersion.Length nor remoteVersion.Length, (x.Length is the last allowed value and will stop the recursion)</param>
        /// <param name="currentDepth">the current index of the comparison, is incremented in every call. Recursion starts with 0</param>
        /// <param name="localVersion">int array of the local version</param>
        /// <param name="remoteVersion">int array of the remote array</param>
        /// <returns></returns>
        private bool CompareVersionIncrementalRecursive(int maxDepth, Int64[] localVersion, Int64[] remoteVersion, int currentDepth=0)
        {
            if (currentDepth == maxDepth)
            {
                return false;
            }
            else if(localVersion[currentDepth] > remoteVersion[currentDepth]){
                return false;
            }
            else if (localVersion[currentDepth] < remoteVersion[currentDepth]){
                return true;
            }
            else
            {
                return CompareVersionIncrementalRecursive(maxDepth, localVersion, remoteVersion, currentDepth + 1);
            }
        }

        private static string GetResponseUrl(string request)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(request);
            WebResponse wResp;
            try
            {
                wResp = req.GetResponse();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //if not available
                return null;
            }

            return wResp.ResponseUri.ToString();
        }

        private static async Task<string> GetResponseUrlAsync(string request)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(request);
            WebResponse wResp;
            try
            {
                // this is not for ui and does not necessarily need to return to the main thread
                wResp = await req.GetResponseAsync().ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return null;
                //if not available
            }

            return wResp.ResponseUri.ToString();
        }

       
    }
#pragma warning restore CA1724 // Do not warn about namespace/classname conflict
}