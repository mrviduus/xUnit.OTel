# Contributing to xUnitOTel

Thank you for your interest in contributing to xUnitOTel! This document provides guidelines and information for contributors.

## Code of Conduct

This project adheres to a Code of Conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## How to Contribute

### Reporting Issues

Before creating an issue, please:

1. **Search existing issues** to avoid duplicates
2. **Check the documentation** to ensure it's not a usage question
3. **Use the issue templates** when available

When reporting bugs, please include:
- **Environment details** (OS, .NET version, xUnit version)
- **Minimal reproduction case**
- **Expected vs actual behavior**
- **Error messages and stack traces**

### Suggesting Features

Feature requests are welcome! Please:

1. **Check existing feature requests** to avoid duplicates
2. **Describe the use case** clearly
3. **Explain the expected behavior**
4. **Consider backward compatibility**

### Development Setup

#### Prerequisites

- .NET 8.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

#### Getting Started

1. **Fork the repository**
   ```bash
   git clone https://github.com/[your-username]/xUnitOTel.git
   cd xUnitOTel
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Build the project**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run tests**
   ```bash
   dotnet test
   ```

#### Project Structure

```
xUnitOTel/
│── src/
│   └── xUnitOTel/           # Main library
│── tests/
│   └── xUnitOTel.Tests/     # Unit tests
├── samples/
│   └── BasicUsage/           # Usage examples
├── docs/                     # Documentation
├── .github/
│   └── workflows/            # CI/CD pipelines
└── README.md
```

### Making Changes

#### Code Style

- Follow standard .NET coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and small

#### Testing

- Write unit tests for new functionality
- Ensure all tests pass before submitting
- Aim for high code coverage
- Test both success and failure scenarios

#### Documentation

- Update relevant documentation for changes
- Add examples for new features
- Update API reference if needed

### Pull Request Process

1. **Create a descriptive PR title**
   - Use conventional commit format: `feat:`, `fix:`, `docs:`, `test:`, etc.
   - Example: `feat: add support for custom sampling strategies`

2. **Fill out the PR template**
   - Describe what changed and why
   - Link related issues
   - Include testing information

3. **Ensure CI passes**
   - All tests must pass
   - Code coverage should not decrease significantly
   - Build must succeed

4. **Request review**
   - Tag appropriate reviewers
   - Respond to feedback promptly
   - Make requested changes

### Development Guidelines

#### Coding Standards

```csharp
// ✅ Good: Clear, descriptive naming
public Activity? StartTestMethod(string className, string methodName)
{
    var activity = _activitySource.StartActivity($"test.{className}.{methodName}");
    activity?.SetTag("test.class", className);
    activity?.SetTag("test.method", methodName);
    return activity;
}

// ❌ Bad: Unclear naming, no documentation
public Activity? Start(string c, string m)
{
    return _source.StartActivity($"test.{c}.{m}");
}
```

#### Error Handling

```csharp
// ✅ Good: Graceful error handling
public void AddTags(params (string Key, object? Value)[] tags)
{
    var activity = Activity.Current;
    if (activity != null)
    {
        foreach (var (key, value) in tags)
        {
            try
            {
                activity.SetTag(key, value?.ToString());
            }
            catch (Exception ex)
            {
                // Log error but don't fail the test
                _logger?.LogWarning(ex, "Failed to set tag {Key}", key);
            }
        }
    }
}
```

#### Testing Patterns

```csharp
// ✅ Good: Clear test structure
[Fact]
public void StartTest_WithValidName_CreatesActivity()
{
    // Arrange
    using var tracer = new XUnitTracer("Test");
    
    // Act
    using var activity = tracer.StartTest("TestActivity");
    
    // Assert
    Assert.NotNull(activity);
    Assert.Equal("test.TestActivity", activity.DisplayName);
}
```

### Release Process

Releases are automated through GitHub Actions when tags are created:

1. **Version Bump**
   - Update version in `xUnitOTel.csproj`
   - Update CHANGELOG.md
   - Commit changes

2. **Create Release**
   - Create and push a git tag: `git tag v1.0.1`
   - GitHub Actions will build and publish to NuGet

3. **Update Documentation**
   - Update examples with new version
   - Announce changes in README

### Performance Considerations

- **Minimize allocations** in hot paths
- **Use object pooling** for frequently created objects
- **Benchmark critical paths** before and after changes
- **Consider async/await impact** on test execution

Example performance test:
```csharp
[Fact]
public void StartTest_Performance_UnderThreshold()
{
    using var tracer = new XUnitTracer("Performance");
    var stopwatch = Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        using var activity = tracer.StartTest($"Test{i}");
    }
    
    stopwatch.Stop();
    Assert.True(stopwatch.ElapsedMilliseconds < 100, 
        $"Expected < 100ms, got {stopwatch.ElapsedMilliseconds}ms");
}
```

### Documentation Guidelines

#### API Documentation

```csharp
/// <summary>
/// Starts a new activity for a test step
/// </summary>
/// <param name="stepName">Name of the step</param>
/// <returns>Activity instance, or null if tracing is disabled</returns>
/// <example>
/// <code>
/// using var activity = tracer.StartStep("Setup");
/// // Setup code here
/// </code>
/// </example>
public Activity? StartStep(string stepName)
```

#### Markdown Documentation

- Use clear headings and structure
- Include code examples
- Link to related documentation
- Keep examples up to date

### Questions and Support

- **General questions**: Use GitHub Discussions
- **Bug reports**: Create GitHub Issues
- **Feature requests**: Create GitHub Issues with feature template
- **Security issues**: Email maintainers directly

### Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- GitHub contributor graphs
- Special recognition for significant contributions

Thank you for contributing to xUnitOTel! 🎉
