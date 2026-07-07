using GearFlow.Modules.Catalog.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GearFlow.Api.Controllers.Catalog;

[ApiController]
[Route("api/catalog")]
public sealed class PublicCatalogController : ControllerBase
{
    private readonly ICatalogBrowsingService _catalogBrowsingService;

    public PublicCatalogController(ICatalogBrowsingService catalogBrowsingService)
    {
        _catalogBrowsingService = catalogBrowsingService;
    }

    [HttpGet("offers")]
    public async Task<ActionResult<PagedResult<PublicCatalogOfferDto>>> BrowseOffersAsync(
        [FromQuery] BrowseCatalogOffersRequest request,
        CancellationToken cancellationToken)
    {
        var offers = await _catalogBrowsingService.BrowseOffersAsync(
            new BrowseCatalogOffersInput(
                request.Type,
                request.Brand,
                request.Model,
                request.Size,
                request.MinPrice,
                request.MaxPrice,
                request.Currency ?? "PLN",
                request.Page,
                request.PageSize),
            cancellationToken);

        return Ok(offers);
    }

    [HttpGet("offers/{variantId:guid}")]
    public async Task<ActionResult<PublicCatalogOfferDto>> GetOfferAsync([FromRoute] Guid variantId, CancellationToken cancellationToken)
    {
        var offer = await _catalogBrowsingService.GetOfferAsync(variantId, cancellationToken);

        return Ok(offer);
    }
}

public sealed record BrowseCatalogOffersRequest
{
    public string? Type { get; init; }
    public string? Brand { get; init; }
    public string? Model { get; init; }
    public string? Size { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? Currency { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
