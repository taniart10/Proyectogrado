using System;
using System.Collections.Generic;

namespace ApiMysql.Models.DBModels
{
    public partial class Eventos
    {
        public int Ideventos { get; set; }
        public int Idtipoevento { get; set; }
        public string Puntos { get; set; }
        public int Idvehiculo { get; set; }
        public int Idusuario { get; set; }
        public float Velocidad { get; set; }
        public string Hora { get; set; }
        public float VelocidadMaxima { get;set; }

    }
}
