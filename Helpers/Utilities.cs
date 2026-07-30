using System.Diagnostics;

namespace SteelSeries.SysMonitor.Helpers
{
    public static class Utilities
    {
        public static string? FindGgoledExecutable()
        {
            var localPath = Path.Combine(AppContext.BaseDirectory, "ggoled.exe");
            if (File.Exists(localPath))
                return localPath;

            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (var dir in pathEnv.Split(Path.PathSeparator))
            {
                var candidate = Path.Combine(dir, "ggoled.exe");
                if (File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        public static async Task RunGgoledAsync(string exePath, params string[] args)
        {
            var startInfo = new ProcessStartInfo(exePath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };
            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            using var process = Process.Start(startInfo);
            if (process is null)
                return;

            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                var err = await process.StandardError.ReadToEndAsync();
                Console.WriteLine($"ggoled ERR ({process.ExitCode}): {err}");
            }
        }
    }
}
