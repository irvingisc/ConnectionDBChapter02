using Dapper; // Query<T> extension method
using Microsoft.Data.SqlClient; // SqlConnection and so on
using System.Data; // CommandType
using System.Data.Common;

SqlConnectionStringBuilder builder = new();
builder.InitialCatalog = "Northwind";
builder.MultipleActiveResultSets = true;
builder.Encrypt = true;
builder.TrustServerCertificate = true;
builder.ConnectTimeout = 10;

WriteLine("Connect to:");
WriteLine(" 1 - SQL Server on local machine");
WriteLine(" 2 - Azure SQL Database");
WriteLine(" 3 - Azure SQL Edge");
WriteLine();

Write("Press a key:");
ConsoleKey key = ReadKey().Key;
WriteLine(); WriteLine();

if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
{
    builder.DataSource = "(localdb)\\mssqllocaldb";
} else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
{
    builder.DataSource = // Azure SQL Database
    "tcp:apps-services-net7.database.windows.net,1433";
} else if (key is ConsoleKey.D3 or ConsoleKey.NumPad3)
{
    builder.DataSource = "tcp:127.0.0.1,1433"; // Azure SQL Edge
}
else
{
    WriteLine("No data source selected.");
    return;
}

WriteLine("Authenticate using:");
WriteLine("  1 - Windows Integrated Security");
WriteLine("  2 - SQL Login, for example, sa");
WriteLine();
Write("Press a key: ");

key = ReadKey().Key;
WriteLine(); WriteLine();

if (key is ConsoleKey.D1 or ConsoleKey.NumPad1)
{
    builder.IntegratedSecurity = true;
}
else if (key is ConsoleKey.D2 or ConsoleKey.NumPad2)
{
    builder.UserID = "sa"; // Azure SQL Edge
                           // "markjprice"; // change to your username

    Write("Enter your SQL Server password: ");
    string? password = ReadLine();
    if (string.IsNullOrWhiteSpace(password))
    {
        WriteLine("Password cannot be empty or null.");
        return;
    }

    builder.Password = password;
    builder.PersistSecurityInfo = false;
}
else
{
    WriteLine("No authentication selected.");
    return;
}


SqlConnection connection = new(builder.ConnectionString);

WriteLine(connection.ConnectionString);
WriteLine();

connection.StateChange += Connection_StateChange;
connection.InfoMessage += Connection_InfoMessage;

try
{
    WriteLine("Opening connection. Please wait up to {0} seconds...", builder.ConnectTimeout);
    WriteLine();

    await connection.OpenAsync();

    WriteLine($"SQL Server version: {connection.ServerVersion}");

    connection.StatisticsEnabled = true;
}
catch (SqlException ex)
{
    WriteLine($"SQL exception: {ex.Message}");
    return;
}

SqlCommand cmd = connection.CreateCommand();
cmd.CommandType = CommandType.Text;
cmd.CommandText = "SELECT ProductId, ProductName, UnitPrice FROM Products";

SqlDataReader r = cmd.ExecuteReader();

WriteLine("----------------------------------------------------------");
WriteLine("| {0,5} | {1,-35} | {2,8} |", "Id", "Name", "Price");
WriteLine("----------------------------------------------------------");

while (r.Read())
{
    WriteLine("| {0,5} | {1,-35} | {2,8:C} |", 
    r.GetInt32("ProductId"),
    r.GetString("ProductName"),
    r.GetDecimal("UnitPrice"));
}

WriteLine("----------------------------------------------------------");

connection.Close();