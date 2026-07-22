using System;

namespace Flunt.Validations
{
    public static class FluntExtensions
    {
        public static Contract<T> IsMaxLength<T>(this Contract<T> contract, string? val, int max, string property, string message)
        {
            if (val != null && val.Length > max)
            {
                contract.AddNotification(property, message);
            }
            return contract;
        }

        public static Contract<T> HasMaxLen<T>(this Contract<T> contract, string? val, int max, string property, string message)
        {
            if (val != null && val.Length > max)
            {
                contract.AddNotification(property, message);
            }
            return contract;
        }
    }
}
