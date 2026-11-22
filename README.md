# Bounce On Modding Suite
⚠️ This repo is in very early Pre-Alpha. Expect most things to not work.

The Bounce On Modding Suite is a C# CLI (console) app with set of functions used to aid in inspecting, extracting and modifying video games made by Phobic Studios (formerly Team Phobic) and IddyBiddyGames, with a particular focus on the Bounce On series.

The tools intend to fully support the PC ports of [Bounce On](https://store.steampowered.com/app/4394300/Bounce_On/) and [Bounce On 2: Drallo's Demise](https://store.steampowered.com/app/4662700/Bounce_On_2_Drallos_Demise/). Other games and mobile ports might also be supported, but they are not prioritized.

The objective of this app is to greatly automate conversion and modding of these games.

## Current Capabilities
- Can **unbundle video game hashbundles using PTB hashtables** to make them more readable
- Can read Phobic Tree Binary files (PTB v1 and PTB v2).

# Installation
Currently, releases aren't yet offered. You'll have to download and build the source code yourself, which will require the .NET 10 SDK.

# Usage
## Requirements
- Have a computer
- Own a copy of a supported video game (Bounce On, Bounce On 2: Drallo's Demise, Gravbot, etc.)

## Commands
Open a command line in the folder with the executable.
```
# <> = required argument, [] = optional argument. Don't include <> or [] in the actual command
# For arguments that contain spaces, like some file paths, surround them with "

# If you have a hashbundle with video game assets you want to unbundle and read, use:
./BoModdingSuite unbundle [hashtablePath] [outputDir]
# Example: ./BoModdingSuite unbundle "/home/user/.local/share/Steam/steamapps/common/Bounce On 2 Drallo's Demise/hashbundle/" /home/user/Documents/bo2extract

# ⚠️ UNFINISHED AND DOESN'T YET WORK
# To convert a PTB file into JSON:
./BoModdingSuite ptbToJson <inputPtbPath> [outputPath] [-c|--compact] [-o|--overwrite]
# Resulting JSON is beautified (formatted) by default. Provide the '-c' or '--compact' argument to not beautify it.
# This command will avoid overwriting any file. If you are OK with it overwriting output files, provide the '-o' or '--overwrite' argument.
# Example: ./BoModdingSuite ptbToJson /home/user/Documents/bo2extract/Content/keys.ptb /home/user/Documents/bo2extract/keys.json -o
```

# Building
## Requirements
- .NET 10 SDK

## Steps
Note: To make the app easier to ship to users and cleaner, properties were set in `.csproj` to publish everything into one file with partial trimming involved. This may or may not introduce issues, though I haven't yet seen any.
1. Run `dotnet publish` in the directory of the project.
2. The output path should be shown to you. The application should be in the `publish` folder (there may also be a copy with multiple files in the folder housing the `publish` folder).

# Credits
- [Spikeyy715](https://www.youtube.com/@spikeyy715) for his PTB python tool - used as reference

# AI Usage
None of this repo's code was written by an AI (at the moment), but an AI has been used for explaining some concepts used to the maintainer, like JsonConverter, single-file publishing and performance practices. 

Some of the code might have been based on examples provided by the AI (Claude).

AI has also been used for figuring out formats that will be supported in the future (in particular, the .ptm map format).
