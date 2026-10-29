SELECT 
    t1.UUID, 
    t1.Date1, 
    t1.Encrypted1, 
    t1.Number1, 
    t1.Text1,
    (
        SELECT t2.UUID, t2.Date2, t2.Text2 
        FROM [dbo].[table2] AS t2 
        WHERE t2.UUID_table1 = t1.UUID 
        FOR JSON PATH
    ) AS Table2_Records,
    (
        SELECT t3.UUID, t3.Date3, t3.Text3 
        FROM [dbo].[table3] AS t3 
        WHERE t3.UUID_table1 = t1.UUID 
        FOR JSON PATH
    ) AS Table3_Records
FROM [dbo].[table1] AS t1;
