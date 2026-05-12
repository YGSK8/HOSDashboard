using System.ComponentModel;
using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
public class Dashboard
{
    private readonly BigQueryClient _client;
    public List<DutyStatusLogRecord> DutyStatusLogRecords{get;}=new List<DutyStatusLogRecord>();
    public Dictionary<string,BigQueryDbType>DeviceCurrentStatusDailySchema {get;set;}
    public Dictionary<string,BigQueryDbType>ClientInfoSchema{get;set;}
    public Dictionary<string,BigQueryDbType>DutyStatusLogsSchema{get;set;}
    public BigQueryParameter FROMPARTITIONDATE{get;set;}
    public BigQueryParameter TOPARTITIONDATE{get;set;}
    public BigQueryParameter CompanyGuid{get;set;}
    public BigQueryParameter HardwareId{get;set;}
    private Dashboard(BigQueryService service)
    {
        _client = service.Client;
    }
    public static async Task<Dashboard> GetDashboard(BigQueryService service,RequestParameters parameters)
    {
        Dashboard dashboard = new Dashboard(service);
        await dashboard.SchemaInitalizer();
        Parameter fromPartitionDate = parameters.Parameters.Find(parameter => parameter.Name=="_FROMPARTITIONDATE")!;
        dashboard.FROMPARTITIONDATE = new BigQueryParameter(fromPartitionDate.Name,BigQueryDbType.Date,fromPartitionDate.Value);
        Parameter toPartitionDate = parameters.Parameters.Find(parameter=>parameter.Name=="_TOPARTITIONDATE")!;
        dashboard.TOPARTITIONDATE = new BigQueryParameter(toPartitionDate.Name,BigQueryDbType.Date,toPartitionDate.Value);
        Parameter guid;
        if (parameters.Parameters.Find(parameter => parameter.Name == "Guid")==null)
        {
            Parameter databaseName = parameters.Parameters.Find(parameter=>parameter.Name=="DatabaseName");
            guid = await dashboard.GetCompanyGuid(databaseName);
        }
        else guid = parameters.Parameters.Find(parameter => parameter.Name == "Guid");
        dashboard.CompanyGuid = new BigQueryParameter(guid.Name,dashboard.DutyStatusLogsSchema["Guid"],guid.Value);
        Parameter hardwareid;
        if(parameters.Parameters.Find(parameter => parameter.Name == "HardwareId") == null)
        {
            Parameter serialNo = parameters.Parameters.Find(parameter=>parameter.Name=="SerialNo");
            hardwareid = await dashboard.GetHardwareId(serialNo);
        }
        else hardwareid = parameters.Parameters.Find(parameter=>parameter.Name=="HardwareId");
        dashboard.HardwareId = new BigQueryParameter(hardwareid.Name,dashboard.DutyStatusLogsSchema[hardwareid.Name],hardwareid.Value);
        await dashboard.GetDutyStatusLogs();
        return dashboard;
    }
    public async Task<Parameter> GetCompanyGuid(Parameter databaseName)//--Returns a parameter with Name: "Guid" and Value: guid in lower case.
    {
        BigQueryParameter BQDatabaseName = new BigQueryParameter(databaseName.Name,DeviceCurrentStatusDailySchema[databaseName.Name],databaseName.Value);
        BigQueryResults results = await new DatabaseGuidQuery([BQDatabaseName]).Execute(_client);
        List<BigQueryRow> rows = results.ToList();
        BigQueryRow row = rows[0];
        Parameter companyguid = new Parameter(Name:"Guid",Value:row["Guid"]);
        return companyguid;

    }
    public async Task<Parameter> GetHardwareId(Parameter serialNo)//-Uses two parameters CompanyGuid and HardwareId. This is to ensure we get only one row as result since devices can be shared across multiple databases.
    {
        BigQueryParameter BQSerialNo = new BigQueryParameter(serialNo.Name,DeviceCurrentStatusDailySchema[serialNo.Name],serialNo.Value);
        BigQueryParameter BQCompanyGuid = new BigQueryParameter("CompanyGuid",CompanyGuid.Type,CompanyGuid.Value);
        BigQueryResults results = await new DeviceStatusQuery([BQCompanyGuid,BQSerialNo]).Execute(_client);
        List<BigQueryRow> rows = results.ToList();
        BigQueryRow row = rows[0];
        Parameter hardwareid = new Parameter(Name:"HardwareId",Value:row["HardwareId"]);
        return hardwareid;
    }
    public async Task GetDutyStatusLogs()
    {
        BigQueryParameter[]bigQueryParameters = [FROMPARTITIONDATE,TOPARTITIONDATE,CompanyGuid,HardwareId];
        DutyStatusLogQuery query = new DutyStatusLogQuery(bigQueryParameters);
        BigQueryResults results = await query.Execute(_client);
        foreach(BigQueryRow row in results)
        {
            DutyStatusLogRecord record = new DutyStatusLogRecord(row);
            DutyStatusLogRecords.Add(record);
        }
    }
    public async Task SchemaInitalizer()
    {
        DeviceCurrentStatusDailySchema= (await SchemaWithBigQueryType.GetSchema(_client,"geotab-embedded-prod","Summary_MyG_US","DeviceCurrentStatusDaily")).Schema;
        ClientInfoSchema = (await SchemaWithBigQueryType.GetSchema(_client,"geotab-myserver","MyGeotab","DutyStatusLogs")).Schema;
        DutyStatusLogsSchema = (await SchemaWithBigQueryType.GetSchema(_client,"geotab-myserver","MyGeotab","DutyStatusLogs")).Schema;
    }
}