using GearFlow.Modules.Catalog.Application.Services;
using GearFlow.Modules.Users.Core.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearFlow.Api.Controllers.Catalog;

[ApiController]
[Route("api/admin/catalog")]
[Authorize(Policy = AuthorizationPolicies.EmployeeOrAdmin)]
public sealed class AdminCatalogController : ControllerBase
{
    private readonly ICatalogAdminService _catalogAdminService;

    public AdminCatalogController(ICatalogAdminService catalogAdminService)
    {
        _catalogAdminService = catalogAdminService;
    }

    [HttpGet("models")]
    public async Task<ActionResult<IReadOnlyCollection<AdminEquipmentModelDto>>> GetEquipmentModelsAsync(CancellationToken cancellationToken)
    {
        var models = await _catalogAdminService.GetEquipmentModelsAsync(cancellationToken);

        return Ok(models);
    }

    [HttpGet("models/{modelId:guid}")]
    public async Task<ActionResult<AdminEquipmentModelDto>> GetEquipmentModelAsync([FromRoute] Guid modelId, CancellationToken cancellationToken)
    {
        var model = await _catalogAdminService.GetEquipmentModelAsync(modelId, cancellationToken);

        return Ok(model);
    }

    [HttpGet("items/{itemId:guid}")]
    public async Task<ActionResult<AdminEquipmentItemDetailsDto>> GetEquipmentItemAsync([FromRoute] Guid itemId, CancellationToken cancellationToken)
    {
        var item = await _catalogAdminService.GetEquipmentItemAsync(itemId, cancellationToken);

        return Ok(item);
    }

    [HttpPost("models")]
    public async Task<ActionResult> CreateEquipmentModelAsync([FromBody] CreateEquipmentModelRequest request, CancellationToken cancellationToken)
    {
        var modelId = await _catalogAdminService.CreateEquipmentModelAsync(
            new CreateEquipmentModelInput(
                request.Brand,
                request.Model,
                request.Type,
                request.Description,
                request.BasePrice,
                request.Currency ?? "PLN",
                request.Publish),
            cancellationToken);

        return CreatedAtAction(nameof(GetEquipmentModelAsync), new { modelId }, new { modelId });
    }

    [HttpPut("models/{modelId:guid}")]
    public async Task<ActionResult> UpdateEquipmentModelAsync(
        [FromRoute] Guid modelId,
        [FromBody] UpdateEquipmentModelRequest request,
        CancellationToken cancellationToken)
    {
        await _catalogAdminService.UpdateEquipmentModelAsync(
            new UpdateEquipmentModelInput(
                modelId,
                request.Brand,
                request.Model,
                request.Description,
                request.BasePrice,
                request.Currency ?? "PLN"),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("models/{modelId:guid}/publish")]
    public async Task<ActionResult> PublishEquipmentModelAsync([FromRoute] Guid modelId, CancellationToken cancellationToken)
    {
        await _catalogAdminService.PublishEquipmentModelAsync(modelId, cancellationToken);

        return NoContent();
    }

    [HttpPost("models/{modelId:guid}/unpublish")]
    public async Task<ActionResult> UnpublishEquipmentModelAsync([FromRoute] Guid modelId, CancellationToken cancellationToken)
    {
        await _catalogAdminService.UnpublishEquipmentModelAsync(modelId, cancellationToken);

        return NoContent();
    }

    [HttpPost("models/{modelId:guid}/variants/standard")]
    public async Task<ActionResult> AddStandardVariantAsync(
        [FromRoute] Guid modelId,
        [FromBody] AddStandardVariantRequest request,
        CancellationToken cancellationToken)
    {
        var variantId = await _catalogAdminService.AddStandardVariantAsync(
            new AddStandardEquipmentVariantInput(modelId, request.Size),
            cancellationToken);

        return CreatedAtAction(nameof(GetEquipmentModelAsync), new { modelId }, new { variantId });
    }

    [HttpPost("models/{modelId:guid}/variants/premium")]
    public async Task<ActionResult> AddPremiumVariantAsync(
        [FromRoute] Guid modelId,
        [FromBody] AddPremiumVariantRequest request,
        CancellationToken cancellationToken)
    {
        var variantId = await _catalogAdminService.AddPremiumVariantAsync(
            new AddPremiumEquipmentVariantInput(
                modelId,
                request.DisplayName,
                request.PublicNote,
                request.PriceOverride,
                request.Currency ?? "PLN",
                request.Size),
            cancellationToken);

        return CreatedAtAction(nameof(GetEquipmentModelAsync), new { modelId }, new { variantId });
    }

    [HttpPost("variants/{variantId:guid}/items")]
    public async Task<ActionResult> AddEquipmentItemAsync(
        [FromRoute] Guid variantId,
        [FromBody] AddEquipmentItemRequest request,
        CancellationToken cancellationToken)
    {
        var itemId = await _catalogAdminService.AddEquipmentItemAsync(
            new AddEquipmentItemInput(
                variantId,
                request.AssetTag,
                request.InternalNote,
                request.Status),
            cancellationToken);

        return CreatedAtAction(nameof(GetEquipmentItemAsync), new { itemId }, new { itemId });
    }
}

public sealed record CreateEquipmentModelRequest(
    string Brand,
    string Model,
    string Type,
    string? Description,
    decimal? BasePrice,
    string? Currency,
    bool Publish);

public sealed record UpdateEquipmentModelRequest(
    string Brand,
    string Model,
    string Description,
    decimal? BasePrice,
    string? Currency);

public sealed record AddStandardVariantRequest(string Size);

public sealed record AddPremiumVariantRequest(
    string DisplayName,
    string PublicNote,
    decimal PriceOverride,
    string? Currency,
    string? Size);

public sealed record AddEquipmentItemRequest(
    string AssetTag,
    string? InternalNote,
    string? Status);
