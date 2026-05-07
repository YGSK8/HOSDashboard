using Microsoft.AspNetCore.Mvc;
[ApiController]
[Route("hos")]
public class HOSController : ControllerBase
{
    private readonly BigQueryService _service;
    public HOSController(BigQueryService service)
    {
        _service = service;
    }
    [HttpPost("logs")]
    public IActionResult GetLogs([FromBody] ClientParameter[] parameters)
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
        Dashboard dashboard = new Dashboard(_service,requestParameters);
        return Ok(dashboard.DutyStatusLogRecords.Count);
        }
        catch(Exception e)
        {
            return StatusCode(500,e.Message);
        }
    }

}

