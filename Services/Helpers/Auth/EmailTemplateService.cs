using Core.DTOs.Auth;

namespace Services.Helpers.Auth
{
    public static class EmailTemplateService
    {
        public static string GetEmailVerificationTemplate(EmailVerificationData data)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verification - {data.CompanyName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #007bff; font-size: 32px; font-weight: bold; }}
        .content {{ margin-bottom: 30px; }}
        .otp-code {{ background: #f8f9fa; border: 2px solid #007bff; padding: 20px; text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; border-radius: 10px; }}
        .footer {{ text-align: center; color: #666; font-size: 14px; margin-top: 30px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>{data.CompanyName}</div>
        </div>
        
        <div class='content'>
            <h2>Email Verification</h2>
            <p>Hello {data.UserName},</p>
            
            <p>Thank you for signing up with {data.CompanyName}! To complete your registration, please verify your email address using the code below:</p>
            
            <div class='otp-code'>
                {data.OtpCode}
            </div>
            
            <div class='warning'>
                <strong>Important:</strong> This verification code will expire in {data.ExpirationMinutes} minutes.
            </div>
            
            <p>If you didn't create an account with {data.CompanyName}, please ignore this email.</p>
        </div>
        
        <div class='footer'>
            <p>Need help?  <a href='mailto:kamal0elsayed0@gmail.com'>Contact our support team.</a> </p>
            <p>&copy; 2026 {data.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetWelcomeEmailTemplate(WelcomeEmailData data)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to {data.CompanyName}</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; margin: 0; padding: 20px; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .logo {{ color: #007bff; font-size: 32px; font-weight: bold; }}
        .content {{ margin-bottom: 30px; }}
        .button {{ display: inline-block; background: #28a745; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .button:hover {{ background: #218838; }}
        .footer {{ text-align: center; color: #666; font-size: 14px; margin-top: 30px; }}
        .features {{ background: #f8f9fa; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .feature-item {{ margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>{data.CompanyName}</div>
        </div>
        
        <div class='content'>
            <h2>Welcome to {data.CompanyName}!</h2>
            <p>Hello {data.UserName},</p>
            
            <p>Welcome to {data.CompanyName}! We're excited to have you join our community. Your account has been successfully created.</p>
            
            <div class='features'>
                <h3>What you can do with {data.CompanyName}:</h3>
                <div class='feature-item'>🚗 Browse thousands of cars</div>
                <div class='feature-item'>🔍 Advanced search filters</div>
                <div class='feature-item'>💖 Save your favorite cars</div>
                <div class='feature-item'>📱 Mobile-friendly experience</div>
            </div>
            
            <p>Ready to start exploring? Click the button below to access your account:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <a href='{data.LoginUrl}' class='button'>Start Exploring</a>
            </div>
            
            <p>If you have any questions, our support team is here to help!</p>
        </div>
        
        <div class='footer'>
            <p>Thank you for choosing {data.CompanyName}!</p>
            <p>&copy; 2024 {data.CompanyName}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        public static string GetPasswordResetOtpTemplate(PasswordResetOtpData data)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #f9f9f9; padding: 20px; border-radius: 10px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .otp-code {{ font-size: 24px; font-weight: bold; color: #007bff; text-align: center; margin: 20px 0; padding: 15px; background-color: #f0f8ff; border-radius: 5px; }}
        .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{data.CompanyName}</h1>
            <h2>Password Reset Request</h2>
        </div>
        
        <p>Hello {data.UserName},</p>
        
        <p>We received a request to reset your password. Use the following verification code to reset your password:</p>
        
        <div class='otp-code'>{data.OtpCode}</div>
        
        <p>This code will expire in {data.ExpirationMinutes} minutes.</p>
        
        <p>If you didn't request this password reset, please ignore this email.</p>
        
        <div class='footer'>
            <p>Best regards,<br>The {data.CompanyName} Team</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}