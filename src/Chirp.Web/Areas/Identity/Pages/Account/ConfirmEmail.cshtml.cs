// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.Text;
using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Chirp.Web.Areas.Identity.Pages.Account {
    public class ConfirmEmailModel : PageModel {
        private readonly UserManager<Author> _userManager;

        public ConfirmEmailModel(UserManager<Author> userManager) {
            _userManager = userManager;
        }

        /// <summary>
        ///     string for the status of the process
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        /**
        * Figures out which status message to display according to whether the email could be confirmed.
        */
        public async Task<IActionResult> OnGetAsync(string userId, string code) {
            if (userId == null || code == null) {
                return RedirectToPage("/Index");
            }

            Author user = await _userManager.FindByIdAsync(userId);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            IdentityResult result = await _userManager.ConfirmEmailAsync(user, code);
            StatusMessage = result.Succeeded
                ? "Thank you for confirming your email."
                : "Error confirming your email.";
            return Page();
        }
    }
}