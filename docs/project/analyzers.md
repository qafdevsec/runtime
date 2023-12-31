## Adding new analyzers to the build

This repo relies on [.NET Compiler Platform analyzers](https://learn.microsoft.com/visualstudio/code-quality/roslyn-analyzers-overview) to help validate the correctness, performance, and maintainability of the code.  Several existing analyzer packages are wired into the build, but it is easy to augment the utilized packages in order to experiment with additional analyzers.

To add an analyzer package to the build:
1. Select a package you want to employ, for example https://www.nuget.org/packages/SonarAnalyzer.CSharp/.  This analyzer package's name is `SonarAnalyzer.CSharp` and the latest version as of this edit is `8.50.0.58025`.
2. Add a `PackageReference` entry to <https://github.com/dotnet/runtime/blob/main/eng/Analyzers.targets>, e.g.
    ```XML
    <PackageReference Include="SonarAnalyzer.CSharp" Version="8.50.0.58025" PrivateAssets="all" />
    ```
3. After that point, all builds will employ all rules in that analyzer package that are enabled by default.  You can change the severity for rules by adding entries to the globalconfig files [CodeAnalysis.src.globalconfig](https://github.com/dotnet/runtime/blob/main/eng/CodeAnalysis.src.globalconfig) or [CodeAnalysis.test.globalconfig](https://github.com/dotnet/runtime/blob/main/eng/CodeAnalysis.test.globalconfig).

The build system in this repo defaults to treating all warnings as errors. It can be helpful when enabling a new rule to temporarily allow warnings to be warnings rather than errors, while you proceed to fix all of them across the repo. Instead of building from the root of the repo with:
```
build.cmd
```
(or `./build.sh` on Unix), warnings-as-errors can be disabled with:
```
build.cmd -warnAsError 0
```
