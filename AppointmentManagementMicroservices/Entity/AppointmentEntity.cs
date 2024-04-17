namespace AppointmentManagementMicroservices.Entity
{
    public class AppointmentEntity
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public int Age { get; set; }
        public string Issue { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
        public int BookedWithDoctor { get; set; }
        public String BookedByPatient { get; set; }
    }
}
