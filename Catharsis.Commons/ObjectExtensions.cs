using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace Catharsis.Commons
{
  /// <summary>
  ///   <para>Set of extensions methods for class <see cref="object"/>.</para>
  ///   <seealso cref="object"/>
  /// </summary>
  public static class ObjectExtensions
  {
    /// <summary>
    ///   <para>Tries to convert given object to specified type and returns <c>null</c> reference on failure.</para>
    ///   <seealso cref="To{T}"/>
    /// </summary>
    /// <typeparam name="T">Type to convert object to.</typeparam>
    /// <param name="subject">Object to convert.</param>
    /// <returns>Object, converted to the specified type, or <c>null</c> reference, if the conversion cannot be performed.</returns>
    /// <remarks>If specified object instance is a <c>null</c> reference, a <c>null</c> reference will be returned as a result.</remarks>
    public static T As<T>(this object subject)
    {
      return subject is T ? (T) subject : default(T);
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="subject"/> is a <c>null</c> reference.</exception>
    public static string Dump(this object subject)
    {
      Assertion.NotNull(subject);

      return subject.ToString(subject.GetType().GetProperties().Select(property => property.Name).ToArray());
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static bool Equality<T>(this T self, T other, params string[] properties)
    {
      if (self == null && other == null)
      {
        return true;
      }

      if (self == null || other == null)
      {
        return false;
      }

      if (ReferenceEquals(self, other))
      {
        return true;
      }

      if (properties == null || !properties.Any())
      {
        var metaProperties = self.GetType().Attributes<EqualsAndHashCodeAttribute>().SelectMany(attribute => attribute.Properties).ToArray();
        return metaProperties.Length == 0 ? self.Equals(other) : self.Equality(other, metaProperties);
      }

      var subjectProperties = properties.Select(property => self.GetType().AnyProperty(property)).Where(property => property != null);
      if (!subjectProperties.Any())
      {
        return self.Equals(other);
      }

      return subjectProperties.All(property =>
      {
        var first = property.GetValue(self, null);
        object second = null;
        try
        {
          second = property.GetValue(other, null);
        }
        catch (TargetException)
        {
        }
        
        if (first == null && second == null)
        {
          return true;
        }

        if (first == null || second == null)
        {
          return false;
        }

        return first.Equals(second);
      });
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="self"></param>
    /// <param name="other"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static bool Equality<T>(this T self, T other, params Expression<Func<T, object>>[] properties)
    {
      if (self == null && other == null)
      {
        return true;
      }

      if (self == null || other == null)
      {
        return false;
      }

      if (ReferenceEquals(self, other))
      {
        return true;
      }

      if (properties == null || properties.Length == 0)
      {
        var metaProperties = self.GetType().Attributes<EqualsAndHashCodeAttribute>().SelectMany(attribute => attribute.Properties).ToArray();
        return metaProperties.Length == 0 ? self.Equals(other) : self.Equality(other, metaProperties);
      }

      return properties.Select(property => property.Compile()).All(property => property(self).Equals(property(other)));
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static bool False(this object subject)
    {
      return !True(subject);
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="name"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.Empty"/> string.</exception>
    public static object Field(this object subject, string name)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(name);

      var subjectField = subject.GetType().AnyField(name);
      return subjectField != null ? subjectField.GetValue(subject) : null;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="finalize"></param>
    /// <remarks></remarks>
    /// <exception cref="ArgumentNullException">If <paramref name="subject"/> is a <c>null</c> reference.</exception>
    public static T Finalize<T>(this T subject, bool finalize = true) where T : class
    {
      Assertion.NotNull(subject);

      if (finalize)
      {
        GC.ReRegisterForFinalize(subject);
      }
      else
      {
        GC.SuppressFinalize(subject);
      }

      return subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static int GetHashCode<T>(this T subject, IEnumerable<string> properties)
    {
      if (subject == null)
      {
        return 0;
      }

      if (properties == null || !properties.Any())
      {
        var metaProperties = subject.GetType().Attributes<EqualsAndHashCodeAttribute>().SelectMany(attribute => attribute.Properties).ToArray();
        return metaProperties.Length == 0 ? subject.GetHashCode() : subject.GetHashCode(metaProperties);
      }

      var hash = 0;
      properties.Select(property => subject.GetType().AnyProperty(property)).Where(property => property != null).Select(property => property.GetValue(subject, null)).Where(value => value != null).Each(value => hash += value.GetHashCode());
      return hash;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    public static int GetHashCode<T>(this T subject, params Expression<Func<T, object>>[] properties)
    {
      if (subject == null)
      {
        return 0;
      }

      if (properties == null || properties.Length == 0)
      {
        var metaProperties = typeof(T).Attributes<EqualsAndHashCodeAttribute>().SelectMany(attribute => attribute.Properties).ToArray();
        return metaProperties.Length == 0 ? subject.GetHashCode() : subject.GetHashCode(metaProperties);
      }

      var hash = 0;
      properties.Select(property => property.Compile()(subject)).Where(value => value != null).Each(value => hash += value.GetHashCode());
      return hash;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="name"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.Empty"/> string.</exception>
    public static bool HasField(this object subject, string name)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(name);

      return subject.GetType().AnyField(name) != null;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="name"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.Empty"/> string.</exception>
    public static bool HasMethod(this object subject, string name)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(name);

      return subject.GetType().AnyMethod(name) != null;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="name"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.Empty"/> string.</exception>
    public static bool HasProperty(this object subject, string name)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(name);

      return subject.GetType().AnyProperty(name) != null;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="name"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="name"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.Empty"/> string.</exception>
    public static object InvokeMethod(this object subject, string name, params object[] parameters)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(name);

      var subjectMethod = subject.GetType().AnyMethod(name);
      return subjectMethod != null ? subjectMethod.Invoke(subject, parameters) : null;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="subject"/> is a <c>null</c> reference.</exception>
    public static bool Is<T>(this object subject)
    {
      Assertion.NotNull(subject);

      return subject is T;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="subject"/> is a <c>null</c> reference.</exception>
    public static bool IsNumeric(this object subject)
    {
      Assertion.NotNull(subject);

      switch (Type.GetTypeCode(subject.GetType()))
      {
        case TypeCode.Byte:
        case TypeCode.Decimal:
        case TypeCode.Double:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.SByte:
        case TypeCode.Single:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return true;
      }

      return false;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="MEMBER"></typeparam>
    /// <param name="subject"></param>
    /// <param name="expression"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="expression"/> is a <c>null</c> reference.</exception>
    public static MEMBER Member<T, MEMBER>(this T subject, Expression<Func<T, MEMBER>> expression)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(expression);

      return expression.Compile()(subject);
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="name"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="name"/> is <see cref="string.Empty"/> string.</exception>
    public static object Property(this object subject, string name)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(name);

      var subjectProperty = subject.GetType().AnyProperty(name);
      return subjectProperty != null ? subjectProperty.GetValue(subject, null) : null;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="property"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="property"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException">If <paramref name="property"/> is <see cref="string.Empty"/> string.</exception>
    public static T SetProperty<T>(this T subject, string property, object value)
    {
      Assertion.NotNull(subject);
      Assertion.NotEmpty(property);

      var subjectProperty = subject.GetType().AnyProperty(property);
      if (subjectProperty != null)
      {
        subjectProperty.SetValue(subject, value, null);
      }
      return subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="properties"/> is a <c>null</c> reference.</exception>
    public static T SetProperties<T>(this T subject, IEnumerable<KeyValuePair<string, object>> properties)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(properties);

      properties.Each(property => subject.SetProperty(property.Key, property.Value));
      return subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="properties"/> is a <c>null</c> reference.</exception>
    public static T SetProperties<T>(this T subject, object properties)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(properties);

      properties.GetType().GetProperties().Each(property => subject.SetProperty(property.Name, properties.Property(property.Name)));
      return subject;
    }

    /// <summary>
    ///   <para>Tries to convert given object to specified type and throws exception on failure.</para>
    ///   <seealso cref="As{T}"/>
    /// </summary>
    /// <typeparam name="T">Type to convert object to.</typeparam>
    /// <param name="subject">Object to convert.</param>
    /// <returns>Object, converted to the specified type.</returns>
    /// <exception cref="InvalidCastException">If conversion to specified type cannot be performed.</exception>
    /// <remarks>If specified object instance is a <c>null</c> reference, a <c>null</c> reference will be returned as a result.</remarks>
    public static T To<T>(this object subject)
    {
      return (T) subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="subject"/> is a <c>null</c> reference.</exception>
    public static string ToString(this object subject, IEnumerable<string> properties)
    {
      Assertion.NotNull(subject);

      const string Separator = ", ";
      var sb = new StringBuilder();
      if (properties != null)
      {
        properties.Where(property => subject.HasProperty(property)).Each(property => sb.AppendFormat("{0}:\"{1}\"{2}", property, subject.Property(property), Separator));
      }
      if (sb.Length > 0)
      {
        sb.Remove(sb.Length - Separator.Length, Separator.Length);
      }
      return "[{0}]".FormatValue(sb.ToString());
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="subject"/>If <paramref name="subject"/> is a <c>null</c> reference.</exception>
    public static string ToString<T>(this T subject, params Expression<Func<T, object>>[] properties)
    {
      Assertion.NotNull(subject);

      const string Separator = ", ";
      var sb = new StringBuilder();
      if (properties != null)
      {
        properties.Each(property => sb.AppendFormat("{0}:\"{1}\"{2}", property.Body.To<UnaryExpression>().Operand.To<MemberExpression>().Member.Name, property.Compile()(subject), Separator));
      }
      if (sb.Length > 0)
      {
        sb.Remove(sb.Length - Separator.Length, Separator.Length);
      }
      return "[{0}]".FormatValue(sb.ToString());
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static bool True(this object subject)
    {
      if (subject == null)
      {
        return false;
      }

      switch (Type.GetTypeCode(subject.GetType()))
      {
        case TypeCode.Boolean :
          return ((bool) subject);

        case TypeCode.Byte :
          return ((byte) subject) > 0;

        case TypeCode.Char :
          return ((char) subject) != Char.MinValue;

        case TypeCode.Decimal :
          return ((decimal) subject) > 0;

        case TypeCode.Double :
          return ((double) subject) > 0;

        case TypeCode.Int16 :
          return ((short) subject) > 0;

        case TypeCode.Int32 :
          return ((int) subject) > 0;

        case TypeCode.Int64 :
          return ((long) subject) > 0;

        case TypeCode.SByte :
          return ((sbyte) subject) > 0;

        case TypeCode.Single :
          return ((Single) subject) > 0;

        case TypeCode.String :
          return ((string) subject).Length > 0;

        case TypeCode.UInt16 :
          return ((ushort) subject) > 0;

        case TypeCode.UInt32 :
          return ((uint) subject) > 0;

        case TypeCode.UInt64 :
          return ((ulong) subject) > 0;
      }

      if (subject is IEnumerable)
      {
        return subject.To<IEnumerable>().Cast<object>().Any();
      }

      if (subject is Match)
      {
        return subject.To<Match>().Success;
      }

      return true;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="action"/> is a <c>null</c> reference.</exception>
    public static T With<T>(this T subject, Action<T> action)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(action);

      if (subject is IDisposable)
      {
        using (subject as IDisposable)
        {
          action(subject);
        }
      }
      else
      {
        action(subject);
      }

      return subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="RESULT"></typeparam>
    /// <param name="subject"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="action"/> is a <c>null</c> reference.</exception>
    public static RESULT With<T, RESULT>(this T subject, Func<T, RESULT> action)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(action);

      var result = default(RESULT);
      subject.With<T>(x => result = action(x));
      return result;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string Xml<T>(this T subject, params Type[] types)
    {
      Assertion.NotNull(subject);

      return new StringWriter().With(writer =>
      {
        subject.Xml(writer, types);
        return writer.ToString();
      });
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="destination"></param>
    /// <param name="encoding"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="destination"/> or <paramref name="types"/> is a <c>null</c> reference.</exception>
    public static T Xml<T>(this T subject, Stream destination, Encoding encoding = null, params Type[] types)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(destination);

      destination.XmlWriter(encoding: encoding).Write(writer => subject.Xml(writer, types));
      return subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="writer"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="writer"/> is a <c>null</c> reference.</exception>
    public static T Xml<T>(this T subject, TextWriter writer, params Type[] types)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(writer);

      var serializer = types != null ? new XmlSerializer(typeof(T), types) : new XmlSerializer(typeof(T));
      writer.XmlWriter(encoding: Encoding.UTF8).Write(xmlWriter => serializer.Serialize(xmlWriter, subject));
      return subject;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subject"></param>
    /// <param name="writer"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="subject"/> or <paramref name="writer"/> is a <c>null</c> reference.</exception>
    public static T Xml<T>(this T subject, XmlWriter writer, params Type[] types)
    {
      Assertion.NotNull(subject);
      Assertion.NotNull(writer);

      var serializer = types != null ? new XmlSerializer(typeof(T), types) : new XmlSerializer(typeof(T));
      serializer.Serialize(writer, subject);
      return subject;
    }
  }
}