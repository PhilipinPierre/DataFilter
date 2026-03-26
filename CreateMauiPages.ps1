$pages = @("LocalFilterPage", "AsyncFilterPage", "HybridFilterPage", "CustomizationPage", "ListViewPage", "CollectionViewPage")
$dir = "d:\Workspaces_Personal\Dev\DataFilter\demo\DataFilter.Demo.Maui\Pages"
if (!(Test-Path $dir)) { New-Item -ItemType Directory -Force $dir }

foreach ($p in $pages) {
    $xaml = @"
<?xml version=`"1.0`" encoding=`"utf-8`" ?>
<ContentPage xmlns=`"http://schemas.microsoft.com/dotnet/2021/maui`"
             xmlns:x=`"http://schemas.microsoft.com/winfx/2009/xaml`"
             xmlns:controls=`"clr-namespace:DataFilter.Maui.Controls;assembly=DataFilter.Maui`"
             xmlns:vm=`"clr-namespace:DataFilter.Maui.Demo.ViewModels`"
             x:Class=`"DataFilter.Maui.Demo.Pages.$p`"
             Title=`"$p`">

    <Grid>
        <controls:FilterableDataGrid ViewModel=`"{Binding ViewModel.GridViewModel}`" />
    </Grid>
</ContentPage>
"@
    
    $cs = @"
using DataFilter.Maui.Demo.ViewModels;
using Microsoft.Maui.Controls;

namespace DataFilter.Maui.Demo.Pages;

public partial class $p : ContentPage
{
    public $($p.Replace('Page', 'ScenarioViewModel')) ViewModel { get; }

    public $p($($p.Replace('Page', 'ScenarioViewModel')) viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        BindingContext = this;
    }
}
"@
    Set-Content -Path "$dir\$p.xaml" -Value $xaml
    Set-Content -Path "$dir\$p.xaml.cs" -Value $cs
}
