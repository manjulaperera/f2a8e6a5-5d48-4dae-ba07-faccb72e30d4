# SonarQube

The SonarQube steps are in [`.github/workflows/ci.yml`](../.github/workflows/ci.yml). The scanner is SonarScanner for .NET 12.0.0, installed from `dotnet-tools.json` during the workflow. It is not a reference of the class library. Analysis wraps the same restore, build, and tests. It runs only when the `SONAR_ENABLED` variable is `true`. Any other value, including an empty one, skips the scanner and still builds and tests. With analysis enabled, the workflow also requires:

- Secret `SONAR_TOKEN`, a SonarQube analysis token.
- Variable `SONAR_HOST_URL`, the server URL. Use `https://sonarcloud.io` for SonarQube Cloud.

The build fails if analysis is enabled and either of those is missing.

Optional variables:

- `SONAR_PROJECT_KEY`, default `DepthCharts`. Create the project in SonarQube with this key before the first analysis. The badges at the top of the README use the configured SonarCloud project.
- `SONAR_ORGANIZATION`, required for SonarQube Cloud and omitted for SonarQube Server.

When analysis is enabled, the workflow starts the scanner, builds `DepthCharts.slnx`, runs both test projects with Coverlet OpenCover reports, and waits for the quality gate. Test projects are marked `SonarQubeTestProject`. Coverage excludes `src/DepthCharts.Console`. That project is a host for trying the library, and the coverage gate measures the library. Coverlet excludes the `DepthCharts.Console` assembly in `coverage.runsettings`. SonarQube excludes `src/DepthCharts.Console/**/*`.

A local run uses the same order. Replace the token and host before running it:

```bash
dotnet tool restore
dotnet tool run dotnet-sonarscanner -- begin /k:"DepthCharts" /d:sonar.token="<token>" /d:sonar.host.url="<host>" /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" /d:sonar.coverage.exclusions="src/DepthCharts.Console/**/*"
dotnet build DepthCharts.slnx --configuration Release --no-incremental --disable-build-servers
dotnet test DepthCharts.slnx --configuration Release --no-build --settings coverage.runsettings --collect:"XPlat Code Coverage"
dotnet tool run dotnet-sonarscanner -- end /d:sonar.token="<token>"
```
