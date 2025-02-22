﻿using DoctorAppointmentTDD.Entities;
using DoctorAppointmentTDD.Infrastructur.Test;
using DoctorAppointmentTDD.Persistence.EF;
using DoctorAppointmentTDD.Persistence.EF.Appointments;
using DoctorAppointmentTDD.Service.Appointments;
using DoctorAppointmentTDD.Service.Appointments.Contracts;
using DoctorAppointmentTDD.Service.Appointments.Exceptions;
using DoctorAppointmentTDD.Service.Doctors.Contracts;
using DoctorAppointmentTDD.Service.Patients.Contracts;
using DoctorAppointmentTDD.Test.Tools.Doctors;
using DoctorAppointmentTDD.Test.Tools.Patients;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DoctorAppointmentTDD.Services.Test.Appointments
{
    public class AppointmentServiceTest
    {
        private readonly ApplicationDbContext _dataContext;
        private readonly EFUnitOfWork _unitOfWork;
        private readonly AppointmentRepository _repository;
        private readonly AppointmentService _sut;

        public AppointmentServiceTest()
        {
            _dataContext =
                new EFInMemoryDatabase()
                .CreateDataContext<ApplicationDbContext>();

            _unitOfWork = new EFUnitOfWork(_dataContext);
            _repository = new EFAppointmentRepository(_dataContext);
            _sut = new AppointmentAppService(_repository, _unitOfWork);
        }

        [Fact]
        public void Add_add_appointment_properly()
        {
            var doctor = DoctorFactory.CreateDoctor("2380132933");
            _dataContext.Manipulate(_ => _.Add(doctor));

            Patient patient = PatientFactory.CreatePatient("2380257515");
            _dataContext.Manipulate(_ => _.Add(patient));

            AddAppointmentDto dto = GenerateAddAppointmentDto(doctor, patient);

            _sut.Add(dto);

            _dataContext.Appointments.Should().Contain(_ => _.DoctorId == dto.DoctorId &&
            _.PatientId == dto.PatientId &&
            _.Date == dto.Date);
        }

        [Fact]
        public void Add_throw_AppointmentAlreadyExistException_when_add_new_appointment()
        {
            var doctor = DoctorFactory.CreateDoctor("2380132933");
            _dataContext.Manipulate(_ => _.Add(doctor));

            Patient patient = PatientFactory.CreatePatient("2380257515");
            _dataContext.Manipulate(_ => _.Add(patient));

            Appointment appointment = CreateAppointment(doctor, patient);
            _dataContext.Manipulate(_ => _.Appointments.Add(appointment));

            AddAppointmentDto dto = GenerateAddAppointmentDto(doctor, patient);

            Action expected = () =>_sut.Add(dto);

            expected.Should().Throw<AppointmentAlreadyExist>();
        }

        [Fact]
        public void Add_throw_AppointmentCountIsFullException_when_doctor_has_more_than_five_appointment_in_one_day()
        {
            var doctor = new Doctor { FirstName = "doctor1", LastName = "doctor1", Field = "field1", NationalCode = "123" };

            var appointments = GenrateListOfAppointmnets(doctor);
            _dataContext.Manipulate(_ =>
            _.Appointments.AddRange(appointments));

            Patient patient = PatientFactory.CreatePatient("2380257515");
            _dataContext.Manipulate(_ => _.Add(patient));

            AddAppointmentDto dto = GenerateAddAppointmentDto(doctor, patient);

            Action expected = () => _sut.Add(dto);

            expected.Should().ThrowExactly<AppointmentCountIsFull>();

        }

        [Fact]
        public void GetAll_returns_all_appointments_with_doctor_and_patient()
        {
            List<Appointment> appointments = CreateAppointmentsIndataBase();
            _dataContext.Manipulate(_ =>
            _.Appointments.AddRange(appointments));

            var expected = _sut.GetAll();

            expected.Should().HaveCount(2);
            expected.Should().Contain(_ => _.Date.Date==appointments[0].Date.Date);
            expected.Should().Contain(_ => _.doctor.FirstName == "doctor1");
            expected.Should().Contain(_ => _.doctor.LastName == "doctor1");
            expected.Should().Contain(_ => _.doctor.Field == "field1");
            expected.Should().Contain(_ => _.doctor.NationalCode == "123");
            expected.Should().Contain(_ => _.doctor.FirstName == "doctor2");
            expected.Should().Contain(_ => _.doctor.LastName == "doctor2");
            expected.Should().Contain(_ => _.doctor.Field == "field2");
            expected.Should().Contain(_ => _.doctor.NationalCode == "1234");

            expected.Should().Contain(_ => _.patient.FirstName == "patient1");
            expected.Should().Contain(_ => _.patient.FirstName == "patient2");
            expected.Should().Contain(_ => _.patient.LastName == "patient1");
            expected.Should().Contain(_ => _.patient.LastName == "patient2");
            expected.Should().Contain(_ => _.patient.NationalCode == "123");
            expected.Should().Contain(_ => _.patient.NationalCode == "1234");
        }

        [Fact]
        public void Get_return_one_appointment_with_doctor_and_pateint()
        {
            var doctor = DoctorFactory.CreateDoctor("2380132933");
            _dataContext.Manipulate(_ => _.Add(doctor));

            Patient patient = PatientFactory.CreatePatient("2380257515");
            _dataContext.Manipulate(_ => _.Add(patient));

            Appointment appointment = CreateAppointment(doctor, patient);
            _dataContext.Manipulate(_ => _.Appointments.Add(appointment));

            GetAppointmentDto expected = _sut.GetAppointmentById(appointment.Id);

            expected.Date.Year.Should().Be(2022);
            expected.doctor.FirstName.Should().Be("dummyName");
            expected.doctor.NationalCode.Should().Be("2380132933");
            expected.patient.FirstName.Should().Be("dummyName");
            expected.patient.LastName.Should().Be("dummyLastname");
            expected.patient.NationalCode.Should().Be("2380257515");

        }

        [Fact]
        public void Update_update_appointment_properly()
        {
            var doctor = DoctorFactory.CreateDoctor("2380132933");
            _dataContext.Manipulate(_ => _.Add(doctor));

            Patient patient = PatientFactory.CreatePatient("2380257515");
            _dataContext.Manipulate(_ => _.Add(patient));

            Appointment appointment = CreateAppointment(doctor, patient);
            _dataContext.Manipulate(_ => _.Appointments.Add(appointment));

            UpdateAppointmentDto dto = GenerateUpdateAppointmentDto(doctor, patient);

            _sut.Update(appointment.Id, dto);

            var expected = _dataContext.Appointments
                .FirstOrDefault(_=>_.Id == appointment.Id);
            expected.DoctorId.Should().Be(dto.DoctorId);
            expected.PatientId.Should().Be(dto.PatientId);
        }

        [Fact]
        public void Delete_delete_appointment_properly()
        {
            var doctor = DoctorFactory.CreateDoctor("2380132933");
            _dataContext.Manipulate(_ => _.Add(doctor));

            Patient patient = PatientFactory.CreatePatient("2380257515");
            _dataContext.Manipulate(_ => _.Add(patient));

            Appointment appointment = CreateAppointment(doctor, patient);
            _dataContext.Manipulate(_ => _.Appointments.Add(appointment));

            _sut.Delete(appointment.Id);

            _dataContext.Appointments.Should()
                .NotContain(_=>_.Id==appointment.Id);

        }


        private UpdateAppointmentDto GenerateUpdateAppointmentDto(Doctor doctor, Patient patient)
        {
            return new UpdateAppointmentDto
            {
                Date = DateTime.Now,
                DoctorId = doctor.Id,
                PatientId = patient.Id,

            };
        }

        private static AddAppointmentDto GenerateAddAppointmentDto(Doctor doctor, Patient patient)
        {
            return new AddAppointmentDto()
            {
                DoctorId = doctor.Id,
                PatientId = patient.Id,
                Date = new DateTime(2022, 04, 28)
            };
        }

        private Appointment CreateAppointment(Doctor doctor, Patient patient)
        {
            return new Appointment
            {
                Date = new DateTime(2022, 04, 28),
                DoctorId = doctor.Id,
                PatientId = patient.Id,
            };
        }

        private List<Appointment> CreateAppointmentsIndataBase()
        {
            return new List<Appointment>
            {
                new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=new Doctor{FirstName="doctor1", LastName="doctor1", Field="field1", NationalCode="123" },
                    Patient=new Patient{FirstName="patient1", LastName="patient1",NationalCode="123"},
                },
                new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=new Doctor{FirstName="doctor2", LastName="doctor2", Field="field2", NationalCode="1234" },
                    Patient=new Patient{FirstName="patient2", LastName="patient2",NationalCode="1234"},
                },            
            }.ToList();
        }

        private static List<Appointment> GenrateListOfAppointmnets(Doctor doctor)
        {

            var appointments= new List<Appointment>
            {
                new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=doctor,
                    Patient=new Patient{FirstName="patient1", LastName="patient1",NationalCode="123"},
                },
               new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=doctor,
                    Patient=new Patient{FirstName="patient2", LastName="patient2",NationalCode="1234"},
                },
               new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=doctor,
                    Patient=new Patient{FirstName="patient3", LastName="patient3",NationalCode="12345"},
                },
              new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=doctor,
                    Patient=new Patient{FirstName="patient4", LastName="patient4",NationalCode="123456"},
                },
               new Appointment {
                    Date = new DateTime(2022, 04, 28),
                    Doctor=doctor,
                    Patient=new Patient{FirstName="patient5", LastName="patient5",NationalCode="1234567"},
                },
            }.ToList();
            return appointments;
        }
    }
}
