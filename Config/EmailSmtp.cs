using System.Net;
using System.Net.Mail;
using MailKit.Security;
using MimeKit;
using Portfolio.Tabelas;

namespace Portfolio.Config
{
    public class EmailSmtp
    {
        private readonly ILogger<EmailSmtp> logger;
        public EmailSmtp(ILogger<EmailSmtp> logger)
        {
            this.logger = logger;
        }
        public void Enviar(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Mensagem, string assunto)
        {
            if (string.IsNullOrWhiteSpace(RemetenteEmail) ||
                string.IsNullOrWhiteSpace(DestinatarioEmail) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(DestinatarioName) ||
                string.IsNullOrWhiteSpace(Mensagem))
            {
                logger.LogWarning("Está faltando informações");
                return;
            }

            //-----------------------------------------------------------//
            //Mensagem configuração

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Ronaldo", RemetenteEmail));
            mensagem.To.Add(new MailboxAddress(DestinatarioName, DestinatarioEmail));

            mensagem.Subject = assunto;

            mensagem.Body = new TextPart("html")
            {
                Text = $"<h1>Olá!</h1><p> {assunto} <strong> {mensagem} </strong>.</p>"
            };

            //-----------------------------------------------------------//
            //enviar Mensagem

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(RemetenteEmail, Senha);
                smtp.Send(mensagem);
                smtp.Disconnect(true);

                logger.LogInformation("Mensagem enviada com sucesso");
            }
            catch (Exception ex)
            { 
                logger.LogError($"Erro inesperado: {ex.Message}");
            }
        }

        public TabelaProblem<string> CodigoSend(string RemetenteEmail, string DestinatarioEmail, string Senha, string DestinatarioName, string Codigo)
        {
            var log = new TabelaProblem<string>
            {
                Dados = null
            };

            if (string.IsNullOrWhiteSpace(RemetenteEmail) ||
                string.IsNullOrWhiteSpace(DestinatarioEmail) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(DestinatarioName))
            {
                logger.LogWarning("Está faltando informações");
                log.success = false;
                log.Message = "Está faltando informações";

                return log;
            }

            //-----------------------------------------------------------//
            //Mensagem configuração

            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress("Ronaldo", RemetenteEmail));
            mensagem.To.Add(new MailboxAddress(DestinatarioName, DestinatarioEmail));

            mensagem.Subject = "Verificação 2 etapa";

            mensagem.Body = new TextPart("html")
            {
                Text = $"<h1>Olá!</h1><p>Este é um e-mail com seu codigo <strong> {Codigo} </strong>.</p>"
            };

            //-----------------------------------------------------------//
            //enviar Mensagem

            try
            {
                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(RemetenteEmail, Senha);
                smtp.Send(mensagem);
                smtp.Disconnect(true);

                logger.LogInformation("Mensagem enviada com sucesso");

                log.success = true;
                log.Message = "Mensagem enviada com sucesso";
                return log;
            }
            catch (Exception ex)
            {
                logger.LogError($"Erro inesperado: {ex.Message}");

                log.success = false;
                log.Message = $"Erro inesperado: {ex.Message}";
                return log;
            }
        }
    }
}
