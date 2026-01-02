// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Chirp.Web.Areas.Identity.Pages.Account {
    public class LoginModel : PageModel {
        private readonly SignInManager<Author> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<Author> signInManager, ILogger<LoginModel> logger) {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///    The model containing data from the user used in the login process
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     List of the different externalLogins
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     The URL to return to
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     A message for errors.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///    The model containing data from the user used in the login process
        /// </summary>
        public class InputModel {
            /// <summary>
            ///   The email that was written
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///      The Password that was written
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     The status of the remember me box
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }
        /**
         * Returns the login page and clears existing cookies
         */
        public async Task OnGetAsync(string returnUrl = null) {
            if (!string.IsNullOrEmpty(ErrorMessage)) {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }
        /**
         * Logs in the user if an account exists.
         */

        public async Task<IActionResult> OnPostAsync(string returnUrl = null) {
            returnUrl ??= Url.Content("~/");

            ExternalLogins =
                (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid) return Page();

            Author user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
            if (user == null || user.UserName == null) {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            SignInResult result = await _signInManager.PasswordSignInAsync(user.UserName,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);
            if (result.Succeeded) {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor) {
                return RedirectToPage("./LoginWith2fa",
                                      new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
            }

            if (result.IsLockedOut) {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}