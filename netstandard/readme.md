# .Net Standard build

The `project.json` file in this folder contains all the necessary directives for compiling SerialPortStream for
.Net Standard 1.5, for its usage in Dot Net Core applications as well as .Net Framework 4.6.x

Some few operations are not supported for either Linux or Windows, determined by the definition constant `NETSTANDARD15`

## How to build

All the required dependencies are declared in `project.json` and will be automatically downloaded with the command: `dotnet restore`. 
This command shall be run in the same folder where the project file is located.

After that, the library can be build with the command: `dotnet build`

For generating a release version, use: `dotnet build --configuration Release`
