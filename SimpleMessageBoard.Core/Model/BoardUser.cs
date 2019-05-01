namespace SimpleMessageBoard.Model
{
    public class BoardUser
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        internal string Password { get; set; }
    }
}
