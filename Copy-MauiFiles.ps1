$src = "d:\Workspaces_Personal\Dev\DataFilter\demo\DataFilter.UwpXaml.Demo"
$dest = "d:\Workspaces_Personal\Dev\DataFilter\demo\DataFilter.Demo.Maui"

New-Item -ItemType Directory -Force "$dest\Services"
New-Item -ItemType Directory -Force "$dest\ViewModels"
New-Item -ItemType Directory -Force "$dest\Pages"

Copy-Item "$src\Services\*.cs" "$dest\Services\"
Copy-Item "$src\ViewModels\*.cs" "$dest\ViewModels\"

Get-ChildItem -Path "$dest" -Include *.cs -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName
    $content = $content -replace "namespace DataFilter.UwpXaml.Demo", "namespace DataFilter.Maui.Demo"
    $content = $content -replace "using DataFilter.UwpXaml.Demo", "using DataFilter.Maui.Demo"
    $content = $content -replace "using Windows.UI.Xaml", "using Microsoft.Maui.Controls"
    $content = $content -replace "public partial FilterableDataGridViewModel", "private FilterableDataGridViewModel"
    $content = $content -replace "public partial System.Collections.ObjectModel", "private System.Collections.ObjectModel"
    $content = $content -replace "public partial bool", "private bool"
    Set-Content $_.FullName $content
}
