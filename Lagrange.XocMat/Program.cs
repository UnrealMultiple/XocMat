using System.Reflection;
using System.Runtime;
using System.Text;

namespace Lagrange.XocMat;

internal abstract class Program
{
    public static void Main(string[] args)
    {
        Console.Title = "Lagrange.XocMat";
        string version = Assembly.GetAssembly(typeof(Program))?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "Unknown Lagrange.XocMat Version";
        Console.WriteLine($"Lagrange.XocMat Version: {version}\n");

        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        GCSettings.LatencyMode = GCLatencyMode.Batch;

        if (!File.Exists("appsettings.json"))
        {
            Console.WriteLine("No exist config file, create it now...");

            Assembly assm = Assembly.GetExecutingAssembly();
            using Stream istr = assm.GetManifestResourceStream("Lagrange.XocMat.Resources.Json.appsettings.json")!;
            using FileStream temp = File.Create("appsettings.json");
            istr.CopyTo(temp);

            istr.Close();
            temp.Close();

            Console.WriteLine("Please Edit the appsettings.json to set configs and press any key to continue");
            Console.ReadLine();
        }

        XocMatApp.Instance
            .Builder()
            .Start();
    }
}