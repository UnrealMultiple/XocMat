using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using Lagrange.XocMat.Extensions;
using Newtonsoft.Json.Linq;

namespace Lagrange.XocMat.Utility;

public class SystemHelper
{

    [DllImport("psapi.dll")]
    private static extern bool EmptyWorkingSet(IntPtr lpAddress);

    public static void FreeMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        foreach (Process process in Process.GetProcesses())
        {
            if ((process.ProcessName == "System") && (process.ProcessName == "Idle"))
                continue;
            try
            {
                EmptyWorkingSet(process.Handle);
            }
            catch { }
        }
    }

    #region 获得内存信息API
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref MEMORY_INFO mi);

    //定义内存的信息结构
    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_INFO
    {
        public uint dwLength; //当前结构体大小
        public uint dwMemoryLoad; //当前内存使用率
        public ulong ullTotalPhys; //总计物理内存大小
        public ulong ullAvailPhys; //可用物理内存大小
        public ulong ullTotalPageFile; //总计交换文件大小
        public ulong ullAvailPageFile; //总计交换文件大小
        public ulong ullTotalVirtual; //总计虚拟内存大小
        public ulong ullAvailVirtual; //可用虚拟内存大小
        public ulong ullAvailExtendedVirtual; //保留 这个值始终为0
    }
    #endregion

    #region 格式化容量大小
    /// <summary>
    /// 格式化容量大小
    /// </summary>
    /// <param name="size">容量（B）</param>
    /// <returns>已格式化的容量</returns>
    public static string FormatSize(double size)
    {
        double d = (double)size;
        int i = 0;
        while ((d > 1024) && (i < 5))
        {
            d /= 1024;
            i++;
        }
        string[] unit = { "B", "KB", "MB", "GB", "TB" };
        return string.Format("{0} {1}", Math.Round(d, 2), unit[i]);
    }
    #endregion

    #region 获得当前内存使用情况
    /// <summary>
    /// 获得当前内存使用情况
    /// </summary>
    /// <returns></returns>
    public static MEMORY_INFO GetMemoryStatus()
    {
        MEMORY_INFO mi = new MEMORY_INFO();
        mi.dwLength = (uint)System.Runtime.InteropServices.Marshal.SizeOf(mi);
        GlobalMemoryStatusEx(ref mi);
        return mi;
    }
    #endregion

    #region 获得当前可用物理内存大小
    /// <summary>
    /// 获得当前可用物理内存大小
    /// </summary>
    /// <returns>当前可用物理内存（B）</returns>
    public static ulong GetAvailPhys()
    {
        MEMORY_INFO mi = GetMemoryStatus();
        return mi.ullAvailPhys;
    }
    #endregion

    #region 获得当前已使用的内存大小
    /// <summary>
    /// 获得当前已使用的内存大小
    /// </summary>
    /// <returns>已使用的内存大小（B）</returns>
    public static ulong GetUsedPhys()
    {
        MEMORY_INFO mi = GetMemoryStatus();
        return mi.ullTotalPhys - mi.ullAvailPhys;
    }
    #endregion

    #region 获得当前总计物理内存大小
    /// <summary>
    /// 获得当前总计物理内存大小
    /// </summary>
    /// <returns&gt;总计物理内存大小（B）&lt;/returns&gt;
    public static ulong GetTotalPhys()
    {
        MEMORY_INFO mi = GetMemoryStatus();
        return mi.ullTotalPhys;
    }
    #endregion

    public static void KillChrome()
    {
        foreach (Process process in Process.GetProcesses())
        {
            if (process.ProcessName.Contains("chrome") && CanAccessProcess(process))
            {
                process.Kill();
            }
        }
    }

    public static bool CanAccessProcess(Process process)
    {
        try
        {
            // 尝试访问进程的一个需要权限的属性
            ProcessModuleCollection modules = process.Modules;
            return true;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // 拒绝访问
        {
            return false;
        }
        catch // 其他异常，如进程已退出
        {
            return false;
        }
    }

    public static Internal.Socket.Internet.Item? GetItemById(int id)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string file = "Lagrange.XocMat.Resources.Json.TerrariaID.json";
        Stream stream = assembly.GetManifestResourceStream(file)!;
        using StreamReader reader = new StreamReader(stream);
        JObject jobj = reader.ReadToEnd().ToObject<JObject>()!;
        JArray array = (JArray)jobj["物品"]!;
        foreach (JToken item in array)
        {
            if (item != null && item["ID"]!.Value<int>() == id)
            {
                return new()
                {
                    Name = item["中文名称"]!.Value<string>()!,
                    netID = id
                };
            }
        }
        return null;
    }

    public static List<Internal.Socket.Internet.Item> GetItemByName(string name)
    {
        List<Internal.Socket.Internet.Item> list = [];
        Assembly assembly = Assembly.GetExecutingAssembly();
        string file = "Lagrange.XocMat.Resources.Json.TerrariaID.json";
        Stream stream = assembly.GetManifestResourceStream(file)!;
        using StreamReader reader = new StreamReader(stream);
        JsonNode? jobj = JsonNode.Parse(reader.ReadToEnd());
        JsonArray array = jobj?["物品"]?.AsArray()!;
        foreach (JsonNode? item in array)
        {
            if (item != null && item["中文名称"]!.GetValue<string>().Contains(name))
            {
                list.Add(new()
                {
                    Name = item["中文名称"]!.GetValue<string>(),
                    netID = item["ID"]!.GetValue<int>()
                });
            }
        }
        return list;
    }

    public static List<Internal.Socket.Internet.Item> GetItemByIdOrName(string ji)
    {
        List<Internal.Socket.Internet.Item> list = [];
        if (int.TryParse(ji, out int i))
        {
            Internal.Socket.Internet.Item? item = GetItemById(i);
            if (item != null)
                list.Add(item);
        }
        else
        {
            list.AddRange(GetItemByName(ji));
        }
        return list;
    }
}
