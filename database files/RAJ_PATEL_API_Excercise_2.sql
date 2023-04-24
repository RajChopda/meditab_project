CREATE OR REPLACE FUNCTION raj_getpatientwithallergy(
_patientid character varying DEFAULT NULL::character varying,
_firstname character varying DEFAULT NULL::character varying,
_lastname character varying DEFAULT NULL::character varying,
_dob date DEFAULT NULL::date,
_sextypeid character varying DEFAULT NULL::character varying,
_clientidin integer DEFAULT NULL::integer,
_clientidnotin integer DEFAULT NULL::integer,
_tpauserid integer DEFAULT NULL::integer,
_userid integer DEFAULT NULL::integer,
_langid character varying DEFAULT NULL::character varying,
_page integer DEFAULT NULL::integer,
_size integer DEFAULT NULL::integer,
_orderby character varying DEFAULT NULL::character varying)
 RETURNS SETOF json
 LANGUAGE plpgsql
AS $function$
declare
	query_start text := 'with patientdata as (
	select pd.patientid, pd.firstname, pd.middlename, pd.lastname, pd.dob, pd.chartno, pd.sextypeid, pd.isactive, pa.patientallergyid, pa.allergymasterid, pa.note,
	dense_rank() over (order by ';
	query_start2 text := 'pd.patientid) DenseRank
	from raj_patient_demographics pd
	left join raj_patient_allergy pa on pa.patientid=pd.patientid and pa.isdeleted = false ';
	where_cond text := 'where pd.isdeleted=false ';
	grp_order_query text := ' group by pd.patientid, pa.patientallergyid order by ';
	pagination_query text := ') select * from patientdata where DenseRank between ';
	final_query text;
begin
	query_start := query_start|| case when $8 is not null then $8||',' else '' end;
	where_cond := where_cond|| case when $1 != '0' then ' and pd.patientid=ANY(STRING_TO_ARRAY($1, '','')::int[])' else '' end
							|| case when $2 is not null then ' and pd.firstname=$2' else '' end
							|| case when $3 is not null then ' and pd.lastname=$3' else '' end
							|| case when $4 is not null then ' and pd.dob=$4' else '' end
							|| case when $5 != '0' then ' and pd.sextypeid=ANY(STRING_TO_ARRAY($5, '','')::int[])' else '' end;
	grp_order_query := grp_order_query || case when $13 is not null then $13 else 'patientid ' end;
	pagination_query := pagination_query || '(('|| case when $11 is not null then $11 else '1' end||' - 1) * ' || case when $12 is not null then $12 else '5' end ||'+1) and (('|| case when $11 is not null then $11 else '1' end ||' - 1) * '|| case when $12 is not null then $12 else '5' end ||' + '|| case when $12 is not null then $12 else '5' end||')';
	final_query := 'SELECT ARRAY_TO_JSON(ARRAY_AGG(ROW_TO_JSON(A))) FROM ('|| query_start || query_start2 || where_cond || grp_order_query || pagination_query ||' ) A';
	return query execute final_query using $1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11, $12, $13;
end;
$function$
;

SELECT * FROM raj_getpatientwithallergy (_patientid => '1', _firstname => NULL,_lastname => NULL,_dob => NULL,_sextypeid => '0',_clientidin => '10007',_clientidnotin => NULL,_tpauserid => NULL,_userId => '1000770',_langId => '', _page => 1, _size => 5 , _orderby =>  NULL);




create or replace function uspNewraj_patient_demographicsCreate(
_patientid character varying,
_firstname character varying,
_middlename character varying,
_lastname character varying,
_dob date,
_sextypeid integer,
_userId character varying,
_langId character varying)
returns TABLE(id integer) as
$$
declare
begin
	return query execute 'insert into raj_patient_demographics (firstname , middlename, lastname, dob, sextypeid) values ($2, $3, $4, $5::date, $6) returning patientid' using $1,$2,$3,$4,$5,$6;
end;
$$ language plpgsql;

SELECT * FROM uspNewraj_patient_demographicsCreate (_patientid => '1', _firstname => 'Raj', _middlename => 'G', _lastname => 'Chopda', _dob => '2002-02-13', _sextypeid => '1',_userId => '1000770',_langId => '')
;





CREATE OR REPLACE FUNCTION uspNewraj_patient_allergyCreate (
_patientid character varying,
_allergymasterid character varying,
_note character varying,
_patientallergyid character varying,
_userid character varying,
_langid character varying)
 RETURNS TABLE(id integer)
 LANGUAGE plpgsql
AS $function$
declare
begin
	return query execute 'insert into raj_patient_allergy (patientid, allergymasterid, note) values ($1::int, $2::int, $3) returning patientallergyid' using $1, $2, $3;
end
$function$
;

SELECT * FROM uspNewraj_patient_allergyCreate (_patientid => '9',_patientallergyid => NULL,_allergymasterid => '3',_note => NULL, _userId => '1000770',_langId => '')
;



CREATE OR REPLACE FUNCTION uspNewraj_patient_demographicsUpdate (
_patientid character varying,
_firstname character varying,
_middlename character varying,
_lastname character varying,
_dob date,
_sextypeid integer,
_userId character varying,
_langId character varying)
 RETURNS TABLE(id integer)
 LANGUAGE plpgsql
AS $function$
declare
begin
	return query execute 'update raj_patient_demographics set firstname=$2, middlename=$3, lastname=$4, dob=$5, sextypeid=$6 where isdeleted=false and patientid=$1::int returning patientid' using $1, $2, $3, $4, $5, $6;
end
$function$
;

SELECT * FROM uspNewraj_patient_demographicsUpdate (_patientid => '10', _firstname => 'Mmm', _middlename => 'M', _lastname => 'V', _dob => NULL,_sextypeid => '2',_userId => '1000770',_langId => '')
;



CREATE OR REPLACE FUNCTION uspNewraj_patient_allergyUpdate (
_patientid character varying,
_patientallergyid character varying,
_allergymasterid character varying,
_note character varying,
_userid character varying,
_langid character varying)
 RETURNS TABLE(id integer)
 LANGUAGE plpgsql
AS $function$
declare
begin
	return query execute 'update raj_patient_allergy set allergymasterid=$3::int, note=$4 where patientid=$1::int and patientallergyid=$2::int and isdeleted=false returning patientallergyid' using $1, $2, $3, $4;
end
$function$
;

SELECT * FROM uspNewraj_patient_allergyUpdate (_patientid => '10',_patientallergyid => '5',_allergymasterid => '2',_note => 'Moderate', _userId => '1000770',_langId => '')
;




CREATE OR REPLACE FUNCTION uspNewraj_patient_allergyDelete  (
_raj_patient_allergyid  character varying,
_lastmodifieddate date,
_clientid character varying,
_machineid character varying,
_oldsystemid character varying,
_issample boolean,
_requestid character varying,
_userid character varying,
_langid character varying)
 RETURNS VOID
 LANGUAGE plpgsql
AS $function$
declare
begin
	update raj_patient_allergy set isdeleted=true where patientallergyid=$1::int and isdeleted=false;
end
$function$
;



CREATE OR REPLACE FUNCTION uspNewraj_patient_demographicsDelete  (
_raj_patient_demographicsid  character varying,
_lastmodifieddate date,
_clientid character varying,
_machineid character varying,
_oldsystemid character varying,
_issample boolean,
_requestid character varying,
_userid character varying,
_langid character varying)
 RETURNS VOID
 LANGUAGE plpgsql
AS $function$
declare
begin
	 update raj_patient_demographics set isdeleted=true where patientid=$1::int and isdeleted=false;
	 perform raj_deleteallpatientallergy(_patientid=>$1);
end
$function$
;

create or replace function raj_deleteallpatientallergy(_patientid character varying)
returns void as
$$
declare
begin
	 update raj_patient_allergy set isdeleted=true where patientid=$1::int;
end
$$ language plpgsql;



select * from uspNewraj_patient_demographicsDelete (_raj_patient_demographicsid => '10',_lastmodifieddate => '1970-01-01 00:00:00.000',_clientid => '10007',_machineid => '::1',_oldsystemid => NULL,_issample => 'false',_requestid => '93d58b5c-b83c-4277-a3be-551bd0ccb929',_userId => '1000770',_langId => '')
;