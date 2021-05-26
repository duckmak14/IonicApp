using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Serialization;

namespace WEB.Models
{/// <summary>
    /// Represents a shipping option
    /// </summary>
    public partial class ShippingOption
    {
        /// <summary>
        /// Gets or sets the system name of shipping rate computation method
        /// </summary>
        public string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets a shipping rate (without discounts, additional shipping charges, etc)
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets a shipping option name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a shipping option description
        /// </summary>
        public string Description { get; set; }
    }
    public partial class ObjectTemp
    {      
        public int ID { get; set; }
        public string Title { get; set; }
    }


    public class ShippingOptionTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                ShippingOption shippingOption = null;
                string valueStr = value as string;
                if (!String.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        using (var tr = new StringReader(valueStr))
                        {
                            var xmlS = new XmlSerializer(typeof(ShippingOption));
                            shippingOption = (ShippingOption)xmlS.Deserialize(tr);
                        }
                    }
                    catch
                    {
                        //xml error
                    }
                }
                return shippingOption;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var shippingOption = value as ShippingOption;
                if (shippingOption != null)
                {
                    var sb = new StringBuilder();
                    using (var tw = new StringWriter(sb))
                    {
                        var xmlS = new XmlSerializer(typeof(ShippingOption));
                        xmlS.Serialize(tw, value);
                        string serialized = sb.ToString();
                        return serialized;
                    }
                }
                else
                {
                    return "";
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


    public class ShippingOptionListTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                List<ShippingOption> shippingOptions = null;
                string valueStr = value as string;
                if (!String.IsNullOrEmpty(valueStr))
                {
                    try
                    {
                        using (var tr = new StringReader(valueStr))
                        {
                            var xmlS = new XmlSerializer(typeof(List<ShippingOption>));
                            shippingOptions = (List<ShippingOption>)xmlS.Deserialize(tr);
                        }
                    }
                    catch
                    {
                        //xml error
                    }
                }
                return shippingOptions;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var shippingOptions = value as List<ShippingOption>;
                if (shippingOptions != null)
                {
                    var sb = new StringBuilder();
                    using (var tw = new StringWriter(sb))
                    {
                        var xmlS = new XmlSerializer(typeof(List<ShippingOption>));
                        xmlS.Serialize(tw, value);
                        string serialized = sb.ToString();
                        return serialized;
                    }
                }
                else
                {
                    return "";
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }


    public class GenericListTypeConverter<T> : TypeConverter
    {
        protected readonly TypeConverter _typeConverter;

        public GenericListTypeConverter()
        {
            _typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (_typeConverter == null)
                throw new InvalidOperationException("No type converter exists for type " + typeof(T).FullName);
        }

        protected virtual string[] GetStringArray(string input)
        {
            if (!String.IsNullOrEmpty(input))
            {
                string[] result = input.Split(',');
                Array.ForEach(result, s => s.Trim());
                return result;
            }
            else
                return new string[0];
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {

            if (sourceType == typeof(string))
            {
                string[] items = GetStringArray(sourceType.ToString());
                return (items.Count() > 0);
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string[] items = GetStringArray((string)value);
                var result = new List<T>();
                Array.ForEach(items, s =>
                {
                    object item = _typeConverter.ConvertFromInvariantString(s);
                    if (item != null)
                    {
                        result.Add((T)item);
                    }
                });

                return result;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string result = string.Empty;
                if (((IList<T>)value) != null)
                {
                    //we don't use string.Join() because it doesn't support invariant culture
                    for (int i = 0; i < ((IList<T>)value).Count; i++)
                    {
                        var str1 = Convert.ToString(((IList<T>)value)[i], CultureInfo.InvariantCulture);
                        result += str1;
                        //don't add comma after the last element
                        if (i != ((IList<T>)value).Count - 1)
                            result += ",";
                    }
                }
                return result;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    public class CommonHelper
    {

        private static readonly ShippingOptionListTypeConverter s_soListTypeConverter = new ShippingOptionListTypeConverter();
        private static readonly Dictionary<Type, TypeConverter> s_customTypeConverters = new Dictionary<Type, TypeConverter>
        {
            { typeof(List<int>), new GenericListTypeConverter<int>() },
            { typeof(List<decimal>), new GenericListTypeConverter<decimal>() },
            { typeof(List<string>), new GenericListTypeConverter<string>() },
            { typeof(ShippingOption), new ShippingOptionTypeConverter() },
            { typeof(List<ShippingOption>), s_soListTypeConverter },
            { typeof(IList<ShippingOption>), s_soListTypeConverter }
        };
        public static string EnsureSubscriberEmailOrThrow(string email)
        {
            string output = EnsureNotNull(email);
            output = output.Trim();
            output = EnsureMaximumLength(output, 255);

            if (!IsValidEmail(output))
            {
                throw new Exception("Email is not valid.");
            }

            return output;
        }

 
        /// <returns>true if the string is a valid e-mail address and false if it's not</returns>
        public static bool IsValidEmail(string email)
        {
            bool result = false;
            if (String.IsNullOrEmpty(email))
                return result;
            email = email.Trim();
            result = Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
            return result;
        }

      
        public static string GenerateRandomDigitCode(int length)
        {
            var random = new Random();
            string str = string.Empty;
            for (int i = 0; i < length; i++)
                str = String.Concat(str, random.Next(10).ToString());
            return str;
        }

  
        public static int GenerateRandomInteger(int min = 0, int max = 2147483647)
        {
            var randomNumberBuffer = new byte[10];
            new RNGCryptoServiceProvider().GetBytes(randomNumberBuffer);
            return new Random(BitConverter.ToInt32(randomNumberBuffer, 0)).Next(min, max);
        }

         
        public static string EnsureMaximumLength(string str, int maxLength)
        {
            if (String.IsNullOrEmpty(str))
                return str;

            if (str.Length > maxLength)
                return str.Substring(0, maxLength);
            else
                return str;
        }

     
        public static string EnsureNumericOnly(string str)
        {
            if (String.IsNullOrEmpty(str))
                return string.Empty;

            var result = new StringBuilder();
            foreach (char c in str)
            {
                if (Char.IsDigit(c))
                    result.Append(c);
            }
            return result.ToString();
        }

      
        public static string EnsureNotNull(string str)
        {
            if (str == null)
                return string.Empty;

            return str;
        }

        
        public static bool AreNullOrEmpty(params string[] stringsToValidate)
        {
            bool result = false;
            Array.ForEach(stringsToValidate, str =>
            {
                if (string.IsNullOrEmpty(str)) result = true;
            });
            return result;
        }


        private static AspNetHostingPermissionLevel? _trustLevel = null; 
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!_trustLevel.HasValue)
            { 
                _trustLevel = AspNetHostingPermissionLevel.None; 
                foreach (AspNetHostingPermissionLevel trustLevel in
                        new AspNetHostingPermissionLevel[] {
                                AspNetHostingPermissionLevel.Unrestricted,
                                AspNetHostingPermissionLevel.High,
                                AspNetHostingPermissionLevel.Medium,
                                AspNetHostingPermissionLevel.Low,
                                AspNetHostingPermissionLevel.Minimal 
                            })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        _trustLevel = trustLevel;
                        break;  
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return _trustLevel.Value;
        }

        
        public static void SetProperty(object instance, string propertyName, object value)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            Type instanceType = instance.GetType();
            PropertyInfo pi = instanceType.GetProperty(propertyName);
            if (pi == null)
                throw new Exception(string.Format("No property '{0}' found on the instance of type '{1}'.", propertyName, instanceType));
            if (!pi.CanWrite)
                throw new Exception(string.Format("The property '{0}' on the instance of type '{1}' does not have a setter.", propertyName, instanceType));
            if (value != null && !value.GetType().IsAssignableFrom(pi.PropertyType))
                value = To(value, pi.PropertyType);
            pi.SetValue(instance, value, new object[0]);
        }

        public static TypeConverter GetCustomTypeConverter(Type type)
        {
           
           

            TypeConverter result;
            if (s_customTypeConverters.TryGetValue(type, out result))
            {
                return result;
            }

            return TypeDescriptor.GetConverter(type);
        }

       
        public static object To(object value, Type destinationType)
        {
            return To(value, destinationType, CultureInfo.InvariantCulture);
        }
 
        public static object To(object value, Type destinationType, CultureInfo culture)
        {
            if (value != null)
            {
                var sourceType = value.GetType();

                TypeConverter destinationConverter = GetCustomTypeConverter(destinationType);
                TypeConverter sourceConverter = GetCustomTypeConverter(sourceType);
                if (destinationConverter != null && destinationConverter.CanConvertFrom(value.GetType()))
                    return destinationConverter.ConvertFrom(null, culture, value);
                if (sourceConverter != null && sourceConverter.CanConvertTo(destinationType))
                    return sourceConverter.ConvertTo(null, culture, value, destinationType);
                if (destinationType.IsEnum && value is int)
                    return Enum.ToObject(destinationType, (int)value);
                if (!destinationType.IsAssignableFrom(value.GetType()))
                    return Convert.ChangeType(value, destinationType, culture);
            }
            return value;
        }
         
        public static T To<T>(object value)
        { 
            return (T)To(value, typeof(T));
        }

       
        public static string ConvertEnum(string str)
        {
            string result = string.Empty;
            char[] letters = str.ToCharArray();
            foreach (char c in letters)
                if (c.ToString() != c.ToString().ToLower())
                    result += " " + c.ToString();
                else
                    result += c.ToString();
            return result;
        }

        public static bool OneToManyCollectionWrapperEnabled
        {
            get
            {
                bool enabled = !String.IsNullOrEmpty(ConfigurationManager.AppSettings["OneToManyCollectionWrapperEnabled"]) &&
                   Convert.ToBoolean(ConfigurationManager.AppSettings["OneToManyCollectionWrapperEnabled"]);
                return enabled;
            }
        }


    }
}