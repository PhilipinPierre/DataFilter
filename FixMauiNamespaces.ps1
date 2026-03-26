$dest = "d:\Workspaces_Personal\Dev\DataFilter\demo\DataFilter.Demo.Maui"

Get-ChildItem -Path "$dest" -Include *.cs -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName
    
    # Replace UWP namespaces with MAUI ones
    $content = $content -replace "using DataFilter.UwpXaml.ViewModels;", "using DataFilter.Maui.ViewModels;"
    $content = $content -replace "using DataFilter.UwpXaml.Models;", "using DataFilter.Maui.Models;"
    $content = $content -replace "using DataFilter.UwpXaml.Services;", "using DataFilter.Maui.Demo.Services;"
    
    # Fix any other DataFilter.UwpXaml mentions
    $content = $content -replace "DataFilter.UwpXaml", "DataFilter.Maui"
    
    # Ensure Task is available (sometimes missing from port)
    if ($content -notmatch "using System.Threading.Tasks;" -and $content -match "Task") {
        $content = @("using System.Threading.Tasks;") + $content
    }

    Set-Content $_.FullName $content
}
