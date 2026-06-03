using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;

public class DeviceStatusQuery
{
    private static string[] _regions = ["US","CA","EU"];
    public string _sqlquery;
    private BigQueryParameter[] _parameters;
    public DeviceStatusQuery(BigQueryParameter[] parameters)
    {
        _parameters = parameters;
    }
    public void ConstructQuery(string region)
    {
        string fieldselection = "Select DatabaseName, SerialNo, HardwareId, DeviceId ";
        string table = $"from `geotab-embedded-prod.Summary_MyG_{region}.DeviceCurrentStatusDaily` ";
        string whereclause = $"Where DATE(PipelineExecutionTime) = DATE_SUB(CURRENT_DATE(),INTERVAL 1 DAY) AND ";
        for(int x = 0; x < _parameters.Length; x++)
        {
            if(x == _parameters.Length - 1)
            {
                string clause = $"{_parameters[x].Name}=@{_parameters[x].Name}";
                whereclause += clause;
            }
            else
            {
                string clause = $"{_parameters[x].Name}=@{_parameters[x].Name} AND ";
                whereclause += clause;
            }
        }
        _sqlquery = fieldselection+table+whereclause;
    }
    public async Task<BigQueryResults> Execute(BigQueryClient client)
    {
        BigQueryResults results=null;
        foreach(string region in _regions)
        {
            ConstructQuery(region);
            if((await client.ExecuteQueryAsync(_sqlquery,_parameters)).TotalRows != 0)
            {
                results = await client.ExecuteQueryAsync(_sqlquery,_parameters);
                break;
            }
        }
        if(results==null) throw new Exception("Could not find a matching HardwareId for Serial Number across DeviceCurrentStatusDaily in US, CA, EU regions for the database/guid provided");
        else return results;
    }


}