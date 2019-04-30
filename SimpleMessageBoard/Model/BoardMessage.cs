namespace SimpleMessageBoard.Model
{
    public class BoardMessage
    {
        public int Id { get; set; }

        public string Message { get; set; }

        internal int AuthorId { get; set; }

        internal BoardUser Author { get; set; }
    }
}
