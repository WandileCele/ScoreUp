using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace ScoreUp.Controllers
{
    public class PagesController : Controller
    {
        private readonly IConfiguration _config;

        public PagesController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Privacy() => View();
        public IActionResult About() => View();
        public IActionResult Terms() => View();

        [HttpGet]
        public IActionResult Contact() => View();

        [HttpPost]
        public async Task<IActionResult> Contact(string name, string email, string topic, string message)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View();
            }

            // Save to file as backup
            try
            {
                var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (!Directory.Exists(wwwroot)) Directory.CreateDirectory(wwwroot);
                var logPath = Path.Combine(wwwroot, "messages.txt");
                var log = $"--- {DateTime.Now:yyyy-MM-dd HH:mm:ss} ---\nName: {name}\nEmail: {email}\nTopic: {topic}\nMessage: {message}\n------------------------\n";
                System.IO.File.AppendAllText(logPath, log);
            }
            catch { }

            // SEND REAL EMAIL TO wandilecele28@gmail.com
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _config["EmailSettings:SenderEmail"] ?? "wandilecele28@gmail.com";
                var senderPassword = _config["EmailSettings:SenderPassword"] ?? "";
                var recipientEmail = _config["EmailSettings:RecipientEmail"] ?? "wandilecele28@gmail.com";

                if (string.IsNullOrWhiteSpace(senderPassword))
                {
                    TempData["Error"] = "Email password not configured. Check appsettings.json - EmailSettings:SenderPassword";
                    return View();
                }

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    var mail = new MailMessage();
                    mail.From = new MailAddress(senderEmail, "ScoreUp Contact Form");
                    mail.To.Add(recipientEmail);
                    mail.ReplyToList.Add(new MailAddress(email, name));
                    mail.Subject = $"[ScoreUp] {topic} - from {name}";
                    mail.Body = $@"You have a new message from ScoreUp contact form:

Name: {name}
Email: {email}
Topic: {topic}

Message:
{message}

---
Sent at: {DateTime.Now}
From: ScoreUp Website
";
                    mail.IsBodyHtml = false;

                    await client.SendMailAsync(mail);
                }

                TempData["Success"] = $"Thanks {name}! Your message was sent to wandilecele28@gmail.com. We'll reply to {email} within 24 hours.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Message saved but email failed to send: {ex.Message}. Check your App Password in appsettings.json. Message is still saved in wwwroot/messages.txt";
            }

            return RedirectToAction("Contact");
        }
    }
}
