using AppointmentManagementMicroservices.DTO;
using AppointmentManagementMicroservices.Entity;
using AppointmentManagementMicroservices.Repo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AppointmentManagementMicroservices.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IHttpClientFactory clientFactory, IAppointmentService appointmentService)
        {
            _clientFactory = clientFactory;
            _appointmentService = appointmentService;
        }
        [HttpGet("allAppointements")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointments();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("booking")]
        [Authorize]
        public async Task<IActionResult> PatientBookingForADoctor(PatientBookingRequestModel request)
        {
            // Get the user's ID from the token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Check if the user ID is valid
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var doctorInfo = await _appointmentService.GetDoctorInfoById(request.DoctorId);

            var appointment = new AppointmentEntity
            {
                PatientName = request.Name,
                Age = request.Age,
                Issue = request.Issues,
                DoctorName = doctorInfo.Name,
                Specialization = doctorInfo.Specialization,
                Date = DateTime.UtcNow, // Assuming you set the date to current UTC time
                Status = "Booked", // Assuming you set the status to "Booked"
                BookedWithDoctor = request.DoctorId, // Assuming the doctor's ID is stored in doctorInfo.Id
                BookedByPatient = userId // Assigning the user's ID from the token
            };

            await _appointmentService.BookAppointment(appointment);

            var bookingResponse = new PatientBookingResponseModel
            {
                Name = request.Name,
                Issues = request.Issues,
                Age = request.Age,
                DoctorName = doctorInfo.Name,
                Specialization = doctorInfo.Specialization
            };

            return Ok(bookingResponse);

        }

        [HttpGet("appointments/{patientId}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetAllAppointmentsForPatient(int patientId)
        {
            var appointments = await _appointmentService.GetAllAppointmentsForPatient(patientId);

            if (appointments == null || !appointments.Any())
            {
                return NotFound("No appointments found for the patient.");
            }

            var appointmentDTOs = appointments.Select(a => new AppointmentDTO
            {
                Id = a.Id,
                PatientName = a.PatientName,
                Age = a.Age,
                Issue = a.Issue,
                DoctorName = a.DoctorName,
                Specialization = a.Specialization,
                Date = a.Date,
                Status = a.Status
            }).ToList();

            return Ok(appointmentDTOs);
        }

        [HttpPut("update/{appointmentId}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> UpdateAppointment(int appointmentId, PatientBookingRequestModel request)
        {
            try
            {
                // Get the user's ID from the token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Check if the user ID is valid
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token.");
                }

                // Check if the appointment exists
                var existingAppointment = await _appointmentService.GetAppointmentById(appointmentId);
                if (existingAppointment == null)
                {
                    return NotFound("Appointment not found.");
                }

                // Check if the appointment is booked by the current user
                if (existingAppointment.BookedByPatient != userId)
                {
                    return Unauthorized("You are not authorized to update this appointment.");
                }

                // Update the appointment details
                existingAppointment.Age = request.Age;
                existingAppointment.Issue = request.Issues;
                existingAppointment.BookedWithDoctor = request.DoctorId;



                await _appointmentService.UpdateAppointment(existingAppointment);

                return Ok("Appointment updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("appointments/{appointmentId}/status")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateAppointmentStatus(int appointmentId, UpdateAppointmentStatusRequestModel request)
        {
            // Get the user's role from the token
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check if the user has the Doctor role
            if (!string.Equals(userRole, "Doctor", StringComparison.OrdinalIgnoreCase))
            {
                return Forbid("Only doctors are allowed to update appointment status.");
            }

            var appointment = await _appointmentService.GetAppointmentById(appointmentId);

            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            appointment.Status = request.NewStatus;

            await _appointmentService.UpdateAppointmentStatus(appointment);

            return Ok("Appointment status updated successfully.");
        }



    }
}
