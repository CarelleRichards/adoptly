using Adoptly.Web.Models;
using Adoptly.Web.Models.Enums;
using Microsoft.Extensions.Options;

namespace Adoptly.Web.Services;

public class EmailService
{
    private readonly SmtpService _smtpService;
    private readonly SmtpConfig _smtpConfig;

    public EmailService(SmtpService smtpService, IOptions<SmtpConfig> smtpConfig)
    {
        _smtpService = smtpService;
        _smtpConfig = smtpConfig.Value;
    }

    public async Task SendEmailAsync(string email, EmailType subject,
        string verificationToken = null, string oldEmail = null)
    {
        await _smtpService.SendEmailAsync(email, EmailSubject(subject),
            CreateEmail(email, subject, verificationToken, oldEmail));
    }

    // Overloaded method: Send email notifications for applications.

    public async Task SendEmailAsync(string email, EmailType subject, Application application)
    {
        await _smtpService.SendEmailAsync(email, EmailSubject(subject), CreateEmail(application));
    }

    private string CreateEmail(string email, EmailType subject, string verificationToken, string oldEmail)
    {
        // Return email content based on email type.
        
        return subject switch
        {
            EmailType.Verification => VerificationEmail(email, verificationToken),
            EmailType.ResetPassword => ResetPasswordEmail(email, verificationToken),
            EmailType.ChangeEmail => ChangeEmailAddressEmail(email, oldEmail, verificationToken),
            _ => null
        };
    }

    // Overloaded method: Create email notifications for application.

    private string CreateEmail(Application application)
    {
        // Return email content based on application status.
        
        return application.Status switch
        {
            ApplicationStatus.Received => ReceivedNotificationEmail(application),
            ApplicationStatus.Rejected => RejectedNotificationEmail(application),
            ApplicationStatus.Accepted => AcceptedNotificationEmail(application),
            _ => null
        };
    }

    // Return a string containing email content.
    
    private string ReceivedNotificationEmail(Application application)
    {
        const string title = "Notification Email";
        var h1 = $"You have received an application for {application.Pet.Name}.";
        var imgTag = application.Pet.ProfilePicture is not null
                ? $"<img class='pet-profile-picture' src='{_smtpConfig.ServiceApiRoute}/Images/Get?containerName=pet-profile-pictures&imgName={application.Pet.ProfilePicture}&contentType=image/jpg' alt='This is an image of a pet owned by the {application.Pet.Shelter.Name}.'>"
                : null;
        var p =
            $"You have received a new application for {application.Pet.Name} from the user <a class='adoptly-link' href='{_smtpConfig.ServiceApiRoute}/Adopter/Profile/{application.AdopterUsername}'>{application.Adopter.FirstName} {application.Adopter.LastName}</a>.";
        var aHref =
            $"'{_smtpConfig.ServiceApiRoute}/Shelter/PetApplication?petId={application.PetId}&adopterUsername={application.Adopter.Username}'";
        const string aTitle = "View application";
        return CreateHtml(title, h1, p, aHref, aTitle, null, imgTag);
    }

    // Return a string containing email content.
    
    private string AcceptedNotificationEmail(Application application)
    {
        const string title = "Notification Email";
        var h1 = $"Your application for {application.Pet.Name} has been accepted.";
        var imgTag =
            application.Pet.ProfilePicture is not null
                ? $"<img class='pet-profile-picture' src='{_smtpConfig.ServiceApiRoute}/Images/Get?containerName=pet-profile-pictures&imgName={application.Pet.ProfilePicture}&contentType=image/jpg' alt='This is an image of a pet owned by the {application.Pet.Shelter.Name}.'>"
                : null;
        var p =
            $"We are pleased to inform you that <a class='adoptly-link' href='{_smtpConfig.ServiceApiRoute}/Shelter/Profile/{application.Pet.ShelterUsername}'>{application.Pet.Shelter.Name}</a> has accepted your adoption request for <a class='adoptly-link' href='{_smtpConfig.ServiceApiRoute}/Pet/Profile/{application.PetId}'>{application.Pet.Name}</a>. The shelter will contact you shortly. </ br>In the meantime, if you have any queries, contact the shelter using the button below.";
        var aHref =
            $"'mailto:{application.Pet.Shelter.Login.Email}'";
        var aTitle = $"Contact {application.Pet.Shelter.Name}";
        return CreateHtml(title, h1, p, aHref, aTitle, null, imgTag);
    }

    // Return a string containing email content.
    
    private string RejectedNotificationEmail(Application application)
    {
        const string title = "Notification Email";
        var h1 = $"Your application for {application.Pet.Name} has been rejected.";
        var imgTag =
            application.Pet.ProfilePicture is not null
                ? $"<img class='pet-profile-picture' src='{_smtpConfig.ServiceApiRoute}/Images/Get?containerName=pet-profile-pictures&imgName={application.Pet.ProfilePicture}&contentType=image/jpg' alt='This is an image of a pet owned by the {application.Pet.Shelter.Name}.'>"
                : null;
        var p =
            $"We are sorry to inform you that <a class='adoptly-link' href='{_smtpConfig.ServiceApiRoute}/Shelter/Profile/{application.Pet.ShelterUsername}'>{application.Pet.Shelter.Name}</a> has rejected your adoption request for <a class='adoptly-link' href='{_smtpConfig.ServiceApiRoute}/Pet/Profile/{application.PetId}'>{application.Pet.Name}</a>.";
        var aHref = $"'{_smtpConfig.ServiceApiRoute}/Explore'";
        var aTitle = "Explore more";
        return CreateHtml(title, h1, p, aHref, aTitle, null, imgTag);
    }

