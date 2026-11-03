
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Functions.Worker;

/// <summary>
/// 
/// </summary>
interface ISequenceNumberService
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Task<int> Next();

}

/// <summary>
/// 
/// </summary>
public class SequenceNumberService: ISequenceNumberService
{
    private readonly BlobClient _blobClient;
    private readonly SemaphoreSlim _criticalSection = new SemaphoreSlim(1, 1);
    private readonly int _dailyPoolSize;  // ajm: acs => Sunstealer.FunctionApp1.Models.Configuration.DailyEFTNumberPoolSize
    private DateTime _date;
    private int _index = 0;
    private readonly List<int> _randomNumbers;
    private readonly string _stateFilePath = "sequence_state.txt";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blobServiceClient"></param>
    /// <param name="dailyPoolSize"></param>
    public SequenceNumberService(BlobServiceClient blobServiceClient, int dailyPoolSize)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient("sequencenumber-container");
        _blobClient = containerClient.GetBlobClient("sequencenumber-state.json") ?? throw new Exception("_blobClient");

        _dailyPoolSize = dailyPoolSize;
        _randomNumbers = new List<int>(_dailyPoolSize);
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task Initialize()
    {
        try
        {
            await _criticalSection.WaitAsync();

            _date = DateTime.UtcNow.Date;

            int daySeed = _date.GetHashCode();
            Random rand = new Random(daySeed);

            for (int i = 1; i <= _dailyPoolSize; i++)
            {
                _randomNumbers.Add(i);
            }

            for (int i = _randomNumbers.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                int k = _randomNumbers[i];
                _randomNumbers[i] = _randomNumbers[j];
                _randomNumbers[j] = k;
            }

            if (await ReadState())
            {
                return;
            }

            _index = 0;
            await WritetState();
        }
        finally
        {
            _criticalSection.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<int> Next()
    {
        try
        {
            await _criticalSection.WaitAsync();

            if (DateTime.UtcNow.Date != _date)
            {
                await Initialize();
            }

            if (_index >= _dailyPoolSize)
            {
                throw new InvalidOperationException($"Daily limit of {_dailyPoolSize} sequence numbers has been exhausted.");
            }

            int uniqueId = _randomNumbers[_index];

            _index++;
            await WritetState();

            return uniqueId;
        }
        finally
        {
            _criticalSection.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task<bool> ReadState()
    {
        // ajm: container 
        await _blobClient.GetParentBlobContainerClient().CreateIfNotExistsAsync();

        if (await _blobClient.ExistsAsync())
        {
            var downloadResult = await _blobClient.DownloadContentAsync();
            //  downloadResult.Value.Content.ToString();
        }
        else
        {
            // blob !exist
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task<bool> WritetState()
    {
        string[] stateData = { _date.ToString("yyyy-MM-dd"), _index.ToString() };
        File.WriteAllLines(_stateFilePath, stateData);  // ajm: storage account
        return true;
    }

    /* /// <summary>
    /// 
    /// </summary>
    public void Test()
    {
        List<int> testArray = new(_dailyPoolSize);

        TestSequeceNumberService(10_000, testArray);

        Console.WriteLine("CRASH");

        TestSequeceNumberService(100_000, testArray);

        Console.WriteLine("CRASH");

        TestSequeceNumberService(1_000_000, testArray);

        Console.WriteLine("CRASH");

        TestSequeceNumberService(10_000_000, testArray);

        Console.WriteLine("CRASH");

        TestSequeceNumberService(-1, testArray);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void TestSequeceNumberService(int crash, List<int> testArray)
    {
        for (int i = 0; i < 99_999_999; i++)
        {
            int j = this.Next();
            Console.WriteLine($"[{i}]={j}");
            if (testArray.Contains(j))
            {
                throw new Exception($"SequenceNumberService.Next() {j} repeated.");
            }
            testArray.Add(j);

            if (i == crash)
            {
                break;
            }

            if (i % 1000 == 0)
            {
                Thread.Sleep(100);
            }
        }
    }*/
}

