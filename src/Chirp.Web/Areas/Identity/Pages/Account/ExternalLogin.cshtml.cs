// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Chirp.Core;
using Chirp.Core.Domain_Model;
using Chirp.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Chirp.Web.Areas.Identity.Pages.Account {
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel {
        private readonly SignInManager<Author> _signInManager;
        private readonly UserManager<Author> _userManager;
        private readonly IUserStore<Author> _userStore;
        private readonly IUserEmailStore<Author> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly ICheepRepository _cheepRepository;
        private readonly IAuthorRepository _authorRepository;

        public ExternalLoginModel(
            SignInManager<Author> signInManager,
            UserManager<Author> userManager,
            IUserStore<Author> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            ICheepRepository cheepRepository,
            IAuthorRepository authorRepository) {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
            _cheepRepository = cheepRepository;
            _authorRepository = authorRepository;
        }

        /// <summary>
        ///  The model containing data from the user used in the login process
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///   Name off the ExternalLogin Provider e.g. GitHub
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     The URL to return to.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     A message to contain errors.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///    The model containing data from the user used in the login process
        /// </summary>
        public class InputModel {
            /// <summary>
            ///     The users Email
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            /// <summary>
            ///     The users UserName
            /// </summary>
            [Required]
            [DataType(DataType.Text)]
            [StringLength(256,
                          ErrorMessage = "The {0} must be between {2} and {1} characters long.",
                          MinimumLength = 4)]
            [Display(Name = "Username")]
            public string UserName { get; set; }

            /// <summary>
            ///     The name the user want displayed.
            /// </summary>
            [DataType(DataType.Text)]
            [StringLength(256,
                          ErrorMessage = "The {0} must be between {2} and {1} characters long.",
                          MinimumLength = 4)]
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; } = "";
        }
        /**
         * Redirects to the login page
         */
        public IActionResult OnGet() => RedirectToPage("./Login");
        /**
        * Initiates the login process by challenging the externalProvider.
         * This redirects to the external provider, which will afterwards return to the callback handler.
        */
        public IActionResult OnPost(string provider, string returnUrl = null) {
            // Request a redirect to the external login provider.
            string redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback",
                                          values: new { returnUrl });
            AuthenticationProperties properties =
                _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        /**
         * Handles the callback from the externalLogin provider, and signs the user in. if no user exist, one will be created
         */
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null,
                                                            string remoteError = null) {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null) {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // get info from external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            SignInResult result =
                await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                                                              info.ProviderKey,
                                                              isPersistent: false,
                                                              bypassTwoFactor: true);
            if (result.Succeeded) {
                _logger.LogWarning("{Name} logged in with {LoginProvider} provider.",
                                   info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut) {
                return RedirectToPage("./Lockout");
            }

            // If the user does not have an account we need to create a new one
            // ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;
            Input = GetInputFromInfo(info);

            if (ModelState.IsValid) {
                return await CreateAndSignIn(info, returnUrl);
            }

            return Page();
        }
        /**
         * Finds the externalLoginInfo if possible, and continues the signin process, otherwise redirects to Login.
         */
        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null) {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Get the information about the user from the external login provider
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid) {
                return await CreateAndSignIn(info, returnUrl);
            }

            return Page();
        }

        /**
         * Returns the InputModel consisting of Email, UserName and DisplayName from the ExternalLoginInfo.
         */
        private InputModel GetInputFromInfo(ExternalLoginInfo info) {
            Input = new InputModel {
                Email = info.Principal.FindFirstValue(ClaimTypes.Email) ??
                        string.Empty,
                UserName = info.Principal.FindFirstValue(ClaimTypes.Name) ??
                           info.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                           string.Empty,
                DisplayName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ??
                              info.Principal.FindFirstValue(ClaimTypes.Name) ??
                              string.Empty
            };
            return Input;
        }

        /**
         * Creates a new Author and signs them in using the externalLogin.
         */
        private async Task<IActionResult>
            CreateAndSignIn(ExternalLoginInfo info, string returnUrl) {
            // create
            Author user = CreateUser();
            user.DisplayName = string.IsNullOrWhiteSpace(Input.DisplayName)
                ? Input.UserName
                : Input.DisplayName;

            await _userStore.SetUserNameAsync(user, Input.UserName, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            IdentityResult createResult = await _userManager.CreateAsync(user);

            // Login
            if (createResult.Succeeded) {
                createResult = await _userManager.AddLoginAsync(user, info);
                if (createResult.Succeeded) {
                    _logger.LogWarning("User created an account using {Name} provider.",
                                       info.LoginProvider);
                    await _signInManager.SignInAsync(user, isPersistent: false,
                                                     info.LoginProvider);
                    var userDTO = new AuthorDTO(user);
                    await _authorRepository.Follow(userDTO, userDTO);
                    return LocalRedirect(returnUrl);
                }
            }

            // Errors
            foreach (IdentityError error in createResult.Errors) {
                ModelState.AddModelError(string.Empty, error.Description);

                // see if the user has a regular account, and if that is causing the error
                Author existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser == null)
                    return Page(); // if not - go to the register page (externallogin.cshtml)

                // else login with the account that is configured to the email
                await _userManager.AddLoginAsync(existingUser, info);
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                return LocalRedirect(returnUrl);
            }

            // register page (externallogin.cshtml) - looks like register
            return Page();
        }
        /**
         * Creates a new Author.
         */
        private Author CreateUser() {
            try {
                return Activator.CreateInstance<Author>();
            } catch {
                throw new InvalidOperationException(
                    $"Can't create an instance of '{nameof(Author)}'. " +
                    $"Ensure that '{nameof(Author)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }
        /**
         * Retrieves the emailStore, if it is configured in the userManager.
         */
        private IUserEmailStore<Author> GetEmailStore() {
            if (!_userManager.SupportsUserEmail) {
                throw new NotSupportedException(
                    "The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<Author>)_userStore;
        }
    }
}