namespace SimpleMessageBoard.DTOs
{
    public sealed class MessageBoardEntry
    {
        public int Id { get; internal set; }

        public string Message { get; set; }

        public string Author { get; internal set; }
    }
}
