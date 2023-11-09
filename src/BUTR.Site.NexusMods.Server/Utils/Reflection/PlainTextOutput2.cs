using BUTR.Site.NexusMods.Server.Extensions;

using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;

using System;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace BUTR.Site.NexusMods.Server.Utils.Reflection;

public class PlainTextOutput2 : ITextOutput
{
    private readonly TextWriter _writer;
    private int _indent;
    private bool _needsIndent;

    private int _line = 1;
    private int _column = 1;

	public string IndentationString { get; set; } = "\t";

	public PlainTextOutput2(TextWriter writer) => _writer = writer ?? throw new ArgumentNullException(nameof(writer));

    public PlainTextOutput2() => _writer = new StringWriter();

    public TextLocation Location => new(_line, _column + (_needsIndent ? _indent : 0));

    public override string? ToString() => _writer.ToString();

    public void Indent() => _indent++;

    public void Unindent() => _indent--;

    void WriteIndent()
	{
		if (_needsIndent)
		{
			_needsIndent = false;
			for (var i = 0; i < _indent; i++)
			{
				_writer.Write(IndentationString);
			}
			_column += _indent;
		}
	}

    public void Write(ReadOnlySpan<char> span)
    {
        WriteIndent();
        _writer.Write(span);
        _column += span.Length;
    }
    
    public void Write(StringBuilder sb, int offset, int length)
    {
        WriteIndent();
        _writer.Write(sb, offset, length);
        _column += length;
    }
    
	public void Write(char ch)
	{
		WriteIndent();
		_writer.Write(ch);
		_column++;
	}

	public void Write(string text)
	{
		WriteIndent();
		_writer.Write(text);
		_column += text.Length;
	}

	public void WriteLine()
	{
		_writer.WriteLine();
		_needsIndent = true;
		_line++;
		_column = 1;
	}

	public void WriteLine(ReadOnlySpan<char> line)
	{
		_writer.WriteLine(line);
		_needsIndent = true;
		_line++;
		_column = 1;
	}

	public void WriteReference(OpCodeInfo opCode, bool omitSuffix = false)
	{
		if (omitSuffix)
		{
			var lastDot = opCode.Name.LastIndexOf('.');
			if (lastDot > 0)
			{
				Write(opCode.Name.Remove(lastDot + 1));
			}
		}
		else
		{
			Write(opCode.Name);
		}
	}

	public void WriteReference(PEFile module, Handle handle, string text, string protocol = "decompile", bool isDefinition = false)
	{
		Write(text);
	}

	public void WriteReference(IType type, string text, bool isDefinition = false)
	{
		Write(text);
	}

	public void WriteReference(IMember member, string text, bool isDefinition = false)
	{
		Write(text);
	}

	public void WriteLocalReference(string text, object reference, bool isDefinition = false)
	{
		Write(text);
	}

	void ITextOutput.MarkFoldStart(string collapsedText, bool defaultCollapsed, bool isDefinition) { }

	void ITextOutput.MarkFoldEnd() { }    
}