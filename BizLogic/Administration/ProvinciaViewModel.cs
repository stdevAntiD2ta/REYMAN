﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BizLogic.Administration
{
    public class ProvinciaViewModel
    {
        [Required, StringLength(100)]
        public string Nombre { get; set; }

    }
}