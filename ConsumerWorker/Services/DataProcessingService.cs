using ConsumerWorker.Data;
using ConsumerWorker.Models;
using ConsumerWorker.Dto;
using Microsoft.EntityFrameworkCore;

namespace ConsumerWorker.Services;

public class DataProcessingService
{
    private readonly IronGridDbContext _dbContext;
    private readonly ValidationService _validationService;
    private readonly Dictionary<string, IAssetProcessor> _processors;

    public DataProcessingService(IronGridDbContext dbContext, ValidationService validationService)
    {
        _dbContext = dbContext;
        _validationService = validationService;
        
        _processors = new Dictionary<string, IAssetProcessor>
        {
            { "UAV", new UAVProcessor() },
            { "PerimeterSensor", new PerimeterSensorProcessor() }
        };
    }

    public async Task<bool> ProcessFieldReportAsync(string jsonMessage)
    {
        try
        {
            var report = _validationService.DeserializeFieldReport(jsonMessage);
            if (report == null)
                return false;

            if (!await _validationService.ValidateAssetExistsAsync(report.AssetId))
                return false;

            var assetLiveStatus = ProcessReportByType(report);
            if (assetLiveStatus == null)
                return false;

            await UpdateAssetLiveStatusAsync(assetLiveStatus);

            Console.WriteLine($"Processed report for Asset {assetLiveStatus.AssetId}: {assetLiveStatus.ProcessedStatus} (Verified: {assetLiveStatus.IsVerified})");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing field report: {ex.Message}");
            return false;
        }
    }

    private AssetLiveStatus? ProcessReportByType(FieldReport report)
    {
        try
        {
            if (!_processors.TryGetValue(report.AssetType, out var processor))
            {
                Console.WriteLine($"Unknown asset type: {report.AssetType}");
                return null;
            }

            return processor.ProcessReport(report);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Report processing error: {ex.Message}");
            return null;
        }
    }

    private async Task UpdateAssetLiveStatusAsync(AssetLiveStatus assetLiveStatus)
    {
        try
        {
            var existingStatus = await _dbContext.AssetLiveStatuses
                .FirstOrDefaultAsync(als => als.AssetId == assetLiveStatus.AssetId);

            if (existingStatus != null)
            {
                existingStatus.AssetType = assetLiveStatus.AssetType;
                existingStatus.RawValue = assetLiveStatus.RawValue;
                existingStatus.ProcessedStatus = assetLiveStatus.ProcessedStatus;
                existingStatus.IsVerified = assetLiveStatus.IsVerified;
                existingStatus.LastUpdate = assetLiveStatus.LastUpdate;
            }
            else
            {
                _dbContext.AssetLiveStatuses.Add(assetLiveStatus);
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database update error: {ex.Message}");
            throw;
        }
    }
}
