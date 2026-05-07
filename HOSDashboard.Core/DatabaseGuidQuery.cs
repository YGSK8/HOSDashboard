using Google.Cloud.BigQuery.V2;

public class DatabaseGuidQuery
{
    private string _sqlquery;
    public BigQueryParameter[] Parameters{get;}
    public DatabaseGuidQuery(BigQueryParameter[] parameters)
    {
        Parameters = parameters;
        ConstructQuery(Parameters);
    }
    public void ConstructQuery(BigQueryParameter[] parameters)
    {
        string fieldselection = "SELECT Guid,DatabaseName,DateTime FROM";
        string table = "(SELECT LOWER(Guid) as GUID,lower(DatabaseName) as databasename,DateTime, ROW_NUMBER() OVER (PARTITION BY Guid,databasename order by datetime desc) AS rn FROM `geotab-gateway.Gateway.ClientInfo` WHERE _PARTITIONDATE = DATE_SUB(CURRENT_DATE(),INTERVAL 1 DAY) AND BYTE_LENGTH(databasename)>1)";
        string whereclause = $"WHERE rn = 1 and LOWER({parameters[0].Name}) = LOWER(@{parameters[0].Name})";
        string orderby = "order by DateTime desc ";
        string limit = "LIMIT 1";
        _sqlquery = fieldselection + table + whereclause + orderby + limit;
    }
    public BigQueryResults Execute(BigQueryClient client)
    {
            BigQueryResults results = client.ExecuteQuery(_sqlquery,Parameters);
            if(results.TotalRows == 0) throw new Exception($"Unable to find guid of the database provided");
            else return results;
    }
}