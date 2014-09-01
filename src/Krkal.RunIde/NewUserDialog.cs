using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.FileSystem;

namespace Krkal.RunIde
{
	public partial class NewUserDialog : Form
	{
		MainConfiguration _mainConfiguration;

		public NewUserDialog(MainConfiguration mainConfiguration) {
			_mainConfiguration = mainConfiguration;
			InitializeComponent();
			label1.Text = mainConfiguration.KrkalApplication.ResourceManager.GetText("_KSID_RunIde__M_NewUserDialog__M_Label", label1.Text);
			okButton.Text = mainConfiguration.KrkalApplication.OkText;
			cancelButton.Text = mainConfiguration.KrkalApplication.CancelText;
			Text = mainConfiguration.KrkalApplication.Caption;	
		}

		public String Profile {
			get { return textBox1.Text + ".user"; }
		}

		private void okButton_Click(object sender, EventArgs e) {
			if (!String.IsNullOrEmpty(textBox1.Text)) {
				if (_mainConfiguration.KrkalApplication.ExistsProfile(Profile)) {
					_mainConfiguration.KrkalApplication.ShowErrorMessage("_KSID_RunIde__M_NewUserDialog__M_ProfileExists:Profile of this name already exists. Please choose different name.");
				} else {
					try {
						_mainConfiguration.KrkalApplication.Profile = Profile;
						if (_mainConfiguration.KrkalApplication.Profile == Profile) {
							DialogResult = DialogResult.OK;
							return;
						}

					}
					catch (FSFileNotFoundException) { }
					catch (ArgumentException) { }
					_mainConfiguration.KrkalApplication.ShowErrorMessage("_KSID_RunIde__M_NewUserDialog__M_UnableToCreateProfile:Unable to create profile. Please check that the name uses only characters valid for file names.");
				}
			}
		}
	}
}
