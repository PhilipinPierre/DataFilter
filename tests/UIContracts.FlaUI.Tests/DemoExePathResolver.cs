using System.Diagnostics;

namespace UIContracts.FlaUI.Tests;

internal readonly record struct DemoExeSpec(
    string ProjectRelativePath,
    string ExeFileName,
    string TargetFramework,
    string? Platform = null);

internal static class DemoExePathResolver
{
    public static string ResolveOrBuild(DemoExeSpec spec)
    {
        var root = FindRepoRoot();
        var configuration = InferTestConfiguration();

        foreach (var config in OrderConfigurations(configuration))
        {
            var exe = GetExePath(root, spec, config);
            if (File.Exists(exe))
                return exe;
        }

        TryBuild(root, spec, configuration);

        foreach (var config in OrderConfigurations(configuration))
        {
            var exe = GetExePath(root, spec, config);
            if (File.Exists(exe))
                return exe;
        }

        var expected = GetExePath(root, spec, configuration);
        throw new FileNotFoundException(
            $"Demo executable not found at '{expected}'. Build failed or output path changed. " +
            $"Try: dotnet build \"{Path.Combine(root, spec.ProjectRelativePath)}\" -c {configuration}" +
            (string.IsNullOrEmpty(spec.Platform) ? "" : $" -p:Platform={spec.Platform}"),
            expected);
    }

    private static IEnumerable<string> OrderConfigurations(string preferred)
    {
        yield return preferred;
        yield return preferred.Equals("Release", StringComparison.OrdinalIgnoreCase) ? "Debug" : "Release";
    }

    private static string InferTestConfiguration()
    {
        var baseDir = AppContext.BaseDirectory.Replace('\\', '/');
        if (baseDir.Contains("/bin/Release/", StringComparison.OrdinalIgnoreCase))
            return "Release";
        if (baseDir.Contains("/bin/Debug/", StringComparison.OrdinalIgnoreCase))
            return "Debug";
        return "Debug";
    }

    private static string GetExePath(string repoRoot, DemoExeSpec spec, string configuration)
    {
        var parts = new List<string> { repoRoot };
        foreach (var segment in spec.ProjectRelativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            if (!string.IsNullOrEmpty(segment))
                parts.Add(segment);
        }

        // Project path is demo/.../Project.csproj — output is under demo/.../bin/
        parts.RemoveAt(parts.Count - 1);
        parts.Add("bin");
        if (!string.IsNullOrEmpty(spec.Platform))
            parts.Add(spec.Platform);
        parts.Add(configuration);
        parts.Add(spec.TargetFramework);
        parts.Add(spec.ExeFileName);
        return Path.Combine(parts.ToArray());
    }

    private static void TryBuild(string repoRoot, DemoExeSpec spec, string configuration)
    {
        var projectPath = Path.Combine(repoRoot, spec.ProjectRelativePath);
        if (!File.Exists(projectPath))
            throw new FileNotFoundException("Demo project not found.", projectPath);

        var args = $"build \"{projectPath}\" -c {configuration}";
        if (!string.IsNullOrEmpty(spec.Platform))
            args += $" -p:Platform={spec.Platform}";

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = repoRoot
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet build.");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit(TimeSpan.FromMinutes(10));

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet build failed ({process.ExitCode}) for '{spec.ProjectRelativePath}'.{Environment.NewLine}{stderr}{Environment.NewLine}{stdout}");
        }
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(dir))
        {
            if (File.Exists(Path.Combine(dir, "DataFilter.slnx")))
                return dir;
            dir = Directory.GetParent(dir)?.FullName ?? string.Empty;
        }

        throw new InvalidOperationException("Repo root not found (DataFilter.slnx).");
    }
}
