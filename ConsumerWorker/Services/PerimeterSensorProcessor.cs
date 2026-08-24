using ConsumerWorker.Dto;
using ConsumerWorker.Enums;
using ConsumerWorker.Models;

namespace ConsumerWorker.Services;

public class PerimeterSensorProcessor : IAssetProcessor
{
    private static readonly string[] VALID_GOOD_VALUES = { "good", "gud" };
    private static readonly string[] VALID_BAD_VALUES = { "bad", "bed" };

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
        return VALID_GOOD_VALUES.Contains(normalized) || VALID_BAD_VALUES.Contains(normalized);
    }

    public ProcessedStatus CalculateProcessedStatus(string rawValue)
    {
        var normalized = rawValue.Trim().ToLower();
        return VALID_GOOD_VALUES.Contains(normalized) 
            ? ProcessedStatus.Stable 
            : ProcessedStatus.Warning;
    }
}
