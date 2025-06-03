using GradingManagementSystem.Core;
using GradingManagementSystem.Core.CustomResponses;
using GradingManagementSystem.Core.DTOs;
using GradingManagementSystem.Core.Entities;
using GradingManagementSystem.Core.Repositories.Contact;
using GradingManagementSystem.Core.Services.Contact;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace GradingManagementSystem.Service
{
    public class AcademicAppointmentsService : IAcademicAppointmentsService
    {
        private readonly IAcademicAppointmentRepository _academicAppointmentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AcademicAppointmentsService(IAcademicAppointmentRepository academicAppointmentRepository,
                                           IUnitOfWork unitOfWork)
        {
            _academicAppointmentRepository = academicAppointmentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse> CreateNewAcademicAppointmentAsync(CreateAcademicAppointmentDto model)
        {
            if (model == null)
                return CreateErrorResponse400BadRequest("Invalid input data.");

            if (string.IsNullOrWhiteSpace(model.Year) ||
                model.Year.Length != 9 ||
                !Regex.IsMatch(model.Year, @"^\d{4}-\d{4}$"))
                return CreateErrorResponse400BadRequest("Invalid year format, Academic year must be in 'YYYY-YYYY' format.");

            var existingAppointment = await _academicAppointmentRepository.FindAsync(a => a.Year == model.Year);
            if (existingAppointment != null)
                return CreateErrorResponse400BadRequest($"Academic appointment for year {model.Year} already exists.");

            if (model.FirstTermStart > model.FirstTermEnd)
                return CreateErrorResponse400BadRequest("First term start date cannot be after its end date.");
            if (model.SecondTermStart > model.SecondTermEnd)
                return CreateErrorResponse400BadRequest("Second term start date cannot be after its end date.");

            if (model.FirstTermStart > model.SecondTermStart)
                return CreateErrorResponse400BadRequest("First term cannot start after second term.");
            if (model.FirstTermEnd > model.SecondTermEnd)
                return CreateErrorResponse400BadRequest("First term cannot end after second term.");

            var newAcademicAppointment = new AcademicAppointment
            {
                Year = model.Year.Trim(),
                FirstTermStart = model.FirstTermStart,
                FirstTermEnd = model.FirstTermEnd,
                SecondTermStart = model.SecondTermStart,
                SecondTermEnd = model.SecondTermEnd
            };

            await _academicAppointmentRepository.AddAsync(newAcademicAppointment);
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, $"Academic appointment for year {model.Year} created successfully.", new { IsSuccess = true });
        }

        public async Task<ApiResponse> GetAllAcademicYearAppointmentsAsync()
        {
            var academicAppointments = await _academicAppointmentRepository.GetAllAsync();

            if (academicAppointments == null || !academicAppointments.Any())
                return CreateErrorResponse404NotFound("No academic appointments found.");


            var academicYearAppointments = academicAppointments.Select(a => new AcademicYearAppointmentDto
            {
                Id = a.Id,
                Year = a.Year,
                FirstTermStart = a.FirstTermStart,
                FirstTermEnd = a.FirstTermEnd,
                SecondTermStart = a.SecondTermStart,
                SecondTermEnd = a.SecondTermEnd,
                Status = a.Status
            })
            .OrderByDescending(a => a.Id);

            return new ApiResponse(200, "Academic appointments retrieved successfully.", new { IsSuccess = true, academicYearAppointments });
        }

        public async Task<ApiResponse> SetNewAcademicYearAppointmentAsync(SetActiveYearDto model)
        {
            if (model.AppointmentId <= 0)
                return CreateErrorResponse400BadRequest("Invalid appointment ID.");

            var academicAppointment = _academicAppointmentRepository.FindAsync(a => a.Id == model.AppointmentId);
            if (academicAppointment == null)
                return CreateErrorResponse404NotFound("Academic appointment not found.");

            var academicYearAppointment = await _academicAppointmentRepository.GetByIdAsync(academicAppointment.Id);

            if (academicYearAppointment.Status == "Active")
                return new ApiResponse(200, "Academic year is already active.", new { IsSuccess = true });

            academicYearAppointment.Status = "Active";
            academicYearAppointment.LastUpdatedAt = DateTime.Now;
            _academicAppointmentRepository.Update(academicYearAppointment);
            var remainingAcademicAppointments = await _academicAppointmentRepository.FindAllAsync(a => a.Id != model.AppointmentId);
            foreach (var appointment in remainingAcademicAppointments)
            {
                appointment.Status = "Inactive";
                _academicAppointmentRepository.Update(appointment);
            }
            await _unitOfWork.CompleteAsync();
            return new ApiResponse(200, $"This academic year appointment {academicYearAppointment.Year} set to active successfully.", new { IsSuccess = true });
        }

        private static ApiResponse CreateErrorResponse400BadRequest(string message)
        {
            return new ApiResponse(400, message, new { IsSuccess = false });
        }

        private static ApiResponse CreateErrorResponse404NotFound(string message)
        {
            return new ApiResponse(404, message, new { IsSuccess = false });
        }
    }
}
