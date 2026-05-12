using Google.Cloud.BigQuery.V2;
public class DutyStatusLogQuery
{
    private string _sqlquery;
    public BigQueryParameter[] Parameters{get;}
    public DutyStatusLogQuery(BigQueryParameter[] parameters)
    {
        Parameters = parameters;
        ConstructQuery(Parameters);
    }
    public void ConstructQuery(BigQueryParameter [] parameters)
    {
        string fieldselection = "SELECT Guid, ParentId, Id, DateTime, EditDateTime, StreamDateTime, VerifyDateTime, Version, Sequence, Status, HardwareId, DriverId, Origin, State, Malfunction, Latitude, Longitude, Odometer, EngineHours ";
        string table = "from `geotab-myserver.MyGeotab.DutyStatusLogs`";
        string whereclause = "WHERE _PARTITIONDATE BETWEEN @_FROMPARTITIONDATE AND @_TOPARTITIONDATE AND ";
        for (int x = 0; x < parameters.Length; x++)
        {
            if (parameters[x].Type != BigQueryDbType.Date)
            {
                if(x == parameters.Length-1)
                {
                    whereclause += $"{parameters[x].Name} = @{parameters[x].Name} ";
                }
                else whereclause += $"{parameters[x].Name} = @{parameters[x].Name} AND ";
            }
        }
        string orderby = "ORDER BY DateTime, EditDateTime, StreamDateTime ASC";
        _sqlquery = fieldselection+table+whereclause+orderby;
    }
    public async Task<BigQueryResults> Execute(BigQueryClient client)
    {
        
        return await client.ExecuteQueryAsync(_sqlquery,Parameters);
    }

}