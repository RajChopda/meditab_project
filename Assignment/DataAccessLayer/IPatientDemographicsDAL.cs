using Microsoft.AspNetCore.JsonPatch;
using PatientDemographicsAPI.Models;

namespace PatientDemographicsAPI.DataAccessLayer
{
    public interface IPatientDemographicsDAL
    {
        public Task<dynamic> GetPatientDataById(int id);
        public Task<PatientDemographicsList> GetPatientList(RequestPatientData req);
        public Task<int> CreatePatient(CreateUpdatePatient createPatient);
        public Task<int> UpdatePatient(int id, CreateUpdatePatient pd);
        public Task<string> DeletePatient(int id);
        public Task<int> PatchPatient(int id, JsonPatchDocument patientDoc);
    }
}
