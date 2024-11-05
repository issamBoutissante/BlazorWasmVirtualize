/* clear the parts*/
truncate table Parts;

select * from Parts;

/* this will insert as many rows you want to parts */
WITH NumberSeries AS (
    SELECT TOP (100000) 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Number
    FROM sys.all_columns a, sys.all_columns b
)
INSERT INTO Parts (Id, Name, CreationDate, Status)
SELECT 
    NEWID(),                                           -- Unique GUID for Id
    CONCAT('Part ', Number),                           -- Sequential Name (e.g., "Part 1", "Part 2", ...)
    DATEADD(DAY, -Number % 365, SYSDATETIMEOFFSET()),  -- CreationDate offset by up to 365 days
    CAST(RAND(CHECKSUM(NEWID())) * 4 AS INT)           -- Random Status (0 to 3)
FROM 
    NumberSeries;



select * from Parts

/*Get Parts in chunks*/

CREATE PROCEDURE GetPartsInChunks
    @LastFetchedId UNIQUEIDENTIFIER = NULL,
    @ChunkSize INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@ChunkSize)
        Id,
        Name,
        CreationDate,
        Status
    FROM Parts
    WHERE (@LastFetchedId IS NULL OR Id > @LastFetchedId)
    ORDER BY Id;
END;


/*Get All Parts*/

CREATE PROCEDURE GetAllParts
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        Id,
        Name,
        CreationDate,
        Status
    FROM Parts
    ORDER BY Id;
END;
