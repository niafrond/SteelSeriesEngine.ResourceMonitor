using SteelSeries.SysMonitor.Hardware;
using SteelSeries.SysMonitor.Helpers;
using Timer = System.Threading.Timer;

namespace SteelSeries.SysMonitor.Display;

public class OledController
{
    private static readonly string _framePath = Path.Combine(Path.GetTempPath(), "steelseries-resource-monitor-frame.png");

    private static string? _ggoledPath;
    private static Timer? _timer;
    private static int _updating;

    public static async Task<bool> StartMonitoringAsync()
    {
        _ggoledPath = Utilities.FindGgoledExecutable();

        if (_ggoledPath is null)
        {
            Console.WriteLine("ggoled.exe introuvable.");
            Console.WriteLine("Telecharge-le sur https://github.com/JerwuQu/ggoled/releases/latest");
            Console.WriteLine("et place-le a cote de cet executable, ou ajoute-le a ton PATH.");
            return false;
        }

        Console.WriteLine($"Using ggoled: {_ggoledPath}");

        SystemResourceMonitor.Initialize();

        await UpdateScreen();

        _timer = new Timer(async _ => await UpdateScreen(), null, 500, 500);
        return true;
    }

    public static void StopMonitoring()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private static async Task UpdateScreen()
    {
        if (Interlocked.Exchange(ref _updating, 1) == 1)
            return;

        try
        {
            var cpu = SystemResourceMonitor.GetCpu();
            var ram = SystemResourceMonitor.GetRam();
            var gpu = SystemResourceMonitor.GetGpu();
            var disk = SystemResourceMonitor.GetDisk();

            OledFrameRenderer.Render(_framePath, cpu, ram, gpu, disk);

            await Utilities.RunGgoledAsync(_ggoledPath!, "img", "-x", "0", "-y", "0", "-C", _framePath);
            Console.WriteLine($"Sent: {cpu.Label} {cpu.Percent:0}% | {ram.Label} {ram.Percent:0}% | {gpu.Label} {gpu.Percent:0}% | {disk.Label} {disk.Percent:0}%");
        }
        finally
        {
            Interlocked.Exchange(ref _updating, 0);
        }
    }
}
