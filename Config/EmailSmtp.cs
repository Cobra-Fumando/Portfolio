using System.Net;
using System.Net.Mail;

namespace Portfolio.Config
{
    public class EmailSmtp
    {
        private readonly ILogger<EmailSmtp> logger;
        public EmailSmtp(ILogger<EmailSmtp> logger)
        {
            this.logger = logger;
        }
        public void Enviar(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Mensagem)
        {
            if (string.IsNullOrWhiteSpace(RemetenteEmail) ||
                string.IsNullOrWhiteSpace(DestinatarioEmail) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(DestinatarioName)||
                string.IsNullOrWhiteSpace(Mensagem))
            {
                logger.LogWarning("Está faltando informações");
                return;
            }

            MailAddress Remetente = new MailAddress(RemetenteEmail, DestinatarioName);
            MailAddress destinatario = new MailAddress(DestinatarioEmail);

            MailMessage message = new MailMessage();

            message.From = Remetente;
            message.To.Add(destinatario);
            message.Subject = Mensagem;
            message.IsBodyHtml = false;

            var stmp = new SmtpClient("smtp.gmail.com", 587) {
                Credentials = new NetworkCredential(RemetenteEmail, Senha),
                EnableSsl = true
            };

            try
            {
                stmp.Send(message);
                logger.LogInformation("Mensagem enviada com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
            }
        }

        public void CodigoSend(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Codigo)
        {
            if (string.IsNullOrWhiteSpace(RemetenteEmail) ||
                string.IsNullOrWhiteSpace(DestinatarioEmail) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(DestinatarioName))
            {
                logger.LogWarning("Está faltando informações");
                return;
            }

            MailAddress Remetente = new MailAddress(RemetenteEmail);
            MailAddress destinatario = new MailAddress(DestinatarioEmail);

            MailMessage message = new MailMessage();

            message.From = Remetente;
            message.To.Add(destinatario);
            message.Subject = $"Olá {DestinatarioName}, seu código de verificação é: {Codigo}";
            message.IsBodyHtml = false;

            var stmp = new SmtpClient("smtp.gmail.com", 587) {
                Credentials = new NetworkCredential(RemetenteEmail, Senha),
                EnableSsl = true
            };

            try
            {
                stmp.Send(message);
                logger.LogInformation("Mensagem enviada com sucesso");
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");
            }
        }
    }
}
