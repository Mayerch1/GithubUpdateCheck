# GithubUpdateCheck

![GitHub](https://img.shields.io/github/license/mayerch1/GithubUpdateCheck)
&nbsp;
[![Nuget](https://img.shields.io/nuget/v/Mayerch1.GithubUpdateCheck)](https://www.nuget.org/packages/Mayerch1.GithubUpdateCheck/)
&nbsp;
[![GitHub release](https://img.shields.io/github/release/mayerch1/GithubUpdateCheck.svg)](https://github.com/mayerch1/GithubUpdateCheck/releases/latest)

NuGet package to check for available updates. Always compares to the latest github release.

***NOTE:*** from version 1.2.0 this project is distributed under MIT. If you wish to continue using GPL, you need to use older versions up to 1.1.0. 

(Distribution under more restrictive licenses may be possible on request, but I do not see any reason for anyone to do so).

---

## Usage
Create an instance of the `GithubUpdateCheck` and specify your Github username and Github repository, as seen in the url.


```cs
using Mayerch1.GithubUpdateCheck;

GithubUpdateCheck update = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck");
bool isUpdate = update.IsUpdateAvailable("1.0.0", VersionChange.Minor);
bool isAsyncUpdate = await update.IsUpdateAvailable("1.0.0.5", VersionChange.Revision);
```

---

### Incremental Compare
The default constructor is implicite setting the compare type to `CompareType.Incremental`.
Alternatively call
```cs
GithubUpdateCheck update = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Incremental);
```

The version will be compared to the latest release on Github (e.g. https://github.com/Mayerch1/GithubUpdateCheck/releases/latest). 
Allowed are the following patterns
```
1.0.0
1.0.0.0

v.1.0.0
v1.0.0

1.0.0.[...].5 (any amount of '.' seperators)
```
`v` can be any non-digit and the version (local and repository) must comply with those patterns, otherwise a `InvalidVersionException` will be thrown 

The second argument `VersionChange` specifies the level of comparison.

</br>For `Major`, only the first number will be compared (x.0.0.0)
</br>For `Minor`, the first and second number will be compared (x.y.0.0)
</br>For `Build`, the numbers 1-3 will be compared (x.y.z.0)
</br>For `Revision`, all numbers will be compared (x.y.z.a)

If the version number if exceeding this enum, simply pass an integer `...IsUpdateAvailable("1.0.0.0.9", (VersionChange)5);`. The index is 1-offset from the Major version and will be corrected if it is too big for the present number.

All numbers below the specified level will be ignored.

The prefix `v.` can be present on none, one or both sides.

---

### Boolean Compare
If the used version system does not match the "Incremental" Compare, you can use the boolean compare.
```cs
GithubUpdateCheck update = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck", CompareType.Boolean);
```
The class will then compare both version strings (local and remote) and will assume an available update as 
soon as they are not equal.
