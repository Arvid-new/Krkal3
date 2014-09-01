using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Compiler;
using System.Globalization;

namespace Krkal.Ide
{
	public partial class ErrorList : WeifenLuo.WinFormsUI.Docking.DockContent
	{
		MainForm _myForm;

		public ErrorList(MainForm myForm) {
			InitializeComponent();
			_myForm = myForm;
		}

		public void ClearSyntax() {
			listView1.Items.Clear();
			SetCounters(0,0);
		}

		private void SetCounters(int errorCount,int warningCount)
		{
			errorLabel.Text = String.Format(CultureInfo.CurrentCulture, "{0} Errors", errorCount);
			warningLabel.Text = String.Format(CultureInfo.CurrentCulture, "{0} Warnings", warningCount);
		}

		public void UpdateSyntax(ErrorLog errorLog) {
			if (errorLog == null) {
				ClearSyntax();
				return;
			}

			listView1.BeginUpdate();
			ClearSyntax();
			SetCounters(errorLog.ErrorCount, errorLog.WarningCount);


			foreach (ErrorDescription description in errorLog.Errors) {
				ListViewItem item = new ListViewItem("", (int)description.ErrorType);

				String num = description.ErrorType.ToString()[0] + ((int)description.ErrorCode).ToString("000", CultureInfo.CurrentCulture);
				item.SubItems.Add(num);
				item.SubItems.Add(description.Message);
				item.SubItems.Add(description.File);
				String line = description.PositionInLines != null ? ((description.PositionInLines.FirstRow + 1).ToString(CultureInfo.CurrentCulture)) : "";
				item.SubItems.Add(line);
				item.Tag = description;

				listView1.Items.Add(item);
			}

			listView1.EndUpdate();
		}


		private void listView1_DoubleClick(object sender, EventArgs e) {
			if (listView1.SelectedItems.Count == 1) {
				ErrorDescription error = (ErrorDescription)listView1.SelectedItems[0].Tag;
				if (error.File == "")
					return;
				KrkalCodeDocument doc = _myForm.OpenFile(error.File);
				if (doc != null && error.PositionInLines != null) {
					doc.SelectText(error.PositionInLines);
				}
			}
		}
	}
}

