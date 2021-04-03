# Torch
Torch is an extensible plugin loader for .NET applications mainly designed for video game modding.
The core APIs provide support for common features required in game modding like text-based
commands, access control, and patching.

# Contributing
See [CONTRIBUTING.md](CONTRIBUTING.md) to learn about the ways you can contribute to Torch!

# Quick Start
* Make sure you have installed the latest versions of .NET and .NET Framework.
* If you are working on Torch.SpaceEngineers, BEFORE you load the solution you need to run `setup.bat`
  and follow the instructions to link the game DLLs to the solution. If you aren't working on Torch.SpaceEnginers,
  just unload the project because nothing else depends on it. This project will be moved to a separate repository
  in the future.
* Run the Torch project in Debug mode to automatically load the supplied testing plugin.

# Documentation
Documentation can be found on the `docs` branch or viewed at https://apeira.github.io/Torch.

## Overview
| Project              | Summary
| :---                 | :---
| Torch                | Entry point for starting a Torch application. 
| Torch.Core           | Game-agnostic service APIs and default implementations.
| Torch.Core.Tests     | Unit tests for Torch.Core.
| TestPlugin           | A debugging plugin automatically loaded by Torch in development.
| Torch.SpaceEngineers | Plugin to load a Space Engineers Dedicated Server through Torch.
