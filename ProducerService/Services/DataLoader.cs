using System.Text.Json;
using ProducerService.Models;

namespace ProducerService.Services;

public class DataLoader
{
    public List<T> LoadData<T>(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<List<T>>(json, options)!;
    }
}
