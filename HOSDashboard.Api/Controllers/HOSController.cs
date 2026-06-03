using Google.Cloud.BigQuery.V2;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("hos")]
public class HOSController : ControllerBase
{
    private BigQueryService _service;
    public HOSController(BigQueryService service)
    {
        _service = service;
    }
    [HttpPost("logs")]
    public async Task<IActionResult> GetLogs([FromBody] ClientParameter[] parameters)
    {
        Parameter[] ClientParameters = new Parameter[parameters.Length];
        for(int x = 0; x < parameters.Length; x++)
        {
            Parameter param = new Parameter(parameters[x].Name,parameters[x].Value);
            ClientParameters[x]=param;
        }
        RequestParameters requestParameters = new RequestParameters(ClientParameters);
        try
        {
        Dashboard dashboard = await Dashboard.GetDashboard(_service,requestParameters);
        return Ok(new{dashboard.DeviceInfo,dashboard.DatabaseInfo,dashboard.DutyStatusLogRecords});
        }
        catch(Exception e)
        {
            if (e.Message.Contains("reauth related error (invalid_rapt)")||e.Message.Contains("Token has been expired or revoked")||e.Message.Contains("application_default_credentials.json"))
            {
                return StatusCode(500,$"run <gcloud auth application-default login> in terminal to reset your Google Application Default Credentials (ADC) for local development.\n Message:{e.Message} \n Trace:{e.StackTrace}");
            }
            return StatusCode(500,$"Message:{e.Message}\nTrace:{e.StackTrace}");
        }
    }

}
