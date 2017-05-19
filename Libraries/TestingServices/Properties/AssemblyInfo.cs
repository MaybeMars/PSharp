﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Microsoft.PSharp.TestingServices")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyProduct("Microsoft.PSharp.TestingServices")]
[assembly: AssemblyCopyright("Copyright © 2017 Microsoft Corporation")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("d88418ab-c8b8-4fb2-9fba-f0e994e42f37")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.2.0.0")]
[assembly: AssemblyFileVersion("1.2.0.0")]

// Libraries
[assembly: InternalsVisibleTo("Microsoft.PSharp.LanguageServices,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]

// Tools
[assembly: InternalsVisibleTo("PSharpTester,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]
[assembly: InternalsVisibleTo("PSharpReplayer,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]
[assembly: InternalsVisibleTo("PSharpCoverageReportMerger,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]
[assembly: InternalsVisibleTo("PSharpTraceViewer,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]
[assembly: InternalsVisibleTo("Microsoft.PSharp.SharedObjects,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]

// Unit tests
[assembly: InternalsVisibleTo("Microsoft.PSharp.TestingServices.Tests.Unit,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]

// Integration tests
[assembly: InternalsVisibleTo("Microsoft.PSharp.TestingServices.Tests.Integration,PublicKey=" +
    "0024000004800000940000000602000000240000525341310004000001000100d7971281941569" +
    "53fd8af100ac5ecaf1d96fab578562b91133663d6ccbf0b313d037a830a20d7af1ce02a6641d71" +
    "d7bc9fd67a08d3fa122120a469158da22a652af4508571ac9b16c6a05d2b3b6d7004ac76be85c3" +
    "ca3d55f6ae823cd287a2810243f2bd6be5f4ba7b016c80da954371e591b10c97b0938f721c7149" +
    "3bc97f9e")]