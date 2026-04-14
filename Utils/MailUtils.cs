using Bugtracker.Configuration;
using Bugtracker.Variables;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Bugtracker.Problem_Descriptors;

namespace Bugtracker.Utils
{
    public static class MailUtils
    {
        /// <summary>
        /// Sends an Email
        /// </summary>
        /// <param name="fromEmail"></param>
        /// <param name="smtpServer"></param>
        /// <param name="smtpPort"></param>
        /// <param name="smtpSsl"></param>
        /// <param name="smtpUser"></param>
        /// <param name="smtpPass"></param>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public static void SendMailMessage(MailMessage message, String smtpServer, int smtpPort, bool smtpSsl, String smtpUser, String smtpPass)
        {
            // Send MailMessage
            SmtpClient smtp = new SmtpClient();
            smtp.Host = smtpServer;
            smtp.Port = smtpPort;
            smtp.EnableSsl = smtpSsl;
            NetworkCredential NetworkCred = new NetworkCredential(smtpUser, smtpPass);
            smtp.Credentials = NetworkCred;

            smtp.Send(message);
        }

        public static MailMessage BuildMailMessage(String sender, String recipient, String subject, String body, bool attachZipFile=false, ProblemDescriptor pd = null)
        {
            body = RunningConfiguration.GetInstance().Variables.ReplaceKeywords(body);
            body = MailUtils.replaceAdditionalMailKeywords(body, pd);

            subject = RunningConfiguration.GetInstance().Variables.ReplaceKeywords(subject);
            subject = MailUtils.replaceAdditionalMailKeywords(subject, pd);

            // Create MailMessage
            MailMessage message = new MailMessage();
            message.From = new MailAddress(sender);
            message.To.Add(recipient);
            message.Subject = subject;

            if(body.Contains("{screenshots}"))
            {
                string screenshotHtml = "";
                //Embed Screenshot(s)
                DirectoryInfo currentFolder = RunningConfiguration.GetInstance().NewestBugtrackerFolder;
                FileInfo[] screenshots = currentFolder.GetFiles("*screenshot*", SearchOption.TopDirectoryOnly);
                if (screenshots.Length > 0)
                {
                    //create Object for each image with distinct cid
                    LinkedResource[] imgs = new LinkedResource[screenshots.Length];
                    int count = 1;
                    foreach (FileInfo screenshot in screenshots)
                    {
                        String cid = $"screenshot{count}";
                        imgs[count - 1] = new LinkedResource(screenshot.FullName, MediaTypeNames.Image.Jpeg);
                        imgs[count - 1].ContentId = cid;

                        screenshotHtml += $"<img src=\"cid:{cid}\" /><br />";

                        count++;
                    }
                    body = body.Replace("{screenshots}", screenshotHtml);
                    //create HtmlView and link previously created image objects
                    AlternateView htmlView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
                    foreach (LinkedResource img in imgs)
                    {
                        htmlView.LinkedResources.Add(img);
                    }
                    //finally add view to mail
                    message.AlternateViews.Add(htmlView);
                } else
                {
                    body = body.Replace("{screenshots}","");
                }
            }

            message.Body = body;
            message.IsBodyHtml = true;

            if(attachZipFile)
            {
                DirectoryInfo currentFolder = RunningConfiguration.GetInstance().NewestBugtrackerFolder;
                String zipFile = $"C:\\temp\\{currentFolder.Name}.zip";

                ZipFile.CreateFromDirectory(currentFolder.FullName, zipFile);
                FileInfo fi = new FileInfo(zipFile);
                if(fi.Length <= 25000000)
                    message.Attachments.Add(new System.Net.Mail.Attachment(zipFile));
            }  

            return message;
        }

        public static string replaceAdditionalMailKeywords(string text, ProblemDescriptor pd = null)
        {
            string newText = text;
            newText = replaceProblemCategoryKeyword(newText);
            if (pd != null) { newText = replaceProblemDescriptionKeyword(newText, pd); }
            newText = replaceLocalBugtrackerFoldersKeyword(newText);
            newText = replaceTargetBugtrackerFoldersKeyword(newText);

            return newText;
        }

        private static string replaceProblemCategoryKeyword(string text)
        {
            string problemCategory = RunningConfiguration.GetInstance().SelectedProblemCategory.Name;
            return text.Replace("{problemcategory}", problemCategory);
        }

        private static string replaceProblemDescriptionKeyword(string text, ProblemDescriptor pd)
        {
            string problemDescription = pd.ProblemDescription.Replace("\r\n", "<br/>"); ;
            return text.Replace("{problemdescription}", problemDescription);
        }

        private static string replaceLocalBugtrackerFoldersKeyword(string text)
        {
            List<string> folders = new List<string>();
            RunningConfiguration.GetInstance().BugtrackerFolders.ForEach(f =>
            {
                folders.Add(f.FullName);
            });
            return text.Replace("{bugtrackfolders_local}", string.Join(", ",folders));
        }

