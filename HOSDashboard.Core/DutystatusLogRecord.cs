using Google.Cloud.BigQuery.V2;

public class DutyStatusLogRecord
{
    public string? CompanyGuid {get;}
    public string? ParentId {get;}
    public string? Id {get;}
    public DateTime? DateTime {get;}
    public DateTime? EditDateTime {get;}
    public DateTime? StreamDateTime {get;}
    public DateTime? VerifyDateTime {get;}
    public string? Version {get;}
    public string? Sequence {get;}
    public string? Status {get;}
    public string? State {get;}
    public int? HardwareId {get;}
    public string? DriverId {get;}
    public string? Origin {get;}
    public string? Malfunction {get;}
    public double? Latitude {get;}
    public double? Longitude {get;}
    public double? Odometer {get;}
    public double? EngineHours {get;}

    public DutyStatusLogRecord(BigQueryRow row)
    {
        CompanyGuid = (string?)row["Guid"];
        ParentId = (string?)row["ParentId"];
        Id = (string?)row["Id"];
        DateTime = (DateTime?)row["DateTime"];
        EditDateTime = (DateTime?)row["EditDateTime"];
        StreamDateTime = (DateTime?)row["StreamDateTime"];
        VerifyDateTime = (DateTime?)row["VerifyDateTime"];
        Version = (string?)row["Version"];
        Sequence = (string?)row["Sequence"];
        Status = (string?)row["Status"];
        State = (string?)row["State"];
        HardwareId = row["HardwareId"]!=null?Convert.ToInt32(row["HardwareId"]):null;
        DriverId = (string?)row["DriverId"];
        Origin = (string?)row["Origin"];
        Malfunction = (string?)row["Malfunction"];
        Latitude = (double?)row["Latitude"];
        Longitude = (double?)row["Longitude"];
        Odometer = (double?)row["Odometer"];
        EngineHours = (double?)row["EngineHours"];
    }
}