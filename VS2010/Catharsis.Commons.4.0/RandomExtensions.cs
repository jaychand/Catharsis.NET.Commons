﻿using System;

namespace Catharsis.Commons
{
  /// <summary>
  ///   <para>Set of extension methods for class <see cref="Random"/>.</para>
  /// </summary>
  /// <seealso cref="Random"/>
  public static class RandomExtensions
  {
    /// <summary>
    ///   <para>Generates specified number of random bytes.</para>
    /// </summary>
    /// <param name="self">Randomization object that is being extended.</param>
    /// <param name="count">Number of bytes to generate.</param>
    /// <returns>Array of randomly generated bytes. Length of array is equal to <paramref name="count"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="self"/> is a <c>null</c> reference.</exception>
    /// <exception cref="ArgumentException"></exception>
    /// <seealso cref="Random.NextBytes(byte[])"/>
    public static byte[] Bytes(this Random self, int count)
    {
      Assertion.NotNull(self);
      Assertion.True(count > 0);

      var numbers = new byte[count];
      self.NextBytes(numbers);
      return numbers;
    }
  }
}