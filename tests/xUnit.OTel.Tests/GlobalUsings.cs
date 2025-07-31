global using Xunit;
using xUnit.OTel.Attributes;
using xUnit.OTel.Tests;
[assembly: AssemblyFixture(typeof(TestFixture))]
[assembly: Trace]
[assembly: CaptureConsole]
