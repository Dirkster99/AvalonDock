# Contributing to AvalonDock

Thank you for considering contributing to AvalonDock! Here's how to get started.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or later)
- Windows (WPF is Windows-only)
- Visual Studio 2022 or later (recommended), or any editor with .NET support

## Building

```bash
dotnet build source/AvalonDock.sln
```

## Running Tests

The test suite uses **NUnit** and requires single-threaded execution for WPF/STA compatibility:

```bash
dotnet test source/AvalonDock.sln -m:1
```

## Submitting a Pull Request

1. Fork the repository and create your branch from `master`
2. Make your changes
3. Ensure the solution builds without errors
4. Run the tests and verify they pass
5. Open a pull request against `master`

### PR expectations

- Keep changes focused — one feature or fix per PR
- Follow existing code style (tabs for indentation, UTF-8, LF line endings — see `.editorconfig`)
- Add or update tests if applicable
- Provide a clear description of what the PR does and why

## Reporting Issues

- Use [GitHub Issues](https://github.com/Dirkster99/AvalonDock/issues) to report bugs or request features
- Include steps to reproduce for bugs
- Include the .NET version and OS version you're using

## License

By contributing, you agree that your contributions will be licensed under the same
[MS-PL / Apache 2.0 License](LICENSE) that covers the project.
