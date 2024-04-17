namespace AppointmentManagementMicroservices.DTO
{
    public class PatientBookingRequestModel
    {
        public string Name { get; set; } // Name of the patient
        public string Issues { get; set; } // Issues or medical condition for which the patient is booking
        public int Age { get; set; } // Age of the patient
        public int DoctorId { get; set; } // ID of the doctor from HospitalManagementMicroservices
    }
}

