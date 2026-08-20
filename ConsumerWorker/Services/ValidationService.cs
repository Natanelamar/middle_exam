using System.Text.Json;
using ConsumerWorker.Data;
using ConsumerWorker.Dto;
using Microsoft.EntityFrameworkCore;

namespace ConsumerWorker.Services;

public class ValidationService
{
    private readonly IronGridDbContext _dbContext;

    public ValidationService(IronGridDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public FieldReport? DeserializeFieldReport(string jsonMessage)
    {
        try
        {
            var report = JsonSerializer.Deserialize<FieldReport>(jsonMessage);
            if (report == null)
            {
                Console.WriteLine("⚠ Failed to deserialize field report");
                return null;
            }
            return report;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Deserialization error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> ValidateAssetExistsAsync(int assetId)
    {
        try
        {
            var asset = await _dbContext.Assets.FindAsync(assetId);
            if (asset == null)
            {
                Console.WriteLine($"⚠ Asset not found: {assetId}");
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Asset validation error: {ex.Message}");
            return false;
        }
    }
}
