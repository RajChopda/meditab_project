using Assignment.DataAccessLayer;
using Assignment.Models;

namespace Assignment.ServicesLayer
{
    public class PatientDemographicsSL:IPatientDemographicsSL
    {
        public readonly IPatientDemographicsDAL _assignmentDAL;
        public PatientDemographicsSL(IPatientDemographicsDAL assignmentDAL)
        {
            _assignmentDAL = assignmentDAL;
        }

        public async Task<dynamic> GetPatientDataById(int id)
        {
            return await _assignmentDAL.GetPatientDataById(id);
        }

        public async Task<PatientDemographicsList> GetPatientsData(RequestPatientData req)
        {
            return await _assignmentDAL.GetPatientsData(req);
        }

        public async Task<int> CreatePatient(PatientDemographics pd)
        {
            return await _assignmentDAL.CreatePatient(pd);
        }

        public async Task<int> UpdatePatient(int id, PatientDemographics pd)
        {
            return await _assignmentDAL.UpdatePatient(id, pd);
        }
        public async Task<string> DeletePatient(int id)
        {
            return await _assignmentDAL.DeletePatient(id);
        }
    }
}
