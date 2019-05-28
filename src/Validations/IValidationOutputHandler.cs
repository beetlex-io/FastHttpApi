using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
namespace BeetleX.FastHttpApi.Validations
{
    public interface IValidationOutputHandler
    {
        void Execute(IHttpContext context, IActionResultHandler handler, ValidationBase validation, ParameterInfo parameterInfo);
    }

    public class ValidationOutputHandler : IValidationOutputHandler
    {
        public void Execute(IHttpContext context, IActionResultHandler handler, ValidationBase validation, ParameterInfo parameterInfo)
        {
            ActionResult actionResult = new ActionResult(validation.Code, validation.GetResultMessage(parameterInfo.Name));
            handler.Success(actionResult);
        }
    }

}
