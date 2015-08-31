using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net.Designers
{
    /// <summary>
    /// Converts a <see cref="X509Certificate"/> object from one data type to another. Access this class through the <see cref="TypeDescriptor"/> object.
    /// </summary>
    public class X509CertificateConverter : ExpandableObjectConverter
    {
        /// <summary>
        /// Determines whether this <see cref="X509CertificateConverter"/> can convert an instance of a specified type to a <see cref="X509Certificate"/>, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="sourceType">A <see cref="Type"/> that specifies the type you want to convert from.</param>
        /// <returns>This method returns true if this <see cref="X509CertificateConverter"/> can perform the conversion; otherwise false.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(byte[]))
                return true;
            if (sourceType == typeof(InstanceDescriptor))
                return false;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Determines whether this <see cref="X509CertificateConverter"/> can convert a <see cref="X509Certificate"/> to an instance of a specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="destinationType">A <see cref="Type"/> that specifies the type you want to convert to.</param>
        /// <returns>This method returns tru if this <see cref="X509CertificateConverter"/> can perform the conversion; otherwise false.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return (destinationType == typeof(X509Certificate) || destinationType == typeof(byte[]) || base.CanConvertTo(context, destinationType));
        }

        /// <summary>
        /// Converts a specified object to an <see cref="X509Certificate"/>.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> that holds information about a specific culture.</param>
        /// <param name="value">The <see cref="object"/> to be converted.</param>
        /// <returns>If this method succeeds, it returns the <see cref="X509Certificate"/> that it created by converting the specified object. Otherwise, it throws an exception.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is byte[])
                return new X509Certificate((byte[])value);
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts a <see cref="X509Certificate"/> (or an object that can be cast to an <see cref="X509Certificate"/>) to a specified type.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">A <see cref="CultureInfo"/> that holds information about a specific culture.</param>
        /// <param name="value">The object to convert. This object should be of type <see cref="X509Certificate"/> or some type that can be cast to <see cref="X509Certificate"/>.</param>
        /// <param name="destinationType">The <see cref="Type"/> to convert the <see cref="X509Certificate"/> to.</param>
        /// <returns>If this method succeeds, it returns the converted object. Otherwise, it throws an exception.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException("destinationType");
            if (destinationType == typeof(string))
                return (value != null) ? value.ToString() : "(none)";
            if (destinationType != typeof(byte[]))
                return base.ConvertTo(context, culture, value, destinationType);
            if (value == null)
                return new byte[0];
            X509Certificate cert = value as X509Certificate;
            if (cert != null)
                return cert.Export(X509ContentType.Cert);
            return null;
        }
    }
}
