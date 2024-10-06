.PHONY: test clean package all

test:
	dotnet test

package:
	dotnet build

run:
	dotnet run
