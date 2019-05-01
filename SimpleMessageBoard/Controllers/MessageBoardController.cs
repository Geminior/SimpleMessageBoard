namespace SimpleMessageBoard.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using SimpleMessageBoard.DTOs;
    using SimpleMessageBoard.Services;
    using System.Threading.Tasks;

    [Authorize]
    [Route("[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class MessageBoardController : ControllerBase
    {
        IMessageBoardService _msgService;

        public MessageBoardController(IMessageBoardService msgService)
        {
            _msgService = msgService;
        }

        [HttpGet()]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<ActionResult<MessageBoardEntry[]>> GetAll()
        {
            return await _msgService.GetAllMessages(this.GetUserId());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<ActionResult<MessageBoardEntry>> Get(int id)
        {
            var msg = await _msgService.GetMessage(id, this.GetUserId());
            if (msg == null)
            {
                return NotFound();
            }

            return msg;
        }

        [HttpPost]
        [ProducesResponseType(201), ProducesResponseType(400)]
        public async Task<ActionResult<MessageBoardEntry>> Create(MessageBoardEntry msg)
        {
            msg = await _msgService.CreateMessage(msg, this.GetUserId());
            if (msg == null)
            {
                return BadRequest();
            }

            return CreatedAtRoute(string.Empty, new { id = msg.Id }, msg);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(404)]
        public async Task<IActionResult> Update(int id, MessageBoardEntry entry)
        {
            if (id != entry.Id)
            {
                return BadRequest();
            }

            var success = await _msgService.UpdateMessage(entry, this.GetUserId());
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204), ProducesResponseType(404)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _msgService.DeleteMessage(id, this.GetUserId());
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
