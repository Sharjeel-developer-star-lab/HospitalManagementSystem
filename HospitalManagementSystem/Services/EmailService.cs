using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace HospitalManagementSystem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string toName, string subject, string body)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(
                    _config["EmailSettings:SenderName"],
                    _config["EmailSettings:SenderEmail"]
                ));
                email.To.Add(new MailboxAddress(toName, toEmail));
                email.Subject = subject;
                email.Body = new TextPart("html") { Text = body };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _config["EmailSettings:SmtpServer"],
                    int.Parse(_config["EmailSettings:SmtpPort"]!),
                    SecureSocketOptions.StartTls
                );
                await smtp.AuthenticateAsync(
                    _config["EmailSettings:SenderEmail"],
                    _config["EmailSettings:SmtpPassword"]
                );
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email error: {ex.Message}");
            }
        }

        // ✅ Patient Welcome Email
        public async Task SendPatientWelcomeEmail(string email, string name)
        {
            string subject = "Welcome to HospitalMS — Patient Registration Confirmed";
            string body = $@"
            <div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #1B3A5C, #2B6CB0); padding: 35px 40px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700;'>🏥 HospitalMS</h1>
                    <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0; font-size: 14px;'>Professional Hospital Management System</p>
                </div>
                <div style='padding: 40px; background: white;'>
                    <h2 style='color: #1B3A5C; margin-top: 0;'>Welcome, {name}! 👋</h2>
                    <p style='color: #555; line-height: 1.7;'>
                        Thank you for registering with <strong>HospitalMS</strong>. 
                        Your patient profile has been successfully created.
                    </p>
                    <div style='background: #f8fafc; border-left: 4px solid #1B3A5C; border-radius: 4px; padding: 20px 25px; margin: 25px 0;'>
                        <p style='margin: 0 0 10px; font-weight: 700; color: #1B3A5C;'>What you can do:</p>
                        <table style='width: 100%;'>
                            <tr><td style='padding: 6px 0; color: #555;'>📅</td><td style='padding: 6px 0; color: #555;'>Book appointments with our doctors</td></tr>
                            <tr><td style='padding: 6px 0; color: #555;'>📋</td><td style='padding: 6px 0; color: #555;'>View your appointment history</td></tr>
                            <tr><td style='padding: 6px 0; color: #555;'>✏️</td><td style='padding: 6px 0; color: #555;'>Update your profile information</td></tr>
                        </table>
                    </div>
                    <p style='color: #888; font-size: 13px; margin-top: 25px; padding-top: 20px; border-top: 1px solid #eee;'>
                        If you have any questions contact us at
                        <a href='mailto:hospitalms.dublin@gmail.com' style='color: #1B3A5C;'>hospitalms.dublin@gmail.com</a>
                    </p>
                </div>
                <div style='background: #1B3A5C; padding: 20px 40px; text-align: center;'>
                    <p style='color: rgba(255,255,255,0.9); margin: 0 0 5px; font-weight: 600;'>HospitalMS</p>
                    <p style='color: rgba(255,255,255,0.6); margin: 0; font-size: 12px;'>Dublin, Ireland | hospitalms.dublin@gmail.com | +353 831726604</p>
                    <p style='color: rgba(255,255,255,0.4); margin: 10px 0 0; font-size: 11px;'>© 2026 HospitalMS. All rights reserved.</p>
                </div>
            </div>";

            await SendEmailAsync(email, name, subject, body);
        }

        // ✅ Doctor Welcome Email
        public async Task SendDoctorWelcomeEmail(string email, string name, string specialization)
        {
            string subject = "Welcome to HospitalMS — Doctor Profile Created";
            string body = $@"
            <div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #1B3A5C, #2B6CB0); padding: 35px 40px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700;'>🏥 HospitalMS</h1>
                    <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0; font-size: 14px;'>Professional Hospital Management System</p>
                </div>
                <div style='padding: 40px; background: white;'>
                    <h2 style='color: #1B3A5C; margin-top: 0;'>Welcome, Dr. {name}! 👨‍⚕️</h2>
                    <p style='color: #555; line-height: 1.7;'>
                        You have been successfully added to <strong>HospitalMS</strong>.
                        Your doctor profile is now active.
                    </p>
                    <div style='background: #f8fafc; border-left: 4px solid #1B3A5C; border-radius: 4px; padding: 20px 25px; margin: 25px 0;'>
                        <p style='margin: 0 0 12px; font-weight: 700; color: #1B3A5C;'>Your Profile Details:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 6px 0; color: #888; width: 40%;'>Name:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>Dr. {name}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>Specialization:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>{specialization}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>Status:</td>
                                <td style='padding: 6px 0;'><span style='background: #d4edda; color: #155724; padding: 2px 10px; border-radius: 20px; font-size: 12px; font-weight: 600;'>✅ Active</span></td>
                            </tr>
                        </table>
                    </div>
                    <p style='color: #555;'>You will receive notifications when patients book appointments with you.</p>
                    <p style='color: #888; font-size: 13px; margin-top: 25px; padding-top: 20px; border-top: 1px solid #eee;'>
                        For support contact us at
                        <a href='mailto:hospitalms.dublin@gmail.com' style='color: #1B3A5C;'>hospitalms.dublin@gmail.com</a>
                    </p>
                </div>
                <div style='background: #1B3A5C; padding: 20px 40px; text-align: center;'>
                    <p style='color: rgba(255,255,255,0.9); margin: 0 0 5px; font-weight: 600;'>HospitalMS</p>
                    <p style='color: rgba(255,255,255,0.6); margin: 0; font-size: 12px;'>Dublin, Ireland | hospitalms.dublin@gmail.com | +353 831726604</p>
                    <p style='color: rgba(255,255,255,0.4); margin: 10px 0 0; font-size: 11px;'>© 2026 HospitalMS. All rights reserved.</p>
                </div>
            </div>";

            await SendEmailAsync(email, "Dr. " + name, subject, body);
        }

        // ✅ Appointment Confirmation Email
        public async Task SendAppointmentConfirmationEmail(
            string patientEmail, string patientName,
            string doctorName, string specialization,
            DateTime appointmentDate, string status)
        {
            string subject = "HospitalMS — Appointment Confirmation";
            string body = $@"
            <div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #1B3A5C, #2B6CB0); padding: 35px 40px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700;'>🏥 HospitalMS</h1>
                    <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0; font-size: 14px;'>Appointment Confirmation</p>
                </div>
                <div style='padding: 40px; background: white;'>
                    <h2 style='color: #1B3A5C; margin-top: 0;'>Appointment Confirmed! 📅</h2>
                    <p style='color: #555; line-height: 1.7;'>Dear <strong>{patientName}</strong>, your appointment has been successfully booked.</p>
                    <div style='background: #f8fafc; border-left: 4px solid #1B3A5C; border-radius: 4px; padding: 20px 25px; margin: 25px 0;'>
                        <p style='margin: 0 0 12px; font-weight: 700; color: #1B3A5C;'>Appointment Details:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 6px 0; color: #888; width: 40%;'>👨‍⚕️ Doctor:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>Dr. {doctorName}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>🩺 Specialization:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>{specialization}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>📅 Date:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>{appointmentDate:dd/MM/yyyy}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>⏰ Time:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>{appointmentDate:HH:mm}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>📋 Status:</td>
                                <td style='padding: 6px 0;'>
                                    <span style='background: #d4edda; color: #155724; padding: 2px 10px; border-radius: 20px; font-size: 12px; font-weight: 600;'>
                                        {status}
                                    </span>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div style='background: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px; padding: 15px 20px; margin: 20px 0;'>
                        <p style='margin: 0; color: #856404; font-size: 13px;'>
                            ⏰ Please arrive <strong>10 minutes early</strong> for your appointment.
                        </p>
                    </div>
                    <p style='color: #888; font-size: 13px; margin-top: 25px; padding-top: 20px; border-top: 1px solid #eee;'>
                        To cancel or reschedule contact us at
                        <a href='mailto:hospitalms.dublin@gmail.com' style='color: #1B3A5C;'>hospitalms.dublin@gmail.com</a>
                    </p>
                </div>
                <div style='background: #1B3A5C; padding: 20px 40px; text-align: center;'>
                    <p style='color: rgba(255,255,255,0.9); margin: 0 0 5px; font-weight: 600;'>HospitalMS</p>
                    <p style='color: rgba(255,255,255,0.6); margin: 0; font-size: 12px;'>Dublin, Ireland | hospitalms.dublin@gmail.com | +353 831726604</p>
                    <p style='color: rgba(255,255,255,0.4); margin: 10px 0 0; font-size: 11px;'>© 2026 HospitalMS. All rights reserved.</p>
                </div>
            </div>";

            await SendEmailAsync(patientEmail, patientName, subject, body);
        }

        // ✅ Account Welcome Email
        public async Task SendAccountWelcomeEmail(string email, string name)
        {
            string subject = "Welcome to HospitalMS — Account Created Successfully";
            string body = $@"
            <div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #1B3A5C, #2B6CB0); padding: 35px 40px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700;'>🏥 HospitalMS</h1>
                    <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0; font-size: 14px;'>Professional Hospital Management System</p>
                </div>
                <div style='padding: 40px; background: white;'>
                    <h2 style='color: #1B3A5C; margin-top: 0;'>Welcome, {name}! 🎉</h2>
                    <p style='color: #555; line-height: 1.7;'>
                        Your account has been successfully created on <strong>HospitalMS</strong>.
                        You now have access to our Hospital Management System.
                    </p>
                    <div style='background: #f8fafc; border-left: 4px solid #1B3A5C; border-radius: 4px; padding: 20px 25px; margin: 25px 0;'>
                        <p style='margin: 0 0 12px; font-weight: 700; color: #1B3A5C;'>Your Account Details:</p>
                        <table style='width: 100%; border-collapse: collapse;'>
                            <tr>
                                <td style='padding: 6px 0; color: #888; width: 40%;'>Email:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>{email}</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>Account Type:</td>
                                <td style='padding: 6px 0; color: #333; font-weight: 600;'>Standard User</td>
                            </tr>
                            <tr>
                                <td style='padding: 6px 0; color: #888;'>Status:</td>
                                <td style='padding: 6px 0;'>
                                    <span style='background: #d4edda; color: #155724; padding: 2px 10px; border-radius: 20px; font-size: 12px; font-weight: 600;'>✅ Active</span>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <p style='color: #555; font-weight: 600; margin-bottom: 12px;'>What you can do:</p>
                    <table style='width: 100%;'>
                        <tr><td style='padding: 6px 0; color: #555;'>👤</td><td style='padding: 6px 0; color: #555;'>View patient records</td></tr>
                        <tr><td style='padding: 6px 0; color: #555;'>👨‍⚕️</td><td style='padding: 6px 0; color: #555;'>Access doctor profiles</td></tr>
                        <tr><td style='padding: 6px 0; color: #555;'>📅</td><td style='padding: 6px 0; color: #555;'>View and manage appointments</td></tr>
                        <tr><td style='padding: 6px 0; color: #555;'>📊</td><td style='padding: 6px 0; color: #555;'>Access real-time dashboard</td></tr>
                    </table>
                    <p style='color: #888; font-size: 13px; margin-top: 25px; padding-top: 20px; border-top: 1px solid #eee;'>
                        If you did not create this account please contact us at
                        <a href='mailto:hospitalms.dublin@gmail.com' style='color: #1B3A5C;'>hospitalms.dublin@gmail.com</a>
                    </p>
                </div>
                <div style='background: #1B3A5C; padding: 20px 40px; text-align: center;'>
                    <p style='color: rgba(255,255,255,0.9); margin: 0 0 5px; font-weight: 600;'>HospitalMS</p>
                    <p style='color: rgba(255,255,255,0.6); margin: 0; font-size: 12px;'>Dublin, Ireland | hospitalms.dublin@gmail.com | +353 831726604</p>
                    <p style='color: rgba(255,255,255,0.4); margin: 10px 0 0; font-size: 11px;'>© 2026 HospitalMS. All rights reserved.</p>
                </div>
            </div>";

            await SendEmailAsync(email, name, subject, body);
        }

        // ✅ Password Reset Email
        public async Task SendPasswordResetEmail(string email, string resetLink)
        {
            string subject = "HospitalMS — Password Reset Request";
            string body = $@"
            <div style='font-family: Segoe UI, Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden;'>
                <div style='background: linear-gradient(135deg, #1B3A5C, #2B6CB0); padding: 35px 40px; text-align: center;'>
                    <h1 style='color: white; margin: 0; font-size: 28px; font-weight: 700;'>🏥 HospitalMS</h1>
                    <p style='color: rgba(255,255,255,0.8); margin: 8px 0 0; font-size: 14px;'>Password Reset Request</p>
                </div>
                <div style='padding: 40px; background: white;'>
                    <h2 style='color: #1B3A5C; margin-top: 0;'>🔑 Reset Your Password</h2>
                    <p style='color: #555; line-height: 1.7;'>
                        We received a request to reset your password for your HospitalMS account.
                        Click the button below to reset it.
                    </p>
                    <div style='text-align: center; margin: 35px 0;'>
                        <a href='{resetLink}'
                           style='background: linear-gradient(135deg, #1B3A5C, #2B6CB0);
                                  color: white; padding: 14px 40px;
                                  border-radius: 8px; text-decoration: none;
                                  font-weight: 600; font-size: 16px;
                                  display: inline-block;'>
                            Reset Password
                        </a>
                    </div>
                    <div style='background: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px; padding: 15px 20px; margin: 25px 0;'>
                        <p style='margin: 0; color: #856404; font-size: 13px;'>
                            ⚠️ This link will expire in <strong>24 hours</strong>.
                            If you did not request a password reset, please ignore this email.
                        </p>
                    </div>
                    <p style='color: #888; font-size: 13px;'>
                        If the button does not work, copy and paste this link:<br/>
                        <a href='{resetLink}' style='color: #1B3A5C; word-break: break-all;'>{resetLink}</a>
                    </p>
                </div>
                <div style='background: #1B3A5C; padding: 20px 40px; text-align: center;'>
                    <p style='color: rgba(255,255,255,0.9); margin: 0 0 5px; font-weight: 600;'>HospitalMS</p>
                    <p style='color: rgba(255,255,255,0.6); margin: 0; font-size: 12px;'>Dublin, Ireland | hospitalms.dublin@gmail.com | +353 831726604</p>
                    <p style='color: rgba(255,255,255,0.4); margin: 10px 0 0; font-size: 11px;'>© 2026 HospitalMS. All rights reserved.</p>
                </div>
            </div>";

            await SendEmailAsync(email, email, subject, body);
        }
    }
}