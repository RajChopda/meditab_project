using Assignment.Models;
using Npgsql;
using System;
using System.Data;
using System.Transactions;

namespace Assignment.DataAccessLayer
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

        /// <summary>
        /// Get patient by id
        /// </summary>
        /// <param name="id">1</param>
        /// <returns>patiend data of given id</returns>
        public async Task<dynamic> GetPatientDataById(int id)
        {
            PatientDemographicsList patientData = new()
            {
                PatientList = new List<PatientDemographics>()
            };
            DataTable table = new();
            string query = @"
                select * from patientget(_patient_id=>@id)";
            NpgsqlDataReader myReader;
            try
            {
                using (NpgsqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new(query, myCon))
                    {
                        myCommand.Parameters.AddWithValue("id", id);
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);
                        if(table.Rows.Count == 0)
                        {
                            return await Task.FromResult(-1);
                        }
                        DataRow dr = table.Rows[0];
                        patientData.PatientList.Add(new PatientDemographics
                        {
                            FirstName = dr["fname"].ToString(),
                            LastName = dr["lname"].ToString(),
                            MiddleName = dr["mname"] == DBNull.Value ? null : dr["mname"].ToString(),
                            Dob = dr["dob"] == DBNull.Value ? null : Convert.ToDateTime(dr["dob"].ToString()),
                            SexTypeId = (int)dr["sex_type_id"],
                            ChartNo = dr["chart_no"].ToString()
                        });
                        myReader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return await Task.FromResult(patientData);
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
        /// Get patients using custom filter
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        ///
        public Task<PatientDemographicsList> GetPatientsData(RequestPatientData req)
        {
            PatientDemographicsList patientData = new()
            {
                PatientList = new List<PatientDemographics>()
            };
            string query = @"
                select * from patientget(_patient_id=>@id, _fname=>@fname, _lname=>@lname, _dob=>@dob, _sex_type_id=>@sextype,
                _pagenumber=>@pagenumber, _pagesize=>@pagesize, _orderby=>@orderby, _sorting=>@sorting)";

            DataTable table = new();
            NpgsqlDataReader myReader;
            try
            {
                using (NpgsqlConnection myCon = new(sqlDataSource))
                {
                    myCon.Open();
                    using (NpgsqlCommand myCommand = new(query, myCon))
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
                        myReader = myCommand.ExecuteReader();
                        table.Load(myReader);

                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            DataRow dr = table.Rows[i];
                            patientData.PatientList.Add(new PatientDemographics
                            {
                                FirstName = dr["fname"].ToString(),
                                LastName = dr["lname"].ToString(),
                                MiddleName = dr["mname"] == DBNull.Value ? null : dr["mname"].ToString(),
                                Dob = dr["dob"] == DBNull.Value ? null : Convert.ToDateTime(dr["dob"].ToString()),
                                SexTypeId = (int)dr["sex_type_id"],
                                ChartNo = dr["chart_no"].ToString()
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
        /// <param name="pd"></param>
        /// <returns>id of new added patient</returns>
        public Task<int> CreatePatient(PatientDemographics pd)
        {
            string query = @"
                select patientcreate(@fname, @mname, @lname, @dob::date, @sex_type_id)
            ";
            int _id = 0;
            NpgsqlDataReader myReader;
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(query, myCon))
                        {
                            myCommand.Parameters.AddWithValue("fname", pd.FirstName == null || pd.FirstName == "" ? throw new NoNullAllowedException() : pd.FirstName);
                            myCommand.Parameters.AddWithValue("mname", pd.MiddleName == null || pd.MiddleName == "" ? DBNull.Value : pd.MiddleName);
                            myCommand.Parameters.AddWithValue("lname", pd.LastName == null || pd.LastName == "" ? throw new NoNullAllowedException() : pd.LastName);
                            myCommand.Parameters.AddWithValue("dob", pd.Dob == null || pd.Dob.ToString() == "" ? DBNull.Value : pd.Dob);
                            myCommand.Parameters.AddWithValue("sex_type_id", pd.SexTypeId == null || pd.SexTypeId == 0 ? throw new NoNullAllowedException() : pd.SexTypeId);

                            myReader = myCommand.ExecuteReader();
                            myReader.Read();
                            _id = (int)myReader[0];
                            myReader.Close();

                            //int i = 1;
                            //int j = 0;
                            //var p = i / j;

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
        /// <param name="pd"></param>
        /// <returns>id of updated patient</returns>
        public async Task<int> UpdatePatient(int id, PatientDemographics pd)
        {
            string query = @"
                 select patientupdate(@id, @fname, @mname, @lname, @dob::date, @sex_type_id)
            ";
            int _id = 0;

            NpgsqlDataReader myReader;
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(query, myCon))
                        {
                            myCommand.Parameters.AddWithValue("id", id);
                            myCommand.Parameters.AddWithValue("fname", pd.FirstName == null || pd.FirstName == "" ? throw new NoNullAllowedException() : pd.FirstName);
                            myCommand.Parameters.AddWithValue("mname", pd.MiddleName == null || pd.MiddleName == "" ? DBNull.Value : pd.MiddleName);
                            myCommand.Parameters.AddWithValue("lname", pd.LastName == null || pd.LastName == "" ? throw new NoNullAllowedException() : pd.LastName);
                            myCommand.Parameters.AddWithValue("dob", pd.Dob == null || pd.Dob.ToString() == "" ? DBNull.Value : pd.Dob);
                            myCommand.Parameters.AddWithValue("sex_type_id", pd.SexTypeId == null || pd.SexTypeId == 0 ? throw new NoNullAllowedException() : pd.SexTypeId);

                            myReader = myCommand.ExecuteReader();
                            myReader.Read();
                            _id = (int)myReader[0];
                            myReader.Close();

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
            return await Task.FromResult(_id);
        }

        /// <summary>
        /// Remove patient data using id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Confirmation message</returns>
        public Task<string> DeletePatient(int id)
        {
            string query = @"
                select patientdelete(@id)
            ";
            using (TransactionScope transactionScope = new())
            {
                try
                {
                    using (NpgsqlConnection myCon = new(sqlDataSource))
                    {
                        myCon.Open();
                        using (NpgsqlCommand myCommand = new(query, myCon))
                        {
                            myCommand.Parameters.AddWithValue("id", id);
                            myCommand.ExecuteReader();

                            transactionScope.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    transactionScope.Dispose();
                    Console.WriteLine(ex.Message);
                }
                return Task.FromResult("Deleted Sccessfully!");
            }
        }
    }
}
