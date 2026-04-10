using FluentValidation;
using PhoneNumbers;

namespace Demo.Application.Security.Commands
{
    public class UserProfileUpdateValidator : AbstractValidator<UserProfileUpdateCommand>
    {
        public UserProfileUpdateValidator()
        {
            //Business logic

            RuleFor(x => x.Model.PhoneNumber)
            .Must(BeAValidAuNumber).WithMessage("Invalid AU Phone Number");
        }

        private bool BeAValidAuNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;

            var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            try
            {
                // Parse for AU region
                var numberProto = phoneUtil.Parse(phone, "AU");
                return phoneUtil.IsValidNumberForRegion(numberProto, "AU");
            }
            catch (NumberParseException)
            {
                return false;
            }
        }
    }
}