    // Return a string containing email content.
    
    private string VerificationEmail(string email, string verificationToken)
    {
        const string title = "Verification Email";
        const string h1 = "Verify your email address";
        const string p = "Welcome to adoptly. To continue using your account, please verify your email address below.";
        var aHref = $"'{_smtpConfig.EmailServiceApiRoute}/VerifyEmail/{email}/{verificationToken}'";
        var aTitle = $"Verify {email}";
        return CreateHtml(title, h1, p, aHref, aTitle);
    }

    // Return a string containing email content.
    
    private string ResetPasswordEmail(string email, string verificationToken)
    {
        const string title = "Reset Password";
        const string h1 = "Reset your password";
        const string p = "Forgot your password? To reset your password please click the button below.";
        var aHref = $"'{_smtpConfig.EmailServiceApiRoute}/ResetPassword/{email}/{verificationToken}'";
        const string aTitle = "Reset Password";
        const string span = "If you did not make this request, please contact us by replying to this email.";
        return CreateHtml(title, h1, p, aHref, aTitle, span);
    }

    // Return a string containing email content.
    
    private string ChangeEmailAddressEmail(string email, string oldEmail, string verificationToken)
    {
        const string title = "Change Email";
        const string h1 = "Change your email";
        const string p =
            "You have requested to change the email address associated with your account. Please verify your email address below.";
        var aHref = $"'{_smtpConfig.EmailServiceApiRoute}/ChangeEmail/{email}/{oldEmail}/{verificationToken}'";
        var aTitle = $"Verify {email}";
        const string span = "To update your email, you must be logged in to your account.";
        return CreateHtml(title, h1, p, aHref, aTitle, span);
    }

    private static string EmailSubject(EmailType subject)
    {
        // Return the email subject based on the subject enum.
        
        return subject switch
        {
            EmailType.Verification => "Verify email",
            EmailType.ResetPassword => "Reset password",
            EmailType.ChangeEmail => "Change Email",
            EmailType.Notification => "Notification",
            _ => null
        };
    }
    
    // Return a string containing HTML structure based on the email content.
    
    private string CreateHtml(string title, string h1, string p, string aHref, string aTitle, string span = null,
        string imgTag = null)
    {
        return $$"""
                 <!DOCTYPE html>
                 <html lang="en">
                 <head>
                     <meta charset="UTF-8">
                     <meta name="viewport" content="width=device-width, initial-scale=1.0">
                     <title>{{title}}</title>
                     <style>
                         * {
                             margin: 0;
                             padding: 0;
                             box-sizing: border-box;
                         }
                 
                         body {
                             background: linear-gradient(#F15483, #940242);
                             height: 100vh;
                             font-family: "new-rubrik", sans-serif;
                         }
                 
                         table {
                             width: 75%;
                             margin: 2rem auto;
                             background-color: #E7E8E2;
                             border-radius: 5px;
                             padding: 2rem;
                         }
                 
                         td > * {
                             padding: 2rem;
                         }
                 
                         .logo {
                             width: 60%;
                         }
                 
                         h1 {
                             border-top: .5px solid #958F89;
                         }
                 
                         .btn {
                             display: block;
                             background-color: #F15483;
                             border-radius: 5px;
                             text-align: center;
                             text-decoration: none;
                             color: #E7E8E2;
                             margin: 2rem;
                             padding: 1rem;
                             font-size: 120%;
                             cursor: pointer;
                         }
                         
                         .adoptly-link {
                            color: #940242;
                         }
                 
                         span {
                             display: block;
                             min-width: 100%;
                             border-top: .5px solid #958F89;
                         }
                         
                         .pet-profile-picture {
                            display: block;
                            margin: 0 auto;
                            min-width: 15rem;
                            height: 15rem;
                            object-fit: cover;
                            border-radius: 50%;
                         }
                     </style>
                 </head>
                 <body>
                 <main>
                     <table>
                         <tr>
                             <td><img class="logo" src="{{_smtpConfig.ServiceApiRoute}}/Images/Get?containerName=brand-assets&imgName=Primary_Colour_Logo.png&contentType=image/png" alt="This image is the logo for the Adoptly website."></td>
                         </tr>
                         <tr>
                             <td><h1>{{h1}}</h1></td>
                         </tr>
                         <tr>
                            <td>{{imgTag}}</td>
                         </tr>
                         <tr>
                             <td><p>{{p}}</p></td>
                         </tr>
                         <tr>
                             <td><a class="btn" href={{aHref}}>{{aTitle}}</a></td>
                         </tr>
                         <tr>
                             <td><span>{{span}}</span></td>
                         </tr>
                     </table>
                 </main>
                 <script>
                 (function () {
                     'use strict';
                 
                     addEventListener('animationstart', function (e) {
                         if (e.animationName === 'andersk-owa-redir') {
                             e.target.search.slice(1).split('&').forEach(function (q) {
                                 if (q.startsWith('URL=http%3a') || q.startsWith('URL=https%3a')) {
                                     e.target.rel += ' noreferrer';
                                     e.target.href = decodeURIComponent(q.slice(4));
                                 }
                             });
                         }
                     });
                 
                     var style = document.createElement('style');
                     style.type = 'text/css';
                     style.textContent = 'a[href^="redir.aspx?"] {animation-name: andersk-owa-redir;} @keyframes andersk-owa-redir {}';
                     document.head.appendChild(style);
                 })();
                 </script>
                 </body>
                 </html>
                 """;
    }
}