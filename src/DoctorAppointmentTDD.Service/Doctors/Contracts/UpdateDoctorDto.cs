﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoctorAppointmentTDD.Service.Doctors.Contracts
{
    public class UpdateDoctorDto
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string NationalCode { get; set; }
        public string Field { get; set; }
    }
}
