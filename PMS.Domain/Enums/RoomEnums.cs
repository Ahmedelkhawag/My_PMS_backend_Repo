namespace PMS.Domain.Enums
{
    /// <summary>
    /// Housekeeping (physical) status of the room.
    /// </summary>
    public enum HKStatus
    {
        Clean = 1,
        Dirty = 2,
        Inspected = 3,
        OOO = 4,  // Out of Order
        OOS = 5   // Out of Service
    }

    /// <summary>
    /// Front Office (operational) status: derived from active reservations.
    /// </summary>
    public enum FOStatus
    {
        Vacant = 1,
        Occupied = 2
    }

    /// <summary>
    /// Bed type for the room.
    /// </summary>
    public enum BedType
    {
        Single = 1,
        Twin = 2,
        Queen = 3,
        King = 4
    }

    /// <summary>
    /// Distinguishes which status family is being updated.
    /// </summary>
    public enum RoomStatusType
    {
        HouseKeeping = 1,
        FrontOffice = 2
    }
}
