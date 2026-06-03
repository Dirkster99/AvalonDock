---
title: Contributing
layout: default
nav_order: 8
description: "How to contribute to AvalonDock."
permalink: /contributing/
---

# Contributing

AvalonDock is an open source project and contributions are welcome! Here's how to get started.

---

## Ways to Contribute

- ⭐ **Star** the [repository](https://github.com/Dirkster99/AvalonDock) to show your support
- 🐛 **Report bugs** via [GitHub Issues](https://github.com/Dirkster99/AvalonDock/issues)
- 💡 **Request features** via [GitHub Issues](https://github.com/Dirkster99/AvalonDock/issues)
- 🔧 **Submit pull requests** with bug fixes or improvements
- 📖 **Improve documentation** — this site is in the `docs/` folder

---

## Development Setup

### Prerequisites

- **Windows** (WPF is Windows-only)
- **.NET 9** and/or **.NET 10** SDK
- **Visual Studio 2022** or later

### Build & Test

```bash
git clone https://github.com/Dirkster99/AvalonDock.git
cd AvalonDock

dotnet build source/AvalonDock.sln
dotnet test source/AvalonDock.sln -m:1
```

{: .important }
Tests must run single-threaded (`-m:1`) due to WPF STA requirements.

---

## Pull Request Guidelines

1. **Fork** from the `master` branch
2. Keep changes **focused** — one feature or fix per PR
3. Follow the existing **code style** (tabs for indentation, UTF-8, LF line endings)
4. Add or update **tests** if applicable
5. Ensure the build passes with **zero warnings** (all warnings are treated as errors)

### Commit Message Format

```
[Fix|Feature] #<IssueNumber> - [<Topic>] <Description>
```

Examples:
- `[Fix] #123 - [Serialization] Handle null ContentId during deserialization`
- `[Feature] #456 - [Themes] Add Arc theme dark variant`

---

## Code Quality Standards

The project enforces strict quality standards:

- **StyleCop.Analyzers** for code style consistency
- **Microsoft.CodeAnalysis.NetAnalyzers** for .NET best practices
- **SonarAnalyzer.CSharp** for security and reliability
- All warnings treated as errors in CI

---

## License

By contributing, you agree that your contributions will be licensed under the [MS-PL / Apache 2.0](https://github.com/Dirkster99/AvalonDock/blob/master/LICENSE) license.
