# Drug Wars

[![Build Status](https://github.com/tinylama/DrugWarsGame/actions/workflows/ci.yml/badge.svg)](https://github.com/tinylama/DrugWarsGame/actions/workflows/ci.yml)
[![Release](https://img.shields.io/github/v/release/tinylama/DrugWarsGame?label=latest%20release)](https://github.com/tinylama/DrugWarsGame/releases)
[![Tests](https://github.com/tinylama/DrugWarsGame/actions/workflows/ci.yml/badge.svg?event=push)](https://github.com/tinylama/DrugWarsGame/actions/workflows/ci.yml)

> **A modern, open-source remake of the classic text-based game Drug Wars.**
> 
> Trade commodities, outsmart loan sharks, and build your fortune in a dynamic, retro-inspired world.

---

## 🚀 Features

- 🎨 **Modern retro-style UI** with terminal/CRT aesthetic
- 🌎 Multiple game locations (NYC, Australia, UK, Medellín)
- 📈 Dynamic market system with price trends and sparklines
- 🦈 Loan shark system with dynamic interest
- 🏆 High score leaderboard
- 🎲 Random events and market manipulations
- 🧥 Trenchcoat inventory system
- 🏦 Banking system
- 💡 Tooltips with price history and market tips

## 📸 Screenshots

> _Coming soon!_

## 🛠️ Requirements

- Windows OS
- .NET 8.0 Runtime ([Download here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0))
- Visual Studio 2022 (for development)

## ⚡ Quick Start

### From Releases
1. Download the latest release from the [Releases page](https://github.com/tinylama/DrugWarsGame/releases)
2. Extract the zip file
3. Run `DrugWars.exe`

### From Source
```sh
git clone https://github.com/tinylama/DrugWarsGame.git
cd DrugWarsGame
dotnet restore
dotnet build
dotnet test # Run all tests
dotnet publish -c Release -r win-x64 --self-contained true
```

## 🧪 Build & Test

- All pushes to `main` are automatically built and tested via GitHub Actions.
- [View build status and test results](https://github.com/tinylama/DrugWarsGame/actions)

## 🧩 Project Structure

- `DrugWars.Core` — Core game logic and models
- `DrugWars.Wpf` — WPF UI implementation
- `DrugWars.Tests` — Automated tests (xUnit)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to your branch
5. Create a Pull Request

## 📢 Community & Support

- [Issues](https://github.com/tinylama/DrugWarsGame/issues) — Bug reports & feature requests
- [Discussions](https://github.com/tinylama/DrugWarsGame/discussions) — Chat, ideas, and Q&A

## 📜 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Original Drug Wars game by John E. Dell
- Modern UI inspiration from various retro-style games 