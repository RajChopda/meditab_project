namespace PatientDemographicsAPI.Models
{
    public interface PatientAllergyId
    {
        public int AllergyId { get; set; }
    }

    public class CreatePatientAllergy
    {
        public int AllergyMasterId { get; set; }
        public string? Note { get; set; } = null;
    }

    public class PatientAllergy : CreatePatientAllergy, PatientAllergyId
    {
        public int AllergyId { get; set; }
    }

    public class DeletePatientAllergy : PatientAllergyId
    {
        public int AllergyId { get; set; }
    }

    public class PatientAllergyList
    {
        public List<PatientAllergy>? AllergyList { get; set; } = null;
    }

    public class AllergyChangeLog
    {
        public List<CreatePatientAllergy> Created { get; set; } = new();
        public List<PatientAllergy> Updated { get; set; } = new();
        public List<DeletePatientAllergy> Deleted { get; set; } = new();
    }
}