global using Xunit;
using xUnit.OTel.Attributes;
using xUnit.OTel.Tests;
[assembly: AssemblyFixture(typeof(TestSetup))]
[assembly: Trace]
[assembly: CaptureConsole]
