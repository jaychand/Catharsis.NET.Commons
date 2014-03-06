﻿using System;
using System.Reflection;

namespace Catharsis.Commons
{
  /// <summary>
  ///   <para>Set of extensions methods for class <see cref="PropertyInfo"/>.</para>
  /// </summary>
  /// <seealso cref="PropertyInfo"/>
  public static class PropertyInfoExtensions
  {
    /// <summary>
    ///   <para></para>
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="property"/> is a <c>null</c> reference.</exception>
    public static bool IsPublic(this PropertyInfo property)
    {
      Assertion.NotNull(property);

      if (property.CanRead)
      {
        return property.GetGetMethod() != null;
      }

      if (property.CanWrite)
      {
        return property.GetSetMethod() != null;
      }

      return false;
    }
  }
}