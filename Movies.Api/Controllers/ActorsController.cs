using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.DTOs;
using Movies.Api.Services;

namespace Movies.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ActorsController : ControllerBase
    {
        private readonly IActorService _actorService;
        public ActorsController(IActorService actorService)
        {
            _actorService = actorService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateActor([FromBody] ActorCreateDto actorCreateDto)
        {
            var actor = await _actorService.CreateActorAsync(actorCreateDto);
            return CreatedAtAction(nameof(GetActorById), new { id = actor.Id }, actor);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllActors()
        {
            var actors = await _actorService.GetAllActorsAsync();
            return Ok(actors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetActorById(int id)
        {
            var result = await _actorService.GetActorAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActorById(int id)
        {
            var result = await _actorService.DeleteActorAsync(id);
            if (result == false)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateActor(int id, [FromBody] ActorCreateDto actorCreateDto)
        {
            var result = await _actorService.UpdateActorAsync(id, actorCreateDto);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}