using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("MapIdeaHub.BirSign.NetFrameworkExtension")]
[assembly: AssemblyDescription(".Net Framework Extension for BirSign")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("MapIdeaHub")]
[assembly: AssemblyProduct("MapIdeaHub.BirSign.NetFrameworkExtension")]
[assembly: AssemblyCopyright("Copyright © MapIdeaHub 2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9b25a29c-937d-41c0-ab44-a90d4793af04")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// Deliberately fixed. Raising AssemblyVersion would force every consuming app to add a
// binding redirect, so it only moves on a breaking change -- not on a release.
// AssemblyFileVersion and AssemblyInformationalVersion are generated from $(Version) in
// Directory.Build.props by the GenerateVersionInfo target in the .csproj, so they always
// match the published package and cannot drift.
[assembly: AssemblyVersion("2.4.0.0")]
