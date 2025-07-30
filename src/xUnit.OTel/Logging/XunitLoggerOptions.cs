using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;

namespace xUnit.OTel.Logging;

public class XunitLoggerOptions
{
    public XunitLoggerOptions()
    {
    }

    public Func<string?, LogLevel, bool> Filter { get; set; } = static (c, l) => true; //By default, log everything
    public Func<string, IMessageSinkMessage> MessageSinkMessageFactory { get; set; } = static (m) => new
        Xunit.v3.DiagnosticMessage(m);

    public bool IncludeScopes { get; set; }
    [StringSyntax(StringSyntaxAttribute.DateTimeFormat)]
    public string? TimestampFormat { get; set; }

    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;
}
