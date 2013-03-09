// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

/////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////
//                                                                                         //
// DO NOT EDIT GlobalAssemblyInfo.cs, it is recreated using AssemblyInfo.template whenever //
// Globefish.Studio.Core is compiled.                                                           //
//                                                                                         //
/////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using Play.Studio.Core;

[assembly: ComVisible(false)]
[assembly: AssemblyCompany("mge")]
[assembly: AssemblyProduct("Play.Studio Studio")]
[assembly: AssemblyCopyright("2010-2012 Alpha for the Play.Studio Team")]
[assembly: AssemblyVersion(RevisionClass.Major + "." + RevisionClass.Minor + "." + RevisionClass.Build + "." + RevisionClass.Revision)]
[assembly: AssemblyInformationalVersion(RevisionClass.FullVersion + "-736b5b66")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly",
	Justification = "AssemblyInformationalVersion does not need to be a parsable version")]

internal static class RevisionClass
{
    public const string Major = "0";
    public const string Minor = "1";
    public const string Build = "0";
    public const string Revision = "1000";
    public const string VersionName = Major + "." + Minor;

    public const string FullVersion = Major + "." + Minor + "." + Build + "." + Revision;
}

