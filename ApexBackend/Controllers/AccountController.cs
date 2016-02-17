using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using ApexBackend.Models;
using ApexBackend.Providers;
using ApexBackend.Results;
using System.Net.Mail;
using System.Text;
using System.Web.Security;

namespace ApexBackend.Controllers
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;
        private ApplicationDbContext db;

        public AccountController()
        {
            db = new ApplicationDbContext();
        }

        public AccountController(ApplicationDbContext context)
        {
            db = context;
        }

        public AccountController(ApplicationUserManager userManager,
            ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        // GET: api/Account/Users
        [Route("Users")]
        [Authorize(Roles = "Admin")]
        public List<ApplicationUser> GetUsers()
        {
            return db.Users.ToList();
        }

        // GET: api/Account/GetAdminEmail
        [Route("GetAdminEmail")]
        [Authorize(Roles = "Admin, Doctor")]
        public String GetAdminEmail()
        {
            var roleAdmin = db.Roles.Where(r => r.Name == "Admin").FirstOrDefault();

            var admin = db.Users
                .Where(x => x.Roles.Select(y => y.RoleId).Contains(roleAdmin.Id))
                .FirstOrDefault();

            return admin.Email;
        }

        // POST: api/Account/EditAdminEmail
        [Route("EditAdminEmail")]
        [Authorize(Roles = "Admin")]
        public IHttpActionResult EditAdminEmail(EditAdminEmailBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var roleAdmin = db.Roles.Where(r => r.Name == "Admin").FirstOrDefault();

            ApplicationUser admin = db.Users
                .Where(x => x.Roles.Select(y => y.RoleId).Contains(roleAdmin.Id))
                .FirstOrDefault();

            var stubUser = db.Users.Where(r => r.Email == model.Email).FirstOrDefault();

            if (stubUser != null)
            {
                if (admin.Id != stubUser.Id)
                {
                    return BadRequest("Email address " + model.Email + " is taken.");
                }
            }

            admin.Email = model.Email;
            admin.UserName = model.Email;

            db.Entry(admin).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();

            return Ok();
        }

        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }

        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RegisterAdmin
        [Authorize(Roles = "Admin")]
        [Route("RegisterAdmin")]
        public async Task<IHttpActionResult> RegisterAdmin(RegisterAdminBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            UserManager.AddToRole(user.Id, "Admin");

            return Ok();
        }

        // POST api/Account/RegisterDoctor
        [Authorize(Roles = "Admin")]
        [Route("RegisterDoctor")]
        public async Task<IHttpActionResult> RegisterDoctor(RegisterDoctorBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            UserManager.AddToRole(user.Id, "Doctor");

            var doctor = new Doctor
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserId = user.Id
            };

            db.Doctors.Add(doctor);
            db.SaveChanges();

            return Ok();
        }

        // POST api/Account/RegisterPatient
        [Authorize(Roles = "Doctor")]
        [Route("RegisterPatient")]
        public async Task<IHttpActionResult> RegisterPatient(RegisterPatientBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Doctor doctor = db.Doctors.Find(model.DoctorId);
            if (doctor == null)
            {
                return BadRequest("Doctor with id " + model.DoctorId + " does not exist.");
            }

            var user = new ApplicationUser() { UserName = model.PatientUsername };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            UserManager.AddToRole(user.Id, "Patient");

            var patient = new Patient
            {
                DoctorId = model.DoctorId,
                UserId = user.Id
            };

            db.Patients.Add(patient);
            db.SaveChanges();

            return Ok();
        }

        // POST: api/Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User with email " + model.Email + " does not exist.");
            }

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

            // Command line argument must the the SMTP host.
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("apexrd.health@gmail.com", "thisismypass");

            MailMessage mm = new MailMessage("noreply@apex.com", model.Email, "Reset Password",
                "<div style='padding - top: 2 %; padding - bottom: 2 %; text - align: center; font - size: 62px; font - weight: 900; background - color: rgb(129, 129, 129); color: white'>   APEX HEALTH</div><div style = 'text-align: center;'>     <div style = 'width: 60%; margin-left: 20%; margin-right: 20%; text-align: left; font-size: 30px; font-weight: 600'>          <p>              You can reset your password by clicking <a href =\'http://localhost:8080/webdashboard/#/setPassword\'>here</a>.</p><p>Please copy the following token and enter it while resetting your password: <span style = 'color: #ac273e'>" + code + "</span>             </p>         </div>     </div>");
            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.IsBodyHtml = true;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);

            return Ok("A mail has been sent to " + model.Email);
        }

        // POST api/Account/SetPassword
        [HttpPost]
        [AllowAnonymous]
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(ResetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = model.Token.Replace(" ", "+");

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                return BadRequest("User with email " + model.Email + " does not exist.");
            }

            IdentityResult result = await UserManager.ResetPasswordAsync(user.Id, token,
            model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest("Resetting password failed.");
            }

            return Ok("Password has been reset");
        }

        //        // DELETE: api/Account/5
        //        [ResponseType(typeof (IdentityUser))]
        //        [Authorize(Roles = "Admin")]
        //        public IHttpActionResult DeleteUser(String id)
        //        {
        //            var user = db.Users.Find(id);
        //            if (user == null)
        //            {
        //                return BadRequest("User with id " + id + " does not exist.");
        //            }
        //
        //            db.Users.Remove(user);
        //
        //            db.SaveChanges();
        //
        //            return Ok(user);
        //        }

        //        // GET api/Account/UserInfo
        //        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //        [Route("UserInfo")]
        //        public UserInfoViewModel GetUserInfo()
        //        {
        //            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
        //
        //            return new UserInfoViewModel
        //            {
        //                Email = User.Identity.GetUserName(),
        //                HasRegistered = externalLogin == null,
        //                LoginProvider = externalLogin != null ? externalLogin.LoginProvider : null
        //            };
        //        }

        //        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        //        [Route("ManageInfo")]
        //        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        //        {
        //            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
        //
        //            if (user == null)
        //            {
        //                return null;
        //            }
        //
        //            List<UserLoginInfoViewModel> logins = new List<UserLoginInfoViewModel>();
        //
        //            foreach (IdentityUserLogin linkedAccount in user.Logins)
        //            {
        //                logins.Add(new UserLoginInfoViewModel
        //                {
        //                    LoginProvider = linkedAccount.LoginProvider,
        //                    ProviderKey = linkedAccount.ProviderKey
        //                });
        //            }
        //
        //            if (user.PasswordHash != null)
        //            {
        //                logins.Add(new UserLoginInfoViewModel
        //                {
        //                    LoginProvider = LocalLoginProvider,
        //                    ProviderKey = user.UserName,
        //                });
        //            }
        //
        //            return new ManageInfoViewModel
        //            {
        //                LocalLoginProvider = LocalLoginProvider,
        //                Email = user.UserName,
        //                Logins = logins,
        //                ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
        //            };
        //        }
        //
        //        // POST api/Account/AddExternalLogin
        //        [Route("AddExternalLogin")]
        //        public async Task<IHttpActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return BadRequest(ModelState);
        //            }
        //
        //            Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //
        //            AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);
        //
        //            if (ticket == null || ticket.Identity == null || (ticket.Properties != null
        //                                                              && ticket.Properties.ExpiresUtc.HasValue
        //                                                              &&
        //                                                              ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
        //            {
        //                return BadRequest("External login failure.");
        //            }
        //
        //            ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);
        //
        //            if (externalData == null)
        //            {
        //                return BadRequest("The external login is already associated with an account.");
        //            }
        //
        //            IdentityResult result = await UserManager.AddLoginAsync(User.Identity.GetUserId(),
        //                new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));
        //
        //            if (!result.Succeeded)
        //            {
        //                return GetErrorResult(result);
        //            }
        //
        //            return Ok();
        //        }
        //
        //        // POST api/Account/RemoveLogin
        //        [Route("RemoveLogin")]
        //        public async Task<IHttpActionResult> RemoveLogin(RemoveLoginBindingModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return BadRequest(ModelState);
        //            }
        //
        //            IdentityResult result;
        //
        //            if (model.LoginProvider == LocalLoginProvider)
        //            {
        //                result = await UserManager.RemovePasswordAsync(User.Identity.GetUserId());
        //            }
        //            else
        //            {
        //                result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(),
        //                    new UserLoginInfo(model.LoginProvider, model.ProviderKey));
        //            }
        //
        //            if (!result.Succeeded)
        //            {
        //                return GetErrorResult(result);
        //            }
        //
        //            return Ok();
        //        }
        //
        //        // GET api/Account/ExternalLogin
        //        [OverrideAuthentication]
        //        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        //        [AllowAnonymous]
        //        [Route("ExternalLogin", Name = "ExternalLogin")]
        //        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        //        {
        //            if (error != null)
        //            {
        //                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
        //            }
        //
        //            if (!User.Identity.IsAuthenticated)
        //            {
        //                return new ChallengeResult(provider, this);
        //            }
        //
        //            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
        //
        //            if (externalLogin == null)
        //            {
        //                return InternalServerError();
        //            }
        //
        //            if (externalLogin.LoginProvider != provider)
        //            {
        //                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //                return new ChallengeResult(provider, this);
        //            }
        //
        //            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
        //                externalLogin.ProviderKey));
        //
        //            bool hasRegistered = user != null;
        //
        //            if (hasRegistered)
        //            {
        //                Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
        //
        //                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
        //                    OAuthDefaults.AuthenticationType);
        //                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
        //                    CookieAuthenticationDefaults.AuthenticationType);
        //
        //                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
        //                Authentication.SignIn(properties, oAuthIdentity, cookieIdentity);
        //            }
        //            else
        //            {
        //                IEnumerable<Claim> claims = externalLogin.GetClaims();
        //                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
        //                Authentication.SignIn(identity);
        //            }
        //
        //            return Ok();
        //        }
        //
        //        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        //        [AllowAnonymous]
        //        [Route("ExternalLogins")]
        //        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        //        {
        //            IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
        //            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();
        //
        //            string state;
        //
        //            if (generateState)
        //            {
        //                const int strengthInBits = 256;
        //                state = RandomOAuthStateGenerator.Generate(strengthInBits);
        //            }
        //            else
        //            {
        //                state = null;
        //            }
        //
        //            foreach (AuthenticationDescription description in descriptions)
        //            {
        //                ExternalLoginViewModel login = new ExternalLoginViewModel
        //                {
        //                    Name = description.Caption,
        //                    Url = Url.Route("ExternalLogin", new
        //                    {
        //                        provider = description.AuthenticationType,
        //                        response_type = "token",
        //                        client_id = Startup.PublicClientId,
        //                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
        //                        state = state
        //                    }),
        //                    State = state
        //                };
        //                logins.Add(login);
        //            }
        //
        //            return logins;
        //        }

        //        // POST api/Account/Register
        //        [AllowAnonymous]
        //        [Route("Register")]
        //        public async Task<IHttpActionResult> Register(RegisterAdminBindingModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return BadRequest(ModelState);
        //            }
        //
        //            var user = new ApplicationUser() { UserName = model.Email, Email = model.Email };
        //
        //            IdentityResult result = await UserManager.CreateAsync(user, model.Password);
        //
        //            if (!result.Succeeded)
        //            {
        //                return GetErrorResult(result);
        //            }
        //
        //            return Ok();
        //        }

        //        // POST api/Account/RegisterExternal
        //        [OverrideAuthentication]
        //        [HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //        [Route("RegisterExternal")]
        //        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        //        {
        //            if (!ModelState.IsValid)
        //            {
        //                return BadRequest(ModelState);
        //            }
        //
        //            var info = await Authentication.GetExternalLoginInfoAsync();
        //            if (info == null)
        //            {
        //                return InternalServerError();
        //            }
        //
        //            var user = new ApplicationUser() {UserName = model.Email, Email = model.Email};
        //
        //            IdentityResult result = await UserManager.CreateAsync(user);
        //            if (!result.Succeeded)
        //            {
        //                return GetErrorResult(result);
        //            }
        //
        //            result = await UserManager.AddLoginAsync(user.Id, info.Login);
        //            if (!result.Succeeded)
        //            {
        //                return GetErrorResult(result);
        //            }
        //            return Ok();
        //        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers

        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    UserName = identity.FindFirstValue(ClaimTypes.Name)
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits%bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits/bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }

        #endregion
    }
}