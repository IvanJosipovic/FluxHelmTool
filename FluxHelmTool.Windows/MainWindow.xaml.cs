using Microsoft.Extensions.DependencyInjection;
using Microsoft.MobileBlazorBindings;
using Microsoft.MobileBlazorBindings.Hosting;
using System.Windows;

namespace FluxHelmTool.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MyBlazorWebView.ComponentType = typeof(WebUI.App);

            var hostBuilder = BlazorWebHost.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Adds web-specific services such as NavigationManager
                    services.AddBlazorHybrid();

                    // Register app-specific services
                    //services.AddSingleton<AppState>();
                })
                .UseWebRoot("wwwroot");

            hostBuilder.UseStaticFiles();

            var host = hostBuilder.Build();

            MyBlazorWebView.Host = host;
        }
    }
}