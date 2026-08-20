using ConsumerWorker.Dto;
using ConsumerWorker.Models;

namespace ConsumerWorker.Services;

public interface IAssetProcessor
{
    string AssetType { get; }
    AssetLiveStatus ProcessReport(FieldReport report);
}
