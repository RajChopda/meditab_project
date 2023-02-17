namespace PatientDemographicsAPI.Models
{
    public class DeletePatientAllergy
    {
        public int AllergyId { get; set; }
    }
    public class PatientAllergy : DeletePatientAllergy
    {
        public int AllergyMasterId { get; set; }
        public string? Note { get; set; } = null;
    }

    public class PatientAllergyList
    {
        public List<PatientAllergy>? AllergyList { get; set; } = null;
    }

    public class AllergyChangeLog
    {
        public List<PatientAllergy> Created { get; set; } = new();
        public List<PatientAllergy> Updated { get; set; } = new();
        public List<DeletePatientAllergy> Deleted { get; set; } = new();
    }
}