using Microsoft.AspNetCore.Mvc;
using RestApi.Models;
using RestApi.Services;

namespace RestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService service, ILogger<ItemsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<Item>> GetAll() => Ok(_service.GetAll());

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Item> GetById(int id)
    {
        var item = _service.GetById(id);
        if (item is null)
        {
            _logger.LogInformation("Item {Id} not found", id);
            return NotFound(new { message = $"Item {id} not found" });
        }
        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Item> Create([FromBody] ItemRequest request)
    {
        var item = _service.Create(request);
        _logger.LogInformation("Created item {Id}", item.Id);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update(int id, [FromBody] ItemRequest request)
    {
        if (!_service.Update(id, request))
            return NotFound(new { message = $"Item {id} not found" });
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        if (!_service.Delete(id))
            return NotFound(new { message = $"Item {id} not found" });
        _logger.LogInformation("Deleted item {Id}", id);
        return NoContent();
    }
}
