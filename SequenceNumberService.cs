/// <summary>
/// 
/// </summary>
public class State
{
    /// <summary>
    /// 
    /// </summary>
    public string Date { get; set; } = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public int Index { get; set; } = 0;
}

/// <summary>
/// 
/// </summary>
public class SequenceNumberService
{
    // private readonly SemaphoreSlim _criticalSection = new SemaphoreSlim(1, 1);
    private readonly int _dailyPoolSize = 99_999_999;
    private readonly List<int> _randomNumbers;
    private readonly string _stateUri = "sequence_state.json";
    private DateTime _date;
    private int _index = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="blobServiceClient"></param>
    /// <param name="logger"></param>
    /// <exception cref="Exception"></exception>
    public SequenceNumberService()
    {
        _randomNumbers = new List<int>(_dailyPoolSize);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Initialize()
    {
        try
        {
            //_criticalSection.WaitAsync();
            Console.WriteLine($"Initialize()");
            _randomNumbers.Clear();
            _date = DateTime.UtcNow.Date;
            int daySeed = _date.GetHashCode();
            Console.WriteLine($"daySeed={daySeed}");
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
            if (ReadState())
            {
                Console.WriteLine($"_date = {_date}, _index={_index}");
                return;
            }
            _index = 0;
            Console.WriteLine($"_date = {_date}, _index={_index}");
            WritetState();
        }
        finally
        {
            //_criticalSection.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int Next()
    {
        try
        {
            // _criticalSection.WaitAsync();
            if (DateTime.UtcNow.Date != _date)
            {
                Initialize();
            }
            if (_index >= _dailyPoolSize)
            {
                throw new InvalidOperationException($"Daily limit of {_dailyPoolSize} sequence numbers has been exhausted.");
            }
            int uniqueId = _randomNumbers[_index];
            _index++;
            WritetState();
            return uniqueId;
        }
        finally
        {
            // _criticalSection.Release();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool ReadState()
    {
        /* if (Sunstealer.FunctionApp1.Models.Configuration.env == "Development")
        {
            if (File.Exists(_stateUri))
            {
                using FileStream openStream = File.OpenRead(_stateUri);
                var state = System.Text.Json.JsonSerializer.DeserializeAsync<State>(openStream);
                if (state == null)
                {
                    throw new Exception("ReadState");
                }
                DateTime.TryParse(state.Date, out DateTime _date);
                _index = state.Index;
                return true;
            }
        }
        else
        {
            // ajm: storage account
        }
        return false;*/
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void WritetState()
    {
        /*if (Sunstealer.FunctionApp1.Models.Configuration.env == "Development")
        {
            var state = new State()
            {
                Date = _date.ToString("yyyy-MM-dd"),
                Index = _index
            };
            using FileStream openStream = File.OpenWrite(_stateUri);
            System.Text.Json.JsonSerializer.SerializeAsync<State>(openStream, state);
        }
        else
        {
            // ajm: storage account
        }*/
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void TestSequeceNumberService(int crash, List<int> testArray)
    {
        for (int i = _index; i < _dailyPoolSize; i++)
        {
            int j = Next();
            if (testArray[i]!=-1)
            {
                throw new Exception($"SequenceNumberService.Next() {j} repeated.");
            }
            testArray[i]=j;

            if (i == crash)
            {
                break;
            }

            if (i % 100000 == 0)
            {
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} [{i}]={j}");
                Task.Delay(100);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public void Test()
    {
        List<int> testArray = Enumerable.Repeat(-1, _dailyPoolSize).ToList();

        File.Delete(_stateUri);
        Initialize();
        TestSequeceNumberService(10_000, testArray);

        Console.WriteLine("CRASH");

        Initialize();
        TestSequeceNumberService(100_000, testArray);

        Console.WriteLine("CRASH");

        Initialize();
        TestSequeceNumberService(1_000_000, testArray);

        Console.WriteLine("CRASH"); 
        Initialize();
        TestSequeceNumberService(10_000_000, testArray);

        Console.WriteLine("CRASH");

        Initialize();
        TestSequeceNumberService(-1, testArray);
    }
}

