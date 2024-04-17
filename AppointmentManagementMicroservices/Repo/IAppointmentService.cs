using AppointmentManagementMicroservices.DTO;
using AppointmentManagementMicroservices.Entity;

namespace AppointmentManagementMicroservices.Repo
{
    public interface IAppointmentService
    {
        public Task<DoctorInfoDTO> GetDoctorInfoById(int doctorId);
        public Task<int> BookAppointment(AppointmentEntity appointment);
        public Task<List<AppointmentEntity>> GetAllAppointmentsForPatient(int patientId);

        public Task<AppointmentEntity> GetAppointmentById(int id);
        public Task<bool> UpdateAppointment(AppointmentEntity appointment);

        public Task<IEnumerable<AppointmentEntity>> GetAllAppointments();
        public Task UpdateAppointmentStatus(AppointmentEntity appointment);
    }
}
