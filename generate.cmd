@echo off
dotnet build src\CodeGenerator\CodeGenerator.csproj
dotnet build src\Aardvark.Base\Aardvark.Base.csproj
pushd bin\Debug\net472\
CodeGenerator.exe