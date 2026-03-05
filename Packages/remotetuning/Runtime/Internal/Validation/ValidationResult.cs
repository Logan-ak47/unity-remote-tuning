using System.Collections.Generic;

namespace Ashutosh.RemoteTuning
{
    internal readonly struct ValidationResult
    {
        public readonly bool IsValid;
        public readonly IReadOnlyList<ValidationError> Errors;

        public ValidationResult(bool isValid, IReadOnlyList<ValidationError> errors)
        {
            IsValid = isValid;
            Errors = errors ?? new List<ValidationError>(0);
        }

        public static ValidationResult Ok() => new ValidationResult(true, new List<ValidationError>(0));

        public static ValidationResult Fail(List<ValidationError> errors)
            => new ValidationResult(false, errors ?? new List<ValidationError>(0));
    }
}