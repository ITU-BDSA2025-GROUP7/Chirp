using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure;
using Chirp.Infrastructure.Repositories;

namespace Chirp.Web;

public class EmailAuthenticationAttribute : ValidationAttribute {
    /**
     * validates if the email is a real and valid email.
     */
    protected override ValidationResult IsValid(
        object? value, ValidationContext validationContext) {
        string? email = value as string;

        if (email != null && AuthorRepository.IsValidEmail(email)) {
            return ValidationResult.Success!;
        } else {
            return new ValidationResult(ErrorMessage);
        }

    }
}