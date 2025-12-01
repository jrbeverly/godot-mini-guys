.PHONY: build test run import export-windows smoke format help

build: ## Build the solution
	dotnet build src/MiniGuys/MiniGuys.sln

test: ## Run unit tests
	dotnet test src/MiniGuys/MiniGuys.sln

run: ## Launch the game in Godot (requires godot on PATH)
	@if command -v godot >/dev/null 2>&1; then \
		godot src/MiniGuys/project.godot; \
	else \
		echo "godot not found on PATH."; \
		echo "Install Godot 4.x .NET edition from https://godotengine.org/"; \
		echo "and ensure the 'godot' binary is on your PATH."; \
		exit 1; \
	fi

import: ## Import project assets headlessly (requires godot on PATH)
	godot --headless --import --path src/MiniGuys

export-windows: import ## Export a Windows x86_64 build to build/windows (requires godot .NET + export templates)
	godot --headless --export-release "Windows Desktop" $(CURDIR)/build/windows/MiniGuys.exe --path src/MiniGuys

smoke: build import test ## Build, import assets, and run tests

format: ## Format C# source with dotnet-format
	dotnet format src/MiniGuys/MiniGuys.sln

help: ## Print available targets and their descriptions
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | \
		awk 'BEGIN {FS = ":.*?## "}; \
		{printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'
