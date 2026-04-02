using Backend.Common;
using Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Abstract base controller that provides generic CRUD endpoints with
/// dynamic filtering, sorting, pagination, and form upload support.
/// 
/// Endpoint pattern:
///   POST  /all        – Get all (with filters + sorting)
///   POST  /page       – Paged (with filters + sorting + pagination)
///   GET   /{id}       – Get by ID
///   POST  /           – Create (JSON body)
///   POST  /form       – Create (multipart form – files)
///   PUT   /           – Update (JSON body)
///   PUT   /form       – Update (multipart form – files)
///   DELETE /{id}      – Delete by ID
/// </summary>
[Authorize(Roles = "ADMIN,MANAGER")]
[ApiController]
public abstract class GenericCrudController<TEntity, TKey> : ControllerBase
    where TEntity : class
{
    protected readonly IGenericCrudRepository<TEntity, TKey> _repository;

    protected GenericCrudController(IGenericCrudRepository<TEntity, TKey> repository)
    {
        _repository = repository;
    }

    // ──────────────── READ ────────────────

    /// <summary>GET /{id} – Get a single entity by ID.</summary>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(TKey id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            return NotFound(new { message = $"Entity with ID {id} not found." });

        return Ok(entity);
    }

    /// <summary>POST /all – Get all entities with optional filters and sorting.</summary>
    [HttpPost("all")]
    public virtual async Task<IActionResult> GetAll([FromBody] GenericQueryRequest request)
    {
        var items = await _repository.GetAllAsync(request);
        return Ok(items);
    }

    /// <summary>POST /page – Get paged entities with filters, sorting, and pagination.</summary>
    [HttpPost("page")]
    public virtual async Task<IActionResult> GetPaged([FromBody] GenericQueryRequest request)
    {
        // Clamp values
        const int maxPageSize = 100;
        if (request.PageIndex < 0) request.PageIndex = 0;
        if (request.PageSize < 1) request.PageSize = 10;
        if (request.PageSize > maxPageSize) request.PageSize = maxPageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(request);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        return Ok(new PagedResult<TEntity>(
            items,
            request.PageIndex,
            request.PageSize,
            totalCount,
            totalPages
        ));
    }

    // ──────────────── CREATE ────────────────

    /// <summary>POST / – Create a new entity (JSON body).</summary>
    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _repository.CreateAsync(entity);
        return Ok(new { success = true, message = "Created successfully.", data = created });
    }

    /// <summary>POST /form – Create a new entity (multipart form – for file uploads).</summary>
    [HttpPost("form")]
    public virtual async Task<IActionResult> CreateForm([FromForm] TEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Override this method to handle IFormFile properties
        var created = await _repository.CreateAsync(entity);
        return Ok(new { success = true, message = "Created successfully.", data = created });
    }

    // ──────────────── UPDATE ────────────────

    /// <summary>PUT / – Update an existing entity (JSON body).</summary>
    [HttpPut]
    public virtual async Task<IActionResult> Update([FromBody] TEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _repository.UpdateAsync(entity);
        return Ok(new { success = true, message = "Updated successfully.", data = updated });
    }

    /// <summary>PUT /form – Update an existing entity (multipart form – for file uploads).</summary>
    [HttpPut("form")]
    public virtual async Task<IActionResult> UpdateForm([FromForm] TEntity entity)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Override this method to handle IFormFile properties
        var updated = await _repository.UpdateAsync(entity);
        return Ok(new { success = true, message = "Updated successfully.", data = updated });
    }

    // ──────────────── DELETE ────────────────

    /// <summary>DELETE /{id} – Delete an entity by ID.</summary>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(TKey id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { message = $"Entity with ID {id} not found." });

        return NoContent();
    }
}
