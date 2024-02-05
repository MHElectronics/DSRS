using Microsoft.AspNetCore.Components.Forms;
namespace AxleLoadSystem.Helpers;

public class CustomFieldClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext,
        in FieldIdentifier fieldIdentifier)
    {
        var isValid = editContext.IsValid(fieldIdentifier);

        return isValid ? "valid" : "is-invalid";

        //if (fieldIdentifier.FieldName == "Name")
        //{
        //    return isValid ? "validField" : "invalidField";
        //}
        //else
        //{
        //    if (editContext.IsModified(fieldIdentifier))
        //    {
        //        return isValid ? "modified valid" : "modified invalid";
        //    }
        //    else
        //    {
        //        return isValid ? "valid" : "invalid";
        //    }
        //}
    }
}
