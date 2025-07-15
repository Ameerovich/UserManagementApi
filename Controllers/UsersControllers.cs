using Microsoft.AspNetCore.Mvc;
using UserManagementApi.Models;
using UserManagementApi.Services;

namespace UserManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }

    // GET: api/users
    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll()
    {
        var users = _service.GetAll();

        if (users == null || !users.Any())
            return NoContent(); // More efficient than returning 200 with empty list

        return Ok(users);
    }

    // GET: api/users/{id}
    [HttpGet("{id:int}")]
    public ActionResult<User> GetById(int id)
    {
        var user = _service.GetById(id);
        return user is not null ? Ok(user) : NotFound();
    }

    // POST: api/users
    [HttpPost]
    public ActionResult<User> Create([FromBody] User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // Redundant if [ApiController] is used, but explicit is fine

        var created = _service.Create(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/users/{id}
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = _service.Update(id, user);
        return updated ? NoContent() : NotFound();
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var deleted = _service.Delete(id);
        return deleted ? NoContent() : NotFound();
    }
}
