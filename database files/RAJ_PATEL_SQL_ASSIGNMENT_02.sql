--Question-1
--Create View to fetch the result of FirstName, LastName, MiddleName, DOB, Chart Number, Sex , Race , Primary Address, Primary Phone, Primary Fax.

create or replace view patient_info as
	select pd.patient_id, pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, st.sex_type_value, rt.race_type_value, at2.address_type_value, a.street, a.city, a.zip, a.state, a.country, pt.phone_type_value, p.phone_no, f.fax_no
	from patient_demographics pd
	left join sex_type st on st.sex_type_id=pd.sex_type_id
	left join race r on r.patient_id=pd.patient_id
	left join race_type rt on rt.race_type_id=r.race_type_id
	left join preference_contact_type pct on pct.preference_type_value ='Primary'
	left join preference_contact pc on pc.patient_id=pd.patient_id and pct.preference_type_id=pc.preference_type_id
	left join address a on a.address_id=pc.address_id
	left join address_type at2 on at2.address_type_id=a.address_type_id
	left join phone p on p.phone_id=pc.phone_id
	left join phone_type pt on pt.phone_type_id=p.phone_type_id
	left join fax f on f.fax_id=pc.fax_id
group by pd.patient_id, st.sex_type_value, rt.race_type_value, at2.address_type_value, a.address_id, pt.phone_type_value, p.phone_no, f.fax_no;

select * from patient_info;

--select * from preference_contact;

--drop view patient_info;



--Questio-2
--Write Query to fetch unique record from the Patient table based on Firstname, LastName, DOB and Sex with number of occurance(count) of same data.

select distinct pd.fname, pd.lname, st.sex_type_value, pd.dob, count(*)  from patient_demographics pd
	left join sex_type st on st.sex_type_id=pd.sex_type_id group by pd.fname, pd.lname, st.sex_type_value, pd.dob;





--Questio-3
--Create Function to stored the data into patient table. Pass all the value in the function parameter and function should return the created new primary key value of the table.

create or replace function insert_patient_info(_fname varchar(30), _mname varchar(30), _lname varchar(30), _dob date, _sex_type_id int)
returns integer as
$$
declare
	pid integer;
begin
	insert into patient_demographics (fname, mname, lname, dob, sex_type_id) values (_fname, _mname, _lname, _dob, _sex_type_id) returning patient_id into pid;
	return pid;
end;
$$ language plpgsql;

select insert_patient_info('Ruchit', 'Jitendrabhai', 'Shah', '2001-09-19', 1) as patient_id;
select insert_patient_info('Meet', 'Mehulbhai', 'Patel', '2000-01-23', 1) as patient_id;
select insert_patient_info('Hemit', 'Sanjaybhai', 'Rana', '2001-09-14', 1) as patient_id;





--Questio-4
--Create Function to get the result of patient’s data by using patientId, lastname, firstname, sex, dob. Need to implement the pagination and sorting(LastName, Firstname, Sex, DOB) in this function.

create or replace function get_patient_info(_patient_id int default null, _fname varchar(30) default null, _lname varchar(30) default null, _dob date default null, _sex_type_id int default null,
_pagenumber int default 1, _pagesize int default 10, _orderby varchar(30) default 'fname')
returns table(fname varchar, mname varchar, lname varchar, dob date, chart_no varchar, sex_type_value varchar) as
$$
declare
	query_start text := 'select pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, st.sex_type_value
	from patient_demographics pd
	left join sex_type st on st.sex_type_id=pd.sex_type_id ';
	where_cond text := 'where 1=1';
	query_end text := ' order by '||$8||' limit $7 offset (($6 - 1) * $7)';
begin
	
	raise notice '%', $8;
	
	where_cond := where_cond|| case when $1 is not null then ' and pd.patient_id=$1' else '' end
							|| case when $2 is not null then ' and pd.fname=$2' else '' end
							|| case when $3 is not null then ' and pd.lname=$3' else '' end
							|| case when $4 is not null then ' and pd.dob=$4' else '' end
							|| case when $5 is not null then ' and pd.sex_type_id=$5' else '' end;
	return query execute query_start || where_cond || query_end using _patient_id, _fname, _lname, _dob, _sex_type_id, _pagenumber, _pagesize, _orderby;
		
end;
$$ language plpgsql;

select * from get_patient_info();

select * from get_patient_info(_orderby=>'lname');

select * from get_patient_info(_patient_id=>1, _fname=>'Raj', _lname=>'Chopda', _dob=>'2002-02-13', _sex_type_id=>1);

select * from get_patient_info(_patient_id=>1, _fname=>'Raj', _sex_type_id=>2);

select * from get_patient_info(_lname=>'Chopda');

select * from get_patient_info(_dob=>'2002-02-13');

select * from get_patient_info(_sex_type_id=>1, _orderby=>'dob');

select * from get_patient_info(_sex_type_id=>1, _pagesize=>2);

select * from get_patient_info(_sex_type_id=>1, _pagenumber=>2, _pagesize=>2);

--drop function get_patient_info;






--Questio-5
--Write Query to search the patient by patient’s phone no

select * from patient_demographics pd
	left join address a on a.patient_id=pd.patient_id
	left join phone p on p.address_id=a.address_id
	where p.phone_no = '1111111111';
	