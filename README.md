# GithubUpdateCheck
NuGet package to check if an update is available. Always compares to the latest github release

## Usage
Create an instance of the `GithubUpdateCheck` and specify your Github username and Github repository, as seen in the url.
```cs
GithubUpdateCheck update = new GithubUpdateCheck("Mayerch1", "GithubUpdateCheck");
bool isUpdate = update.IsUpdateAvailable("1.0.0", VersionChange.Minor);
bool isAsyncUpdate = await update.IsUpdateAvailable("1.0.0.5", VersionChange.Revision);
```

The version will be compared to the latest release on Github (e.g. https://github.com/Mayerch1/GithubUpdateCheck/releases/latest). 
Allowed are the following patterns
```
1.0.0
1.0.0.0

v.1.0.0.0
v.1.0.0

v1.0.0.0
v1.0.0
```
(`v` can be any non-digit)
The version string and the tag of the github release must comply with those patterns, otherwise a `InvalidVersionException` will be thrown.

The second argument `VersionChange changeLevel` specifies the level of comparison.


</br>For `Major`, only the first number will be compared (x.0.0.0)
</br>For `Minor`, the first and second number will be compared (x.y.0.0)
</br>For `Build`, the numbers 1-3 will be compared (x.y.z.0)
</br>For `Revision`, all numbers will be compared (x.y.z.a)



All numbers below the specified level (all 0 is this example) will be ignored.
If the remote or local version has a lower level than the specified compare Method (e.g. `1.2.0` at `Revision`), only 
the present version levels will be compared.

The prefix `v.` can be present on none, one or both sides.
