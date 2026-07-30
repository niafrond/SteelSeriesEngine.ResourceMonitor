using SteelSeries.SysMonitor;

using var mutex = new Mutex(initiallyOwned: true, "Global\\SteelSeries.SysMonitor.SingleInstance", out var createdNew);
if (!createdNew)
{
    MessageBox.Show(
        "SteelSeries Resource Monitor est déjà en cours d'exécution (voir la barre des tâches).",
        "SteelSeries Resource Monitor",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    return;
}

ApplicationConfiguration.Initialize();
Application.Run(new TrayApplicationContext());
