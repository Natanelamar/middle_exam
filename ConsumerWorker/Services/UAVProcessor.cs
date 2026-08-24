using ConsumerWorker.Dto;
using ConsumerWorker.Enums;
using ConsumerWorker.Models;

namespace ConsumerWorker.Services;

public class UAVProcessor : IAssetProcessor
{
    private const int MIN_BATTERY_LEVEL = 0;
    private const int MAX_BATTERY_LEVEL = 100;
    private const int LOW_BATTERY_THRESHOLD = 20;

    public string AssetType => "UAV";

    public AssetLiveStatus ProcessReport(FieldReport report)
    {
        var isVerified = VerifyReport(report.RawValue);
        var processedStatus = CalculateProcessedStatus(report.RawValue, isVerified);

        return new AssetLiveStatus
        {
            AssetId = report.AssetId,
            AssetType = report.AssetType,
            RawValue = report.RawValue,
            ProcessedStatus = processedStatus,
            IsVerified = isVerified,
            LastUpdate = DateTime.UtcNow
        };
    }

    private bool VerifyReport(string rawValue)
    {
        if (int.TryParse(rawValue, out int batteryLevel))
        {
            return batteryLevel >= MIN_BATTERY_LEVEL && batteryLevel <= MAX_BATTERY_LEVEL;
        }
        return false;
    }

    private ProcessedStatus CalculateProcessedStatus(string rawValue, bool isVerified)
    {
        if (!isVerified)
        {
            return ProcessedStatus.Warning;
        }

        int batteryLevel = int.Parse(rawValue);

        if (batteryLevel >= LOW_BATTERY_THRESHOLD)
        {
            return ProcessedStatus.Stable;
        }
        
        return ProcessedStatus.Warning;
    }
}
