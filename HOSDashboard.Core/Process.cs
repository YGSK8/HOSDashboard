using System.ComponentModel;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
public class Dashboard
{
    private readonly BigQueryClient _client;
    public List<DutyStatusLogRecord> DutyStatusLogRecords{get;}=new List<DutyStatusLogRecord>();
    public Dictionary<string,BigQueryDbType>DeviceCurrentStatusDailySchema {get;}
    public Dictionary<string,BigQueryDbType>ClientInfoSchema{get;}
    public Dictionary<string,BigQueryDbType>DutyStatusLogsSchema{get;}
    public BigQueryParameter FROMPARTITIONDATE{get;}
    public BigQueryParameter TOPARTITIONDATE{get;}
    public BigQueryParameter CompanyGuid{get;}
    public BigQueryParameter HardwareId{get;}
    public Dashboard(BigQueryService service, RequestParameters parameters)
    {
        _client = service.Client;
        DeviceCurrentStatusDailySchema = new SchemaWithBigQueryType(service.Client,"geotab-embedded-prod","Summary_MyG_US","DeviceCurrentStatusDaily").Schema;
        ClientInfoSchema = new SchemaWithBigQueryType(service.Client,"geotab-gateway","Gateway","ClientInfo").Schema;
        DutyStatusLogsSchema = new SchemaWithBigQueryType(service.Client,"geotab-myserver","MyGeotab","DutyStatusLogs").Schema;
        Parameter fromPartitionDate = parameters.Parameters.Find(parameter => parameter.Name=="_FROMPARTITIONDATE")!;
        FROMPARTITIONDATE = new BigQueryParameter(fromPartitionDate.Name,BigQueryDbType.Date,fromPartitionDate.Value);
        Parameter toPartitionDate = parameters.Parameters.Find(parameter=>parameter.Name=="_TOPARTITIONDATE")!;
        TOPARTITIONDATE = new BigQueryParameter(toPartitionDate.Name,BigQueryDbType.Date,toPartitionDate.Value);
        Parameter guid;
        if (parameters.Parameters.Find(parameter => parameter.Name == "Guid")==null)
        {
            Parameter databaseName = parameters.Parameters.Find(parameter=>parameter.Name=="DatabaseName");
            guid = GetCompanyGuid(databaseName);
        }
        else guid = parameters.Parameters.Find(parameter => parameter.Name == "Guid");
        CompanyGuid = new BigQueryParameter(guid.Name,DutyStatusLogsSchema["Guid"],guid.Value);
        Parameter hardwareid;
        if(parameters.Parameters.Find(parameter => parameter.Name == "HardwareId") == null)
        {
            Parameter serialNo = parameters.Parameters.Find(parameter=>parameter.Name=="SerialNo");
            hardwareid = GetHardwareId(serialNo);
        }
        else hardwareid = parameters.Parameters.Find(parameter=>parameter.Name=="HardwareId");
        HardwareId = new BigQueryParameter(hardwareid.Name,DutyStatusLogsSchema[hardwareid.Name],hardwareid.Value);
        GetDutyStatusLogs();
    }
    public Parameter GetCompanyGuid(Parameter databaseName)//--Returns a parameter with Name: "Guid" and Value: guid in lower case.
    {
        BigQueryParameter BQDatabaseName = new BigQueryParameter(databaseName.Name,DeviceCurrentStatusDailySchema[databaseName.Name],databaseName.Value);
        BigQueryResults results = new DatabaseGuidQuery([BQDatabaseName]).Execute(_client);
        List<BigQueryRow> rows = results.ToList();
        BigQueryRow row = rows[0];
        Parameter companyguid = new Parameter(Name:"Guid",Value:row["Guid"]);
        return companyguid;

    }
    public Parameter GetHardwareId(Parameter serialNo)//-Uses two parameters CompanyGuid and HardwareId. This is to ensure we get only one row as result since devices can be shared across multiple databases.
    {
        BigQueryParameter BQSerialNo = new BigQueryParameter(serialNo.Name,DeviceCurrentStatusDailySchema[serialNo.Name],serialNo.Value);
        BigQueryParameter BQCompanyGuid = new BigQueryParameter("CompanyGuid",CompanyGuid.Type,CompanyGuid.Value);
        BigQueryResults results = new DeviceStatusQuery([BQCompanyGuid,BQSerialNo]).Execute(_client);
        List<BigQueryRow> rows = results.ToList();
        BigQueryRow row = rows[0];
        Parameter hardwareid = new Parameter(Name:"HardwareId",Value:row["HardwareId"]);
        return hardwareid;
    }
    public void GetDutyStatusLogs()
    {
        BigQueryParameter[]bigQueryParameters = [FROMPARTITIONDATE,TOPARTITIONDATE,CompanyGuid,HardwareId];
        DutyStatusLogQuery query = new DutyStatusLogQuery(bigQueryParameters);
        BigQueryResults results = query.Execute(_client);
        foreach(BigQueryRow row in results)
        {
            DutyStatusLogRecord record = new DutyStatusLogRecord(row);
            DutyStatusLogRecords.Add(record);
        }
    }
}