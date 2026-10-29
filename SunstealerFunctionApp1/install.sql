delete from database1..table1;
delete from database1..table2;
delete from database1..table3;

declare @Encrypted1 as nvarchar(max) = N'Encrypted';
insert into database1..table1 ([Date1], [Encrypted1], [Number1], [Text1]) values (getdate(), @Encrypted1, 1, N'ClearText');

declare @uuid int = scope_identity();

insert into database1..table2 ([Date2], [Text2], UUID_table1) values (getdate(), 'ClearText2.1', @uuid);
insert into database1..table2 ([Date2], [Text2], UUID_table1) values (getdate(), 'ClearText2.2', @uuid);
insert into database1..table2 ([Date2], [Text2], UUID_table1) values (getdate(), 'ClearText2.3', @uuid);

insert into database1..table3 ([Date3], [Text3], UUID_table1) values (getdate(), 'ClearText3.1', @uuid);
insert into database1..table3 ([Date3], [Text3], UUID_table1) values (getdate(), 'ClearText3.2', @uuid);
insert into database1..table3 ([Date3], [Text3], UUID_table1) values (getdate(), 'ClearText3.3', @uuid);
