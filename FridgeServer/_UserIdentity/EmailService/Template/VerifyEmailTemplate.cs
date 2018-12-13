using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FridgeServer._UserIdentity.EmailService.Template;
using FridgeServer.EmailService.SendGrid;
using FridgeServer.Helpers;
namespace FridgeServer.EmailService.Template
{
    public static class VerifyEmailTemplate
    {
        public static async Task<SendEmailDetails> CreateEmailDetails(SendEmailDetails details, string title, string content1, string content2, string buttonText, string buttonUrl)
        {
            var templateText = VerificationTemplateString.TemplateString;

            // Replace special values with those inside the template
            templateText = templateText.Replace("--Title--", title)
                                        .Replace("--Content1--", content1)
                                        .Replace("--Content2--", content2)
                                        .Replace("--ButtonText--", buttonText)
                                        .Replace("--ButtonUrl--", buttonUrl);

            // Set the details content to this template content
            details.Content = templateText;

            // Send email
            return details;
        }
    }
}
