Copy of https://www.codeproject.com/Tips/469452/WPF-ExceptionViewer \
Licence: https://www.codeproject.com/info/cpol10.aspx \
Nuget: https://www.nuget.org/packages/WpfExceptionViewer2 \

### Supported frameworks
- net45
- netcoreapp3.1

## Usage
### App.xaml

    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/WpfExceptionViewer2;component/DarkColors.xaml" />
            
            <!--or bright colors-->
            <!--<ResourceDictionary Source="pack://application:,,,/WpfExceptionViewer2;component/BrightColors.xaml" />-->
        </ResourceDictionary.MergedDictionaries>

        <!--or custom colors-->
        <!--
        <SolidColorBrush x:Key="BackgroundBrush"
             options:Freeze="True"
             Color="Pink" />

        <SolidColorBrush x:Key="ForegroundBrush"
             options:Freeze="True"
             Color="Green" />
        -->
    </ResourceDictionary>

### Invoke Dialog

    WpfExceptionViewer2.ExceptionViewer.DefaultTitle = "Sorry";
    new WpfExceptionViewer2.ExceptionViewer(ex.Message, ex).Show();
