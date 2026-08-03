using System.Reflection;

namespace Epros.ERP.DfeCalculos.Objects
{
    public class ZeusFiscalExtensions
    {
        public static decimal GetPropDecimalValue(object instance, string propName)
        {
            try
            {
                var property = instance.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);

                if (property != null)
                {
                    return (decimal?)property.GetValue(instance, null) ?? 0M;
                }

                return 0M;
            }
            catch (Exception)
            {
                return 0M;
            }
        }
    }
}
