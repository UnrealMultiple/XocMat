using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Lagrange.XocMat.Utility;

public sealed class SystemMonitor : IDisposable
{
    #region 基础字段和属性
    private Timer _timer;
    private NetworkUsageData _prevNetworkData;
    private CpuUsageData _prevCpuData;
    private long _prevNetworkTimestamp;

    // 监控属性
    public float CpuUsagePercent { get; private set; }
    public float MemoryUsagePercent { get; private set; }
    public long TotalPhysicalMemory { get; private set; }
    public long UsedPhysicalMemory { get; private set; }
    public long AvailablePhysicalMemory { get; private set; }
    public float NetworkUploadKbps { get; private set; }
    public float NetworkDownloadKbps { get; private set; }
    #endregion

    #region 初始化与核心监控逻辑
    public SystemMonitor(int updateIntervalMs = 1000)
    {
        InitializeMemory();
        _prevCpuData = GetCpuData();
        _prevNetworkTimestamp = Stopwatch.GetTimestamp();
        _prevNetworkData = new NetworkUsageData();
        _timer = new Timer(UpdateMetrics, null, 0, updateIntervalMs);
    }

    private void UpdateMetrics(object? state)
    {
        try
        {
            UpdateCpuUsage();
            UpdateMemoryUsage();
            UpdateNetworkUsage();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[监控错误] {ex.Message}");
        }
    }
    #endregion

    #region CPU监控实现
    private struct CpuUsageData
    {
        public ulong TotalTime;
        public ulong IdleTime;
    }

    private void UpdateCpuUsage()
    {
        var current = GetCpuData();
        var totalDiff = current.TotalTime - _prevCpuData.TotalTime;
        var idleDiff = current.IdleTime - _prevCpuData.IdleTime;

        if (totalDiff > 0 && idleDiff <= totalDiff)
        {
            CpuUsagePercent = (float)((totalDiff - idleDiff) * 100.0 / totalDiff);
            CpuUsagePercent = Math.Clamp(CpuUsagePercent, 0, 100);
        }
        _prevCpuData = current;
    }

