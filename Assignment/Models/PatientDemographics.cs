namespace Assignment.Models
{
    /// <summary>
    /// patient list to add each patient info
    /// </summary>
    public class PatientDemographicsList
    {
        public List<PatientDemographics>? PatientList { get; set; }
    }

    /// <summary>
    /// patient data responce format
    /// </summary>
    public class PatientDemographics
    {
        public string? FirstName { get; set; } = null;
        public string? MiddleName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public DateTime? Dob { get; set; } = null;
        public int? SexTypeId { get; set; } = null;
        public string? ChartNo { get; set; } = null;
    }

    /// <summary>
    /// patient data request format for custom filter
    /// </summary>
    public class RequestPatientData
    {
        public int? PatientId { get; set; } = null;
        public string? FirstName { get; set; } = null;
        public string? LastName { get; set; } = null;
        public DateTime? Dob { get; set; } = null;
        public int? SexTypeId { get; set; } = null;
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 3;
        public string? OrderBy { get; set; } = null;
        public string? Sorting { get; set; } = null;
    }
}
