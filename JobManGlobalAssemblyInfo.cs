#pragma warning disable CS0436

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyCompanyAttribute("JobMan")]
[assembly: AssemblyProductAttribute("JobMan Library")]
[assembly: AssemblyVersion(JobManAssembly.VERSION)]
//#pragma warning disable CS0436 // Type conflicts with imported type
[assembly: AssemblyFileVersion(JobManAssembly.VERSION)]
[assembly: AssemblyInformationalVersion(JobManAssembly.VERSION)]
//#pragma warning restore CS0436 // Type conflicts with imported type

[assembly: InternalsVisibleTo("JobMan.Tests")]
[assembly: InternalsVisibleTo("Jobman.UI.AspNetCore")]

#pragma warning disable CS8600
public static class JobManAssembly
{
    //TODO: TypeHelper
    public const string VERSION = "0.4.1.00015";
}