    private CpuUsageData GetCpuData()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return GetWindowsCpuData();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return GetLinuxCpuData();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return GetMacCpuData();
        throw new PlatformNotSupportedException();
    }

    // Windows实现
    [StructLayout(LayoutKind.Sequential)]
    struct FILETIME { public uint dwLowDateTime; public uint dwHighDateTime; }

    [DllImport("kernel32.dll")]
    static extern bool GetSystemTimes(out FILETIME idle, out FILETIME kernel, out FILETIME user);

    private static CpuUsageData GetWindowsCpuData()
    {
        GetSystemTimes(out var idle, out var kernel, out var user);
        return new CpuUsageData
        {
            IdleTime = ((ulong)idle.dwHighDateTime << 32) | idle.dwLowDateTime,
            TotalTime = (((ulong)kernel.dwHighDateTime << 32) | kernel.dwLowDateTime) +
                       (((ulong)user.dwHighDateTime << 32) | user.dwLowDateTime)
        };
    }

    // Linux实现
    private static CpuUsageData GetLinuxCpuData()
    {
        var lines = File.ReadAllLines("/proc/stat");
        var values = lines.First(l => l.StartsWith("cpu ")).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return new CpuUsageData
        {
            TotalTime = values.Skip(1).Take(3).Select(ulong.Parse).Aggregate((a, b) => a + b),
            IdleTime = ulong.Parse(values[4])
        };
    }

    // macOS实现
    [DllImport("libSystem.dylib")]
    static extern int sysctlbyname(string name, IntPtr ptr, ref uint size, IntPtr newp, uint newlen);

    private static CpuUsageData GetMacCpuData()
    {
        const string name = "kern.cp_time";
        var size = (uint)Marshal.SizeOf<CpuUsageData>();
        var ptr = Marshal.AllocHGlobal((int)size);
        try
        {
            if (sysctlbyname(name, ptr, ref size, IntPtr.Zero, 0) == 0)
                return Marshal.PtrToStructure<CpuUsageData>(ptr);
            throw new Win32Exception();
        }
        finally { Marshal.FreeHGlobal(ptr); }
    }
    #endregion

    #region 内存监控实现
    private void InitializeMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) InitWindowsMemory();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) InitLinuxMemory();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) InitMacMemory();
    }

    private void UpdateMemoryUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) UpdateWindowsMemory();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) UpdateLinuxMemory();
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) UpdateMacMemory();

        UsedPhysicalMemory = TotalPhysicalMemory - AvailablePhysicalMemory;
        if (TotalPhysicalMemory > 0)
            MemoryUsagePercent = (float)(UsedPhysicalMemory * 100.0 / TotalPhysicalMemory);
    }

    // Windows内存实现
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
        public MEMORYSTATUSEX() => dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

    private void InitWindowsMemory()
    {
        var memStatus = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(memStatus))
        {
            TotalPhysicalMemory = (long)memStatus.ullTotalPhys;
            AvailablePhysicalMemory = (long)memStatus.ullAvailPhys;
        }
    }

    private void UpdateWindowsMemory()
    {
        var memStatus = new MEMORYSTATUSEX();
        if (GlobalMemoryStatusEx(memStatus))
            AvailablePhysicalMemory = (long)memStatus.ullAvailPhys;
    }

    // Linux内存实现
    private void InitLinuxMemory()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            TotalPhysicalMemory = GetMemValue(lines, "MemTotal") * 1024;
            AvailablePhysicalMemory = GetMemValue(lines, "MemAvailable") * 1024;
        }
        catch (Exception ex) { Debug.WriteLine($"[Linux内存初始化错误] {ex.Message}"); }
    }

    private void UpdateLinuxMemory()
    {
        try
        {
            var lines = File.ReadAllLines("/proc/meminfo");
            var memAvailable = lines.FirstOrDefault(l => l.StartsWith("MemAvailable"));
            if (memAvailable != null)
            {
                AvailablePhysicalMemory = GetMemValue(lines, "MemAvailable") * 1024;
            }
            else
            {
                // 如果没有MemAvailable，则使用MemFree + Buffers + Cached
                AvailablePhysicalMemory = (GetMemValue(lines, "MemFree") +
                                         GetMemValue(lines, "Buffers") +
                                         GetMemValue(lines, "Cached")) * 1024;
            }
        }
        catch (Exception ex) { Debug.WriteLine($"[Linux内存更新错误] {ex.Message}"); }
    }

    // macOS内存实现
    [DllImport("libSystem.dylib")]
    static extern int sysctlbyname(string name, out long value, ref IntPtr size, IntPtr newp, uint newlen);

    private void InitMacMemory()
    {
        try
        {
            // 获取总内存
            IntPtr len = (IntPtr)sizeof(long);
            if (sysctlbyname("hw.memsize", out long total, ref len, IntPtr.Zero, 0) == 0)
                TotalPhysicalMemory = total;

            // 初始化可用内存
            UpdateMacMemory();
        }
        catch (Exception ex) { Debug.WriteLine($"[macOS内存初始化错误] {ex.Message}"); }
    }

    private void UpdateMacMemory()
    {
        try
        {
            var psi = new ProcessStartInfo("vm_stat") { RedirectStandardOutput = true };
            using var process = Process.Start(psi);
            var output = process?.StandardOutput.ReadToEnd();
            process?.WaitForExit();

            if (string.IsNullOrEmpty(output)) return;

            var lines = output.Split('\n');
            var free = GetMacMemValue(lines, "Pages free");
            var speculative = GetMacMemValue(lines, "Pages speculative");
            var pageSize = GetPageSize();
            AvailablePhysicalMemory = (free + speculative) * pageSize;
        }
        catch (Exception ex) { Debug.WriteLine($"[macOS内存更新错误] {ex.Message}"); }
    }

    private static long GetMemValue(string[] lines, string key)
    {
        var line = lines.FirstOrDefault(l => l.StartsWith(key));
        if (line == null) return 0;

        var parts = line.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return 0;

        return long.TryParse(parts[1], out var value) ? value : 0;
    }

    private static long GetMacMemValue(string[] lines, string key)
    {
        var line = lines.FirstOrDefault(l => l.Contains(key));
        if (line == null) return 0;

        var parts = line.Split(':');
        if (parts.Length < 2) return 0;

        var valueStr = parts[1].Trim().Split('.')[0];
        return long.TryParse(valueStr, out var value) ? value : 0;
    }

    private static long GetPageSize()
    {
        IntPtr len = (IntPtr)sizeof(long);
        _ = sysctlbyname("hw.pagesize", out long size, ref len, IntPtr.Zero, 0);
        return size;
    }
    #endregion

    #region 网络监控实现
    private struct NetworkUsageData { public long ReceivedBytes; public long SentBytes; }

    private void UpdateNetworkUsage()
    {
        try
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => !n.Description.Contains("Virtual"))
                .Where(n => !n.Description.Contains("Pseudo"))
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            var current = new NetworkUsageData
            {
                ReceivedBytes = interfaces.Sum(n => n.GetIPStatistics().BytesReceived),
                SentBytes = interfaces.Sum(n => n.GetIPStatistics().BytesSent)
            };

            var timestamp = Stopwatch.GetTimestamp();
            var elapsed = (timestamp - _prevNetworkTimestamp) / (double)Stopwatch.Frequency;

            if (elapsed > 0.5 && _prevNetworkData.ReceivedBytes != 0)
            {
                NetworkDownloadKbps = (float)((current.ReceivedBytes - _prevNetworkData.ReceivedBytes) / elapsed / 1024);
                NetworkUploadKbps = (float)((current.SentBytes - _prevNetworkData.SentBytes) / elapsed / 1024);
            }

            _prevNetworkData = current;
            _prevNetworkTimestamp = timestamp;
        }
        catch (Exception ex) { Debug.WriteLine($"[网络监控错误] {ex.Message}"); }
    }
    #endregion

    #region 内存清理功能
    public class CleanResult
    {
        public bool Success { get; set; }
        public long FreedBytes { get; set; }
        public double BeforeUsage { get; set; }
        public double AfterUsage { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public CleanResult CleanMemory()
    {
        var result = new CleanResult();
        try
        {
            result.BeforeUsage = MemoryUsagePercent;
            var before = AvailablePhysicalMemory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) CleanWindowsMemory();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) CleanLinuxMemory();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) CleanMacMemory();

            Thread.Sleep(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 1000 : 500);
            UpdateMemoryUsage();

            result.FreedBytes = AvailablePhysicalMemory - before;
            result.AfterUsage = MemoryUsagePercent;
            result.Success = true;
            result.Message = FormatCleanMessage(result.FreedBytes);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"清理失败: {GetErrorMessage(ex)}";
        }
        return result;
    }

    [DllImport("psapi.dll")]
    static extern bool EmptyWorkingSet(IntPtr hProcess);

    [DllImport("kernel32.dll")]
    static extern bool SetProcessWorkingSetSize(IntPtr process, int min, int max);

    private void CleanWindowsMemory()
    {
        if (!SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1))
            throw new Win32Exception(Marshal.GetLastWin32Error());

        foreach (var p in Process.GetProcesses())
        {
            try { if (p.Handle != IntPtr.Zero) EmptyWorkingSet(p.Handle); }
            catch { /* 忽略权限问题 */ }
        }
    }

    private void CleanLinuxMemory()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = "-c \"sync; echo 3 > /proc/sys/vm/drop_caches\"",
                UseShellExecute = false,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };
        process.Start();
        process.WaitForExit(2000);
        if (process.ExitCode != 0)
            throw new Exception(process.StandardError.ReadToEnd());
    }

    [DllImport("libSystem.dylib")]
    static extern int malloc_zone_pressure_relief(int zone, int percent);

    private void CleanMacMemory()
    {
        if (malloc_zone_pressure_relief(-1, 100) != 0)
            throw new Exception("内存压缩失败");

        using var process = Process.Start(new ProcessStartInfo("purge") { UseShellExecute = true });
        process?.WaitForExit(1000);
    }

    private string FormatCleanMessage(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB" };
        int i = 0;
        double size = bytes;
        while (size >= 1024 && i < units.Length - 1) { size /= 1024; i++; }
        return bytes > 0 ? $"释放 {size:0.##} {units[i]}" : "无显著内存释放";
    }

    private string GetErrorMessage(Exception ex)
    {
        if (ex is Win32Exception winEx && winEx.NativeErrorCode == 5)
            return "需要管理员权限";
        return ex.Message;
    }
    #endregion

    #region 资源清理
    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}