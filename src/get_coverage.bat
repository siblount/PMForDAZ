dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"**/*.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html