namespace SimpleMessageBoard.Configuration
{
    using System;
    using System.Text;

    public class TokensConfig
    {
        public string Secret { get; set; } //This should of course be stored in Secret Manager or similar

        public TimeSpan? TokenDuration { get; set; }

        public byte[] SecretBytes => Encoding.ASCII.GetBytes(this.Secret);
    }
}
