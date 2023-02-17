namespace PatientDemographicsAPI.Models
{
    /// <summary>
    /// patient data model having basic data member
    /// </summary>
    public class PatientDataModel
    {
        public int? PatientId { get; set; } = null;

        public string? FirstName { get; set; } = null;
        public string? MiddleName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public string? Dob { get; set; } = null;
        public int? SexTypeId { get; set; } = null;
        public bool? IsActive { get; set; } = null;
    }

    /// <summary>
    /// create patient model
    /// </summary>
    public class CreateUpdatePatient : PatientDataModel
    {
        public AllergyChangeLog AllergyChangeLog { get; set; } = new();
    }

    /// <summary>
    /// patient data responce format
    /// </summary>
    public class PatientDemographics : PatientDataModel
    {
        public string? ChartNo { get; set; } = null;
        public List<PatientAllergy>? PatientAllergy { get; set; } = null;
    }

    /// <summary>
    /// patient list to add each patient info
    /// </summary>
    public class PatientDemographicsList
    {
        public List<PatientDemographics>? PatientList { get; set; }
    }

    /// <summary>
    /// patient data request format for custom filter
    /// </summary>
    public class RequestPatientData
    {
        public int? PatientId { get; set; } = null;
        public string? FirstName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public string? Dob { get; set; } = null;
        public int? SexTypeId { get; set; } = null;
        public int? PageNumber { get; set; } = null;
        public int? PageSize { get; set; } = null;
        public string? OrderBy { get; set; } = null;
        public string? Sorting { get; set; } = null;
        public int? AllergyMasterId { get; set; } = null;

    }
}
