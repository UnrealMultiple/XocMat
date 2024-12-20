using Lagrange.XocMat.Event;
using Lagrange.XocMat.Extensions;

namespace Lagrange.XocMat.Configuration;

public abstract class JsonConfigBase<T> where T : JsonConfigBase<T>, new()
{
    private static T? _instance;

    protected virtual string Filename => typeof(T).Namespace ?? typeof(T).Name;

    protected virtual string? ReloadMsg => null;

    protected virtual void SetDefault()
    {
    }

    private string FullFilename => Path.Combine(XocMatAPI.SAVE_PATH, $"{Filename}.json");


    private static T GetConfig()
    {
        var t = new T();
        var file = t.FullFilename;
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
        var filepath = path ?? FullFilename;
        var dirPath = Path.GetDirectoryName(filepath);
        if (!string.IsNullOrEmpty(dirPath))
        {
            var dirInfo = new DirectoryInfo(dirPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }
        File.WriteAllText(filepath, this.ToJson());
    }


    public static void Save()
    {
        Instance.SaveTo();
    }

    // .cctor is lazy load
    public static string Load()
    {
        OperatHandler.OnReload += args =>
        {
            _instance = GetConfig();
            if (!string.IsNullOrEmpty(_instance.ReloadMsg))
                args.Message.Text(_instance.ReloadMsg);
            return ValueTask.CompletedTask;
        };
        return Instance.Filename;
    }

    public static T Instance => _instance ??= GetConfig();
}