using EvaluApp.Mobile.Helpers;
using EvaluApp.Mobile.Models;
using EvaluApp.Mobile.Services;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace EvaluApp.Mobile.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        //declaracion de todas componentes publicas para hacerlas privadas 
        private readonly INavigationService _navigationService;
        private readonly ApiService _apiService;
        private string _password;
        private bool _isRunning;
        private bool _isEnabled;
        private DelegateCommand _loginCommand;
        private DelegateCommand _registrarCommand;
        private DelegateCommand _restaurarPassCommand;
        private bool _encendido;

        public LoginPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService =  new ApiService();
            _isEnabled = true;
            
        }

        #region Propiedades
        //Pasando las variables publicas a privadas
        public DelegateCommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(Login));
        public DelegateCommand RegistrarCommand => _registrarCommand ?? (_registrarCommand = new DelegateCommand(RegisterUser));
        public DelegateCommand RestaurarPassCommand => _restaurarPassCommand ?? (_restaurarPassCommand = new DelegateCommand(OlvidePasswordPage));

        //toma de los datos
        public string Email { get; set; }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
        #endregion

        #region Metodos
        private async void Login()
        {
            //proceso de login
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                //presenta alecta no conexion
                await App.Current.MainPage.DisplayAlert("Información", "No se pudo conectar a internet por favor intente más tarde.", "Aceptar");

                return;
            }

            if (string.IsNullOrEmpty(Email))
            {
                //No introdujo usuario
                await App.Current.MainPage.DisplayAlert("Información", "Debe ingresar un usuario.", "Aceptar");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                // no ingreso contra
                await App.Current.MainPage.DisplayAlert("Información", "Debe ingresar una contraseña.", "Aceptar");
                return;
            }
            // isrunning boton cargando 
            IsRunning = true; 
            IsEnabled = false;



            var url = App.Current.Resources["UrlAPI"].ToString();
            //conexion con api 

            var response = await _apiService.GetUsuarioByEmailAsync(url, "/api", "/Usuario/authenticate", Email, Password);
            //conecta api service luego toma informacion de usuario con el email y password 
            if (!response.IsSuccess)
            {

                IsRunning = false;
                IsEnabled = true;
                //no hay conexion al servidor
                if (response.Message == "")
                {
                    response.Message = "No se pudo conectar con el Servidor por favor intente más tarde.";
                }

                var isValid = IsValidJson.IsValid(response.Message);
                if (isValid)
                {
                   
                    var respuesta = JsonConvert.DeserializeObject<ResponseMessage>(response.Message);
                    await App.Current.MainPage.DisplayAlert("Información", respuesta.Message, "Aceptar");
                    Password = string.Empty;
                    return;
                }

                await App.Current.MainPage.DisplayAlert("Información", response.Message, "Aceptar");
                Password = string.Empty;
                return;
            }

            //si se conecto correctamente
            Password = string.Empty;

            IsRunning = false;

            //cambia a la pagina de vehiculos al iniciar sesion
            Preferences.Set("idUsuario", response.Result.Idusuario);
            Preferences.Set("nombreCompleto", $"{ response.Result.Nombre1} { response.Result.Nombre2} {response.Result.Apellido1 }");
            Preferences.Set("TotalPagado", "0");
            Preferences.Set("idPagado", "0");


            var parameter = new NavigationParameters();
            parameter.Add("Usuario", response.Result);
            await _navigationService.NavigateAsync("/MenuPage/NavigationPage/VehiculosPage", parameter);
        }

        //si se toca el boton de registrar usuario abre la visual de nuevo usuario
        private async void RegisterUser()
        {
            await _navigationService.NavigateAsync("NuevoUsuarioPage");
        }
        //si se olvido la contrasena abre la visual 
        private async void OlvidePasswordPage()
        {
            await _navigationService.NavigateAsync("OlvidePasswordPage");

        }

        #endregion
    }
}
