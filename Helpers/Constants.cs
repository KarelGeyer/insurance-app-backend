using Microsoft.AspNetCore.Mvc;

namespace insurance_backend.Helpers
{
    public static class Constants
    {
        public const int YEAR = 12;

        public const string DYNAMIC = "Dynamická";
        public const string CONSERVATIVE = "Konzervativní";
        public const string BALANCED = "Vyvážená";
    }

    public static class Messages
    {
        public const string Args_Exception = "Wrong data passed to the request";
        public const string Too_Young = "Must be 18+ years old";

        public static string CannotBeValueOf_Error<T>(string ControllerName, T argValue)
        {
            return $"{ControllerName} - {nameof(argValue)} cannot be a value of {argValue}";
        }

        public static string MissingProperty_Error<T>(T argumentName)
        {
            return $"{nameof(argumentName)} cannot be null or empty";
        }
    }
}
