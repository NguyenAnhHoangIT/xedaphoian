﻿using System.ComponentModel.DataAnnotations;

namespace ThueXeDapHoiAn.Areas.Repository.Validation
{

    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] extensions = { "jpg", "png", "jpeg" };

                bool result = extensions.Any(x => extension.EndsWith(x));
                if (!result)
                {
                    return new ValidationResult("Chỉ cho phép file có đuôi jpg, png, jpeg");
                }
            }
            return ValidationResult.Success;
        }
    }
}
