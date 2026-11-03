using System.Data;
using Microsoft.Data.SqlClient;

namespace ElevatorSystem;

/// <summary>
/// Owns the connection string and every database call.
/// Forms and buttons must never write SQL themselves.
/// </summary>
public class DatabaseHelper
{
    // Built at runtime from the folder next to the exe. Never hard-coded.
    private readonly string _dataFolder;
    private readonly string _mdfPath;
    private readonly string _ldfPath;
    private readonly string _connectionString;
    private readonly string _masterConnectionString;

    /// <summary>
    /// Builds relative Data folder paths and both connection strings.
    /// </summary>
    public DatabaseHelper()
    {
        _dataFolder = Path.Combine(AppContext.BaseDirectory, "Data");
        _mdfPath = Path.Combine(_dataFolder, "ElevatorLog.mdf");
        _ldfPath = Path.Combine(_dataFolder, "ElevatorLog_log.ldf");

        _masterConnectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Initial Catalog=master";

        _connectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;AttachDbFilename=" + _mdfPath +
            ";Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=30";
    }

    /// <summary>
    /// Creates the Data folder, the database file if missing, and the ElevatorLog table.
    /// Shows a friendly message and returns false if LocalDB cannot be used.
    /// </summary>
    public bool InitializeDatabase()
    {
        try
        {
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }

            if (!File.Exists(_mdfPath))
            {
                CreateDatabaseFile();
            }

            CreateTableIfMissing();
            return true;
        }
        catch (Exception ex)
        {
            ShowError("Could not start the database. The elevator still works, but logging is off.\n\n" + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Asks LocalDB to create ElevatorLog.mdf in the app Data folder.
    /// ExecuteNonQuery is allowed here because this is one-off setup, not a log insert.
    /// </summary>
    private void CreateDatabaseFile()
    {
        string sql =
            "CREATE DATABASE [ElevatorLog] ON PRIMARY " +
            "(NAME = N'ElevatorLog', FILENAME = N'" + EscapeSqlPath(_mdfPath) + "') " +
            "LOG ON (NAME = N'ElevatorLog_log', FILENAME = N'" + EscapeSqlPath(_ldfPath) + "')";

        using (SqlConnection connection = new SqlConnection(_masterConnectionString))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Creates the ElevatorLog table if it is not already in the database.
    /// </summary>
    private void CreateTableIfMissing()
    {
        const string sql =
            "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'ElevatorLog') " +
            "CREATE TABLE ElevatorLog (" +
            "Id INT IDENTITY(1,1) PRIMARY KEY, " +
            "ElevatorId INT NOT NULL, " +
            "FromFloor INT NOT NULL, " +
            "ToFloor INT NOT NULL, " +
            "Status NVARCHAR(50) NOT NULL, " +
            "Source NVARCHAR(50) NOT NULL, " +
            "LogTime DATETIME NOT NULL DEFAULT GETDATE(), " +
            "CONSTRAINT CK_ElevatorLog_Floors CHECK " +
            "(FromFloor BETWEEN 1 AND 3 AND ToFloor BETWEEN 1 AND 3))";

        using (SqlConnection connection = OpenWorkingConnection())
        {
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Inserts one log row using the disconnected model (DataTable + adapter.Update).
    /// Does not use ExecuteNonQuery for the insert.
    /// </summary>
    public void SaveLog(int elevatorId, int fromFloor, int toFloor, string status, string source)
    {
        try
        {
            using (SqlConnection connection = OpenWorkingConnection())
            {
                using (SqlDataAdapter adapter = CreateLogAdapter(connection))
                {
                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                    DataTable table = new DataTable();
                    adapter.FillSchema(table, SchemaType.Source);
                    adapter.Fill(table);

                    if (table.Columns.Contains("Id"))
                    {
                        table.Columns["Id"].AutoIncrement = true;
                    }

                    DataRow row = table.NewRow();
                    row["ElevatorId"] = elevatorId;
                    row["FromFloor"] = fromFloor;
                    row["ToFloor"] = toFloor;
                    row["Status"] = status;
                    row["Source"] = source;
                    row["LogTime"] = DateTime.Now;
                    table.Rows.Add(row);

                    adapter.Update(table);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not save a log row.\n\n" + ex.Message);
        }
    }

    /// <summary>
    /// Loads every log row, newest first, for the DataGridView.
    /// </summary>
    public DataTable LoadLog()
    {
        DataTable table = new DataTable();

        try
        {
            using (SqlConnection connection = OpenWorkingConnection())
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT Id, ElevatorId, FromFloor, ToFloor, Status, Source, LogTime " +
                    "FROM ElevatorLog ORDER BY LogTime DESC, Id DESC",
                    connection))
                {
                    adapter.Fill(table);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError("Could not load the log table.\n\n" + ex.Message);
        }

        return table;
    }

    /// <summary>
    /// Deletes every row from the ElevatorLog table.
    /// Uses try-catch so it never crashes the application if the database has an issue.
    /// </summary>
    public bool ClearLog()
    {
        try
        {
            using (SqlConnection connection = OpenWorkingConnection())
            {
                using (SqlCommand command = new SqlCommand("DELETE FROM ElevatorLog", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            ShowError("Could not clear the log table.\n\n" + ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Returns the Data folder used at runtime (relative to the exe).
    /// </summary>
    public string GetDataFolder()
    {
        return _dataFolder;
    }

    /// <summary>
    /// Adapter used only for inserting new rows. SELECT returns no rows, just the columns.
    /// </summary>
    private SqlDataAdapter CreateLogAdapter(SqlConnection connection)
    {
        SqlDataAdapter adapter = new SqlDataAdapter(
            "SELECT Id, ElevatorId, FromFloor, ToFloor, Status, Source, LogTime FROM ElevatorLog WHERE 1 = 0",
            connection);
        adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
        return adapter;
    }

    /// <summary>
    /// Opens a connection to ElevatorLog.mdf. Falls back to Initial Catalog if already attached.
    /// </summary>
    private SqlConnection OpenWorkingConnection()
    {
        try
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        catch (SqlException)
        {
            SqlConnection fallback = new SqlConnection(
                "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ElevatorLog;Integrated Security=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=30");
            fallback.Open();
            return fallback;
        }
    }

    /// <summary>
    /// Doubles single quotes so a Windows path is safe inside a SQL string.
    /// </summary>
    private static string EscapeSqlPath(string path)
    {
        return path.Replace("'", "''");
    }

    /// <summary>
    /// Shows a clear error without crashing the program.
    /// </summary>
    private static void ShowError(string message)
    {
        if (Application.OpenForms.Count > 0 && Application.OpenForms[0].InvokeRequired)
        {
            Form form = Application.OpenForms[0];
            form.BeginInvoke(new Action(() =>
                MessageBox.Show(message, "Database", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
            return;
        }

        MessageBox.Show(message, "Database", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
