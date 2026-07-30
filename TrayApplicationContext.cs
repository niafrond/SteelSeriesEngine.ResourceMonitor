using SteelSeries.SysMonitor.Display;
using SteelSeries.SysMonitor.Helpers;

namespace SteelSeries.SysMonitor;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _startupMenuItem;

    public TrayApplicationContext()
    {
        _startupMenuItem = new ToolStripMenuItem("Démarrer avec Windows")
        {
            CheckOnClick = true,
            Checked = StartupManager.IsEnabled()
        };
        _startupMenuItem.Click += OnToggleStartup;

        var exitMenuItem = new ToolStripMenuItem("Quitter");
        exitMenuItem.Click += OnExit;

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_startupMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add(exitMenuItem);

        _notifyIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            Text = "SteelSeries Resource Monitor",
            ContextMenuStrip = contextMenu,
            Visible = true
        };

        _ = StartAsync();
    }

    private async Task StartAsync()
    {
        var started = await OledController.StartMonitoringAsync();
        if (!started)
        {
            _notifyIcon.ShowBalloonTip(
                5000,
                "SteelSeries Resource Monitor",
                "ggoled.exe est introuvable. Place-le à côté de l'exécutable ou ajoute-le au PATH, puis relance l'application.",
                ToolTipIcon.Warning);
        }
    }

    private void OnToggleStartup(object? sender, EventArgs e)
    {
        StartupManager.SetEnabled(_startupMenuItem.Checked);
    }

    private void OnExit(object? sender, EventArgs e)
    {
        OledController.StopMonitoring();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        ExitThread();
    }
}
