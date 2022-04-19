using Microsoft.AspNetCore.Components.Forms;
using System;

namespace BlazorValidation
{
    public static class FieldIdentifierHelper
    {
        private static readonly char[] _separators = new[] { '.', '[' };

        public static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
        {
            // This method parses property paths like 'SomeProp.MyCollection[123].ChildProp'
            // and returns a FieldIdentifier which is an (instance, propName) pair. For example,
            // it would return the pair (SomeProp.MyCollection[123], "ChildProp"). It traverses
            // as far into the propertyPath as it can go until it finds any null instance.

            var obj = editContext.Model;
            var property = string.Empty;
            foreach (var propertyStr in propertyPath.Split(_separators))
            {
                object newObj;
                property = propertyStr;
                if (property.EndsWith("]"))
                {
                    // It's an indexer
                    // This code assumes C# conventions (one indexer named Item with one param)
                    property = property.Substring(0, property.Length - 1);
                    var prop = obj.GetType().GetProperty("Item");
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Could not find property named {property} on object of type {obj.GetType().FullName}.");
                    }
                    var indexerType = prop.GetIndexParameters()[0].ParameterType;
                    var indexerValue = Convert.ChangeType(property, indexerType);
                    newObj = prop.GetValue(obj, new object[] { indexerValue });
                }
                else
                {
                    // It's a regular property
                    var prop = obj.GetType().GetProperty(property);
                    if (prop == null)
                    {
                        throw new InvalidOperationException($"Could not find property named {property} on object of type {obj.GetType().FullName}.");
                    }
                    newObj = prop.GetValue(obj);
                }

                if (newObj == null)
                {
                    // This is as far as we can go
                    return new FieldIdentifier(obj, property);
                }

                obj = newObj;
            }

            return new FieldIdentifier(obj, property);
        }

        //public static FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
        //{
        //    // This method parses property paths like 'SomeProp.MyCollection[123].ChildProp'
        //    // and returns a FieldIdentifier which is an (instance, propName) pair. For example,
        //    // it would return the pair (SomeProp.MyCollection[123], "ChildProp"). It traverses
        //    // as far into the propertyPath as it can go until it finds any null instance.

        //    var obj = editContext.Model;

        //    while (true)
        //    {
        //        var nextTokenEnd = propertyPath.IndexOfAny(_separators);
        //        if (nextTokenEnd < 0)
        //        {
        //            return new FieldIdentifier(obj, propertyPath);
        //        }

        //        var nextToken = propertyPath.Substring(0, nextTokenEnd);
        //        propertyPath = propertyPath.Substring(nextTokenEnd + 1);

        //        object newObj;
        //        if (nextToken.EndsWith("]"))
        //        {
        //            // It's an indexer
        //            // This code assumes C# conventions (one indexer named Item with one param)
        //            nextToken = nextToken.Substring(0, nextToken.Length - 1);
        //            var prop = obj.GetType().GetProperty("Item");
        //            var indexerType = prop.GetIndexParameters()[0].ParameterType;
        //            var indexerValue = Convert.ChangeType(nextToken, indexerType);
        //            newObj = prop.GetValue(obj, new object[] { indexerValue });
        //        }
        //        else
        //        {
        //            // It's a regular property
        //            var prop = obj.GetType().GetProperty(nextToken);
        //            if (prop == null)
        //            {
        //                throw new InvalidOperationException($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.");
        //            }
        //            newObj = prop.GetValue(obj);
        //        }

        //        if (newObj == null)
        //        {
        //            // This is as far as we can go
        //            return new FieldIdentifier(obj, nextToken);
        //        }

        //        obj = newObj;
        //    }
        //}
    }
}
