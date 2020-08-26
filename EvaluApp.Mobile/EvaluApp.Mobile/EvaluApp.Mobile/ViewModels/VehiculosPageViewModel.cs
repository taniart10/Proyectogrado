﻿using EvaluApp.Mobile.Models;
using EvaluApp.Mobile.Services;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace EvaluApp.Mobile.ViewModels
{
    public class VehiculosPageViewModel : ViewModelBase
    {

        private readonly INavigationService _navigationService;
        private ApiService _apiService;
        private List<Vehiculo> _listaVehiculos;
        private int _usuario;
        private bool _isRunning;
        private List<Eventos> _listaEventos;
        private DelegateCommand _selectHistoralCommand;
        private DelegateCommand _nuevoVehiculoCommand;

        private float _penalizacion;
        private string _idPage;
        private float _totapagado;
        private int _idpagado;
        private Vehiculo _Vehiculo;
        private int _idvehiculo;

        public VehiculosPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _apiService = new ApiService();
            _listaEventos = new List<Eventos>();
            _usuario = Preferences.Get("idUsuario", 0);
            _totapagado = float.Parse(Preferences.Get("TotalPagado", "0"));
            _idpagado = int.Parse(Preferences.Get("idPagado", "0"));
            Title = "Inicio";
            GetVehiculos();

        }


        #region Propiedades

        public DelegateCommand SelectHistoralCommand => _selectHistoralCommand ?? (_selectHistoralCommand = new DelegateCommand(SelectHistorialPage));

        public DelegateCommand NuevoVehiculoCommand => _nuevoVehiculoCommand ?? (_nuevoVehiculoCommand = new DelegateCommand(NuevoVehiculo));


        public List<Vehiculo> ListaVehiculos
        {
            get => _listaVehiculos;
            set => SetProperty(ref _listaVehiculos, value);
        }
        public List<Eventos> ListaEventos
        {
            get => _listaEventos;
            set => SetProperty(ref _listaEventos, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public Vehiculo VehiculoObj
        {
            get => _Vehiculo;
            set => SetProperty(ref _Vehiculo, value);
        }
        #endregion

        #region Metodos


        public async override void OnNavigatedTo(INavigationParameters parameters)
        {
            _idPage = Preferences.Get("menuPage", "default_value");
            if (_idPage == "NuevoVehiculoPageViewModel")
            {

                await GetVehiculos();

            }

        }

        private async Task GetEventos()
        {
            IsRunning = true;

            for (int i = 0; i < ListaVehiculos.Count; i++)
            {


                var request = new
                {
                    idusuario = _usuario,
                    idvehiculo = ListaVehiculos[i].Idvehiculo
                };

                var url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();



                var response = await _apiService.GetEventosDeHoy(url, "/api", "/Datos/buscarDatos", request);
                if (!response.IsSuccess)
                {
                    IsRunning = false;
                    if (response.Message == "")
                    {
                        response.Message = "No se pudo conectar con el Servidor por favor intente más tarde.";
                    }

                    await Prism.PrismApplicationBase.Current.MainPage.DisplayAlert("Información", response.Message.ToString(), "Aceptar");
                    await _navigationService.GoBackToRootAsync();
                    return;
                }

                _penalizacion = 0;
                if (response.Result.Count > 0)
                {

                    for (int iev = 0; iev < response.Result.Count; iev++)
                    {
                        ListaEventos.Add(new Eventos()
                        {
                            Ideventos = response.Result[iev].Ideventos,
                            Idtipoevento = response.Result[iev].Idtipoevento,
                            Puntos = response.Result[iev].Puntos,
                            Idvehiculo = response.Result[iev].Idvehiculo,
                            Idusuario = response.Result[iev].Idusuario,
                            Velocidad = response.Result[iev].Velocidad,
                            Hora = response.Result[iev].Hora,
                            VelocidadMaxima = response.Result[iev].VelocidadMaxima
                        });



                        _penalizacion += float.Parse(ListaEventos[iev].Puntos.ToString());
                    }

                }


                var total = 5000 - _penalizacion;
                ListaVehiculos[i].Puntos = total;

            }

            if (_totapagado > 0)
            {
                ListaEventos.Add(new Eventos()
                {
                    Ideventos = 0,
                    Idtipoevento = 3,
                    Puntos = _totapagado.ToString(),
                    Idvehiculo = _idpagado,
                    Idusuario = _usuario,
                    Velocidad = 0,
                    Hora = DateTime.Now.ToString("hh:mm tt"),
                    VelocidadMaxima = 0
                });

                ListaVehiculos.FirstOrDefault(x => x.Idvehiculo == _idpagado).Puntos -= _totapagado;
            }

            var src = ListaVehiculos;
            ListaVehiculos = null;
            ListaVehiculos = src;
            IsRunning = false;
        }

        private async Task GetVehiculos()
        {
            IsRunning = true;

            var url = Prism.PrismApplicationBase.Current.Resources["UrlAPI"].ToString();
            var response = await _apiService.GetVehiculosUser(url, "/api", "/Vehiculos", _usuario);

            if (!response.IsSuccess)
            {
                IsRunning = false;
                if (response.Message == "")
                {
                    response.Message = "No se pudo conectar con el Servidor por favor intente más tarde.";
                }


                await Prism.PrismApplicationBase.Current.MainPage.DisplayAlert("Información", response.Message.ToString(), "Aceptar");
                await _navigationService.GoBackToRootAsync();
                return;
            }

            if (response.Result.Count > 0)
            {
                for (int i = 0; i < response.Result.Count; i++)
                {
                    response.Result[i].Puntos = 5000;
                }
                ListaVehiculos = response.Result.ToList();
                await GetEventos();
            }
            var vehiculos = JsonConvert.SerializeObject(ListaVehiculos);
            Preferences.Set("vehiculos", vehiculos);

            IsRunning = false;

        }


        private async void SelectHistorialPage()
        {
            //    Vehiculo vehiculo = new Vehiculo() 
            //    {
            //        Idvehiculo = VehiculoObj.Idvehiculo
            //    };
            //    var parameter = new NavigationParameters();
            //    parameter.Add("Historial", ListaEventos.Where(x=> x.Idvehiculo == vehiculo.Idvehiculo));
            //    await _navigationService.NavigateAsync("HistoralPage", parameter);

            var parameter = new NavigationParameters();
            parameter.Add("Historial", ListaEventos);
            await _navigationService.NavigateAsync("HistoralPage", parameter);



        }
        private async void NuevoVehiculo()
        {
            await _navigationService.NavigateAsync("NuevoVehiculoPage");
        }
        #endregion
    }

}