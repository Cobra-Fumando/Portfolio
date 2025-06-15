namespace Portfolio.Config
{
    public class ValidacaoEmail
    {
        public bool EmailValido(string Email)
        {
            try
            {
                Email = Email.Trim(); //Tira espaçamento que a pessoa pode colocar
                var endereco = new System.Net.Mail.MailAddress(Email); //pega o Email da Maneira certa
                return endereco.Address == Email; //Compara o Email enviado com o Email com formato certo
            }
            catch
            {
                return false;
            }
        }
    }
}
