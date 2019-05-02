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

        /// <summary>Gets all messages.</summary>
        /// <remarks>
        /// Sample:
        ///
        ///     GET /MessageBoard
        /// </remarks>
        /// <returns>An array of messages</returns>
        /// <response code="200">List of messages retrieved.</response>
        [HttpGet()]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<ActionResult<MessageBoardEntry[]>> GetAll()
        {
            return await _msgService.GetAllMessages(this.GetUserId());
        }

        /// <summary>Gets a specific message by Id.</summary>
        /// <remarks>
        /// Sample:
        ///
        ///     GET /MessageBoard/2
        /// </remarks>
        /// <param name="id">The message Id.</param>
        /// <returns>The message if found.</returns>
        /// <response code="200">The message was found and returned.</response>
        /// <response code="404">The message was not found.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
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

        /// <summary>
        /// Creates a new message.
        /// </summary>
        /// <remarks>
        /// Sample:
        ///
        ///     POST /MessageBoard
        ///     {
        ///        "message": "some message"
        ///     }
        ///
        /// </remarks>
        /// <param name="msg">The message to create.</param>
        /// <returns>The message if created.</returns>
        /// <response code="201">The message was created.</response>
        /// <response code="400">The posting user is not a valid author.</response>
        /// <response code="401">The posting user is not authorized.</response>
        [HttpPost]
        [ProducesResponseType(201), ProducesResponseType(400), ProducesResponseType(401)]
        public async Task<ActionResult<MessageBoardEntry>> Create(MessageBoardEntry msg)
        {
            msg = await _msgService.CreateMessage(msg, this.GetUserId());
            if (msg == null)
            {
                return BadRequest();
            }

            return CreatedAtRoute(string.Empty, new { id = msg.Id }, msg);
        }

        /// <summary>
        /// Updates the specified message.
        /// </summary>
        /// <remarks>
        /// Sample:
        ///
        ///     PUT /MessageBoard/1
        ///     {
        ///        "id": 1,
        ///        "message": "some message"
        ///     }
        ///
        /// </remarks>
        /// <param name="id">The message Id.</param>
        /// <param name="entry">The edited message.</param>
        /// <response code="204">The message was updated.</response>
        /// <response code="400">Message identity mismatch.</response>
        /// <response code="401">The posting user is not authorized.</response>
        /// <response code="404">The message was not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(204), ProducesResponseType(400), ProducesResponseType(401), ProducesResponseType(404)]
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

        /// <summary>
        /// Deletes the specified message.
        /// </summary>
        /// <remarks>
        /// Sample:
        ///
        ///     DELETE /MessageBoard/2
        ///
        /// </remarks>
        /// <param name="id">The message Id.</param>
        /// <response code="204">The message was deleted.</response>
        /// <response code="401">The posting user is not authorized.</response>
        /// <response code="404">The message was not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204), ProducesResponseType(401), ProducesResponseType(404)]
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
