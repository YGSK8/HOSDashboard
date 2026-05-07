using Google.Cloud.BigQuery.V2; 
//-- Use this class to gain access to a BigQueryClient which uses Application Default Credentials(ADC) found on the local machine. This is the client that will query GBQ tables.
public class BigQueryService
{
    public string ProjectId{get;}
    public BigQueryClient Client{get;}

    public BigQueryService(string projectid)
    {
        ProjectId = projectid;
        Client = BigQueryClient.Create(ProjectId);
    }
}
