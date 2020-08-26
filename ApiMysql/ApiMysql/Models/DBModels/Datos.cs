﻿using System;
using System.Collections.Generic;

namespace ApiMysql.Models.DBModels
{
    public partial class Datos
    {
        public int Idusuario { get; set; }
        public int Idvehiculo { get; set; }
        public string Longitud { get; set; }
        public string Latitud { get; set; }
        public string Acex { get; set; }
        public string Acey { get; set; }
        public string Acez { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan Hora { get; set; }
        public float Velocidad { get; set; }
        public float Gforce { get; set; }

    
    }
}
