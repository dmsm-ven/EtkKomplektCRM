namespace EtkBlazorApp.BL
{
    public struct ImapConnectionData
    {
		public string Email { get; }
		public string Password { get; }
		public string Port { get;  }
		public string Host { get; }

        public ImapConnectionData(string email, string password, string host, string port)
        {
			Email = email;
			Password = password;
			Host = host;
			Port = port;
        }
    }
}
