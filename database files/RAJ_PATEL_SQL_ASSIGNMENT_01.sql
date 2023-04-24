create table raj_sex_type(
	sex_type_id serial primary key,
	sex_type_value varchar(20) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
)

create table raj_patient_demographics(
	patient_id serial primary key,
	fname varchar(30) not null,
	mname varchar(30),
	lname varchar(30) not null,
	dob date,
	sex_type_id int references raj_sex_type(sex_type_id),
	chart_no varchar(30) not null generated always as ('CHART'||patient_id::text) stored,
	is_active bool default true,
	is_deleted bool default false not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

insert into raj_sex_type(sex_type_value) values('Male');

insert into raj_sex_type(sex_type_value) values('Female');

insert into raj_sex_type(sex_type_value) values('Unknown');

select * from raj_sex_type;

insert into raj_patient_demographics (fname, mname, lname, dob, sex_type_id) values ('Raj', 'Gordhanbhai', 'Chopda', '2002-02-13', 1);

select * from raj_patient_demographics;

create table race_type(
	race_type_id serial primary key,
	race_type_value varchar(30) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

create table race(
	race_id serial primary key,
	patient_id int references patient_demographics(patient_id),
	race_type_id int references race_type(race_type_id),
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

select * from race;

create table address_type(
	address_type_id serial primary key,
	address_type_value varchar(20) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

create table address(
	address_id serial primary key,
	patient_id int references patient_demographics(patient_id),
	address_type_id int references address_type(address_type_id),
	street varchar(30) not null,
	zip varchar(20) not null,
	city varchar(30) not null,
	state varchar(20) not null,
	country varchar(20) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

select * from address;

create table phone_type(
	phone_type_id serial primary key,
	phone_type_value varchar(10) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

create table phone(
	phone_id serial primary key,
	phone_type_id int references phone_type(phone_type_id),
	address_id int references address(address_id),
	address_type_id int references address_type(address_type_id),
	phone_no varchar(10) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

select * from phone;

create table fax(
	fax_id serial primary key,
	address_id int references address(address_id),
	fax_no varchar(10) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

select * from fax;

create table preference_contact_type(
	preference_type_id serial primary key,
	preference_type_value varchar(20) not null,
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

select * from preference_contact;

create table preference_contact(
	preference_contact_id serial primary key,
	preference_type_id int references preference_contact_type(preference_type_id),
	patient_id int references patient_demographics(patient_id),
	address_id int references address(address_id),
	phone_id int references phone(phone_id),
	fax_id int references fax(fax_id),
    created_on timestamp default current_timestamp not null,
    modified_on timestamp default current_timestamp not null
);

insert into preference_contact(preference_type_id, patient_id, address_id, phone_id, fax_id) values (2, 1, 2, 4, 2);

