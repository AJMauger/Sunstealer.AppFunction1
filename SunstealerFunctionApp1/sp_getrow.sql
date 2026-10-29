use database1;
go

exec sp_getrow @encrypted1 = 'Encrypted', @number1 = 1, @text1 = 'ClearText';