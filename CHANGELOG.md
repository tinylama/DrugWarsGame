# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2024-04-30

### Added
- Initial release of Drug Wars game
- Modern retro-style UI with terminal aesthetics
- Multiple game locations (New York City, Australia, United Kingdom, Medellín)
- Dynamic market system with price fluctuations
- Loan shark system with dynamic interest rates
- High score system with persistent storage
- Random events and market manipulations
- Trenchcoat inventory system
- Banking system
- Drug tooltips with price trend indicators
- Styled error windows

### Fixed
- Drug price min/max values now properly track historical prices
- Improved tooltip readability with formatted numbers
- Fixed window inheritance to consistently use GameWindowBase
- Replaced file logging with debug output

### Security
- High scores now stored in isolated storage for improved security 