namespace SimpleMessageBoard.DTOs
{
    public sealed class MessageBoardEntry
    {
        public int Id { get; set; }

        public string Message { get; set; }

        public string Author { get; internal set; }

        public bool CanEdit { get; internal set; }
    }
}
