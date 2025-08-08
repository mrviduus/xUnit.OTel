global using Xunit;
using xUnitOTel.Attributes;
using xUnitOTel.Tests;
[assembly: AssemblyFixture(typeof(TestSetup))]
[assembly: Trace]
[assembly: CaptureConsole]
