namespace SimpleMessageBoard.Services
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using SimpleMessageBoard.DAL;
    using SimpleMessageBoard.DTOs;
    using SimpleMessageBoard.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public sealed class MessageBoardService : IMessageBoardService
    {
        private MessageBoardDbContext _ctx;
        private ILogger _logger;

        public MessageBoardService(MessageBoardDbContext ctx, ILogger<MessageBoardService> logger)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MessageBoardEntry[]> GetAllMessages(int? requesterId)
        {
            _logger.LogInformation("[S] Retrieving all messages.");

            var res = await BaseQuery(requesterId).ToArrayAsync();

            _logger.LogInformation("[E] Retrieved all messages.");
            return res;
        }

        public async Task<MessageBoardEntry> GetMessage(int id, int? requesterId)
        {
            _logger.LogInformation("[S] Retrieving message with id: {Id}.", id);

            var res = await BaseQuery(requesterId)
                .Where(m => m.Id == id)
                .FirstOrDefaultAsync();

            _logger.LogInformation("[E] Retrieved message with id: {Id}.", id);
            return res;
        }

        private IQueryable<MessageBoardEntry> BaseQuery(int? requesterId)
        {
            return _ctx.Messages
                    .Include(m => m.Author)
                    .Select(m =>
                    new MessageBoardEntry
                    {
                        Id = m.Id,
                        Message = m.Message,
                        Author = m.Author.UserName,
                        CanEdit = (m.AuthorId == requesterId)
                    });
        }

        public async Task<MessageBoardEntry> CreateMessage(MessageBoardEntry message, int requesterId)
        {
            if (message == null)
            {
                _logger.LogWarning("Message is null trying to create new message for author: {AuthorId}.", requesterId);
                return null;
            }

            _logger.LogInformation("[S] Creating new message for author: {AuthorId}.", requesterId);

            string authorName;
            try
            {
                authorName = await _ctx.Users.Where(u => u.Id == requesterId).Select(u => u.UserName).FirstAsync();
            }
            catch (InvalidOperationException iex)
            {
                _logger.LogWarning(iex, "[E] Message creation attempted by invalid user.");
                return null;
            }

            var msg = new BoardMessage
            {
                AuthorId = requesterId,
                Message = message.Message
            };

            _ctx.Messages.Add(msg);

            await _ctx.SaveChangesAsync();

            message.Id = msg.Id;
            message.Author = authorName;
            message.CanEdit = true;

            _logger.LogInformation("[E] Created new message for author: {AuthorId}.", requesterId);
            return message;
        }

        public async Task<bool> UpdateMessage(MessageBoardEntry editedMessage, int requesterId)
        {
            if (editedMessage == null)
            {
                _logger.LogWarning("[E] Message is null.");
                return false;
            }

            _logger.LogInformation("[S] Updating message with Id: {Id}.", editedMessage.Id);

            var msg = await _ctx.Messages.FindAsync(editedMessage.Id);
            if (msg == null || msg.AuthorId != requesterId)
            {
                _logger.LogInformation("[E] Update of message with Id: {Id} for author {AuthorId} rejected.", editedMessage.Id, requesterId);
                return false;
            }

            msg.Message = editedMessage.Message;
            var rowsAffected = await _ctx.SaveChangesAsync();

            _logger.LogInformation("[E] Updated message with Id: {Id}.", msg.Id);
            return rowsAffected == 1;
        }

        public async Task<bool> DeleteMessage(int id, int requesterId)
        {
            _logger.LogInformation("[S] Deleting message with Id: {Id}.", id);

            var msg = await _ctx.Messages.FindAsync(id);
            if (msg == null)
            {
                _logger.LogInformation("[E] Message with Id: {Id} already deleted.", id);
                return true;
            }
            else if (msg.AuthorId != requesterId)
            {
                _logger.LogInformation("[E] Deleting message with Id: {Id} rejected for author {AuthorId}.", msg.Id, requesterId);
                return false;
            }

            _ctx.Messages.Remove(msg);

            await _ctx.SaveChangesAsync();

            _logger.LogInformation("[E] Deleted message with Id: {Id}.", msg.Id);
            return true;
        }
    }
}
