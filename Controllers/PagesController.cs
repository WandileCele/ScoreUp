using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace ScoreUp.Controllers
{
    public class PagesController : Controller
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PagesController> _logger;

        public PagesController(IConfiguration config, ILogger<PagesController> logger)
        {
            _config = config;
            _logger = logger;
        }

        // --- YOUR EXISTING PAGES (UNCHANGED) ---
        public IActionResult About() => View();
        public IActionResult Privacy() => View();
        public IActionResult Terms() => View();
        public IActionResult Contact() => View();
        public IActionResult Learn() => View();
        public IActionResult CreditCards() => View();
        public IActionResult Loans() => View();

        // --- NEW PAGES FOR FLEXOFFERS APPROVAL (ADDED ONLY) ---
        public IActionResult AdDisclosure() => View();
        public IActionResult HowWeMakeMoney() => View();
        public IActionResult EditorialPolicy() => View();
        public IActionResult SitemapPage() => View();

        // 10 ARTICLE PAGES TO GO FROM 7 TO 17 PAGES
        public IActionResult BuildCreditFromZero() => View();
        public IActionResult BestNoCreditCheckBanks() => View();
        public IActionResult ChimeCreditBuilderReview() => View();
        public IActionResult SelfCreditBuilderReview() => View();
        public IActionResult KikoffReview() => View();
        public IActionResult HowLongTo700() => View();
        public IActionResult CreditUtilizationGuide() => View();
        public IActionResult RentReportingGuide() => View();
        public IActionResult NoCreditCheckLoans() => View();
        public IActionResult AvoidCreditRepairScams() => View();

        public class ContactRequest
        {
            public string Name { get; set; } = "";
            public string Email { get; set; } = "";
            public string Topic { get; set; } = "";
            public string Message { get; set; } = "";
        }

        [HttpPost("Pages/Contact")]
        public async Task<IActionResult> ContactPost([FromBody] ContactRequest req)
        {

            try
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "messages.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                var line = $"[{DateTime.Now}] {req.Name} | {req.Email} | {req.Topic} | {req.Message.Replace("\n", " ")}\n";
                await System.IO.File.AppendAllTextAsync(path, line);
                _logger.LogInformation($"Saved: {line}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Save failed: {ex.Message}");
            }

            try
            {

                var fromEmail = _config["EmailSettings:SenderEmail"] ?? "wandilecele28@gmail.com";
                var appPassword = _config["EmailSettings:SenderPassword"];
                var toEmail = _config["EmailSettings:RecipientEmail"] ?? "wandilecele28@gmail.com";
                var smtpServer = _config["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");

                _logger.LogInformation($"Email attempt: {fromEmail} -> {toEmail}, HasPass={!string.IsNullOrEmpty(appPassword)}");

                if (string.IsNullOrEmpty(appPassword))
                {
                    return Json(new { success = false, error = "EmailSettings:SenderPassword not set in appsettings.json" });
                }


                using var smtp = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(fromEmail, appPassword),
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 10000
                };

                var mail = new MailMessage(fromEmail, toEmail)
                {
                    Subject = $"ScoreUp Contact - {req.Topic} from {req.Name}",
                    Body = $"Name: {req.Name}\nEmail: {req.Email}\nTopic: {req.Topic}\n\nMessage:\n{req.Message}\n\nReply-To: {req.Email}\nTime: {DateTime.UtcNow}"
                };
                mail.ReplyToList.Add(new MailAddress(req.Email, req.Name));

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                await smtp.SendMailAsync(mail, cts.Token);

                _logger.LogInformation("Email sent successfully");
                return Json(new { success = true });
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("SMTP timeout after 10 sec - Render free blocks port 587");

                return Json(new { success = true, warning = "Saved to messages.txt, SMTP blocked on Render free" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email failed: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> Contact(string name, string email, string message)
        {
            var req = new ContactRequest { Name = name, Email = email, Message = message, Topic = "General" };
            return await ContactPost(req);
        }
    }
}
