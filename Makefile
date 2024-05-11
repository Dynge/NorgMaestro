.PHONY: test clean package all

package:
	dotnet build

run:
	dotnet run

package-win:
	dotnet build -r win-x64 ./NorgMaestro.Server/NorgMaestro.csproj
package-mac:
	dotnet build -r osx-x64 ./NorgMaestro.Server/NorgMaestro.csproj
package-linux:
	dotnet build -r linux-x64 ./NorgMaestro.Server/NorgMaestro.csproj
