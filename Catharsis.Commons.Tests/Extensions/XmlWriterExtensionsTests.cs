﻿using System;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace Catharsis.Commons.Extensions
{
  /// <summary>
  ///   <para>Tests set for class <see cref="XmlWriterExtensions"/>.</para>
  /// </summary>
  public sealed class XmlWriterExtensionsTests
  {
    /// <summary>
    ///   <para>Performs testing of <see cref="XmlWriterExtensions.Write{WRITER}(WRITER, Action{WRITER})"/> method.</para>
    /// </summary>
    [Fact]
    public void Write_Method()
    {
      Assert.Throws<ArgumentNullException>(() => XmlWriterExtensions.Write<XmlWriter>(null, writer => {}));
      Assert.Throws<ArgumentNullException>(() => XmlWriterExtensions.Write(XmlWriter.Create(Path.GetTempFileName()), null));

      const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><article>text</article>";
      var stringWriter = new StringWriter();
      var xmlWriter = stringWriter.XmlWriter(Encoding.Unicode, true);
      Assert.True(ReferenceEquals(xmlWriter.Write(writer =>
      {
        writer.WriteStartDocument();
        writer.WriteElementString("article", "text");
        writer.WriteEndDocument();
      }), xmlWriter));
      Assert.Throws<InvalidOperationException>(() => xmlWriter.WriteRaw(string.Empty));
      Assert.True(stringWriter.ToString() == xml);
      Assert.Throws<ObjectDisposedException>(() => stringWriter.WriteLine());
    }
  }
}