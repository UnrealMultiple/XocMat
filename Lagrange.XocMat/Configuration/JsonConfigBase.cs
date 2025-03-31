using Lagrange.XocMat.Event;
using Lagrange.XocMat.EventArgs;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Configuration;

public abstract class JsonConfigBase<T> where T : JsonConfigBase<T>, new()
{
    private static T? _instance;

    protected virtual string Filename => typeof(T).Namespace ?? typeof(T).Name;

    protected virtual void SetDefault()
    {
    }

    protected virtual void Reload(ReloadEventArgs args)
    {
        args.Message.Text($"[{Filename}] config reload successfully!\n");
    }

    private string FullFilename => Path.Combine(XocMatAPI.SAVE_PATH, $"{Filename}.json");


    private static T GetConfig()
    {
        T t = new();
        string file = t.FullFilename;
        if (File.Exists(file))
        {
            return File.ReadAllText(file).ToObject<T>() ?? t;
        }
        t.SetDefault();
        t.SaveTo();
        return t;
    }

    public virtual void SaveTo(string? path = null)
    {
        string filepath = path ?? FullFilename;
        string? dirPath = Path.GetDirectoryName(filepath);
        if (!string.IsNullOrEmpty(dirPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }
        File.WriteAllText(filepath, this.ToJson());
    }

    protected virtual ValueTask OnReload(ReloadEventArgs args)
    {
        _instance = GetConfig();
        Save();
        _instance.Reload(args);
        return ValueTask.CompletedTask;
    }

    public static void Save()
    {
        Instance.SaveTo();
    }

    public static string Load()
    {
        OperatHandler.OnReload += Instance.OnReload;
        return Instance.Filename;
    }

    public static string UnLoad()
    {
        OperatHandler.OnReload -= Instance.OnReload;
        return Instance.Filename;
    }

    public static T Instance => _instance ??= GetConfig();
}