create table allergy_master(
	allergy_master_id serial primary key,
	allergy_code varchar(10),
	allergy_name varchar(30),
	created_on timestamp default current_timestamp not null,
	modified_on timestamp default current_timestamp not null
);

insert into allergy_master(allergy_code, allergy_name) values ('A1', 'peanuts');
insert into allergy_master(allergy_code, allergy_name) values ('A2', 'milk');
insert into allergy_master(allergy_code, allergy_name) values ('A3', 'bread');

select * from allergy_master;


create table patient_allergy(
	patient_allergy_id serial primary key,
	patient_id int references patient_demographics(patient_id),
	allergy_master_id int references allergy_master(allergy_master_id),
	note varchar(100),
	created_on timestamp default current_timestamp not null,
	modified_on timestamp default current_timestamp not null,
	is_deleted bool default false
);

select * from patient_allergy;


create or replace function getallergyofpatient(_patient_id int)
returns table(patient_allergy_id int, allergy_master_id int, note varchar) as
$$
declare
begin
	return query execute 'select patient_allergy_id, allergy_master_id, note from patient_allergy where patient_id=$1 and is_deleted=false' using $1;
end
$$ language plpgsql;


create or replace function createpatientallergy(_patient_id int, _allergy_master_id int, _note varchar(100))
returns table(patient_allergy_id int) as
$$
declare
begin
	return query execute 'insert into patient_allergy (patient_id, allergy_master_id, note) values ($1, $2, $3) returning patient_allergy_id' using $1, $2, $3;
end
$$ language plpgsql;


create or replace function updatepatientallergy(_patient_id int, _patient_allergy_id int, _allergy_master_id int, _note varchar(100))
returns table(patient_allergy_id int) as
$$
declare
begin
	return query execute 'update patient_allergy set allergy_master_id=$3, note=$4 where patient_id=$1 and patient_allergy_id=$2 and is_deleted=false returning patient_allergy_id' using $1, $2, $3, $4;
end
$$ language plpgsql;


create or replace function deletepatientallergy(_patient_id int, _patient_allergy_id int)
returns void as
$$
declare
begin
	 update patient_allergy set is_deleted=true where patient_id=$1 and patient_allergy_id=$2;
end
$$ language plpgsql;


create or replace function deleteallpatientallergy(_patient_id int)
returns void as
$$
declare
begin
	 update patient_allergy set is_deleted=true where patient_id=$1;
end
$$ language plpgsql;




select * from getallergyofpatient(_patient_id=>1);


create or replace function testing(_patient_id int default null, _fname varchar(30) default null, _lname varchar(30) default null, _dob date default null, _sex_type_id int default null,
_pagenumber int default null, _pagesize int default null, _orderby varchar(30) default null, _sorting varchar default null, _allergy_master_id int default null)
returns table(patient_id int, fname varchar, mname varchar, lname varchar, dob date, chart_no varchar, sex_type_id int, is_active bool, patient_allergy_id int, allergy_master_id int, note varchar, rowno bigint) as
$$
declare
	query_start text := 'with patientdata as (
	select pd.patient_id, pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, pd.sex_type_id, pd.is_active, pa.patient_allergy_id, pa.allergy_master_id, pa.note,
	dense_rank() over (order by ';
	query_start2 text := 'pd.patient_id) rowno
	from patient_demographics pd
	left join patient_allergy pa on pa.patient_id=pd.patient_id ';
	where_cond text := 'where (pd.is_deleted=false and (pa.is_deleted=false or pa.is_deleted is null)) ';
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
		raise notice '%', query_start || query_start2 || where_cond || grp_order_query || pagination_query;
	return query execute query_start || query_start2 || where_cond || grp_order_query || pagination_query using $1, $2, $3, $4, $5, $6, $7, $8, $9, $10;
end;
$$ language plpgsql;

select * from testing();

select * from testing(_patient_id=>1);
select * from testing(_patient_id=>44);

select * from testing(_pagesize=>100);

select * from testing(_pagesize=>10, _pagenumber=>3);

select * from testing(_allergy_master_id=>1);

select * from testing( _fname => 'Raj',  _orderby=>'fname');

select * from patient_allergy;




with patientdata as (
select pd.patient_id, pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, pd.sex_type_id, pd.is_active, pa.patient_allergy_id, pa.allergy_master_id, pa.note,
	dense_rank() over (order by fname, pd.patient_id) denserank
	from patient_demographics pd
	left join patient_allergy pa on pa.patient_id=pd.patient_id and pa.is_deleted = false
	where (pd.patient_id=62) and (pd.is_deleted=false and (pa.is_deleted=false or pa.is_deleted is null)) group by pd.patient_id,pa.patient_allergy_id order by fname
) select * from patientdata where denserank between 1 and 10;


with patientdata as (
select pd.patient_id, pd.fname, pd.mname, pd.lname, pd.dob, pd.chart_no, pd.sex_type_id, pd.is_active, pa.patient_allergy_id, pa.allergy_master_id, pa.note,
	dense_rank() over (order by pd.patient_id) rowno
	from patient_demographics pd
	left join patient_allergy pa on pa.patient_id=pd.patient_id
	 where (pd.is_deleted=false and (pa.is_deleted=false or pa.is_deleted is null)) group by pd.patient_id,pa.patient_allergy_id order by patient_id
) select * from patientdata where rowno between 1 and 5;















create or replace function update_modified_on()
returns trigger as
$$
declare
begin
	new.modified_on := current_timestamp;
	return new;
end;
$$ language plpgsql;


create trigger address_updated
	before update on address
	for each row
	execute procedure update_modified_on();

create trigger address_type_updated
	before update on address_type
	for each row
	execute procedure update_modified_on();

create trigger allergy_master_updated
	before update on allergy_master
	for each row
	execute procedure update_modified_on();

create trigger fax_updated
	before update on fax
	for each row
	execute procedure update_modified_on();

create trigger patient_allergy_updated
	before update on patient_allergy
	for each row
	execute procedure update_modified_on();

create trigger patient_demographics_updated
	before update on patient_demographics
	for each row
	execute procedure update_modified_on();

create trigger phone_updated
	before update on phone
	for each row
	execute procedure update_modified_on();

create trigger phone_type_updated
	before update on phone_type
	for each row
	execute procedure update_modified_on();

create trigger preference_contact_updated
	before update on preference_contact
	for each row
	execute procedure update_modified_on();

create trigger preference_contact_type_updated
	before update on preference_contact_type
	for each row
	execute procedure update_modified_on();

create trigger race_updated
	before update on race
	for each row
	execute procedure update_modified_on();

create trigger race_type_updated
	before update on race_type
	for each row
	execute procedure update_modified_on();

create trigger sex_type_updated
	before update on sex_type
	for each row
	execute procedure update_modified_on();
	


--create or replace function patientpatch(_patient_id int, _fname varchar(30) default null, _mname varchar(30) default null, _lname varchar(30) default null, _dob date default null, _sex_type_id int default null)
--returns int as
--$$
--declare 
--	query_start text := 'update patient_demographics set patient_id=$1';
--	where_cond text := ' where is_deleted=false and patient_id=$1';
--begin
--	query_start := query_start	|| case when $2 is not null then ',fname=$2' else '' end
--								|| case when $3 is not null then ',mname=$3' else '' end
--								|| case when $4 is not null then ',lname=$4' else '' end
--								|| case when $5 is not null then ',dob=$5' else '' end
--								|| case when $6 is not null then ',sex_type_id=$6' else '' end;
--	return query execute query_start || where_cond using $1, $2, $3, $4, $5, $6;
--		
--end;
--$$ language plpgsql;
