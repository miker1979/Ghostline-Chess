# Ghostline Chess

**Every move awakens something.**

Ghostline Chess is a horror-themed Windows chess game built with C# and Windows Forms. The Pale Court and Shadow Court fight across a gothic board while the Ghostline Tome records each move and the Graveyard displays captured pieces.

![Ghostline Chess gameplay](docs/screenshots/ghostline-chess-gameplay.png)

## Current development build

The latest source includes:

- Complete legal movement and captures
- Check and checkmate detection with a crimson king warning
- Kingside and queenside castling, including castling safety validation
- En passant and all four promotion choices
- Algebraic move history in the Ghostline Tome
- Captured-piece tracking in the Graveyard
- FEN import, export, validation, and move counters
- Stalemate and insufficient-material draws
- Threefold-repetition and fifty-move-rule draws
- Custom Pale Court and Shadow Court artwork

## Requirements

- Windows 10 or Windows 11
- Visual Studio 2026 with the **.NET desktop development** workload, or the .NET 10 SDK

## Build and run

```powershell
dotnet build
dotnet run --project GhostlineChess/GhostlineChess.csproj
```

## Windows release

The existing packaged Windows release remains available from the [Releases page](https://github.com/miker1979/Ghostline-Chess/releases). The source on this branch is newer than the current packaged release and will become the basis for the next downloadable build.

## Project status

The core chess-engine validation pass is complete. The next milestone is audiovisual polish: sound effects, spectral capture animation, and additional ambient effects.

Built by Michael Robinson for Haunted Echoes Studios.
