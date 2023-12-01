public record LicenseInfo
{
    string? ticketId;
    string? serialNumber;

    // the two can;t be null at the same time.
    // the two can't have value at the same time
}


// PostgreSQL table LicenseInfo, column ticketid, column serialNumber. Only has 1 row. Singleton.


public record LicenseInfo{}

public record OfflineLicenseInfo : LicenseInfo
{
    string ticketId;
}

public record OnlineLicenseInfo : LicenseInfo
{
    string serialNumber;
}

// PostgreSQL: serialize it (including type)?

//            vvvvvvvvvvv -> Is the alias really necessary?
public record LicenseInfo : OneOf<OfflineLicenseInfo, OnlineLicenseInfo>
{

}

OneOf<OfflineLicenseInfo, OnlineLicenseInfo>




