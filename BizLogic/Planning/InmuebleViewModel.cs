﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BizLogic.Planning
{
    public class InmuebleViewModel
    {
        [Required, StringLength(100)]
        public string Direccion { get; set; }

        public int PlanID { get; set; }

        public int Id { get; set; }

        public string button { get; set; }

    }
}
