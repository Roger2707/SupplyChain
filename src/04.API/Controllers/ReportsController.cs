using Inventory.Application.DTOs.Reports;
using Inventory.Application.DTOs.TrialBalances;
using Inventory.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SupplyChain.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<IEnumerable<TrialBalanceDto>>> GetAll([FromQuery] TrialBalanceRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetTrialBalanceAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("general-ledger")]
    public async Task<ActionResult<IEnumerable<GeneralLedgerDto>>> GetAll([FromQuery] GeneralLedgerRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetGeneralLedgerAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    [HttpGet("income-statement")]
    public async Task<ActionResult<IEnumerable<IncomeStatementDto>>> GetAll([FromQuery] IncomeStatementRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetIncomeStatementAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result.ErrorMessage);
        }

        return Ok(result.Data);
    }
}

