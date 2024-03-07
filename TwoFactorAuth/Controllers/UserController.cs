using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtpNet;
using System.Security.Claims;
using System.Text;
using TwoFactorAuth.Models;
using TwoFactorAuth.Services;

namespace TwoFactorAuth.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            string username = User.Identity.Name;
            var user = await _userService.GetUserByUsername(username);
            return View(user);
        }
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(User user)
        {

            if (ModelState.IsValid)
            {
                bool LoginStatus = await _userService.Login(user);

                if (LoginStatus)
                {
                    bool isTwoFactorEnabled = await _userService.IsTwoFactorEnabled(user.Username);
                    if (isTwoFactorEnabled)
                    {
                        return RedirectToAction("LoginTwoStep", new { username = user.Username });
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username)
                    };
                    ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["UserLoginFailed"] = "Login Failed.Please enter correct credentials";
                    return View();
                }
            }
            else
                return View();

        }
        public async Task<IActionResult> LoginTwoStep(string username)
        {
            return View(new EnableAuthenticatorViewModel { UserName = username });
        }
        [HttpPost]
        public async Task<IActionResult> LoginTwoStep(EnableAuthenticatorViewModel model)
        {
            string secrectKey = await _userService.GetSecretKeyFor2FA(model.UserName);
            if (!string.IsNullOrEmpty(secrectKey) && !string.IsNullOrEmpty(model.Code))
            {
                var totp = new Totp(Base32Encoding.ToBytes(secrectKey));
                var verifed = totp.VerifyTotp(model.Code, out _, new VerificationWindow(2, 2)); // Adjust window size as needed
                if (verifed)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.UserName)
                    };
                    ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                    await HttpContext.SignOutAsync();
                    await HttpContext.SignInAsync(principal);
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                bool RegistrationStatus = await _userService.RegisterUser(user);
                if (RegistrationStatus)
                {
                    ModelState.Clear();
                    TempData["Success"] = "Registration Successful!";
                    return View();
                }
                else
                {
                    TempData["Fail"] = "Registration Failed.";
                    return View();
                }
            }
            return View();
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactorAuth()
        {
            EnableAuthenticatorViewModel model = new EnableAuthenticatorViewModel();
            string username = User.Identity.Name;

            // Generate a random secret key
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var random = new Random();
            var secretKey = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                secretKey.Append(validChars[random.Next(validChars.Length)]);
            }

            model.SharedKey = secretKey.ToString();

            var encodedUsername = Uri.EscapeDataString(username);
            var qrCodeUrl = $"otpauth://totp/{encodedUsername}?secret={secretKey}&issuer=TwoFactorAuthApp";

            model.AuthenticatorUri = qrCodeUrl;
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EnableTwoFactorAuth(EnableAuthenticatorViewModel model)
        {
            string username = User.Identity.Name;

            var totp = new Totp(Base32Encoding.ToBytes(model.SharedKey));
            var verifed = totp.VerifyTotp(model.Code, out _, new VerificationWindow(2, 2)); // Adjust window size as needed
            if (verifed)
            {
                var res = await _userService.SaveUserScrectKeyFor2FA(username, model.SharedKey);
                if (res)
                {
                    return RedirectToAction("Profile");
                }

            }

            ModelState.AddModelError("", "Invalid verification code");
            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DisableTwoFactorAuth()
        {
            string username = User.Identity.Name;
            var res = await _userService.DisableTwoFactorAuth(username);
            return RedirectToAction("Profile");
        }
    }
}
