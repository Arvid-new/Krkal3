//////////////////////////////////////////////////////////////////////////////
///
///		Krkal.Compiler - L i n e s
///
///		Helps to transform file position to position in lines
///		A: Honza M.D. Krcek
///
///////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Text;

namespace Krkal.Compiler
{
	internal class Lines
	{
		private List<int> _linePositions = new List<int>();

		// CONSTRUCTOR
		internal Lines(String text) {
			int pos = 0;
			_linePositions.Add(0);
			while ((pos = text.IndexOf('\n', pos)) != -1) {
				pos++;
				if (pos < text.Length && text[pos] == '\r')
					pos++;
				_linePositions.Add(pos);
			}
		}


		public void FindLinePos(int pos, out int line, out int posInLine) {
			line = _linePositions.BinarySearch(pos);
			if (line < 0)
				line = (~line) - 1;
			posInLine = pos - _linePositions[line];
		}
	}


	public class PositionInLines
	{

		/// <summary>
		/// The start row of the range
		/// </summary>
		private int _firstRow;
		public int FirstRow {
			get { return _firstRow; }
		}

		/// <summary>
		/// The start column of the range
		/// </summary>
		private int _firstColumn;
		public int FirstColumn {
			get { return _firstColumn; }
		}

		/// <summary>
		/// The end row of the range
		/// </summary>
		private int _lastRow;
		public int LastRow {
			get { return _lastRow; }
		}

		/// <summary>
		/// The end column of the range
		/// </summary>
		private int _lastColumn;
		public int LastColumn {
			get { return _lastColumn; }
		}


		// CONSTRUCTOR
		internal PositionInLines(int pos, int size, Lines lines) {
			lines.FindLinePos(pos, out _firstRow, out _firstColumn);
			lines.FindLinePos(pos + size - 1, out _lastRow, out _lastColumn);
		}
	}
}
