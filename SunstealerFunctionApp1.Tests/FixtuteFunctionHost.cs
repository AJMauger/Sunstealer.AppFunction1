using System.Diagnostics;

namespace Sunslealer.FunctionApp1.Tests;

/// <summary>
/// The FixtureFunctionHost creates a instance of the SunstealerFunctionApp1 Function Host that allows Azure Function Framework to be tested via XUnit Tests.
/// </summary>
public class FixtureFunctionHost : IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    public string _baseUrl { get; }
    /// <summary>
    /// 
    /// </summary>
    private string _build { get; set; } = "Release";
    /// <summary>
    /// 
    /// </summary>
    public HttpClient _client { get; }
    /// <summary>
    /// 
    /// </summary>
    private Process _hostProcess;
    /// <summary>
    /// See lauchSettings.json.
    /// </summary>
    private ushort _port { get; } = 7079;

    /// <summary>
    /// 
    /// </summary>
    public FixtureFunctionHost()
    {
        _baseUrl = $"http://localhost:{_port}";

#if DEBUG
        _build = "Debug";
#endif
        var localAppData = Environment.GetEnvironmentVariable("LocalAppData");

        if (string.IsNullOrEmpty(localAppData) || localAppData.Contains(".."))
        {
            throw new ArgumentException("LocalAppData");
        }

        var functionHostFolder = $@"{localAppData}\AzureFunctionsTools\Releases\4.132.0\cli_x64";
        var version = $"{Environment.Version.Major}.{Environment.Version.Minor}";

        var startInfo = new ProcessStartInfo
        {
            // ajm: FileName = "cmd.exe",
            // ajm: Arguments = @$"/K {functionHostFolder}\func.exe host start --port {_port}",

            FileName = @$"{functionHostFolder}\func.exe",
            // ajm: Arguments = @$"host start --port {_port}",

            WorkingDirectory = AppContext.BaseDirectory.Replace(".Tests", ""),
            UseShellExecute = false,
            CreateNoWindow = false
        };

        startInfo.ArgumentList.Add("host");
        startInfo.ArgumentList.Add("start");
        startInfo.ArgumentList.Add("--port");
        startInfo.ArgumentList.Add(_port.ToString());

        _hostProcess = new Process
        {
            StartInfo = startInfo
        };

        Assert.NotNull(_hostProcess);

        _client = new HttpClient();
    }

    /// <summary>
    /// 
    /// </summary>
    public async Task Initialize()
    {
        var b = _hostProcess.Start();
        Assert.True(b);

        bool initialized = false;
        for (int i = 0; i < 10; i++)
        {
            try
            {
                var response = await _client.GetAsync($"http://localhost:{_port}");
                initialized = true;
                break;
            }
            catch (HttpRequestException)
            {
                await Task.Delay(1000);
            }
        }
        Assert.True(initialized);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        if (!_hostProcess.HasExited)
        {
            _hostProcess.Kill(entireProcessTree: true);
        }
        _client.Dispose();
        _hostProcess.Dispose();
    }
}
