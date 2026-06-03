using Google.Cloud.BigQuery.V2; 
using Google.Apis.Auth.OAuth2;
//-- Use this class to gain access to a BigQueryClient which uses Application Default Credentials(ADC) found on the local machine. This is the client that will query GBQ tables.
public class BigQueryService
{
    public string ProjectId{get;}
    public BigQueryClient Client{get;set;}

    public BigQueryService(string projectid)
    {
        ProjectId = projectid;
        Client = BigQueryClient.Create(ProjectId);
    }
    public BigQueryClient RefreshClient()
    {
        var adcPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".config", "gcloud", "application_default_credentials.json");
        GoogleCredential credential = GoogleCredential.FromFile(adcPath);
        Client = BigQueryClient.Create("geotab-myserver", credential);
        return Client;
    }
}
