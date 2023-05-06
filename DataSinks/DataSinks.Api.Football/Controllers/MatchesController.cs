using Microsoft.AspNetCore.Mvc;

namespace DataSinks.Api.Football.Controllers;

[ApiController]
[Route("api/matches")]
public class MatchesController : ControllerBase
{
    private readonly DataService _dataService;

    public MatchesController(DataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet]
    public async Task<IEnumerable<Match>> List([FromQuery] MatchesFilter filter, CancellationToken cancellationToken)
    {
        return await _dataService.GetAsync(filter, cancellationToken);
    }

    [HttpGet("{code:length(32)}")]
    public async Task<ActionResult<Match>> Get(string code, CancellationToken cancellationToken)
    {
        var result = await _dataService.GetAsync(code, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return result;
    }
}
