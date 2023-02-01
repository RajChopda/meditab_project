using Assignment.Models;

namespace Assignment.DataAccessLayer
{
    public interface IPatientDemographicsDAL
    {
        public Task<dynamic> GetPatientDataById(int id);
        public Task<PatientDemographicsList> GetPatientsData(RequestPatientData req);
        public Task<int> CreatePatient(PatientDemographics pd);
        public Task<int> UpdatePatient(int id, PatientDemographics pd);
        public Task<string> DeletePatient(int id);
    }
}
