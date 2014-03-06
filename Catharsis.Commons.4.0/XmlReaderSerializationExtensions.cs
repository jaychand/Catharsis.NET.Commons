﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Catharsis.Commons
{
  /// <summary>
  ///   <para>Set of extension methods for class <see cref="XmlReader"/>.</para>
  /// </summary>
  /// <seealso cref="XmlReader"/>
  public static class XmlReaderSerializationExtensions
  {
    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="type"></param>
    /// <param name="types"></param>
    /// <param name="close"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="reader"/> or <paramref name="type"/> is a <c>null</c> reference.</exception>
    public static object Deserialize(this XmlReader reader, Type type, IEnumerable<Type> types = null, bool close = false)
    {
      Assertion.NotNull(reader);
      Assertion.NotNull(type);

      object result;
      try
      {
        result = (types != null ? new XmlSerializer(type, types.ToArray()) : new XmlSerializer(type)).Deserialize(reader);
      }
      finally
      {
        if (close)
        {
          reader.Close();
        }
      }
      return result;
    }

    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="reader"></param>
    /// <param name="types"></param>
    /// <param name="close"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="reader"/> is a <c>null</c> reference.</exception>
    public static T Deserialize<T>(this XmlReader reader, IEnumerable<Type> types = null, bool close = false)
    {
      Assertion.NotNull(reader);

      return reader.Deserialize(typeof(T), types, close).As<T>();
    }
  }
}