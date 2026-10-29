
using static System.Runtime.InteropServices.JavaScript.JSType;

/// <summary>
/// 
/// </summary>
public class SequenceNumberService
{
    private const int DailyPoolSize = 999_999;  // ajm: acs
    private const string StateFilePath = "sequence_state.txt";

    private List<int> _shuffledDeck = new List<int>();
    private int _index;
    private DateTime _date;
    private readonly object _criticalSection = new object();

    /// <summary>
    /// 
    /// </summary>
    public SequenceNumberService()
    {
        lock (_criticalSection)
        {
            Initialize();
        }
    }

    private void Initialize()
    {
        _date = DateTime.UtcNow.Date;

        int daySeed = _date.GetHashCode();
        Random rand = new Random(daySeed);

        _shuffledDeck = new List<int>(DailyPoolSize);
        for (int i = 1; i <= DailyPoolSize; i++)
        {
            _shuffledDeck.Add(i);
        }

        for (int i = _shuffledDeck.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            int k = _shuffledDeck[i];
            _shuffledDeck[i] = _shuffledDeck[j];
            _shuffledDeck[j] = k;
        }

        // acs { day: _date.ToString("yyyy-MM-dd"), index: _index.ToString() }
        if (File.Exists(StateFilePath))
        {
            string[] lines = File.ReadAllLines(StateFilePath);
            if (lines.Length >= 2 && DateTime.TryParse(lines[0], out DateTime savedDate) && savedDate == _date)
            {
                if (int.TryParse(lines[1], out int savedIndex))
                {
                    _index = savedIndex;
                    return;
                }
            }
        }

        _index = 0;
        PersistState();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int Next()
    {
        lock (_criticalSection)
        {
            if (DateTime.UtcNow.Date != _date)
            {
                Initialize();
            }

            if (_index >= DailyPoolSize)
            {
                throw new InvalidOperationException("Daily limit of 999,999 sequence numbers has been exhausted.");
            }

            int uniqueId = _shuffledDeck[_index];

            _index++;
            PersistState();

            return uniqueId;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void PersistState()
    {
        string[] stateData = { _date.ToString("yyyy-MM-dd"), _index.ToString() };
        File.WriteAllLines(StateFilePath, stateData);
    }

}

