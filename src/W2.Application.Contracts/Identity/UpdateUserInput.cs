using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace W2.Identity
{
    public class UpdateUserInput
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Name { get; set; }

        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        [PasswordValidationAttribute]
        public string Password { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool LockoutEnabled { get; set; }

        [Required]
        public List<string> RoleNames { get; set; }

        [Required]
        public List<Guid> CustomPermissionIds { get; set; }
    }
}

public class PasswordValidationAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value != null)
        {
            string password = value.ToString();

            // Check if the password has at least 6 characters
            if (password.Length < 6)
            {
                return new ValidationResult("Password must be at least 6 characters long.");
            }

            // Check if the password contains at least one lowercase letter
            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return new ValidationResult("Password must contain at least one lowercase letter.");
            }

            // Check if the password contains at least one uppercase letter
            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return new ValidationResult("Password must contain at least one uppercase letter.");
            }

            // Check if the password contains at least one special character
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?':{}|<>]"))
            {
                return new ValidationResult("Password must contain at least one special character.");
            }
        }

        return ValidationResult.Success;
    }
}
