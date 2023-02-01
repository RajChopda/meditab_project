using Assignment.Models;
using Assignment.ServicesLayer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace Assignment.Controllers
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
/*
        [HttpGet("testing")]
        public async Task<Stream> Index()
        {
            var response = await _httpClient.GetAsync("https://localhost:7139/api/patientdemographics/1");
            return await response.Content.ReadAsStreamAsync();
        }
*/
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
        [HttpPost("getAllPatientData")]
        public async Task<PatientDemographicsList> GetPatientsData(RequestPatientData req)
        {
            return await _patientDemographicsSL.GetPatientsData(req);
        }

        /// <summary>
        /// Add patient data
        /// </summary>
        /// <param name="pd"></param>
        /// <returns>id of new added patient</returns>
        [HttpPost]
        public async Task<int> CreatePatient(PatientDemographics pd)
        {
            return await _patientDemographicsSL.CreatePatient(pd);
        }

        /// <summary>
        /// Update patient data using id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pd"></param>
        /// <returns>id of updated patient</returns>
        [HttpPut("{id}")]
        public async Task<int> UpdatePatient(int id, PatientDemographics pd)
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
    }
}
