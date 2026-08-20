using ConsumerWorker.Dto;
using ConsumerWorker.Enums;
using ConsumerWorker.Models;

namespace ConsumerWorker.Services;

public class PerimeterSensorProcessor : IAssetProcessor
{
    public string AssetType => "PerimeterSensor";

    public AssetLiveStatus ProcessReport(FieldReport report)
    {
        return new AssetLiveStatus
        {
            AssetId = report.AssetId,
            AssetType = report.AssetType,
            RawValue = report.RawValue,
            ProcessedStatus = CalculateProcessedStatus(report.RawValue),
            IsVerified = VerifyReport(report.RawValue),
            LastUpdate = DateTime.UtcNow
        };
    }

    public bool VerifyReport(string rawValue)
    {
        var normalized = rawValue.Trim().ToLower();

        if (normalized == "good" || normalized == "gud")
        {
            return true;
        }

        if (normalized == "bad" || normalized == "bed")
        {
            return true;
        }

        return false;
    }

    public ProcessedStatus CalculateProcessedStatus(string rawValue)
    {
        var normalized = rawValue.Trim().ToLower();

        if (normalized == "good" || normalized == "gud")
        {
            return ProcessedStatus.Stable;
        }

        return ProcessedStatus.Warning;
    }
}
