using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Validations
{
    public class StringRegion : ValidationBase
    {

        public StringRegion()
        {

        }

        public int Min { get; set; } = int.MinValue;

        public int Max { get; set; } = int.MinValue;

        public override string GetResultMessage(string parameter)
        {
            if (Min != int.MinValue && Max != int.MinValue)
                return $"The '{parameter}' value length must between ({Min},{Max})";
            else if (Min != int.MinValue)
                return $"The '{parameter}' value length must >= {Min}";
            else
                return $"The '{parameter}' value length must <= {Max}";
        }

        public override bool Execute(object data)
        {
            if (!Non && string.IsNullOrEmpty((string)data))
                return true;
            if (data == null)
                return false;
            string value = (string)data;
            bool result = true;
            if (Min != int.MinValue)
            {
                result = value.Length >= Min;
                if (!result)
                    return false;
            }
            if (Max != int.MinValue)
                result = value.Length <= Max;
            return result;
        }
    }

    public class NumberRegion : ValidationBase
    {

        public NumberRegion()
        {
        }

        public long Min { get; set; } = long.MinValue;

        public long Max { get; set; } = long.MinValue;

        public override string GetResultMessage(string parameter)
        {
            if (Min != long.MinValue && Max != long.MinValue)
                return $"The '{parameter}' value must between in ({Min},{Max})";
            else if (Min != int.MinValue)
                return $"The '{parameter}' value must >= {Min}";
            else
                return $"The '{parameter}' value must <= {Max}";
        }

        public override bool Execute(object data)
        {
            if (!Non && data == null)
                return true;
            if (data == null)
                return false;
            long value = (long)System.Convert.ChangeType(data, typeof(long));
            bool result = true;
            if (Min != long.MinValue)
            {
                result = value >= Min;
                if (!result)
                    return false;
            }
            if (Max != long.MinValue)
                result = value <= Max;
            return result;
        }
    }

    public class DoubleRegion : ValidationBase
    {

        public DoubleRegion()
        {

        }

        public double Min { get; set; } = double.MinValue;

        public double Max { get; set; } = double.MinValue;

        public override string GetResultMessage(string parameter)
        {
            if (Min != double.MinValue && Max != double.MinValue)
                return $"The '{parameter}' value must between in ({Min},{Max})";
            else if (Min != int.MinValue)
                return $"The '{parameter}' value must >= {Min}";
            else
                return $"The '{parameter}' value must <= {Max}";
        }

        public override bool Execute(object data)
        {
            if (!Non && data == null)
                return true;
            if (data == null)
                return false;
            double value = (double)System.Convert.ChangeType(data, typeof(double));
            bool result = true;
            if (Min != double.MinValue)
            {
                result = value >= Min;
                if (!result)
                    return false;
            }
            if (Max != double.MinValue)
                result = value <= Max;
            return result;
        }
    }

    public class DateRegion : ValidationBase
    {

        public DateRegion()
        {

        }

        public string Min { get; set; }

        public string Max { get; set; }

        public override string GetResultMessage(string parameter)
        {
            if (Min != null && Max != null)
                return $"The '{parameter}' value must between in ({Min},{Max})";
            else if (Min != null)
                return $"The '{parameter}' value must >= {Min}";
            else
                return $"The '{parameter}' value must <= {Max}";
        }

        public override bool Execute(object data)
        {
            if (!Non && data == null)
                return true;
            if (data == null)
                return false;
            DateTime value = (DateTime)data;
            bool result = true;
            if (Min != null)
            {
                result = value >= DateTime.Parse(Min);
                if (!result)
                    return false;
            }
            if (Max != null)
                result = value <= DateTime.Parse(Max);
            return result;
        }
    }

}
