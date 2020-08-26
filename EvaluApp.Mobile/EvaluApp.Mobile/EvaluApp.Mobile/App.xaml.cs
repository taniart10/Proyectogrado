using EvaluApp.Mobile.Services;
using EvaluApp.Mobile.ViewModels;
using EvaluApp.Mobile.Views;
using Prism;
using Prism.Ioc;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace EvaluApp.Mobile
{
    public partial class App
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/LoginPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();            
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<LoginPage, LoginPageViewModel>();
            containerRegistry.RegisterForNavigation<VehiculosPage, VehiculosPageViewModel>();
            containerRegistry.RegisterForNavigation<MenuPage, MenuPageViewModel>();
            containerRegistry.RegisterForNavigation<NuevoUsuarioPage, NuevoUsuarioPageViewModel>();
            containerRegistry.RegisterForNavigation<OlvidePasswordPage, OlvidePasswordPageViewModel>();
            containerRegistry.RegisterForNavigation<HistoralPage, HistoralPageViewModel>();
            containerRegistry.RegisterForNavigation<StorePage, StorePageViewModel>();
            containerRegistry.RegisterForNavigation<PerfilUsuarioPage, PerfilUsuarioPageViewModel>();
            containerRegistry.RegisterForNavigation<NuevoVehiculoPage, NuevoVehiculoPageViewModel>();
            containerRegistry.RegisterForNavigation<FacturaPage, FacturaPageViewModel>();
        }
    }
}
