using Lagrange.XocMat.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginExample;

// This is a config class that inherits from JsonConfigBase
[Lagrange.XocMat.Attributes.ConfigSeries]
public class ExampleConfig : JsonConfigBase<ExampleConfig>
{
    protected override string Filename => "example.json";

    [JsonProperty("禁用指令")]
    public List<string> DisabledCommands { get; set; } = [];
}
