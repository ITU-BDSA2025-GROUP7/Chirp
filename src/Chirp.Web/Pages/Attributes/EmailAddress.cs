using System.ComponentModel.DataAnnotations;
using Chirp.Infrastructure;

namespace Chirp.Web.Pages.Attributes;

public class EmailAddress : DataTypeAttribute
{
    public EmailAddress(DataType dataType) : base(dataType) {
    }

    public EmailAddress(string customDataType) : base(customDataType) {
    }

    public override bool IsValid(object? value) {
        if (value == null)
        {
            return true;
        }

        if (!(value is string valueAsString))
        {
            return false;
        }

        if (valueAsString.AsSpan().ContainsAny('\r', '\n'))
        {
            return false;
        }

        //Checks whether the toString of the value can be
        //parsed to a System.Net.Mail.MailAddress object.
        return CheepRepository.IsValidEmail(valueAsString);

    }
}
