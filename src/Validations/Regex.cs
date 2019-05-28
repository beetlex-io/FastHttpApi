using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Validations
{
    public class Regex : ValidationBase
    {

        public Regex(string pattern)
        {
            Pattern = pattern;
        }

        public string Pattern { get; set; }

        public override bool Execute(object data)
        {
            string value = (string)data;
            if (!Non && string.IsNullOrEmpty(value))
                return true;
            if (string.IsNullOrEmpty(value))
                return false;
            return System.Text.RegularExpressions.Regex.IsMatch(value, Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not match'{Pattern}'";
        }
    }

    public class HourFormater : Regex
    {
        public HourFormater() : base(@"^([--:\w?@%&+~#=]*\.[a-z]{2,4}\/{0,2})((?:[?&](?:\w+)=(?:\w+))+|[--:\w?@%&+~#=]+)?$")
        {

        }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a hour data format";
        }
    }


    public class UrlFormater : Regex
    {
        public UrlFormater() : base(@"^([--:\w?@%&+~#=]*\.[a-z]{2,4}\/{0,2})((?:[?&](?:\w+)=(?:\w+))+|[--:\w?@%&+~#=]+)?$") { }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a url data format";
        }
    }


    public class PasswordFormater : Regex
    {
        public PasswordFormater() : base(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[a-zA-Z]).{8,}$") { }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a password data format";
        }
    }


    public class PhoneFormater : Regex
    {
        public PhoneFormater() : base(@"^\s*(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)\s*$")
        {


        }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a phone data format";
        }
    }


    public class MPhoneFormater : Regex
    {
        public MPhoneFormater() : base(@"^(13\d|14[579]|15[^4\D]|17[^49\D]|18\d)\d{8}$") { }

        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a mobile phone data format";
        }
    }


    public class IDCardFormater : Regex
    {

        public IDCardFormater() : base(@"^\d{17}[0-9Xx]|\d{15}$") { }

        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a identity card data format";
        }
    }


    public class DateFormater : Regex
    {
        public DateFormater() : base(@"^(([0-9]{3}[1-9]|[0-9]{2}[1-9][0-9]{1}|[0-9]{1}[1-9][0-9]{2}|[1-9][0-9]{3})-(((0[13578]|1[02])-(0[1-9]|[12][0-9]|3[01]))|((0[469]|11)-(0[1-9]|[12][0-9]|30))|(02-(0[1-9]|[1][0-9]|2[0-8]))))|((([0-9]{2})(0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))-02-29)$")
        {

        }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a date data format";
        }
    }


    public class EmailFormater : Regex
    {
        public EmailFormater() : base(@"^([A-Z|a-z|0-9](\.|_){0,1})+[A-Z|a-z|0-9]\@([A-Z|a-z|0-9])+((\.){0,1}[A-Z|a-z|0-9]){2}\.[a-z]{2,3}$") { }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a email data format";
        }
    }

    public class IPFormater : Regex
    {
        public IPFormater() : base(@"^(?:(?:2(?:[0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9])\.){3}(?:(?:2([0-4][0-9]|5[0-5])|[0-1]?[0-9]?[0-9]))$") { }
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value not a ip address data format";
        }
    }

}
