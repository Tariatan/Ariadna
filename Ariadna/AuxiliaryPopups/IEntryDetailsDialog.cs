#nullable enable
namespace Ariadna.AuxiliaryPopups;

public interface IEntryDetailsDialog
{
    int StoredDbEntryId { get; }
    Utilities.EFormCloseReason FormCloseReason { get; }
}
