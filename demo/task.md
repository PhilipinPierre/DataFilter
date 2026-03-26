# Task: Audit & Fix Demo Projects Against DemoFeatures.md

## Audit (research)
- [x] Read DemoFeatures.md contract
- [x] Explore all demo project structures
- [x] Read WPF reference implementation (all 6 scenarios)
- [x] Read src library APIs (WinForms, WinUI3, UWP, MAUI, Blazor)

## Implementation Planning
- [x] Write implementation_plan.md
- [ ] Get user approval

## WinForms Demo (`DataFilter.WinForms.Demo`)
- [x] Restructure project: add Services/, ViewModels/, Views/ folders
- [x] Add `MockEmployeeApiService` (copy from WPF demo)
- [x] Add `IMockEmployeeApiService`
- [x] Add `LocalFilterScenarioViewModel`
- [x] Add `AsyncFilterScenarioViewModel`
- [x] Add `HybridFilterScenarioViewModel`
- [x] Add `CustomizationScenarioViewModel`
- [x] Add `ListViewScenarioViewModel`
- [x] Add `CollectionViewScenarioViewModel`
- [x] Rewrite `MainForm` with TabControl + shell controls (RowCount, Regenerate, Clear)
- [x] Add individual tab panels for each scenario

## WinUI3 Demo (`DataFilter.WinUI3.Demo`)
- [x] Add project reference to DataFilter.Demo.Shared and DataFilter.WinUI3 src
- [x] Add `MockEmployeeApiService`
- [x] Add scenario ViewModels (6)
- [x] Add MainViewModel with RowCount, Regenerate, ClearFilters commands
- [x] Rewrite MainWindow with NavigationView + shell controls
- [x] Add individual pages for each scenario

## UWP Demo (`DataFilter.UwpXaml.Demo`)
- [x] Add project reference to DataFilter.Demo.Shared and DataFilter.UwpXaml src
- [x] Add `MockEmployeeApiService`
- [x] Add 6 scenario ViewModels
- [x] Add MainViewModel
- [x] Rewrite MainPage with Pivot/tabs + shell controls
- [x] Add individual pages for each scenario

## MAUI Demo (`DataFilter.Demo.Maui`)
- [ ] Add project reference to DataFilter.Demo.Shared and DataFilter.Maui src
- [ ] Add `MockEmployeeApiService`
- [ ] Add 6 scenario ViewModels
- [ ] Add MainViewModel
- [ ] Rewrite AppShell with TabBar/Shell pages
- [ ] Add individual content pages for each scenario

## Blazor Shared Demo (`DataFilter.Blazor.Demo.Shared`)
- [ ] Rewrite (or extend) DemoPage.razor into 6 scenario sub-pages/routes
- [ ] Add Routes, NavMenu, shell component
- [ ] Add `MockEmployeeApiService` (Blazor-aware)
- [ ] Add scenario ViewModels / state management
- [ ] Implement all 6 scenarios with proper async, hybrid, theme-switching, ListView equivalent

## Blazor Server Demo (`DataFilter.Blazor.Demo.Server`)
- [ ] Ensure it references the updated Shared and uses all scenarios

## Verification
- [ ] Build: `dotnet build DataFilter.slnx` — passes with 0 errors
- [ ] Manual: User reviews each running demo app
