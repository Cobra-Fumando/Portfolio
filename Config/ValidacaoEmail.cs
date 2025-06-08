namespace Portfolio.Config
{
    public class ValidacaoEmail
    {
        public bool EmailValido(string Email)
        {
            try
            {
                Email = Email.Trim();
                var endereco = new System.Net.Mail.MailAddress(Email);
                return endereco.Address == Email;
            }
            catch
            {
                return false;
            }
        }
    }
}
