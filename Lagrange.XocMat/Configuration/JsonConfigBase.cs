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
        var t = new T();
        var file = t.FullFilename;
        if (File.Exists(file))
        {
            var obj = File.ReadAllText(file).ToObject<T>() ?? t;
            obj.SaveTo();
            return obj;
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
            _instance.Reload(args);
            return ValueTask.CompletedTask;
        };
        return Instance.Filename;
    }

    public static T Instance => _instance ??= GetConfig();
}