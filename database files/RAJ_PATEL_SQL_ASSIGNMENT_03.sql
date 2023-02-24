
--Get patient info using id
create or replace function patientgetbyid(_patient_id int)
returns table(fname varchar, mname varchar, lname varchar, dob date, chart_no varchar, sex_type_id int) as
$$
declare
begin	
	return query select pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, pd.sex_type_id 
	from patient_demographics pd
	left join sex_type st on st.sex_type_id=pd.sex_type_id where is_deleted=false and pd.patient_id=$1;
end;
$$ language plpgsql;





create or replace function patientget(_patient_id int default null, _fname varchar(30) default null, _lname varchar(30) default null, _dob date default null, _sex_type_id int default null,
_pagenumber int default 1, _pagesize int default 5, _orderby varchar(30) default null, _sorting varchar default null, _allergy_master_id int default null)
returns table(patient_id int, fname varchar, mname varchar, lname varchar, dob date, chart_no varchar, sex_type_id int, is_active bool) as
$$
declare
	query_start text := 'select pd.patient_id, pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, pd.sex_type_id, pd.is_active
	from patient_demographics pd
	left join sex_type st on st.sex_type_id=pd.sex_type_id
	left join patient_allergy pa on pa.patient_id=pd.patient_id
	left join allergy_master am on am.allergy_master_id=pa.allergy_master_id ';
	where_cond text := 'where pd.is_deleted=false ';
	query_end text := ' group by pd.patient_id order by ';
begin
	where_cond := where_cond|| case when $1 is not null then ' and pd.patient_id=$1' else '' end
							|| case when $2 is not null then ' and pd.fname=$2' else '' end
							|| case when $3 is not null then ' and pd.lname=$3' else '' end
							|| case when $4 is not null then ' and pd.dob=$4' else '' end
							|| case when $5 is not null then ' and pd.sex_type_id=$5' else '' end
							|| case when $10 is not null then ' and am.allergy_master_id=$10' else '' end;
	query_end := query_end 	|| case when $8 is not null then $8 else 'patient_id' end
							|| case when $9 is not null then $9 else ' asc' end || ' limit $7 offset (($6 - 1) * $7)';
	return query execute query_start || where_cond || query_end using $1, $2, $3, $4, $5, $6, $7, $8, $9, $10;
		
end;
$$ language plpgsql;

select * from patientget(_orderby=>'lname');

select * from patientget(_dob=>'2023-02-14');

select * from patientget(_fname=>'Raj');

select * from patientget(_allergy_master_id=>3);

select * from patient_demographics;



create or replace function getpatientwithallergy(_patient_id int default null, _fname varchar(30) default null, _lname varchar(30) default null, _dob date default null, _sex_type_id int default null,
_pagenumber int default null, _pagesize int default null, _orderby varchar(30) default null, _sorting varchar default null, _allergy_master_id int default null)
returns table(PatientId int, FirstName varchar, MiddleName varchar, LastName varchar, Dob date, ChartNo varchar, SexTypeId int, IsActive bool, PatientAllergyId int, AllergyMasterId int, Note varchar, denserank bigint) as
$$
declare
	query_start text := 'with patientdata as (
	select pd.patient_id, pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, pd.sex_type_id, pd.is_active, pa.patient_allergy_id, pa.allergy_master_id, pa.note,
	dense_rank() over (order by ';
	query_start2 text := 'pd.patient_id) rowno
	from patient_demographics pd
	left join patient_allergy pa on pa.patient_id=pd.patient_id and pa.is_deleted = false ';
	where_cond text := 'where pd.is_deleted=false ';
	grp_order_query text := ' group by pd.patient_id, pa.patient_allergy_id order by ';
	pagination_query text := ') select * from patientdata where rowno between ';
begin
	query_start := query_start|| case when $8 is not null then $8||',' else '' end;
	where_cond := where_cond|| case when $1 is not null then ' and pd.patient_id=$1' else '' end
							|| case when $2 is not null then ' and pd.fname=$2' else '' end
							|| case when $3 is not null then ' and pd.lname=$3' else '' end
							|| case when $4 is not null then ' and pd.dob=$4' else '' end
							|| case when $5 is not null then ' and pd.sex_type_id=$5' else '' end
							|| case when $10 is not null then ' and pa.allergy_master_id=$10' else '' end;
	grp_order_query := grp_order_query || case when $8 is not null then $8 else 'patient_id' end
							|| case when $9 is not null then $9 else ' asc' end; -- || ' limit $7 offset (($6 - 1) * $7)';
	pagination_query := pagination_query || '(('|| case when $6 is not null then $6 else '1' end||' - 1) * ' || case when $7 is not null then $7 else '5' end ||'+1) and (('|| case when $6 is not null then $6 else '1' end ||' - 1) * '|| case when $7 is not null then $7 else '5' end ||' + '|| case when $7 is not null then $7 else '5' end||')';
	return query execute query_start || query_start2 || where_cond || grp_order_query || pagination_query using $1, $2, $3, $4, $5, $6, $7, $8, $9, $10;
end;
$$ language plpgsql;

select * from getpatientwithallergy(_patient_id=>62);

select * from getpatientwithallergy();

select * from getpatientwithallergy(_patient_id=>1);

select * from getpatientwithallergy(_pagesize=>100);

select * from getpatientwithallergy(_pagesize=>10, _pagenumber=>3);

select * from getpatientwithallergy(_allergy_master_id=>1);

select * from getpatientwithallergy( _fname => 'Raj',  _orderby=>'fname');

select * from patient_allergy;










--Insert
create or replace function patientcreate(_fname varchar(30), _mname varchar(30), _lname varchar(30), _dob date, _sex_type_id int)
returns integer as
$$
declare
	pid integer;
begin
	insert into patient_demographics (fname, mname, lname, dob, sex_type_id) values ($1, $2, $3, $4, $5) returning patient_id into pid;
	return pid;
end;
$$ language plpgsql;





--update
create or replace function patientupdate(_patient_id int, _fname varchar(30), _mname varchar(30), _lname varchar(30), _dob date, _sex_type_id int)
returns int as
$$
declare
pid int;
begin
	update patient_demographics set fname=$2, mname=$3, lname=$4, dob=$5, sex_type_id=$6 where is_deleted=false and patient_id=$1 returning patient_id into pid;
return pid;
end;
$$ language plpgsql;

select * from patientupdate(16, 'Test', 'string', 'string', '2023-01-12', 1);

select * from patientupdate(5, 'Hemit', null, 'Rana', null, 1);

select * from patient_demographics;



--delete
create or replace function patientdelete(_patient_id int)
returns void as
$$
declare
begin
	update patient_demographics set is_deleted=true where patient_id=$1 and is_deleted=false;
end;
$$ language plpgsql;
