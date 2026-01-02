// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel.DataAnnotations;
using Chirp.Core.Domain_Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Chirp.Web.Areas.Identity.Pages.Account.Manage {
    public class IndexModel : PageModel {
        private readonly UserManager<Author> _userManager;
        private readonly SignInManager<Author> _signInManager;

        public IndexModel(UserManager<Author> userManager,
                          SignInManager<Author> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///    The userName
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///  a message of the status of the processes
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     Contains the input written by the user
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Contains the input written by the user
        /// </summary>
        public class InputModel {

            /**
             * The DisplayName that was written
             */
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Display Name")]
            [StringLength(
                256, ErrorMessage = "The {0} must be between {2} and {1} characters long.",
                MinimumLength = 4)]
            public string DisplayName { get; set; }

            /// <summary>
            ///   The phonenumber written
            /// </summary>
            [Phone]
            [DataType(DataType.PhoneNumber)]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }
        /**
         * loads the username and phone number to be displayed
         */

        private async Task LoadAsync(Author user) {
            Username = await _userManager.GetUserNameAsync(user);
            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Input = new InputModel {
                DisplayName = user.DisplayName,
                PhoneNumber = phoneNumber
            };
        }
        /**
         * Finds the user and loads info to be displayed.
         */
        public async Task<IActionResult> OnGetAsync() {
            Author user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }
        /**
         * Updates information about the user, if the input has changed.
         */
        public async Task<IActionResult> OnPostAsync() {
            Author user = await _userManager.GetUserAsync(User);
            if (user == null) {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid) {
                await LoadAsync(user);
                return Page();
            }

            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber) {
                IdentityResult setPhoneResult =
                    await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded) {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (Input.DisplayName != user.DisplayName) {
                user.DisplayName = Input.DisplayName;
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}