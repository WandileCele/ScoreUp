using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        public IActionResult About() => View();
        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
        public IActionResult Contact() => View();
        public IActionResult Learn() => View();

        // NEW
        public IActionResult CreditCards() => View();
        public IActionResult Loans() => View();

        [HttpPost]
        public IActionResult Contact(string name, string email, string message)
        {
            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "messages.txt");
                System.IO.File.AppendAllText(path, $"[{DateTime.Now}] {name} | {email} | {message}\n");

                var fromEmail = _config["EmailSettings:SenderEmail"] ?? "wandilecele28@gmail.com";
                var appPassword = _config["EmailSettings:SenderPassword"];

                if (string.IsNullOrEmpty(appPassword))
                {
                    ViewBag.Error = "Email not configured on server. But message saved!";
                    return View();
                }

                var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true,
                };

                var mail = new MailMessage(fromEmail, "wandilecele28@gmail.com")
                {
                    Subject = $"ScoreUp Contact from {name}",
                    Body = $"Name: {name}\nEmail: {email}\n\n{message}"
                };
                smtp.Send(mail);
                ViewBag.Success = true;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View();
        }
    }
}
