using Microsoft.Extensions.Options;
using FridgeServer.EmailService.SendGrid;
using FridgeServer.EmailService.Template;
using FridgeServer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FridgeServer.EmailService
{
    public interface IVerificationEmail
    {
        Task<SendEmailResponse> SendUserVerificationEmailAsync(string displayName, string email, string verificationUrl);
    }
    public class VerificationEmail : IVerificationEmail
    {
        protected AppSettings appSettings;
        protected ISendgrindEmailSender emailSender;
        public VerificationEmail(IOptions<AppSettings> options, ISendgrindEmailSender sender)
        {
            appSettings = options.Value;
            emailSender = sender;
        }
        public async Task<SendEmailResponse> SendUserVerificationEmailAsync(string displayName, string email, string verificationUrl)
        {
            var emailDetails =await VerifyEmailTemplate.CreateEmailDetails(new SendEmailDetails
            {
                IsHTML = true,
                FromEmail = appSettings.EmailVerficationInfo.FromEmail,
                FromName = appSettings.EmailVerficationInfo.FromName,
                ToEmail = email,
                ToName = displayName,
                Subject = "Verify Your Email - Fasetto Word"
            },
            "Verify Email",
            $"Hi {displayName ?? "stranger"},",
            "Thanks for creating an account with us.<br/>To continue please verify your email with us.",
            "Verify Email",
            verificationUrl
            );
            return await emailSender.SendEmailAsync(emailDetails);
        }

    }
}
