using Bugtracker.Logging;
using Bugtracker.Problem_Descriptors;
using Bugtracker.Utils;
using System;
using static Bugtracker.Logging.Log;

namespace Bugtracker.Targeting.Targets
{
    /// <summary>
    /// Target that sends bugtracker data via email with optional ZIP attachment
    /// </summary>
    [TargetType("mail")]
    public class MailTarget : Target
    {
        public override string TypeIdentifier => "mail";

        /// <summary>
        /// Email address to send from
        /// </summary>
        [XmlConfig("sender", required: true)]
        public string SenderAddress { get; set; }

        /// <summary>
        /// SMTP server address
        /// </summary>
        [XmlConfig("smtpserver", required: true)]
        public string SmtpServer { get; set; }

        /// <summary>
        /// SMTP server port
        /// </summary>
        [XmlConfig("smtpport", required: true)]
        public int SmtpPort { get; set; }

        /// <summary>
        /// Whether to use SSL for SMTP connection
        /// </summary>
        [XmlConfig("smtpssl", required: true)]
        public bool SmtpSSL { get; set; }

        /// <summary>
        /// SMTP authentication username
        /// </summary>
        [XmlConfig("smtpuser", required: true)]
        public string SmtpUser { get; set; }

        /// <summary>
        /// SMTP authentication password
        /// </summary>
        [XmlConfig("smtppass", required: true)]
        public string SmtpPass { get; set; }

        /// <summary>
        /// Email address to send to
        /// </summary>
        [XmlConfig("recipient", required: true)]
        public string RecipientAddress { get; set; }

        /// <summary>
        /// Email subject line (supports variable substitution)
        /// </summary>
        [XmlConfig("subject", required: true)]
        public string Subject { get; set; }

        /// <summary>
        /// HTML template source for the email body. Accepts:
        ///   - File path (local or UNC): read from disk at send time
        ///   - http/https URL: downloaded at send time
        ///   - Inline HTML string: used directly (must start with '&lt;')
        /// </summary>
        [XmlConfig("htmltemplate", required: true)]
        public string HtmlTemplate { get; set; }

        /// <summary>
        /// Whether to attach a ZIP file of the bugtracker folder
        /// </summary>
        [XmlConfig("attachzip", required: true)]
        public bool AttachZipFile { get; set; }

        public override bool ValidateConfiguration(out string errorMessage)
        {
            if (string.IsNullOrEmpty(SenderAddress))
            {
                errorMessage = "Mail target requires 'sender' attribute";
                return false;
            }

            if (string.IsNullOrEmpty(SmtpServer))
            {
                errorMessage = "Mail target requires 'smtpserver' attribute";
                return false;
            }

            if (SmtpPort <= 0)
            {
                errorMessage = "Mail target requires valid 'smtpport' attribute";
                return false;
            }

            if (string.IsNullOrEmpty(SmtpUser))
            {
                errorMessage = "Mail target requires 'smtpuser' attribute";
                return false;
            }

            if (string.IsNullOrEmpty(SmtpPass))
            {
                errorMessage = "Mail target requires 'smtppass' attribute";
                return false;
            }

            if (string.IsNullOrEmpty(RecipientAddress))
            {
                errorMessage = "Mail target requires 'recipient' attribute";
                return false;
            }

            if (string.IsNullOrEmpty(Subject))
            {
                errorMessage = "Mail target requires 'subject' attribute";
                return false;
            }

            if (string.IsNullOrEmpty(HtmlTemplate))
            {
                errorMessage = "Mail target requires 'htmltemplate' attribute";
                return false;
            }

            // Validate htmltemplate based on detected mode
            bool isUrl = HtmlTemplate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         HtmlTemplate.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            bool isInlineHtml = HtmlTemplate.TrimStart().StartsWith("<");

            if (!isUrl && !isInlineHtml && !System.IO.File.Exists(HtmlTemplate))
            {
                errorMessage = $"HTML template file not found: {HtmlTemplate}";
                Logger.Log(errorMessage, LoggingSeverity.Warning);
                // Return true anyway — file may appear on a network share later
                return true;
            }

            errorMessage = null;
            return true;
        }

        public override SendResult Send(ProblemDescriptor problemDescriptor = null)
        {
            try
            {
                // Create MailConfig object with current settings
                MailUtils.MailConfig mailConfig = new MailUtils.MailConfig(
                    SenderAddress,
                    SmtpServer,
                    SmtpPort,
                    SmtpSSL,
                    SmtpUser,
                    SmtpPass,
                    RecipientAddress,
                    Subject,
                    HtmlTemplate,
                    AttachZipFile
                );

                // Send the mail
                mailConfig.Send(problemDescriptor);

                Logger.Log($"Successfully sent email to {RecipientAddress}", LoggingSeverity.Info);
                return SendResult.Ok($"Successfully sent email to {RecipientAddress}");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Failed to send email to {RecipientAddress}: {ex.Message}";
                Logger.Log(errorMsg, LoggingSeverity.Error);
                return SendResult.Fail(errorMsg, ex);
            }
        }

        public override string GetSummary()
        {
            return base.GetSummary() +
                   $"\nRecipient: {RecipientAddress}" +
                   $"\nSender: {SenderAddress}" +
                   $"\nSMTP Server: {SmtpServer}:{SmtpPort}" +
                   $"\nSSL: {SmtpSSL}" +
                   $"\nAttach ZIP: {AttachZipFile}";
        }
    }
}
