using Azure.Communication.Email;
using JWT.Algorithms;
using JWT.Serializers;
using JWT;
using Microsoft.AspNetCore.Http;
using MimeTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PEAS
{
    internal class Helper
    {

        public static string GetIP(HttpRequest req)
        {
            string ip = req.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
            {
                ip = req.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            }
            return ip;
        }

        public static bool VerifyEmail(string email)
        {
            var allowedEmails = Environment.GetEnvironmentVariable("AllowedEmailHosts").Split(",");
            return allowedEmails.Any(x => email.EndsWith(x));
        }

        public static string GetSalt()
        {
            return Environment.GetEnvironmentVariable("Salt");
        }

        public static string HashEmail(string email)
        {
            return ComputeSha256Hash(ComputeSha256Hash(email + GetSalt()) + GetSalt());
        }



        // https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/create-communication-resource?tabs=windows&pivots=platform-azp#store-your-connection-string
        // https://learn.microsoft.com/en-us/azure/communication-services/quickstarts/email/send-email?pivots=programming-language-csharp
        // dotnet add package Azure.Communication.Email --prerelease
        public static void SendOTPEMail(string email, string otp)
        {
            string connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
            EmailClient emailClient = new EmailClient(connectionString);
            EmailContent emailContent = new EmailContent("PEAuthentification Services");
            emailContent.PlainText = $"Hello\n\nYour OTP:\n{otp}\n\nHave a great day!";
            List<EmailAddress> emailAddresses = new List<EmailAddress> { new EmailAddress(email) };
            EmailRecipients emailRecipients = new EmailRecipients(emailAddresses);
            EmailMessage emailMessage = new EmailMessage("donotreply@azuremail.alptbz.xyz", emailRecipients, emailContent);
            EmailSendOperation emailResult = emailClient.Send(Azure.WaitUntil.Completed, emailMessage, CancellationToken.None);
        }

        public static string GetMimeType(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return MimeTypeMap.GetMimeType(fileInfo.Extension);
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return String.Join(String.Empty, Array.ConvertAll(bytes, x => x.ToString("X2")));
            }
        }

        public static string GetJwtToken(string sharedSecret, string domain, string group, string email)
        {
            var payload = new Dictionary<string, object>
            {
                { "domain", domain },
                { "group", group },
                { "email", email },
                { "sub", email },
            };

#pragma warning disable CS0618 // Type or member is obsolete
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
#pragma warning restore CS0618 // Type or member is obsolete
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();

            var key = Encoding.UTF8.GetBytes(sharedSecret);

            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            var token = encoder.Encode(payload, key);

            return token;
        }

    }
}
