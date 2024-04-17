using AppointmentManagementMicroservices.Context;
using AppointmentManagementMicroservices.DTO;
using AppointmentManagementMicroservices.Entity;
using Dapper;
using System;
using System.Threading.Tasks;

namespace AppointmentManagementMicroservices.Repo
{
    public class AppointmentRepository : IAppointmentService
    {
        private readonly DapperContext _context;

        public AppointmentRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AppointmentEntity>> GetAllAppointments()
        {
            var sql = "SELECT * FROM Appointments";
            return await _context.CreateConnection().QueryAsync<AppointmentEntity>(sql);
        }
        public async Task<int> BookAppointment(AppointmentEntity appointment)
        {
            var sql = "INSERT INTO Appointments (PatientName, Age, Issue, DoctorName, Specialization, Date, Status, BookedWithDoctor, BookedByPatient) " +
                      "VALUES (@PatientName, @Age, @Issue, @DoctorName, @Specialization, @Date, @Status, @BookedWithDoctor, @BookedByPatient)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteAsync(sql, appointment);
        }

        public async Task<DoctorInfoDTO> GetDoctorInfoById(int doctorId)
        {
            var doctorApiUrl = $"https://localhost:7181/api/doctors/{doctorId}";
            var client = new HttpClient(); // You can inject HttpClient or use IHttpClientFactory
            var response = await client.GetAsync(doctorApiUrl);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to retrieve doctor information.");
            }

            return await response.Content.ReadFromJsonAsync<DoctorInfoDTO>();
        }

        public async Task<List<AppointmentEntity>> GetAllAppointmentsForPatient(int patientId)
        {
            var sql = "SELECT * FROM Appointments WHERE BookedByPatient = @PatientId";
            var appointments = await _context.CreateConnection().QueryAsync<AppointmentEntity>(sql, new { PatientId = patientId });

            // Explicitly convert appointments to a list
            return appointments.ToList();
        }

        public async Task<AppointmentEntity> GetAppointmentById(int id)
        {
            var sql = "SELECT * FROM Appointments WHERE Id = @Id";
            return await _context.CreateConnection().QueryFirstOrDefaultAsync<AppointmentEntity>(sql, new { Id = id });
        }

        public async Task<bool> UpdateAppointment(AppointmentEntity appointment)
        {
            var sql = "UPDATE Appointments SET PatientName = @PatientName, Age = @Age, Issue = @Issue ,BookedWithDoctor =@BookedWithDoctor " +
                      "WHERE Id = @Id";
            var affectedRows = await _context.CreateConnection().ExecuteAsync(sql, appointment);
            return affectedRows > 0;
        }
        public async Task UpdateAppointmentStatus(AppointmentEntity appointment)
        {
            var sql = "UPDATE Appointments SET Status = @Status WHERE Id = @Id";
            await _context.CreateConnection().ExecuteAsync(sql, new { appointment.Status, appointment.Id });
        }
    }
}
