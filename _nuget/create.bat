nuget pack JobMan.nuspec -OutputDirectory .\_output\ 
dotnet pack ..\JobMan.UI.AspNetCore\Jobman.UI.AspNetCore.ForNuget.csproj -c Release -o .\_output\
nuget pack JobMan.Storage.PostgreSql.nuspec -OutputDirectory .\_output\ 
nuget pack JobMan.Storage.SqlServer.nuspec -OutputDirectory .\_output\ 

