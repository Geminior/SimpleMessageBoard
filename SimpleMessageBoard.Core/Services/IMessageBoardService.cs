namespace SimpleMessageBoard.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleMessageBoard.DTOs;

    public interface IMessageBoardService
    {
        Task<MessageBoardEntry> CreateMessage(MessageBoardEntry message, int requesterId);

        Task<bool> DeleteMessage(int id, int requesterId);

        Task<bool> UpdateMessage(MessageBoardEntry editedMessage, int requesterId);

        Task<MessageBoardEntry[]> GetAllMessages(int? requesterId);

        Task<MessageBoardEntry> GetMessage(int id, int? requesterId);
    }
}