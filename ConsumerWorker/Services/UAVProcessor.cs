using ConsumerWorker.Dto;
using ConsumerWorker.Enums;
using ConsumerWorker.Models;

namespace ConsumerWorker.Services;

public class UAVProcessor : IAssetProcessor
{
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
            return batteryLevel >= 0 && batteryLevel <= 100;
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

        if (batteryLevel >= 20)
        {
            return ProcessedStatus.Stable;
        }
        
        return ProcessedStatus.Warning;
    }
}