        private static string replaceTargetBugtrackerFoldersKeyword(string text)
        {
            List<string> paths = new List<string>();
            RunningConfiguration rc = RunningConfiguration.GetInstance();
            rc.SelectedProblemCategory.Targets.ForEach(p =>
            {
                Targeting.Target t = rc.Targets.GetTargetByName(p.Name);
                if (t.TypeIdentifier == "folder")
                {
                    // Cast to FolderTarget to access folder-specific properties
                    if (t is Targeting.Targets.FolderTarget folderTarget)
                    {
                        string folderName = string.IsNullOrEmpty(folderTarget.CustomBugtrackerFolderName)
                            ? rc.NewestBugtrackerFolder.Name
                            : folderTarget.CustomBugtrackerFolderName;
                        paths.Add(folderTarget.Path + "\\" + folderName);
                    }
                }
            });
            return text.Replace("{bugtrackfolders_target}", string.Join(", ", paths));
        }
        public class MailConfig
        {
            /// <summary>
            /// the recipient address
            /// </summary>
            public string RecipientAddress { get; set; }

            /// <summary>
            /// the recipient address
            /// </summary>
            public string SenderAddress { get; set; }

            /// <summary>
            /// the smtp server address
            /// </summary>
            public string SmtpServer { get; set; }

            /// <summary>
            /// the smtp server port
            /// </summary>
            public int SmtpPort { get; set; }

            /// <summary>
            /// whether smtp over ssl should be used
            /// </summary>
            public bool SmtpSSL { get; set; }

            /// <summary>
            /// the smtp username
            /// </summary>
            public string SmtpUser { get; set; }

            /// <summary>
            /// the smtp password
            /// </summary>
            public string SmtpPass { get; set; }

            /// <summary>
            /// the mail subject
            /// </summary>
            public string Subject { get; set; }

            /// <summary>
            /// HTML template source: a file path (local or UNC), an http/https URL, or an inline HTML string
            /// </summary>
            public string HtmlTemplate { get; set; }

            /// <summary>
            /// whether to attach a zip file of the current bugtracker folder
            /// </summary>
            public bool AttachZipFile { get; set; }

            /// <summary>
            /// Constructor without body
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="smtpServer"></param>
            /// <param name="smtpPort"></param>
            /// <param name="smtpSsl"></param>
            /// <param name="smtpUser"></param>
            /// <param name="smtpPass"></param>
            /// <param name="recipient"></param>
            /// <param name="subject"></param>
            /// <param name="htmlTemplate"></param>
            public MailConfig(String sender, String smtpServer, int smtpPort, bool smtpSsl, String smtpUser, String smtpPass, String recipient, String subject, String htmlTemplate, bool attachZipFile)
            {
                this.SenderAddress = sender;
                this.SmtpServer = smtpServer;
                this.SmtpPort = smtpPort;
                this.SmtpSSL = smtpSsl;
                this.SmtpUser = smtpUser;
                this.SmtpPass = smtpPass;
                this.RecipientAddress = recipient;
                this.Subject = subject;
                this.HtmlTemplate = htmlTemplate;
                this.AttachZipFile = attachZipFile;
            }

            public void Send(ProblemDescriptor pd = null)
            {
                string htmlBody = MailUtils.ResolveHtmlTemplate(this.HtmlTemplate);
                MailUtils.SendMailMessage(MailUtils.BuildMailMessage(this.SenderAddress, this.RecipientAddress, this.Subject, htmlBody, this.AttachZipFile, pd), this.SmtpServer, this.SmtpPort, this.SmtpSSL, this.SmtpUser, this.SmtpPass);
            }
        }

        /// <summary>
        /// Resolves an htmltemplate config value to an HTML string.
        /// Supports three formats, auto-detected from the value:
        ///   - http/https URL  → downloaded via HTTP GET
        ///   - Inline HTML     → any value whose trimmed content starts with '&lt;'
        ///   - File path       → local path or UNC share, read from disk
        /// </summary>
        /// <param name="value">The raw htmltemplate attribute value from config</param>
        /// <returns>HTML string ready to use as the mail body</returns>
        public static string ResolveHtmlTemplate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("htmltemplate value is empty.");

            // URL: download content via HTTP
            if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Logging.Logger.Log($"Downloading HTML template from URL: {value}", Logging.LoggingSeverity.Info);
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(15);
                return client.GetStringAsync(value).Result;
            }

            // Inline HTML: value itself is the template body
            if (value.TrimStart().StartsWith("<"))
            {
                Logging.Logger.Log("Using inline HTML as mail template.", Logging.LoggingSeverity.Info);
                return value;
            }

            // File path: local or UNC
            Logging.Logger.Log($"Reading HTML template from path: {value}", Logging.LoggingSeverity.Info);
            return File.ReadAllText(value);
        }
    }
}
