-- Manual script for SSMS if you want to create the table yourself.
-- The app also creates this table automatically on first run.
--
-- 1. Make sure LocalDB is running: sqllocaldb start MSSQLLocalDB
-- 2. Create or attach ElevatorLog.mdf under the app Data folder, then run this.

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ElevatorLog')
BEGIN
    CREATE TABLE ElevatorLog
    (
        Id          INT IDENTITY(1, 1) PRIMARY KEY,
        ElevatorId  INT NOT NULL,
        FromFloor   INT NOT NULL,
        ToFloor     INT NOT NULL,
        Status      NVARCHAR(50) NOT NULL,
        Source      NVARCHAR(50) NOT NULL,
        LogTime     DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT CK_ElevatorLog_Floors CHECK
        (
            FromFloor BETWEEN 1 AND 3
            AND ToFloor BETWEEN 1 AND 3
        )
    );
END
GO
