﻿using DoctorAppointmentTDD.Entities;
using DoctorAppointmentTDD.Infrastructur.Application;
using DoctorAppointmentTDD.Service.Doctors.Contracts;
using DoctorAppointmentTDD.Service.Doctors.Exceptions;
using System.Collections.Generic;

namespace DoctorAppointmentTDD.Service.Doctors
{
    public class DoctorAppService : DoctorService
    {
        private readonly DoctorRepository _repository;
        private readonly UnitOfWork _unitOfWork;

        public DoctorAppService(
            DoctorRepository repository,
            UnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public void Add(AddDoctorDto dto)
        {
            var doctor = new Doctor
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Field = dto.Field,
                NationalCode = dto.NationalCode,
            };

            var isDoctorExist = _repository
                .IsExistNationalCode(doctor.NationalCode);
            PerventToAddDoctorWhenNationalCodeExists(isDoctorExist);

            _repository.Add(doctor);
            _unitOfWork.Commit();
        }


        public List<GetDoctorDto> GetAll()
        {
            return _repository.GetAll();
        }

        public GetDoctorDto GetByDto(int id)
        {
            return _repository.GetByDto(id);
        }

        public void Update(int id, UpdateDoctorDto dto)
        {
            var doctor = _repository.GetById(id);

            PreventToUpdateWhenDoctorNotExist(doctor);
            if (doctor.NationalCode != dto.NationalCode)
            {
                var isDoctorExist = _repository
              .IsExistNationalCodeExceptSelf(id, dto.NationalCode);

                if (isDoctorExist)
                {
                    throw new DoctorAlreadyExistException();
                }
            }

            doctor.FirstName = dto.FirstName;
            doctor.LastName = dto.LastName;
            doctor.Field = dto.Field;
            doctor.NationalCode = dto.NationalCode;
            _unitOfWork.Commit();
        }

        public void Delete(int id)
        {
            var doctor = _repository.GetById(id);

            PreventToDeleteWhenDoctorNotExist(doctor);
            _repository.Delete(doctor);
            _unitOfWork.Commit();

        }

        private static void PreventToDeleteWhenDoctorNotExist(Doctor doctor)
        {
            if (doctor == null)
            {
                throw new DoctorNotFoundException();
            }
        }

        private static void PreventToUpdateWhenDoctorNotExist(Doctor doctor)
        {
            if (doctor == null)
            {
                throw new DoctorNotFoundException();
            }
        }

        private static void PerventToAddDoctorWhenNationalCodeExists(bool isDoctorExist)
        {
            if (isDoctorExist)
            {
                throw new DoctorAlreadyExistException();
            }
        }

    }
}
