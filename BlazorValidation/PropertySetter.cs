using System;
using System.Reflection;

namespace BlazorValidation
{
    public static class PropertySetter
    {
        public static void SetProperty(this object model, string propertyPath, object value)
        {
            string[] propertyNameParts = propertyPath.Split('.');
            PropertyInfo pi;
            var obj = model;
            var propertyIndex = propertyNameParts.Length - 1;
            var index = 0;
            if (propertyNameParts.Length != 1)
            {
                while (index < propertyIndex)
                {
                    pi = obj.GetType().GetProperty(propertyNameParts[index]);
                    obj = pi.GetValue(obj);
                    index += 1;
                }
            }
            obj.GetType().InvokeMember(propertyNameParts[index], BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, obj, new object[] { value });
        }
    }
}
