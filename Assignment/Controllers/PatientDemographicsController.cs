using PatientDemographicsAPI.Models;
using PatientDemographicsAPI.ServicesLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

namespace PatientDemographicsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientDemographicsController : ControllerBase
    {
        public readonly IPatientDemographicsSL _patientDemographicsSL;
        //private readonly HttpClient _httpClient;

        public PatientDemographicsController(IPatientDemographicsSL patientDemographicsSL)
        {
            _patientDemographicsSL = patientDemographicsSL;
            //_httpClient= new HttpClient();
        }

        /// <summary>
        /// Get patient by id
        /// </summary>
        /// <param name="id">1</param>
        /// <returns>patiend data with id 1</returns>
        [HttpGet("{id}")]
        public async Task<dynamic> GetPatientDataById(int id)
        {
            return await _patientDemographicsSL.GetPatientDataById(id);
        }

        /// <summary>
        /// Get patients using custom filter
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GetPatientList")]
        public async Task<PatientDemographicsList> GetPatientList(RequestPatientData req)
        {
            return await _patientDemographicsSL.GetPatientList(req);
        }

        /// <summary>
        /// Add patient data
        /// </summary>
        /// <param name="createPatient"></param>
        /// <returns>id of new added patient</returns>
        [HttpPost]
        public async Task<int> CreatePatient(CreateUpdatePatient createPatient)
        {
            return await _patientDemographicsSL.CreatePatient(createPatient);
        }

        /// <summary>
        /// Update patient data using id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pd"></param>
        /// <returns>id of updated patient</returns>
        [HttpPut("{id}")]
        public async Task<int> UpdatePatient(int id, CreateUpdatePatient pd)
        {
            return await _patientDemographicsSL.UpdatePatient(id, pd);
        }

        /// <summary>
        /// Remove patient data using id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Confirmation message</returns>
        [HttpDelete("{id}")]
        public async Task<string> DeletePatient(int id)
        {
            return await _patientDemographicsSL.DeletePatient(id);
        }

        [HttpPatch("{id}")]
        public async Task<int> PatchPatient(int id, JsonPatchDocument patientDoc)
        {
            return await _patientDemographicsSL.PatchPatient(id, patientDoc);
            /*
            [
              {
                "path": "middleName",
                "op": "replace",
                "value": "G"
              },
              {
                "path": "allergyChangeLog/updated/-",
                "op": "replace",
                "value": {
                           "allergyId": 17,
                           "allergyMasterId": 3,
                            "note": "New Note"
                         }
              }
            ]
            */
        }
    }
}
