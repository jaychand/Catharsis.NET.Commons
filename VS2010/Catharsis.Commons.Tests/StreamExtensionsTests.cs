﻿using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Xunit;

namespace Catharsis.Commons
{
  /// <summary>
  ///   <para>Tests set for class <see cref="StreamExtensions"/>.</para>
  /// </summary>
  public sealed class StreamExtensionsTests
  {
    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.BinaryReader(Stream, Encoding)"/> method.</para>
    /// </summary>
    [Fact]
    public void BinaryReader_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.BinaryReader(null));

      var bytes = Guid.NewGuid().ToByteArray();
      var stream = new MemoryStream(bytes);
      stream.BinaryReader().With(reader =>
      {
        Assert.True(ReferenceEquals(reader.BaseStream, stream));
        Assert.True(reader.ReadBytes(bytes.Length).SequenceEqual(bytes));
      });
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.BinaryWriter(Stream, Encoding)"/> method.</para>
    /// </summary>
    [Fact]
    public void BinaryWriter_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.BinaryWriter(null));

      var bytes = Guid.NewGuid().ToByteArray();
      var stream = new MemoryStream();
      stream.BinaryWriter().With(writer =>
      {
        Assert.True(ReferenceEquals(writer.BaseStream, stream));
        writer.Write(bytes);
      });
      Assert.True(stream.ToArray().SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtendedExtensions.Buffered(Stream, int?)"/> method.</para>
    /// </summary>
    [Fact]
    public void Buffered_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtendedExtensions.Buffered(null));
      Assert.Throws<ArgumentOutOfRangeException>(() => Stream.Null.Buffered(-1));
      Assert.Throws<ArgumentOutOfRangeException>(() => Stream.Null.Buffered(0));

      var bytes = Guid.NewGuid().ToByteArray();
      var stream = new MemoryStream(bytes);
      stream.Buffered().With(buffered => buffered.Write(bytes));
      Assert.True(stream.ToArray().SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.Bytes(Stream, bool)"/> method.</para>
    /// </summary>
    [Fact]
    public void Bytes_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.Bytes(null));

      Assert.Equal(0, Stream.Null.Bytes().Length);

      var bytes = Guid.NewGuid().ToByteArray();

      var stream = new MemoryStream(bytes);
      Assert.True(stream.Bytes().SequenceEqual(bytes));
      Assert.Equal(-1, stream.ReadByte());
      stream.Close();
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
      
      stream = new MemoryStream(bytes);
      Assert.True(stream.Bytes(true).SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }
    
    /// <summary>
    ///   <para>Performs testing of following methods :</para>
    ///   <list type="bullet">
    ///     <item><description><see cref="StreamCompressionExtensions.Deflate(Stream, CompressionMode)"/></description></item>
    ///     <item><description><see cref="StreamCompressionExtensions.Deflate{STREAM}(STREAM, byte[])"/></description></item>
    ///     <item><description><see cref="StreamCompressionExtensions.Deflate(Stream)"/></description></item>
    ///   </list>
    /// </summary>
    [Fact]
    public void Deflate_Methods()
    {
      Assert.Throws<ArgumentNullException>(() => StreamCompressionExtensions.Deflate(null, CompressionMode.Compress));
      Assert.Throws<ArgumentNullException>(() => StreamCompressionExtensions.Deflate<Stream>(null, Enumerable.Empty<byte>().ToArray()));
      Assert.Throws<ArgumentNullException>(() => Stream.Null.Deflate(null));
      Assert.Throws<ArgumentNullException>(() => StreamCompressionExtensions.Deflate(null));

      var bytes = Guid.NewGuid().ToByteArray();

      var stream = new MemoryStream();
      var compressed = new byte[] { };
      stream.Deflate(CompressionMode.Compress).With(deflate =>
      {
        Assert.True(ReferenceEquals(deflate.BaseStream, stream));
        Assert.Throws<InvalidOperationException>(() => deflate.ReadByte());
        deflate.Write(bytes);
      });
      compressed = stream.ToArray();
      Assert.False(compressed.SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());

      stream = new MemoryStream(compressed);
      var decompressed = new byte[] { };
      stream.Deflate(CompressionMode.Decompress).With(deflate =>
      {
        Assert.True(ReferenceEquals(deflate.BaseStream, stream));
        decompressed = deflate.Bytes();
      });
      Assert.True(decompressed.SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());

      new MemoryStream().With(x =>
      {
        Assert.True(ReferenceEquals(x.Deflate(bytes), x));
        Assert.True(x.ToArray().SequenceEqual(compressed));
        Assert.Equal(0, x.Bytes().Length);
        Assert.True(x.CanRead);
        Assert.True(x.CanWrite);
      });

      new MemoryStream(compressed).With(x =>
      {
        Assert.True(x.Deflate().SequenceEqual(bytes));
        Assert.Equal(0, x.Bytes().Length);
        Assert.True(x.CanRead);
        Assert.True(x.CanWrite);
      });

      Assert.True(new MemoryStream().Deflate(bytes).Rewind().Deflate().SequenceEqual(bytes));
    }

    /// <summary>
    ///   <para>Performs testing of following methods :</para>
    ///   <list type="bullet">
    ///     <item><description><see cref="StreamCompressionExtensions.GZip(Stream, CompressionMode)"/></description></item>
    ///     <item><description><see cref="StreamCompressionExtensions.GZip{STREAM}(STREAM, byte[])"/></description></item>
    ///     <item><description><see cref="StreamCompressionExtensions.GZip(Stream)"/></description></item>
    ///   </list>
    /// </summary>
    [Fact]
    public void GZip_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamCompressionExtensions.GZip(null, CompressionMode.Compress));
      Assert.Throws<ArgumentNullException>(() => StreamCompressionExtensions.GZip<Stream>(null, Enumerable.Empty<byte>().ToArray()));
      Assert.Throws<ArgumentNullException>(() => Stream.Null.GZip(null));
      Assert.Throws<ArgumentNullException>(() => StreamCompressionExtensions.GZip(null));

      var bytes = Guid.NewGuid().ToByteArray();
      
      var stream = new MemoryStream();
      var compressed = new byte[] {};
      stream.GZip(CompressionMode.Compress).With(gzip =>
      {
        Assert.True(ReferenceEquals(gzip.BaseStream, stream));
        Assert.Throws<InvalidOperationException>(() => gzip.ReadByte());
        gzip.Write(bytes);
      });
      compressed = stream.ToArray();
      Assert.False(compressed.SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());

      stream = new MemoryStream(compressed);
      var decompressed = new byte[] {};
      stream.GZip(CompressionMode.Decompress).With(gzip =>
      {
        Assert.True(ReferenceEquals(gzip.BaseStream, stream));
        decompressed = gzip.Bytes();
      });
      Assert.True(decompressed.SequenceEqual(bytes));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());

      new MemoryStream().With(x =>
      {
        Assert.True(ReferenceEquals(x.GZip(bytes), x));
        Assert.True(x.ToArray().SequenceEqual(compressed));
        Assert.Equal(0, x.Bytes().Length);
        Assert.True(x.CanRead);
        Assert.True(x.CanWrite);
      });

      new MemoryStream(compressed).With(x =>
      {
        Assert.True(x.GZip().SequenceEqual(bytes));
        Assert.Equal(0, x.Bytes().Length);
        Assert.True(x.CanRead);
        Assert.True(x.CanWrite);
      });

      Assert.True(new MemoryStream().GZip(bytes).Rewind().GZip().SequenceEqual(bytes));
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.Rewind{STREAM}(STREAM)"/> method.</para>
    /// </summary>
    [Fact]
    public void Rewind_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.Rewind<Stream>(null));

      Assert.True(ReferenceEquals(Stream.Null.Rewind(), Stream.Null));
      
      var bytes = Guid.NewGuid().ToByteArray();
      var stream = new MemoryStream(bytes);
      Assert.Equal(0, stream.Position);
      stream.Seek(0, SeekOrigin.End);
      Assert.Equal(stream.Length, stream.Position);
      stream.Rewind();
      Assert.Equal(0, stream.Position);
      stream.Rewind();
      Assert.Equal(0, stream.Position);
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.Text"/> method.</para>
    /// </summary>
    [Fact]
    public void Text_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.Text(null));

      var bytes = Guid.NewGuid().ToByteArray();

      var stream = new MemoryStream(bytes);
      var text = stream.Text();
      Assert.Equal(text, stream.Rewind().TextReader().Text());
      Assert.Equal(-1, stream.ReadByte());
      Assert.Equal(text, stream.Rewind().Text(true));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());

      stream = new MemoryStream(bytes);
      text = stream.Text(encoding: Encoding.Unicode);
      Assert.Equal(text, stream.Rewind().TextReader(Encoding.Unicode).Text());
      Assert.Equal(text, bytes.String(Encoding.Unicode));
      Assert.Equal(-1, stream.ReadByte());
      Assert.Equal(text, stream.Rewind().Text(true, Encoding.Unicode));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.TextReader(Stream, Encoding)"/> method.</para>
    /// </summary>
    [Fact]
    public void TextReader_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.TextReader(null));

      var text = Guid.NewGuid().ToString();
      
      var stream = new MemoryStream(text.Bytes());
      Assert.Equal(text, stream.TextReader().Text());
      Assert.Equal(-1, stream.ReadByte());
      
      stream = new MemoryStream(text.Bytes(Encoding.Unicode));
      Assert.Equal(text, stream.TextReader(Encoding.Unicode).Text());
      Assert.Equal(-1, stream.ReadByte());
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.TextWriter(Stream, Encoding)"/> method.</para>
    /// </summary>
    [Fact]
    public void TextWriter_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.TextWriter(null));

      var text = Guid.NewGuid().ToString();
      
      var stream = new MemoryStream();
      stream.TextWriter().With(writer => writer.Write(text));
      Assert.True(stream.ToArray().SequenceEqual(text.Bytes()));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());

      stream = new MemoryStream();
      stream.TextWriter(Encoding.Unicode).With(writer => writer.Write(text));
      Assert.True(stream.ToArray().SequenceEqual(text.Bytes(Encoding.Unicode)));
      Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
    }

    /// <summary>
    ///   <para>Performs testing of following methods :</para>
    ///   <list type="bullet">
    ///     <item><description><see cref="StreamExtensions.Write{STREAM}(STREAM, byte[])"/></description></item>
    ///     <item><description><see cref="StreamExtensions.Write{STREAM}(STREAM, Stream)"/></description></item>
    ///     <item><description><see cref="StreamExtensions.Write{STREAM}(STREAM, string, Encoding)"/></description></item>
    ///   </list>
    /// </summary>
    [Fact]
    public void Write_Methods()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.Write<Stream>(null, Enumerable.Empty<byte>().ToArray()));
      Assert.Throws<ArgumentNullException>(() => Stream.Null.Write((byte[])null));
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.Write<Stream>(null, Stream.Null));
      Assert.Throws<ArgumentNullException>(() => Stream.Null.Write((Stream)null));
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.Write<Stream>(null, "text"));
      Assert.Throws<ArgumentNullException>(() => Stream.Null.Write((string)null));

      var bytes = Guid.NewGuid().ToByteArray();
      var text = bytes.ToString();

      var stream = new MemoryStream();
      Assert.True(ReferenceEquals(stream.Write(bytes), stream));
      stream.Close();
      Assert.True(stream.ToArray().SequenceEqual(bytes));

      var from = new MemoryStream(bytes);
      var to = new MemoryStream();
      Assert.True(ReferenceEquals(to.Write(from), to));
      Assert.True(to.ToArray().SequenceEqual(bytes));
      Assert.Equal(0, from.Bytes().Length);
      Assert.True(from.CanRead);
      Assert.Equal(0, to.Bytes().Length);
      Assert.True(to.CanWrite);
      from.Close();
      to.Close();

      new MemoryStream().With(x =>
      {
        Assert.True(ReferenceEquals(x.Write(string.Empty), x));
        Assert.Equal(string.Empty, x.Text());
      });

      new MemoryStream().With(x =>
      {
        Assert.True(ReferenceEquals(x.Write(text), x));
        Assert.Equal(text, x.Rewind().Text());
      });

      new MemoryStream().With(x =>
      {
        Assert.True(ReferenceEquals(x.Write(text, Encoding.Unicode), x));
        Assert.Equal(text, x.Rewind().Text(encoding: Encoding.Unicode));
      });
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamXmlExtensions.Xml{T}(Stream, bool, Type[])"/> method.</para>
    /// </summary>
    [Fact]
    public void Xml_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamXmlExtensions.Xml<object>(null));

      var subject = Guid.Empty;
      new MemoryStream().With(stream =>
      {
        subject.Xml(stream, Encoding.Unicode);
        Assert.Equal(subject, stream.Rewind().Xml<Guid>());
        Assert.True(stream.CanWrite);
      });
      new MemoryStream().With(stream =>
      {
        subject.Xml(stream, Encoding.Unicode);
        Assert.Equal(subject, stream.Rewind().Xml<Guid>(true));
        Assert.False(stream.CanWrite);
      });
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.XDocument(Stream, bool)"/> method.</para>
    /// </summary>
    [Fact]
    public void XDocument_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.XDocument(null));
      Assert.Throws<XmlException>(() => Stream.Null.XDocument());

      const string Xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><article>text</article>";

      new MemoryStream(Xml.Bytes(Encoding.UTF32)).With(x => Assert.Throws<XmlException>(() => x.XDocument()));
      
      new MemoryStream(Xml.Bytes(Encoding.Unicode)).With(stream =>
      {
        Assert.Equal("<article>text</article>", stream.XDocument().ToString());
        Assert.Equal(0, stream.Bytes().Length);
        Assert.Equal(-1, stream.ReadByte());
      });

      new MemoryStream(Xml.Bytes(Encoding.Unicode)).With(stream =>
      {
        Assert.Equal("<article>text</article>", stream.XDocument(true).ToString());
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
      });
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamXmlExtensions.XmlDocument(Stream, bool)"/> method.</para>
    /// </summary>
    [Fact]
    public void XmlDocument_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamXmlExtensions.XmlDocument(null));
      Assert.Throws<XmlException>(() => Stream.Null.XmlDocument());

      const string Xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><article>text</article>";
      
      new MemoryStream(Xml.Bytes(Encoding.UTF32)).With(x => Assert.Throws<XmlException>(() => x.XmlDocument()));
      
      new MemoryStream(Xml.Bytes(Encoding.Unicode)).With(stream =>
      {
        Assert.Equal(Xml, stream.XmlDocument().String());
        Assert.Equal(0, stream.Bytes().Length);
        Assert.Equal(-1, stream.ReadByte());
      });

      new MemoryStream(Xml.Bytes(Encoding.Unicode)).With(stream =>
      {
        Assert.Equal(Xml, stream.XmlDocument(true).String());
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
      });
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.XmlReader(Stream, bool)"/> method.</para>
    /// </summary>
    [Fact]
    public void XmlReader_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.XmlReader(null));

      const string Xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><article>text</article>";

      new MemoryStream(Xml.Bytes()).With(stream =>
      {
        var reader = stream.XmlReader();
        Assert.False(reader.Settings.CloseInput);
        Assert.True(reader.Settings.IgnoreComments);
        Assert.True(reader.Settings.IgnoreWhitespace);
        reader.ReadStartElement("article");
        Assert.Equal("text", reader.ReadString());
        reader.ReadEndElement();
        reader.Close();
        Assert.Equal(0, stream.Bytes().Length);
        Assert.Equal(-1, stream.ReadByte());
      });

      new MemoryStream(Xml.Bytes()).With(stream =>
      {
        stream.XmlReader(true).Close();
        //Assert.True(reader.Settings.CloseInput);
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
      });
    }

    /// <summary>
    ///   <para>Performs testing of <see cref="StreamExtensions.XmlWriter"/> method.</para>
    /// </summary>
    [Fact]
    public void XmlWriter_Method()
    {
      Assert.Throws<ArgumentNullException>(() => StreamExtensions.XmlWriter(null));
      
      var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><article>text</article>";
      new MemoryStream().With(stream =>
      {
        stream.XmlWriter().Write(writer =>
        {
          Assert.False(writer.Settings.CloseOutput);
          Assert.Equal(Encoding.UTF8.ToString(), writer.Settings.Encoding.ToString());
          Assert.False(writer.Settings.Indent);
          writer.WriteElementString("article", "text");
        });
        Assert.True(stream.ToArray().SequenceEqual(xml.Bytes(Encoding.UTF8)));
        Assert.Equal(0, stream.Bytes().Length);
        Assert.Equal(-1, stream.ReadByte());

        stream.Rewind().XmlWriter(true).Write(writer =>
        {
          Assert.True(writer.Settings.CloseOutput);
          Assert.Equal(Encoding.UTF8.ToString(), writer.Settings.Encoding.ToString());
        });
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
      });

      xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><article>text</article>";
      new MemoryStream().With(stream =>
      {
        stream.XmlWriter(encoding: Encoding.Unicode).Write(writer =>
        {
          Assert.False(writer.Settings.CloseOutput);
          Assert.Equal(Encoding.Unicode.ToString(), writer.Settings.Encoding.ToString());
          writer.WriteElementString("article", "text");
        });
        Assert.True(stream.ToArray().SequenceEqual(xml.Bytes(Encoding.Unicode)));
        Assert.Equal(0, stream.Bytes().Length);
        Assert.Equal(-1, stream.ReadByte());

        stream.XmlWriter(true, Encoding.Unicode).Write(writer =>
        {
          Assert.True(writer.Settings.CloseOutput);
          Assert.Equal(Encoding.Unicode.ToString(), writer.Settings.Encoding.ToString());
        });
        Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
      });
    }
  }
}