using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleMessageBoard.DAL;
using SimpleMessageBoard.Model;

namespace SimpleMessageBoard.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoardMessagesController : ControllerBase
    {
        private readonly MessageBoardDbContext _context;

        public BoardMessagesController(MessageBoardDbContext context)
        {
            _context = context;
        }

        // GET: api/BoardMessages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoardMessage>>> GetMessages()
        {
            return await _context.Messages.ToListAsync();
        }

        // GET: api/BoardMessages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BoardMessage>> GetBoardMessage(int id)
        {
            var boardMessage = await _context.Messages.FindAsync(id);

            if (boardMessage == null)
            {
                return NotFound();
            }

            return boardMessage;
        }

        // PUT: api/BoardMessages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBoardMessage(int id, BoardMessage boardMessage)
        {
            if (id != boardMessage.Id)
            {
                return BadRequest();
            }

            _context.Entry(boardMessage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BoardMessageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BoardMessages
        [HttpPost]
        public async Task<ActionResult<BoardMessage>> PostBoardMessage(BoardMessage boardMessage)
        {
            _context.Messages.Add(boardMessage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBoardMessage", new { id = boardMessage.Id }, boardMessage);
        }

        // DELETE: api/BoardMessages/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<BoardMessage>> DeleteBoardMessage(int id)
        {
            var boardMessage = await _context.Messages.FindAsync(id);
            if (boardMessage == null)
            {
                return NotFound();
            }

            _context.Messages.Remove(boardMessage);
            await _context.SaveChangesAsync();

            return boardMessage;
        }

        private bool BoardMessageExists(int id)
        {
            return _context.Messages.Any(e => e.Id == id);
        }
    }
}
