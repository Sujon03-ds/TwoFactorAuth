namespace TwoFactorAuth.Models
{
    public class EnableAuthenticatorViewModel
    {
        public string UserName { get; set; }
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
        public string Code { get; set; }
    }
}
