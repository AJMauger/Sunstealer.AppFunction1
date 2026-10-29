using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using System.Data;
using System.Security.Cryptography;

namespace Sunstealer.FunctionApp1.Models;

// ajm: -------------------------------------------------------------------------------------------
/// <summary>
/// 
/// </summary>
public class Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public static string env = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ?? "Error";
    /// <summary>
    /// 
    /// </summary>
    public static List<string> log = new();
    /// <summary>
    /// 
    /// </summary>
    public static int ChunkResponse = 0;

    /// <summary>
    /// 
    /// </summary>
    public static void CreateCEK()
    {
        try
        {
            string sqlConnectionString = "Server=AJMWIN11-01\\SQLEXPRESS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Application Name=\"Sunstealer\";Column Encryption Setting=Enabled";

            string cmkName = "CMK";
            string cekName = "CEK";
            string keyPath = "https://sunstealerkv.vault.azure.net/keys/AlwaysEncrypted/6bafe0e515a74bbea21977c2c8e7d0e9";
            var credential = new DefaultAzureCredential();
            var akvProvider = new SqlColumnEncryptionAzureKeyVaultProvider(credential);

            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(new Dictionary<string, SqlColumnEncryptionKeyStoreProvider> {
                { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, akvProvider }
            });

            byte[] cekPlaintext = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(cekPlaintext);
            }

            string algorithm = "RSA_OAEP";
            byte[] cekEncrypted = akvProvider.EncryptColumnEncryptionKey(keyPath, algorithm, cekPlaintext);
            string encryptedValueHex = "0x" + BitConverter.ToString(cekEncrypted).Replace("-", "");
            string createCekTsql = $@"create column encryption key [{cekName}] with values (column_master_key = [{cmkName}], algorithm = '{algorithm}', encrypted_value = {encryptedValueHex});";

            using (var conn = new SqlConnection(sqlConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = createCekTsql;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"CEK created with {createCekTsql}.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"CreateCEK(). {e.Message}\r\n{e.StackTrace}");
        }
    }
}
