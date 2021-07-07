While the Mobile.BuildTools does not add any external dependencies to your projects, we could not do what we do without being able to build on the hard work from other projects. Though the Mobile.BuildTools is licensed under the MIT License, some of our dependencies use alternate Open Source licenses.

| Project | License | GitHub |
|:-------:|:-------:|:------:|
| MSBuild | MIT | [microsoft/msbuild](https://github.com/microsoft/msbuild) |
| Newtonsoft.Json | MIT | [JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) |
| Microsoft.Web.Xdt* | Apache | [aspnet/xdt](https://github.com/aspnet/xdt) |
| Svg.Skia | MIT | [wieslawsoltes/Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia)
| SkiaSharp | MIT | [mono/SkiaSharp](https://github.com/mono/SkiaSharp) |

!!! note Note
    Microsoft.Web.Xdt is the only reference added to projects, if using the Mobile.BuildTools.Configuration package. There is never any bloat added by the core Mobile.BuildTools package to your applications. Despite being from the aspnet team this package does not bring in any additional references.
