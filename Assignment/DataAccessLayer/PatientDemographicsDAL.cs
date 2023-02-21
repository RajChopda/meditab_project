using PatientDemographicsAPI.Models;
using Npgsql;
using System.Data;
using System.Transactions;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace PatientDemographicsAPI.DataAccessLayer
{
    public class PatientDemographicsDAL : IPatientDemographicsDAL
    {
        private readonly IConfiguration _configuration;
        private readonly string sqlDataSource;
        public PatientDemographicsDAL(IConfiguration configuration)
        {
            _configuration = configuration;
            sqlDataSource = _configuration.GetConnectionString("DBcon");
        }
        private static object CheckNullValue(int? val)
        {
            return val == null ? "null" : val;
        }
        private static object CheckNullValue(string? val)
        {
            return string.IsNullOrEmpty(val) ? "null" : "'" + val + "'";
        }
        private static object CheckNullForNoNullValue(int? val)
        {
            return val == null || val <= 0 ? throw new NoNullAllowedException() : val;
        }
        private static object CheckNullForNoNullValue(string? val)
        {
            return string.IsNullOrEmpty(val) ? throw new NoNullAllowedException() : "'" + val + "'";
        }

        /// <summary>
        /// Get patient by id
        /// </summary>
        /// <param name="id">1</param>
        /// <returns>patiend data of given id</returns>
        public async Task<dynamic> GetPatientDataById(int id)
        {
            List<PatientDemographics> patients = new();
            string patient_get_query = "select * from getpatientwithallergy(_patient_id=>" + id + ")";
            try
            {
                using (NpgsqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    var patientdata = (await myCon.QueryAsync<PatientDemographics, PatientAllergy, PatientDemographics>(patient_get_query,
                                                                                                                            (patient, allergy) => {
                                                                                                                                if (allergy != null)
                                                                                                                                    patient.PatientAllergy.Add(allergy);
                                                                                                                                return patient;
                                                                                                                            }, splitOn: "PatientAllergyId"))
                                                                                                                        .GroupBy(patient => patient.PatientId);
                    Console.WriteLine(patientdata.Count());

                        PatientDemographics patient = patientdata.ElementAt(0).ElementAt(1);
                        foreach (PatientDemographics patientrecord in patientdata.ElementAt(0).Skip(1))
                        {
                            patient.PatientAllergy.Add(patientrecord.PatientAllergy[0]);
                        }
                        patients.Add(patient);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return patients;
        }

        /// <summary>
        /// Get patients using custom filter
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        ///
        public async Task<List<PatientDemographics>> GetPatientList(RequestPatientData req)
        {
            string patient_list_get_query = "select * from getpatientwithallergy(" +
                                            "_patient_id=>" + CheckNullValue(req.PatientId) +
                                            ", _fname=>" + CheckNullValue(req.FirstName) +
                                            ", _lname=>" + CheckNullValue(req.LastName) +
                                            ", _dob=>" + CheckNullValue(req.Dob) +
                                            ", _sex_type_id=>" + CheckNullValue(req.SexTypeId) +
                                            ", _pagenumber=>" + CheckNullValue(req.PageNumber) +
                                            ", _pagesize=>" + CheckNullValue(req.PageSize) +
                                            ", _orderby=>" + CheckNullValue(req.OrderBy) +
                                            ", _sorting=>" + CheckNullValue(req.Sorting) +
                                            ", _allergy_master_id=>" + CheckNullValue(req.AllergyMasterId) +
                                            ")";
            List<PatientDemographics> patients = new();
            try
            {
                using (NpgsqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    var patientdata = (await myCon.QueryAsync<PatientDemographics, PatientAllergy, PatientDemographics>(patient_list_get_query,
                                                                                                                            (patient, allergy) => {
                                                                                                                                if (allergy != null)
                                                                                                                                    patient.PatientAllergy.Add(allergy);
                                                                                                                                return patient;
                                                                                                                            }, splitOn: "PatientAllergyId"))
                                                                                                                        .GroupBy(patient => patient.PatientId);

                    foreach (IGrouping<int, PatientDemographics> patientGroup in patientdata)
                    {
                        PatientDemographics patient = patientGroup.First();
                        foreach (PatientDemographics patientrecord in patientGroup.Skip(1))
                        {
                            patient.PatientAllergy.Add(patientrecord.PatientAllergy[0]);
                        }
                        patients.Add(patient);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return patients;
        }

        /// <summary>
        /// Add patient data
        /// </summary>
        /// <param name="createPatient"></param>
        /// <returns>id of new added patient</returns>
        public Task<int> CreatePatient(CreateUpdatePatient createPatient)
        {
            string patientCreateQuery = "select patientcreate(" +
                                        "_fname=>" + CheckNullForNoNullValue(createPatient.FirstName) +
                                        ", _mname=>" + CheckNullValue(createPatient.MiddleName) +
                                        ", _lname=>" + CheckNullForNoNullValue(createPatient.LastName) +
                                        ", _dob=>" + CheckNullValue(createPatient.Dob) +
                                        ", _sex_type_id=>" + CheckNullForNoNullValue(createPatient.SexTypeId) +
                                        ")";
            int _id = 0;
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        _id = myCon.Query<int>(patientCreateQuery).First();
                        for (int i = 0; i < createPatient.AllergyChangeLog.Created.Count; i++)
                        {
                            string allergyCreateQuery = "select * from createpatientallergy(" +
                                                        "_patient_id=>" + _id +
                                                        ", _allergy_master_id=>" + createPatient.AllergyChangeLog.Created[i].AllergyMasterId +
                                                        ", _note=>" + CheckNullValue(createPatient.AllergyChangeLog.Created[i].Note) + ")";
                            myCon.Query(allergyCreateQuery);

                        }
                    }
                        transactionScope.Complete();
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex);
                }
            }
            return Task.FromResult(_id);
        }

        /// <summary>
        /// Update patient data using id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatePatient"></param>
        /// <returns>id of updated patient</returns>
        public async Task<int> UpdatePatient(int id, CreateUpdatePatient updatePatient)
        {
            string patientUpdateQuery = "select patientupdate(" +
                                        "_patient_id=>" + id +
                                        ", _fname=>" + CheckNullForNoNullValue(updatePatient.FirstName) +
                                        ", _mname=>" + CheckNullValue(updatePatient.MiddleName) +
                                        ", _lname=>" + CheckNullForNoNullValue(updatePatient.LastName) +
                                        ", _dob=>" + CheckNullValue(updatePatient.Dob) +
                                        ", _sex_type_id=>" + CheckNullForNoNullValue(updatePatient.SexTypeId) +
                                        ")";
            int _id = 0;

            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        _id = myCon.Query<int>(patientUpdateQuery).First();

                        foreach (PatientAllergy allergy in updatePatient.AllergyChangeLog.Created)
                        {
                            string allergyCreateQuery = "select * from createpatientallergy(" +
                                                        "_patient_id=>" + _id +
                                                        ", _allergy_master_id=>" + allergy.AllergyMasterId +
                                                        ", _note=>" + CheckNullValue(allergy.Note) +
                                                        ")";
                            myCon.Query(allergyCreateQuery);
                        }
                        foreach (PatientAllergy allergy in updatePatient.AllergyChangeLog.Updated)
                        {
                            string allergyUpadateQuery = "select updatepatientallergy(" +
                                                        "_patient_id=>" + _id +
                                                        ", _patient_allergy_id=>" + allergy.PatientAllergyId +
                                                        ", _allergy_master_id=>" + allergy.AllergyMasterId +
                                                        ", _note=>" + CheckNullValue(allergy.Note) +
                                                        ")";
                            myCon.Query(allergyUpadateQuery);
                        }
                        foreach (DeletePatientAllergy allergy in updatePatient.AllergyChangeLog.Deleted)
                        {
                            string allergyDeleteQuery = "select deletepatientallergy(" +
                                                        "_patient_id=>" + _id +
                                                        ", _patient_allergy_id=>" + allergy.PatientAllergyId +
                                                        ");";
                            myCon.Query(allergyDeleteQuery);
                        }
                        transactionScope.Complete();
                    }
                    return await Task.FromResult(_id);
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex);
                    return await Task.FromResult(0);
                }
            }
        }

        /// <summary>
        /// Remove patient data along with all patient allergy of that patient using id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Confirmation message</returns>
        public Task<string> DeletePatient(int id)
        {
            string patientDeleteQuery = "select patientdelete(" + id + ")";
            string allAllergyDeleteQuery = "select deleteallpatientallergy(" + id + ")";
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        myCon.Query(patientDeleteQuery);
                        myCon.Query(allAllergyDeleteQuery);
                    }
                    transactionScope.Complete();
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex);
                }
                return Task.FromResult("Deleted Sccessfully!");
            }
        }

        /// <summary>
        /// Update some part of the data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patientDoc"></param>
        /// <returns></returns>
        public async Task<int> PatchPatient(int id, JsonPatchDocument patientDoc)
        {
            dynamic patientData = await GetPatientDataById(id);

            if (patientData.GetType() == typeof(int))
            {
                return -1;
            }
            CreateUpdatePatient updatePatient = JsonConvert.DeserializeObject<CreateUpdatePatient>(JsonConvert.SerializeObject(patientData.PatientList[0]));
            patientDoc.ApplyTo(updatePatient);
            return await UpdatePatient(id, updatePatient);
        }
        
        /// <summary>
        /// Testing
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<List<PatientDemographics>> Testing(RequestPatientData req)
        {
            string patient_list_get_query = "select * from getpatientwithallergy(" +
                                            "_patient_id=>" + CheckNullValue(req.PatientId) +
                                            ", _fname=>" + CheckNullValue(req.FirstName) +
                                            ", _lname=>" + CheckNullValue(req.LastName) +
                                            ", _dob=>" + CheckNullValue(req.Dob) +
                                            ", _sex_type_id=>" + CheckNullValue(req.SexTypeId) +
                                            ", _pagenumber=>" + CheckNullValue(req.PageNumber) +
                                            ", _pagesize=>" + CheckNullValue(req.PageSize) +
                                            ", _orderby=>" + CheckNullValue(req.OrderBy) +
                                            ", _sorting=>" + CheckNullValue(req.Sorting) +
                                            ", _allergy_master_id=>" + CheckNullValue(req.AllergyMasterId) +
                                            ")";

            using (NpgsqlConnection myCon = new(sqlDataSource))
            {
                myCon.Open();
                var patientdata = (await myCon.QueryAsync<PatientDemographics, PatientAllergy, PatientDemographics>(patient_list_get_query,
                                                                                                                        (patient, allergy) => {
                                                                                                                        if(allergy != null)
                                                                                                                            patient.PatientAllergy.Add(allergy);
                                                                                                                        return patient;
                                                                                                                    }, splitOn: "PatientAllergyId"))
                                                                                                                    .GroupBy(patient => patient.PatientId);

                List<PatientDemographics> patients = new();
                foreach (IGrouping<int, PatientDemographics> patientGroup in patientdata)
                {
                    PatientDemographics patient =  patientGroup.First();
                    foreach(PatientDemographics patientrecord in patientGroup.Skip(1))
                    {
                        patient.PatientAllergy.Add(patientrecord.PatientAllergy[0]);
                    }
                    patients.Add(patient);
                }

/*                    
                foreach (var patient in patientdata)
                {
                    if (dr["patient_allergy_id"] != DBNull.Value)
                    {
                        patientAllergyList.AllergyList.Add(new PatientAllergy
                        {
                            PatientAllergyId = (int)dr["patient_allergy_id"],
                            AllergyMasterId = (int)dr["allergy_master_id"],
                            Note = dr["note"] == DBNull.Value ? "!" : dr["note"].ToString()
                        });
                    }

                    if (table.Rows.IndexOf(dr) == table.Rows.Count - 1 || (int)dr["patient_id"] != (int)table.Rows[table.Rows.IndexOf(dr) + 1]["patient_id"])
                    {
                        patientData.PatientList.Add(new PatientDemographics
                        {
                            Patient_Id = (int)dr["patient_id"],
                            FirstName = dr["fname"].ToString(),
                            LastName = dr["lname"].ToString(),
                            MiddleName = dr["mname"] == DBNull.Value ? null : dr["mname"].ToString(),
                            Dob = dr["dob"] == DBNull.Value ? null : Convert.ToDateTime(dr["dob"]).ToString("dd/MM/yyyy"),
                            SexTypeId = (int)dr["sex_type_id"],
                            ChartNo = dr["chart_no"].ToString(),
                            IsActive = Convert.ToBoolean(dr["is_active"]),
                            PatientAllergy = patientAllergyList.AllergyList
                        });
                        patientAllergyList = new()
                        {
                            AllergyList = new List<PatientAllergy>()
                        };
                    }
                }
*/

                return patients;
            }
        }
    }
}
