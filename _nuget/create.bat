nuget pack JobMan.nuspec -OutputDirectory .\_output\ 
nuget pack JobMan.UI.nuspec -OutputDirectory .\_output\ 
nuget pack JobMan.Storage.PostgreSql.nuspec -OutputDirectory .\_output\ 
nuget pack JobMan.Storage.SqlServer.nuspec -OutputDirectory .\_output\ 


rem -Properties Configuration=Release