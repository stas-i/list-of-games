using DataWarehouse.Data.Football;
using DataWarehouse.Data.Football.Queries;
using Football.Contracts.ApiData;
using Microsoft.AspNetCore.Mvc;

namespace DataWarehouse.Read.Api.Controllers;

[ApiController]
[Route("matches")]
public class MatchesController : ControllerBase
{
    private readonly IDapperExecutor _dapperExecutor;

    public MatchesController(IDapperExecutor dapperExecutor)
    {
        _dapperExecutor = dapperExecutor;
    }

    [HttpGet("{code:length(32)}")]
    public async Task<ActionResult<MatchEntity>> Get(string code, CancellationToken cancellationToken)
    {
        var result = await _dapperExecutor.QuerySingleOrDefault(new GetMatchByIdQuery { Code = code }, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return result;
    }
}
