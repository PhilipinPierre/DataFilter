using System.Diagnostics;
using System.Net;

namespace DataFilter.Blazor.Demo.PlaywrightTests;

public enum DemoHostKind
{
    Server,
    WasmHosted
}

public sealed class DemoHostFixture : IAsyncLifetime
{
    private Process? _process;
    private string _repoRoot = "";
    private readonly Queue<string> _recentLogs = new();
    private readonly object _logLock = new();

    public DemoHostKind HostKind { get; }
    public string BaseUrl { get; private set; } = "";

    public DemoHostFixture()
    {
        HostKind = ResolveHostKind();
    }

    public async Task InitializeAsync()
    {
        _repoRoot = FindRepoRoot();

        var httpPort = GetFreeTcpPort();
        BaseUrl = $"http://127.0.0.1:{httpPort}";

        var projectPath = HostKind switch
        {
            DemoHostKind.Server => Path.Combine(_repoRoot, "demo", "DataFilter.Blazor.Demo.Server", "DataFilter.Blazor.Demo.Server.csproj"),
            DemoHostKind.WasmHosted => Path.Combine(_repoRoot, "demo", "DataFilter.Blazor.Demo.Wasm", "DataFilter.Blazor.Demo.Wasm", "DataFilter.Blazor.Demo.Wasm.csproj"),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!File.Exists(projectPath))
            throw new FileNotFoundException("Could not locate demo host project.", projectPath);

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{projectPath}\" -c Release --no-launch-profile --urls \"{BaseUrl}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["ASPNETCORE_DETAILEDERRORS"] = "true";
        psi.Environment["Logging__LogLevel__Default"] = "Debug";
        psi.Environment["Logging__LogLevel__Microsoft_AspNetCore_Components"] = "Debug";
        psi.Environment["Logging__LogLevel__Microsoft_AspNetCore_Components_Server"] = "Debug";
        psi.Environment["Logging__LogLevel__Microsoft.AspNetCore.Components.Server.Circuits"] = "Debug";
        psi.Environment["Logging__LogLevel__Microsoft.AspNetCore.SignalR"] = "Debug";
        psi.Environment["Logging__LogLevel__Microsoft_AspNetCore_SignalR"] = "Debug";
        // Avoid redirecting to HTTPS when tests run on HTTP only.
        psi.Environment["ASPNETCORE_HTTPS_PORT"] = "";
        psi.Environment["DF_E2E"] = "1";

        _process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start demo host process.");

        // Avoid deadlocks: drain output in background.
        _ = Task.Run(async () =>
        {
            try
            {
                while (_process != null && !_process.HasExited)
                {
                    var line = await _process.StandardOutput.ReadLineAsync();
                    if (line != null) AddLog("OUT", line);
                }
            }
            catch { }
        });
        _ = Task.Run(async () =>
        {
            try
            {
                while (_process != null && !_process.HasExited)
                {
                    var line = await _process.StandardError.ReadLineAsync();
                    if (line != null) AddLog("ERR", line);
                }
            }
            catch { }
        });

        await WaitUntilReadyAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (_process == null)
                return;

            if (!_process.HasExited)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        finally
        {
            _process?.Dispose();
            _process = null;
        }
    }

    private async Task WaitUntilReadyAsync()
    {
        var attachUrl = BaseUrl.TrimEnd('/') + "/demo/attach";

        using var http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(2)
        };

        var start = DateTimeOffset.UtcNow;
        var timeout = TimeSpan.FromSeconds(60);

        while (DateTimeOffset.UtcNow - start < timeout)
        {
            if (_process is { HasExited: true })
                throw new InvalidOperationException(
                    $"Demo host exited early (exit code {_process.ExitCode}).{Environment.NewLine}RecentHostLogs:{Environment.NewLine}{GetRecentLogs()}");

            try
            {
                using var resp = await http.GetAsync(attachUrl);
                if (resp.StatusCode == HttpStatusCode.OK)
                    return;
            }
            catch
            {
                // ignored: still starting
            }

            await Task.Delay(250);
        }

        throw new TimeoutException($"Demo host did not become ready within {timeout.TotalSeconds} seconds: {attachUrl}");
    }

    private static DemoHostKind ResolveHostKind()
    {
        var raw = (Environment.GetEnvironmentVariable("DF_DEMO_HOST") ?? "").Trim();
        return raw.ToLowerInvariant() switch
        {
            "server" => DemoHostKind.Server,
            "wasm" => DemoHostKind.WasmHosted,
            "wasmhosted" => DemoHostKind.WasmHosted,
            "" => DemoHostKind.Server,
            _ => throw new InvalidOperationException("Unknown DF_DEMO_HOST. Expected: server | wasm")
        };
    }

    private static int GetFreeTcpPort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var slnx = Path.Combine(dir.FullName, "DataFilter.slnx");
            if (File.Exists(slnx))
                return dir.FullName;

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root (DataFilter.slnx) from test output directory.");
    }

    public string GetRecentLogs()
    {
        lock (_logLock)
        {
            return string.Join(Environment.NewLine, _recentLogs);
        }
    }

    private void AddLog(string stream, string line)
    {
        lock (_logLock)
        {
            _recentLogs.Enqueue($"[{stream}] {line}");
            while (_recentLogs.Count > 200)
                _recentLogs.Dequeue();
        }
    }
}

