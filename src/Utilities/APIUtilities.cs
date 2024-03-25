using System.Text.Json;

namespace StepNet.API.Utilities
{
    public class APIUtilities
    {
        public static List<Plugin> ConvertToPlugin(string json)
        {
            // Filler method for now
            return JsonSerializer.Deserialize<List<Plugin>>(json);
        }
    }
}
