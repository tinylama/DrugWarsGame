# Drug Wars

A modern remake of the classic text-based game Drug Wars, built with WPF and .NET 8.0. Trade commodities across different locations while avoiding loan sharks and managing your finances.

## Features

- Modern retro-style UI with a terminal aesthetic
- Multiple game locations (New York City, Australia, United Kingdom, Medellín)
- Dynamic market system with price fluctuations
- Loan shark system with dynamic interest rates
- High score system
- Random events and market manipulations
- Trenchcoat inventory system
- Banking system

## Requirements

- Windows OS
- .NET 8.0 Runtime
- Visual Studio 2022 (for development)

## Installation

### From Releases
1. Download the latest release from the [Releases page](https://github.com/yourusername/DrugWars/releases)
2. Extract the zip file
3. Run `DrugWars.exe`

### From Source
1. Clone the repository
2. Open `DrugWars.sln` in Visual Studio 2022
3. Build and run the solution

## Development

The solution consists of two projects:
- `DrugWars.Core`: Core game logic and models
- `DrugWars.Wpf`: WPF UI implementation

### Building

```powershell
dotnet restore
dotnet build
dotnet publish -c Release -r win-x64 --self-contained true
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Original Drug Wars game by John E. Dell
- Modern UI inspiration from various retro-style games 