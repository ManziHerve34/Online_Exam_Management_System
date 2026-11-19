using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace OnlineExamsSystem.Services
{
    public static class EmailService
    {
        public static string SmtpHost => ConfigurationManager.AppSettings["SmtpHost"] ?? "smtp.gmail.com";

        public static int SmtpPort
        {
            get
            {
                string portStr = ConfigurationManager.AppSettings["SmtpPort"];
                return int.TryParse(portStr, out int p) ? p : 587;
            }
        }

        public static string SmtpUsername => ConfigurationManager.AppSettings["SmtpUsername"] ?? "";
        public static string SmtpPassword => ConfigurationManager.AppSettings["SmtpPassword"] ?? "";
        public static string SmtpFrom => ConfigurationManager.AppSettings["SmtpFrom"] ?? "no-reply@exams-scheduling.com";

        public static bool SmtpEnableSsl
        {
            get
            {
                string sslStr = ConfigurationManager.AppSettings["SmtpEnableSsl"];
                return bool.TryParse(sslStr, out bool ssl) ? ssl : true;
            }
        }

        public static bool SendVerificationEmail(string toEmail, string candidateName, string verificationCode, out string message)
        {
            message = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(toEmail))
                {
                    message = "Recipient email address is invalid.";
                    return false;
                }

                string subject = $"Your Exam Account Verification Code: {verificationCode}";
                string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'/>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; border: 1px solid #e2e8f0; overflow: hidden; }}
                        .header {{ background-color: #0f172a; padding: 24px; text-align: center; color: #ffffff; }}
                        .header h1 {{ margin: 0; font-size: 20px; }}
                        .content {{ padding: 32px 24px; color: #1e293b; line-height: 1.6; }}
                        .code-box {{ background-color: #eff6ff; border: 2px dashed #2563eb; border-radius: 8px; padding: 18px; text-align: center; margin: 24px 0; }}
                        .code {{ font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #2563eb; font-family: monospace; }}
                        .footer {{ background-color: #f1f5f9; padding: 16px; text-align: center; font-size: 12px; color: #64748b; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Online Professional Exams Scheduling and Management System</h1>
                        </div>
                        <div class='content'>
                            <p>Dear <strong>{candidateName}</strong>,</p>
                            <p>Thank you for creating your candidate account. To verify your email address and activate your account for examination scheduling, please use the 6-digit verification code below:</p>
                            
                            <div class='code-box'>
                                <div class='code'>{verificationCode}</div>
                            </div>
                            
                            <p>Enter this verification code in the application to complete your account setup and access the examination catalog.</p>
                            <p>If you did not register for an exam account, please disregard this email or contact administrative staff.</p>
                        </div>
                        <div class='footer'>
                            Online Professional Exams Scheduling and Management System<br/>
                            Developer: Manzi Herve (ID: 24605) • AUCA DotNet Group A
                        </div>
                    </div>
                </body>
                </html>";

                return SendEmail(toEmail, subject, htmlBody, out message);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        public static bool SendPasswordResetEmail(string toEmail, string otpCode, out string message)
        {
            message = string.Empty;
            try
            {
                string subject = $"Password Reset OTP: {otpCode}";
                string htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='utf-8'/>
                    <style>
                        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f8fafc; margin: 0; padding: 20px; }}
                        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; border: 1px solid #e2e8f0; overflow: hidden; }}
                        .header {{ background-color: #0f172a; padding: 24px; text-align: center; color: #ffffff; }}
                        .header h1 {{ margin: 0; font-size: 20px; }}
                        .content {{ padding: 32px 24px; color: #1e293b; line-height: 1.6; }}
                        .code-box {{ background-color: #fef2f2; border: 2px dashed #ef4444; border-radius: 8px; padding: 18px; text-align: center; margin: 24px 0; }}
                        .code {{ font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #dc2626; font-family: monospace; }}
                        .footer {{ background-color: #f1f5f9; padding: 16px; text-align: center; font-size: 12px; color: #64748b; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Exam Account Password Recovery</h1>
                        </div>
                        <div class='content'>
                            <p>Hello,</p>
                            <p>We received a request to reset your password. Use the following One-Time Password (OTP) to proceed with resetting your password:</p>
                            
                            <div class='code-box'>
                                <div class='code'>{otpCode}</div>
                            </div>
                            
                            <p>This OTP will expire in <strong>15 minutes</strong>. If you did not initiate this request, your account is safe and no action is required.</p>
                        </div>
                        <div class='footer'>
                            Online Professional Exams Scheduling and Management System
                        </div>
                    </div>
                </body>
                </html>";

                return SendEmail(toEmail, subject, htmlBody, out message);
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        private static bool SendEmail(string toEmail, string subject, string htmlBody, out string message)
        {
            message = string.Empty;
            try
            {
                string host = SmtpHost;
                int port = SmtpPort;
                string username = SmtpUsername;
                string password = SmtpPassword;
                string from = SmtpFrom;
                bool enableSsl = SmtpEnableSsl;

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(string.IsNullOrWhiteSpace(from) ? "no-reply@exams.com" : from, "Online Exams System");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = htmlBody;
                    mail.IsBodyHtml = true;
                    mail.BodyEncoding = Encoding.UTF8;

                    using (var client = new SmtpClient(host, port))
                    {
                        client.EnableSsl = enableSsl;
                        client.Timeout = 12000;

                        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                        {
                            client.UseDefaultCredentials = false;
                            client.Credentials = new NetworkCredential(username, password);
                        }
                        else
                        {
                            client.UseDefaultCredentials = true;
                        }

                        client.Send(mail);
                        message = "Verification email dispatched successfully to your inbox.";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                message = "SMTP Dispatch Notice: " + ex.Message;
                return false;
            }
        }
    }
}
