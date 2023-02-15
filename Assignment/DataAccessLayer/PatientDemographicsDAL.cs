using PatientDemographicsAPI.Models;
using Npgsql;
using System;
using System.Data;
using System.Transactions;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;

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
            return val == null ? DBNull.Value : val;
        }
        private static object CheckNullValue(string? val)
        {
            return val == null ? DBNull.Value : val;
        }
        private static object CheckNullValue(DateTime? val)
        {
            return val == null ? DBNull.Value : val;
        }

        /// <summary>
        /// Get patient by id
        /// </summary>
        /// <param name="id">1</param>
        /// <returns>patiend data of given id</returns>
        public async Task<dynamic> GetPatientDataById(int id)
        {
            PatientDemographicsList patientDataList = new()
            {
                PatientList = new List<PatientDemographics>()
            };
            PatientAllergyList? patientAllergyList = new()
            {
                AllergyList = new List<PatientAllergy>()
            };
            string patient_get_query = "select * from patientget(_patient_id=>"+id+")";
            string patient_allergy_get_query = "select * from getallergyofpatient(_patient_id=>"+id+")";
            NpgsqlDataReader myReader, myReader2;
            try
            {
                using (NpgsqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new(patient_get_query, myCon))
                    {
                        myReader = myCommand.ExecuteReader();
                        DataTable table = new();
                        table.Load(myReader);
                        if (table.Rows.Count == 0)
                        {
                            return await Task.FromResult(-1);
                        }



                        using (NpgsqlCommand myCommand2 = new(patient_allergy_get_query, myCon))
                        {
                            myReader2 = myCommand2.ExecuteReader();
                            DataTable table2 = new();
                            table2.Load(myReader2);
                            if (table2.Rows.Count != 0)
                            {
                                for (int i = 0; i < table2.Rows.Count; i++)
                                {
                                    DataRow dr2 = table2.Rows[i];
                                    patientAllergyList.AllergyList.Add(new PatientAllergy
                                    {
                                        AllergyId = (int)dr2["patient_allergy_id"],
                                        AllergyMasterId = (int)dr2["allergy_master_id"],
                                        Note = dr2["note"] == DBNull.Value ? null : dr2["note"].ToString()
                                    });
                                }
                            }
                            myReader2.Close();
                        }



                        DataRow dr = table.Rows[0];
                        patientDataList.PatientList.Add(new PatientDemographics
                        {
                            FirstName = dr["fname"].ToString(),
                            LastName = dr["lname"].ToString(),
                            MiddleName = dr["mname"] == DBNull.Value ? null : dr["mname"].ToString(),
                            //Dob = dr["dob"] == DBNull.Value ? null : dr["dob"].ToString().Split(" ")[0],
                            Dob = dr["dob"] == DBNull.Value ? null : Convert.ToDateTime(dr["dob"]).ToString("dd/MM/yyyy"),
                            SexTypeId = (int)dr["sex_type_id"],
                            ChartNo = dr["chart_no"].ToString(),
                            IsActive = Convert.ToBoolean(dr["is_active"]),
                            PatientAllergy = patientAllergyList.AllergyList
                        });
                        myReader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return await Task.FromResult(patientDataList);
        }

        /// <summary>
        /// Get patients using custom filter
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        ///
        public Task<PatientDemographicsList> GetPatientList(RequestPatientData req)
        {
            PatientDemographicsList patientData = new()
            {
                PatientList = new List<PatientDemographics>()
            };
            string patient_list_get_query = @"
                select * from patientget(_patient_id=>@id, _fname=>@fname, _lname=>@lname, _dob=>@dob::date, _sex_type_id=>@sextype,
                _pagenumber=>@pagenumber, _pagesize=>@pagesize, _orderby=>@orderby, _sorting=>@sorting, _allergy_master_id=>@allergymasterid)";
            /*
            string patient_list_get_query2 = "select * from patientget(_patient_id=>"+ CheckNullValue(req.PatientId) + ", _fname=>"+ CheckNullValue(req.FirstName) + ", _lname=>"+ CheckNullValue(req.LastName) +
                ", _dob=>"+ CheckNullValue(req.Dob) + ", _sex_type_id=>"+ CheckNullValue(req.SexTypeId) + ",_pagenumber=>"+ CheckNullValue(req.PageNumber) + ", _pagesize=>"+ CheckNullValue(req.PageSize) +
                ", _orderby=>"+ CheckNullValue(req.OrderBy) + ", _sorting=>"+ CheckNullValue(req.Sorting) + ", _allergy_master_id=>"+ CheckNullValue(req.AllergyMasterId) + ")";
            Console.WriteLine(patient_list_get_query2);
            */
            string patient_allergy_get_query = @"select * from getallergyofpatient(_patient_id=>@id)";
            DataTable table = new();
            NpgsqlDataReader myReader, myReader2;
            try
            {
                using (NpgsqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new(patient_list_get_query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("id", CheckNullValue(req.PatientId));
                        myCommand.Parameters.AddWithValue("fname", CheckNullValue(req.FirstName));
                        myCommand.Parameters.AddWithValue("lname", CheckNullValue(req.LastName));
                        myCommand.Parameters.AddWithValue("dob", CheckNullValue(req.Dob));
                        myCommand.Parameters.AddWithValue("sextype", CheckNullValue(req.SexTypeId));
                        myCommand.Parameters.AddWithValue("pagenumber", CheckNullValue(req.PageNumber));
                        myCommand.Parameters.AddWithValue("pagesize", CheckNullValue(req.PageSize));
                        myCommand.Parameters.AddWithValue("orderby", CheckNullValue(req.OrderBy));
                        myCommand.Parameters.AddWithValue("sorting", CheckNullValue(req.Sorting));
                        myCommand.Parameters.AddWithValue("allergymasterid", CheckNullValue(req.AllergyMasterId));
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);

                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            DataRow dr = table.Rows[i];

                            PatientAllergyList? patientAllergyList = new()
                            {
                                AllergyList = new List<PatientAllergy>()
                            };

                            using (NpgsqlCommand myCommand2 = new(patient_allergy_get_query, myCon))
                            {
                                myCommand2.Parameters.AddWithValue("id", (int)dr["patient_id"]);
                                myReader2 = myCommand2.ExecuteReader();
                                DataTable table2 = new();
                                table2.Load(myReader2);
                                if (table2.Rows.Count != 0)
                                {
                                    for (int j = 0; j < table2.Rows.Count; j++)
                                    {
                                        DataRow dr2 = table2.Rows[j];
                                        patientAllergyList.AllergyList.Add(new PatientAllergy
                                        {
                                            AllergyId = (int)dr2["patient_allergy_id"],
                                            AllergyMasterId = (int)dr2["allergy_master_id"],
                                            Note = dr2["note"] == DBNull.Value ? null : dr2["note"].ToString()
                                        });
                                    }
                                }
                                myReader2.Close();
                            }


                            patientData.PatientList.Add(new PatientDemographics
                            {
                                FirstName = dr["fname"].ToString(),
                                LastName = dr["lname"].ToString(),
                                MiddleName = dr["mname"] == DBNull.Value ? null : dr["mname"].ToString(),
                                //Dob = dr["dob"] == DBNull.Value ? null : dr["dob"].ToString().Split(" ")[0],
                                Dob = dr["dob"] == DBNull.Value ? null : Convert.ToDateTime(dr["dob"]).ToString("dd/MM/yyyy"),
                                SexTypeId = (int)dr["sex_type_id"],
                                ChartNo = dr["chart_no"].ToString(),
                                IsActive = Convert.ToBoolean(dr["is_active"]),
                                PatientAllergy = patientAllergyList.AllergyList
                            });
                        }

                        myReader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.FromResult(patientData);
        }

        /// <summary>
        /// Add patient data
        /// </summary>
        /// <param name="createPatient"></param>
        /// <returns>id of new added patient</returns>
        public Task<int> CreatePatient(CreateUpdatePatient createPatient)
        {
            string patientCreateQuery = @"
                select patientcreate(@fname, @mname, @lname, @dob::date, @sex_type_id)
            ";
            string allergyCreateQuery = @"select * from createpatientallergy(@patient_id, @allergy_master_id, @note)";
            int _id = 0;
            NpgsqlDataReader myReader;
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(patientCreateQuery, myCon))
                        {
                            myCommand.Parameters.AddWithValue("fname", createPatient.FirstName == null || createPatient.FirstName == "" ? throw new NoNullAllowedException() : createPatient.FirstName);
                            myCommand.Parameters.AddWithValue("mname", createPatient.MiddleName == null || createPatient.MiddleName == "" ? DBNull.Value : createPatient.MiddleName);
                            myCommand.Parameters.AddWithValue("lname", createPatient.LastName == null || createPatient.LastName == "" ? throw new NoNullAllowedException() : createPatient.LastName);
                            myCommand.Parameters.AddWithValue("dob", createPatient.Dob == null || createPatient.Dob.ToString() == "" ? DBNull.Value : createPatient.Dob);
                            myCommand.Parameters.AddWithValue("sex_type_id", createPatient.SexTypeId == null || createPatient.SexTypeId == 0 ? throw new NoNullAllowedException() : createPatient.SexTypeId);

                            myReader = myCommand.ExecuteReader();
                            myReader.Read();
                            _id = (int)myReader[0];
                            myReader.Close();


                            for (int i = 0; i < createPatient.AllergyChangeLog.Created.Count; i++)
                            {
                                NpgsqlDataReader myReader2;
                                using (NpgsqlCommand myCommand2 = new(allergyCreateQuery, myCon))
                                {
                                    myCommand2.Parameters.AddWithValue("patient_id", _id);
                                    myCommand2.Parameters.AddWithValue("allergy_master_id", createPatient.AllergyChangeLog.Created[i].AllergyMasterId);
                                    myCommand2.Parameters.AddWithValue("note", createPatient.AllergyChangeLog.Created[i].Note == null || createPatient.AllergyChangeLog.Created[i].Note == "" ? DBNull.Value : createPatient.AllergyChangeLog.Created[i].Note);
                                    myReader2 = myCommand2.ExecuteReader();
                                    myReader2.Close();
                                }
                            }
                            transactionScope.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex.Message);
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
            string patientUpdateQuery = @"
                 select patientupdate(@id, @fname, @mname, @lname, @dob::date, @sex_type_id)
            ";
            string allergyCreateQuery = @"select * from createpatientallergy(@patient_id, @allergy_master_id, @note)";
            string allergyUpadateQuery = @"select updatepatientallergy(@patient_id, @patient_allergy_id, @allergy_master_id, @note);";
            string allergyDeleteQuery = @"select deletepatientallergy(@patient_id, @patient_allergy_id);";
            int _id = 0;

            NpgsqlDataReader myReader;
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(patientUpdateQuery, myCon))
                        {
                            myCommand.Parameters.AddWithValue("id", id);
                            myCommand.Parameters.AddWithValue("fname", updatePatient.FirstName == null || updatePatient.FirstName == "" ? throw new NoNullAllowedException() : updatePatient.FirstName);
                            myCommand.Parameters.AddWithValue("mname", updatePatient.MiddleName == null || updatePatient.MiddleName == "" ? DBNull.Value : updatePatient.MiddleName);
                            myCommand.Parameters.AddWithValue("lname", updatePatient.LastName == null || updatePatient.LastName == "" ? throw new NoNullAllowedException() : updatePatient.LastName);
                            myCommand.Parameters.AddWithValue("dob", updatePatient.Dob == null || updatePatient.Dob.ToString() == "" ? DBNull.Value : updatePatient.Dob);
                            myCommand.Parameters.AddWithValue("sex_type_id", updatePatient.SexTypeId == null || updatePatient.SexTypeId == 0 ? throw new NoNullAllowedException() : updatePatient.SexTypeId);

                            myReader = myCommand.ExecuteReader();
                            myReader.Read();
                            _id = (int)myReader[0];
                            myReader.Close();

                            for (int i = 0; i < updatePatient.AllergyChangeLog.Created.Count; i++)
                            {
                                NpgsqlDataReader myReader2;
                                using (NpgsqlCommand myCommand2 = new(allergyCreateQuery, myCon))
                                {
                                    myCommand2.Parameters.AddWithValue("patient_id", _id);
                                    myCommand2.Parameters.AddWithValue("allergy_master_id", updatePatient.AllergyChangeLog.Created[i].AllergyMasterId);
                                    myCommand2.Parameters.AddWithValue("note", updatePatient.AllergyChangeLog.Created[i].Note == null || updatePatient.AllergyChangeLog.Created[i].Note == "" ? DBNull.Value : updatePatient.AllergyChangeLog.Created[i].Note);
                                    myReader2 = myCommand2.ExecuteReader();
                                    myReader2.Close();
                                }
                            }
                            for (int i = 0; i < updatePatient.AllergyChangeLog.Updated.Count; i++)
                            {
                                NpgsqlDataReader myReader2;
                                using (NpgsqlCommand myCommand2 = new(allergyUpadateQuery, myCon))
                                {
                                    myCommand2.Parameters.AddWithValue("patient_id", _id);
                                    myCommand2.Parameters.AddWithValue("patient_allergy_id", updatePatient.AllergyChangeLog.Updated[i].AllergyId);
                                    myCommand2.Parameters.AddWithValue("allergy_master_id", updatePatient.AllergyChangeLog.Updated[i].AllergyMasterId);
                                    myCommand2.Parameters.AddWithValue("note", updatePatient.AllergyChangeLog.Updated[i].Note == null || updatePatient.AllergyChangeLog.Updated[i].Note == "" ? DBNull.Value : updatePatient.AllergyChangeLog.Updated[i].Note);
                                    myReader2 = myCommand2.ExecuteReader();
                                    myReader2.Close();
                                }
                            }
                            for (int i = 0; i < updatePatient.AllergyChangeLog.Deleted.Count; i++)
                            {
                                using (NpgsqlCommand myCommand2 = new(allergyDeleteQuery, myCon))
                                {
                                    myCommand2.Parameters.AddWithValue("patient_id", _id);
                                    myCommand2.Parameters.AddWithValue("patient_allergy_id", updatePatient.AllergyChangeLog.Deleted[i].AllergyId);
                                    myCommand2.ExecuteReader();
                                }
                            }
                            transactionScope.Complete();
                        }
                    }
                    return await Task.FromResult(_id);
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex.Message);
                    return await Task.FromResult(0);
                }
            }
        }

        /// <summary>
        /// Remove patient data using id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Confirmation message</returns>
        public Task<string> DeletePatient(int id)
        {
            string patientDeleteQuery = "select patientdelete("+id+")";
            string allAllergyDeleteQuery = "select deleteallpatientallergy("+id+")";
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(allAllergyDeleteQuery, myCon))
                        {
                            myCommand.Parameters.AddWithValue("id", id);
                            myCommand.ExecuteReader();
                        }
                    }
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(patientDeleteQuery, myCon))
                        {
                            myCommand.Parameters.AddWithValue("id", id);
                            myCommand.ExecuteReader();

                        }
                    }
                    transactionScope.Complete();
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex.Message);
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
    }
}
