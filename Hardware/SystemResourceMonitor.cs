using System.Diagnostics;
using System.Runtime.InteropServices;
using LibreHardwareMonitor.Hardware;

namespace SteelSeries.SysMonitor.Hardware
{
    public readonly record struct ResourceStat(string Label, float Percent, string Extra);

    public static class SystemResourceMonitor
    {
        private static Computer _computer = null!;
        private static PerformanceCounter _diskTimeCounter = null!;

        public static void Initialize()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMotherboardEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
                IsNetworkEnabled = true
            };
            _computer.Open();

            _diskTimeCounter = new PerformanceCounter("LogicalDisk", "% Disk Time", "C:", true);
            _diskTimeCounter.NextValue();
        }

        public static ResourceStat GetCpu()
        {
            float cpuLoad = 0, cpuTemp = 0;
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "CPU Total")
                            cpuLoad = sensor.Value.GetValueOrDefault();
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            if (sensor.Name.Contains("Core"))
                                cpuTemp = sensor.Value.GetValueOrDefault();
                            else if (cpuTemp == 0 && sensor.Value.HasValue)
                                cpuTemp = sensor.Value.Value;
                        }
                    }
                }
            }
            return new ResourceStat("CPU", cpuLoad, $"{cpuTemp:0}C");
        }

        public static ResourceStat GetRam()
        {
            float ramLoad = 0, usedGb = 0, availableGb = 0;
            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.Memory)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "Memory")
                            ramLoad = sensor.Value.GetValueOrDefault();
                        if (sensor.SensorType == SensorType.Data && sensor.Name == "Memory Used")
                            usedGb = sensor.Value.GetValueOrDefault();
                        if (sensor.SensorType == SensorType.Data && sensor.Name == "Memory Available")
                            availableGb = sensor.Value.GetValueOrDefault();
                    }
                }
            }
            float totalGb = usedGb + availableGb;
            return new ResourceStat("RAM", ramLoad, $"{usedGb:0.0}/{totalGb:0.0}GB");
        }

        public static ResourceStat GetGpu()
        {
            float gpuLoad = 0, gpuTemp = 0;

            foreach (var hardware in _computer.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name == "GPU Core")
                            gpuLoad = sensor.Value.GetValueOrDefault();
                        if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                            gpuTemp = sensor.Value.GetValueOrDefault();
                    }
                }
            }
            return new ResourceStat("GPU", gpuLoad, $"{gpuTemp:0}C");
        }

        public static ResourceStat GetDisk()
        {
            float percent = Math.Clamp(_diskTimeCounter.NextValue(), 0, 100);
            return new ResourceStat("C:", percent, "BUSY");
        }

        public static int GetBatteryPercent()
        {
            try
            {
                var percent = System.Windows.Forms.SystemInformation.PowerStatus.BatteryLifePercent;
                if (percent <= 1f)
                    percent *= 100f;

                return (int)Math.Clamp(Math.Round(percent), 0, 100);
            }
            catch
            {
                return 0;
            }
        }

        public static int GetVolumePercent()
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
                device.Activate(typeof(IAudioEndpointVolume).GUID, 0, IntPtr.Zero, out var volumeObj);
                var endpointVolume = (IAudioEndpointVolume)volumeObj!;
                endpointVolume.GetMasterVolumeLevelScalar(out var level);
                return (int)Math.Clamp(Math.Round(level * 100f), 0, 100);
            }
            catch
            {
                return 0;
            }
        }

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out IMMDeviceCollection ppDevices);
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
            int GetDevice(string pwstrId, out IMMDevice ppDevice);
            int RegisterEndpointNotificationCallback(IMMNotificationClient pClient);
            int UnregisterEndpointNotificationCallback(IMMNotificationClient pClient);
        }

        [Guid("E3F0B43E-2D06-4B3B-9873-0D6B2B88C24C")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceCollection
        {
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        }

        [Guid("5CDF2C82-841E-4546-9722-0D87059F2A4D")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            int GetMasterVolumeLevelScalar(out float pfLevel);
        }

        [Guid("E2F5D6F2-4D14-4B1B-8B83-6A6A0D25D14D")]
        private interface IMMNotificationClient
        {
        }

        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2,
            eDataFlow_enum_count = 3
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2,
            eRole_enum_count = 3
        }
    }
}
