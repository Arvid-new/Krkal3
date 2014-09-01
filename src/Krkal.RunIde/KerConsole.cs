using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;
using System.Globalization;

namespace Krkal.RunIde
{
	public partial class KerConsole : UserControl
	{
		RuntimeErrorConvertor _convertor;

		public KerConsole() {
			InitializeComponent();
		}


		Color _colorFatalError = Color.FromArgb(153, 0, 51);
		public Color ColorFatalError {
			get { return _colorFatalError; }
			set { _colorFatalError = value; }
		}
		Color _colorError = Color.FromArgb(255, 0, 0);
		public Color ColorError {
			get { return _colorError; }
			set { _colorError = value; }
		}
		Color _colorWarning = Color.FromArgb(255, 153, 0);
		public Color ColorWarning {
			get { return _colorWarning; }
			set { _colorWarning = value; }
		}
		Color _colorInfo = Color.FromArgb(0, 0, 255);
		public Color ColorInfo {
			get { return _colorInfo; }
			set { _colorInfo = value; }
		}
		Color _colorUser = Color.FromArgb(0, 0, 0);
		public Color ColorUser {
			get { return _colorUser; }
			set { _colorUser = value; }
		}



		public void LoadErrorTexts() {
			_convertor = new RuntimeErrorConvertor();
			_convertor.ErrorMessage += new EventHandler<RuntimeErrorEventArgs>(_convertor_ErrorMessage);
		}

		void _convertor_ErrorMessage(object sender, RuntimeErrorEventArgs e) {
			Color color;
			switch (e.Group) {
				case 3:
					color = _colorWarning;
					break;
				case 4:
					color = _colorInfo;
					break;
				case 8:
					color = _colorUser;
					break;
				case 0:
				case 1:
					color = _colorFatalError;
					break;
				default:
					color = _colorError;
					break;
			}

			if (InvokeRequired) {
				Invoke(new WriteLineDelegate(WriteLine), e.WholeMessage, color);
			} else {
				WriteLine(e.WholeMessage, color);
			}
		}

		delegate void WriteLineDelegate(String message, Color color);
		private void WriteLine(String message, Color color) {
			richTextBox1.SelectionColor = color;
			richTextBox1.AppendText(message);
			richTextBox1.ScrollToCaret();
		}

		public void WriteLine(int time, int errorNum, int errorParam, string message) {
			_convertor.WriteLine(time, errorNum, errorParam, message);
		}

		
	}




	public class RuntimeErrorConvertor : IDisposable
	{
		FSRegisterFile _errorTextsFile;
		FSRegister _errorTexts;


		// CONSTRUCTOR
		public RuntimeErrorConvertor() {
			_errorTextsFile = new FSRegisterFile("$ERRORS$", "ERRORS");
			FSRegKey key;
			if (_errorTextsFile.OpenError != FSRegOpenError.OK || (key = _errorTextsFile.Reg.FindKey("Ker RTE")).IsNull) {
				_errorTextsFile.Dispose();
				_errorTextsFile = null;
			} else {
				_errorTexts = key.Subregister;
			}
		}


		public void WriteLine(int time, int errorNum, int errorParam, string message) {
			var errorMessage = ErrorMessage;
			if (errorMessage != null)
				errorMessage(this, new RuntimeErrorEventArgs(time, errorNum, errorParam, message, _errorTexts));
		}

		public event EventHandler<RuntimeErrorEventArgs> ErrorMessage;


		public void Dispose() {
			if (_errorTextsFile != null)
				_errorTextsFile.Dispose();
			_errorTextsFile = null;
			_errorTexts = new FSRegister();
		}

	}


	public class RuntimeErrorEventArgs : EventArgs
	{
		int _time;
		public int Time {
			get { return _time; }
		}

		int _codedErrorNum;
		public int CodedErrorNum {
			get { return _codedErrorNum; }
		}

		int _group;
		public int Group {
			get { return _group; }
		}

		int _errorNum;
		public int ErrorNum {
			get { return _errorNum; }
		}

		int _errorParam;
		public int ErrorParam {
			get { return _errorParam; }
		}

		String _errorText;
		public String ErrorText {
			get { return _errorText; }
		}

		String _userMessage;
		public String UserMessage {
			get { return _userMessage; }
		}

		String _wholeMessage;
		public String WholeMessage {
			get { return _wholeMessage; }
		}

		public RuntimeErrorEventArgs(int time, int errorNum, int errorParam, string message, FSRegister errorTexts) {
			_time = time;
			_codedErrorNum = errorNum;
			_errorParam = errorParam;
			_userMessage = message;
			_errorText = String.Empty;
			if (!errorTexts.IsNull) {
				FSRegKey key = errorTexts.FindKey(errorNum.ToString(CultureInfo.InvariantCulture));
				if (!key.IsNull) {
					key.Pos = 0;
					_errorText = key.StringRead();
				}
			}

			_group = errorNum >> 16;
			_errorNum = errorNum & 0xFFFF;
			_wholeMessage = String.Format("{0,10} : {1:X}.{2,-3} : {5,-80} : {3,8} : {4}\n", time, _group, _errorNum, errorParam, message, _errorText);
		}
	}
}

