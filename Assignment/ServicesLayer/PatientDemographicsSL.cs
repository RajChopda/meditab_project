using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using PatientDemographicsAPI.DataAccessLayer;
using PatientDemographicsAPI.Models;

namespace PatientDemographicsAPI.ServicesLayer
{
    public class PatientDemographicsSL:IPatientDemographicsSL
    {
        public readonly IPatientDemographicsDAL _patientDemographicsDAL;
        public PatientDemographicsSL(IPatientDemographicsDAL patientDemographicsDAL)
        {
            _patientDemographicsDAL = patientDemographicsDAL;
        }

        public async Task<dynamic> GetPatientDataById(int id)
        {
            return await _patientDemographicsDAL.GetPatientDataById(id);
        }

        public async Task<List<PatientDemographics>> GetPatientList(RequestPatientData req)
        {
            return await _patientDemographicsDAL.GetPatientList(req);
        }

        public async Task<int> CreatePatient(CreateUpdatePatient createPatient)
        {
            return await _patientDemographicsDAL.CreatePatient(createPatient);
        }

        public async Task<int> UpdatePatient(int id, CreateUpdatePatient pd)
        {
            return await _patientDemographicsDAL.UpdatePatient(id, pd);
        }
        public async Task<string> DeletePatient(int id)
        {
            return await _patientDemographicsDAL.DeletePatient(id);
        }
        public async Task<int> PatchPatient(int id, JsonPatchDocument patientDoc)
        {
            return await _patientDemographicsDAL.PatchPatient(id, patientDoc);
        }
        public Task<List<PatientDemographics>> Testing(RequestPatientData req)
        {
            return _patientDemographicsDAL.Testing(req);
        }
    }
}
