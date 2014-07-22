using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catharsis.Commons
{
  /// <summary>
  ///   <para>Set of extensions methods for interface <see cref="IEnumerable" />.</para>
  /// </summary>
  /// <seealso cref="IEnumerable"/>
  public static class IEnumerableExtensions
  {
    /// <summary>
    ///   <para>Iterates through a sequence, calling a delegate for each element in it.</para>
    /// </summary>
    /// <typeparam name="T">Type of elements in a sequence.</typeparam>
    /// <param name="self">Source sequence for iteration.</param>
    /// <param name="action">Delegate to be called for each element in a sequence.</param>
    /// <returns>Back reference to the current sequence.</returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="self"/> or <paramref name="action"/> is a <c>null</c> reference.</exception>
    public static IEnumerable<T> Each<T>(this IEnumerable<T> self, Action<T> action)
    {
      Assertion.NotNull(self);
      Assertion.NotNull(action);

      foreach (var value in self)
      {
        action(value);
      }

      return self;
    }
    
    /// <summary>
    ///   <para>Concatenates all elements in a sequence into a string, using specified separator.</para>
    /// </summary>
    /// <typeparam name="T">Type of elements in a sequence.</typeparam>
    /// <param name="self">Source sequence of elements.</param>
    /// <param name="separator">String to use as a separator between concatenated elements from <paramref name="self"/>.</param>
    /// <returns>String which is formed from string representation of each element in a <paramref name="self"/> with a <paramref name="separator"/> between them.</returns>
    /// <exception cref="ArgumentNullException">If either <paramref name="self"/> or <paramref name="separator"/> is a <c>null</c> reference.</exception>
    public static string Join<T>(this IEnumerable<T> self, string separator)
    {
      Assertion.NotNull(self);
      Assertion.NotNull(separator);

      var sb = new StringBuilder();
      self.Each(element => sb.AppendFormat("{0}{1}", element, separator));
      if (sb.Length > 0)
      {
        sb.Remove(sb.Length - separator.Length, separator.Length);
      }
      return sb.ToString();
    }

    /// <summary>
    ///   <para>Performs "pagination" of a sequence, returning a fragment ("page") of its contents.</para>
    /// </summary>
    /// <typeparam name="T">Type of elements in a sequence.</typeparam>
    /// <param name="self">Source sequence from which a fragment is to be taken.</param>
    /// <param name="page">Number of fragment/slice that is to be taken. Numbering starts from 1.</param>
    /// <param name="pageSize">Size of fragment ("page"), number of entities to be taken. Must be a positive number.</param>
    /// <returns>Source that represent a fragment of the original <paramref name="self"/> sequence and consists of no more than <paramref name="pageSize"/> elements.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="self"/> is a <c>null</c> reference.</exception>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> self, int page = 1, int pageSize = 10)
    {
      Assertion.NotNull(self);

      if (page <= 0)
      {
        page = 1;
      }

      if (pageSize <= 0)
      {
        pageSize = 10;
      }

      return self.Skip((page - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    ///   <para>Picks up random element from a specified sequence and returns it.</para>
    /// </summary>
    /// <typeparam name="T">Type of elements in a sequence.</typeparam>
    /// <param name="self">Source sequence of elements.</param>
    /// <returns>Random member of <paramref name="self"/> sequence.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="self"/> is a <c>null</c> reference. If <paramref name="self"/> contains no elements, returns <c>null</c>.</exception>
    public static T Random<T>(this IEnumerable<T> self)
    {
      Assertion.NotNull(self);

      var max = self.Count();
      return (T) (max > 0 ? self.ElementAt(new Random().Next(max)) : (object) null);
    }

    /// <summary>
    ///   <para>Concatenates all elements in a sequence into a string, using comma as a separator and placing the result inside a square brackets.</para>
    /// </summary>
    /// <typeparam name="T">Type of elements in a sequence.</typeparam>
    /// <param name="self">Source sequence of elements.</param>
    /// <returns>String which is formed from string representation of each element in a <paramref name="self"/> with a comma-character separator between them, all inside square brackets.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="self" /> is a <c>null</c> reference.</exception>
    public static string ToListString<T>(this IEnumerable<T> self)
    {
      Assertion.NotNull(self);

      return string.Format("[{0}]", self.Join(", "));
    }
  }
}