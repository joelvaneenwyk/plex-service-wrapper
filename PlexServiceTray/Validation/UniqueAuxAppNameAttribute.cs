﻿using PlexServiceTray.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace PlexServiceTray.Validation
{
    internal class UniqueAuxAppNameAttribute : ValidationAttribute
    {
        private new const string ErrorMessage = "There's already an Auxilliary Application called `{0}`.";

        private SettingsViewModel? _context;

        public override string FormatErrorMessage(string? name)
        {
            return string.Format(ErrorMessage, name ?? string.Empty);
        }

        public override bool IsValid(object? value)
        {
            return IsValid(value, null) == ValidationResult.Success;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext? validationContext)
        {
            if (validationContext != null)
                _context = validationContext.ObjectInstance as SettingsViewModel;
            string? name = value as string;
            base.ErrorMessage = FormatErrorMessage(name);
            if (string.IsNullOrEmpty(name) || _context?.AuxiliaryApplications == null)
            {
                return ValidationResult.Success;
            }

            return (_context?.AuxiliaryApplications.Count(a => a.Name == name) ?? 0) > 1
                ? new ValidationResult(base.ErrorMessage)
                : ValidationResult.Success;
        }
    }
}
