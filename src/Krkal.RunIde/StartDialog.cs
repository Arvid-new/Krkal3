using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Krkal.Runtime;
using Krkal.Compiler;
using System.Globalization;
using Krkal.FileSystem;
using System.IO;

namespace Krkal.RunIde
{
	public partial class StartDialog : Form
	{

		MainConfiguration _mainConfiguration;
		DataSource _dataSource;
		RootNames _rootNames;
		StartInformation _startInformation;

		bool _languageWasSelected;
		bool _gameWasSelected;
		int _selectedGameIndex;

		public StartDialog(MainConfiguration mainConfiguration) {
			_mainConfiguration = mainConfiguration;
			_mainConfiguration.StartAction = StartAction.None;
			_rootNames = new RootNames(true);
			_dataSource = new DataSource(_rootNames.GetKernel(), _mainConfiguration.KrkalApplication.DataEnvironment);
			_startInformation = new StartInformation(_dataSource);
			InitializeComponent();
			Initialize2();
		}

		private void Initialize2() {
			this.SuspendLayout();
			IKrkalResourceManager rm = _mainConfiguration.KrkalApplication.ResourceManager;

			languageLabel.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_LanguageLabel", languageLabel.Text);
			userLabel.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_UserLabel", userLabel.Text);
			newProfileButton.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_NewUserLabel", newProfileButton.Text);
			authorLabel.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_AuthorLabel", authorLabel.Text);
			webPageLabel.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_WebPageLabel", webPageLabel.Text);
			startGameButton.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_StartGameButton", startGameButton.Text);
			runideButton.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_RunIdeButton", runideButton.Text);
			setupButton.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_SetupButton", setupButton.Text);
			exitButton.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_ExitButton", exitButton.Text);
			dontShowAgainCheckBox.Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_DontShowAgain", dontShowAgainCheckBox.Text);
			Text = rm.GetText("_KSID_RunIde__M_StartDialog__M_Caption", Text);

			languageCombo.Items.Clear();
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.LoadLanguagesFromConfigurations) != 0) {

				foreach (KsidName name in _startInformation.Languages) {
					languageCombo.Items.Add(_dataSource.Kernel.ReadUserName(name));
					if (_mainConfiguration.PrimaryLanguage == name.Identifier.LastPart.Name)
						languageCombo.SelectedIndex = languageCombo.Items.Count - 1;
				}

			} else {
				languageCombo.Enabled = false;
			}

			profileCombo.Items.Clear();
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.LoadProfileFromConfigurations) != 0) {

				foreach (String profile in _startInformation.Profiles) {
					AddProfile(profile);
				}


			} else {
				profileCombo.Enabled = false;
				newProfileButton.Enabled = false;
			}

			ShowGames();

			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.AllowHideOption) == 0)
				dontShowAgainCheckBox.Visible = false;
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.AllowIde) == 0)
				runideButton.Visible = false;
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.AllowPluginsSetup) == 0)
				setupButton.Visible = false;

			dontShowAgainCheckBox.Checked = _mainConfiguration.DontShowStartDialog;

			this.ResumeLayout(false);
			this.PerformLayout();
		}



		private void ShowGames() {
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.LoadGameFromConfigurations) == 0) {
				gameList.Enabled = false;
				ShowGameDescription(_mainConfiguration.KrkalApplication.GameInfo);
			} else {
				ShowGameDescription(null);
				gameList.Items.Clear();
				foreach (GameInfo game in _startInformation.Games) {
					String icon = GetIcon(game.Icon);
					gameList.Items.Add(_dataSource.Kernel.ReadUserName(game.Name), icon);
					if (_mainConfiguration.Game == game.Name) {
						gameList.SelectedIndices.Clear();
						gameList.SelectedIndices.Add(gameList.Items.Count - 1);
						ShowGameDescription(game);
					}
				}
			}
		}

		private void ShowGameDescription(GameInfo game) {
			if (game == null) {
				gameImage.Visible = false;
				gameNameLabel.Visible = false;
				authorLabel.Visible = false;
				authorLabel2.Visible = false;
				webPageLabel.Visible = false;
				webPageLabel2.Visible = false;
				gameDescriptionBox.Visible = false;
			} else {
				gameImage.Image = gameIcons.Images[GetIcon(game.Icon)];
				gameImage.Visible = true;
				gameNameLabel.Text = _dataSource.Kernel.ReadUserName(game.Name);
				gameNameLabel.Visible = true;

				if (String.IsNullOrEmpty(game.Author)) {
					authorLabel.Visible = false;
					authorLabel2.Visible = false;
				} else {
					authorLabel2.Text = game.Author;
					authorLabel.Visible = true;
					authorLabel2.Visible = true;
				}

				if (String.IsNullOrEmpty(game.WebPage)) {
					webPageLabel.Visible = false;
					webPageLabel2.Visible = false;
				} else {
					webPageLabel2.Text = game.WebPage;
					webPageLabel.Visible = true;
					webPageLabel2.Visible = true;
				}

				String description = _dataSource.Kernel.ReadComment(game.Name);
				if (String.IsNullOrEmpty(description)) {
					gameDescriptionBox.Visible = false;
				} else {
					gameDescriptionBox.Text = description;
					gameDescriptionBox.Visible = true;
				}

			}
		}



		private string GetIcon(KsidName ksidName) {
			if (ksidName == null)
				return "defaultIcon";
			String key = ksidName.Identifier.ToKsidString();
			if (gameIcons.Images.ContainsKey(key))
				return key;

			String fileName = _dataSource.Kernel.ReadFileAttribute(ksidName);
			if (!String.IsNullOrEmpty(fileName)) {
				int size = FS.FileSystem.GetFileSize(fileName);
				if (size > 0) {
					byte[] buffer = new byte[size];
					if (FS.FileSystem.ReadFile(fileName, buffer) == 1) {
						using(MemoryStream stream = new MemoryStream(buffer)) {
							try {
								if (fileName.EndsWith(".ico", true, CultureInfo.CurrentCulture)) {
									gameIcons.Images.Add(key, new Icon(stream));
								} else {
									gameIcons.Images.Add(key, Image.FromStream(stream));
								}
								return key;
							}
							catch (Exception) { }
						}
					}
				}
			}

			return "defaultIcon";
		}




		private void AddProfile(string profile) {
			if (profile.EndsWith(".user", true, CultureInfo.CurrentCulture)) {
				profileCombo.Items.Add(profile.Substring(0, profile.Length - 5));
			} else {
				profileCombo.Items.Add(profile);
			}
			if (_mainConfiguration.Profile == profile)
				profileCombo.SelectedIndex = profileCombo.Items.Count - 1;
		}




		private void StartDialog_FormClosed(object sender, FormClosedEventArgs e) {
			if (_dataSource != null)
				_dataSource.Dispose();
			_dataSource = null;
			if (_rootNames != null)
				_rootNames.Dispose();
			_rootNames = null;
		}


		private void languageCombo_SelectedIndexChanged(object sender, EventArgs e) {
			if (languageCombo.SelectedIndex == -1)
				return;
			String language = _startInformation.Languages[languageCombo.SelectedIndex].Identifier.LastPart.Name;
			if (language != _mainConfiguration.PrimaryLanguage) {
				_languageWasSelected = true;
				_mainConfiguration.PrimaryLanguage = language;
				_mainConfiguration.ApplyLanguages();
				Initialize2();
			}
		}

		private void profileCombo_SelectedIndexChanged(object sender, EventArgs e) {
			if (profileCombo.SelectedIndex == -1)
				return;
			String profile = _startInformation.Profiles[profileCombo.SelectedIndex];
			if (profile != _mainConfiguration.Profile) {
				_mainConfiguration.ChangeProfile(profile);
				if (_languageWasSelected) {
					_mainConfiguration.PrimaryLanguage = _startInformation.Languages[languageCombo.SelectedIndex].Identifier.LastPart.Name;
				} else {
					_mainConfiguration.ApplyLanguages();
				}
				if (_gameWasSelected)
					_mainConfiguration.Game = _startInformation.Games[_selectedGameIndex].Name;
				Initialize2();
			}

		}

		private void newProfileButton_Click(object sender, EventArgs e) {
			using (NewUserDialog dlg = new NewUserDialog(_mainConfiguration)) {
				if (dlg.ShowDialog() == DialogResult.OK) {
					_startInformation.AddProfile(dlg.Profile);
					_mainConfiguration.CreateProfile(dlg.Profile);
					AddProfile(dlg.Profile);
				}				
			}
		}

		private void exitButton_Click(object sender, EventArgs e) {
			_mainConfiguration.StartAction = StartAction.Exit;
			Close();
		}

		private void sutupButton_Click(object sender, EventArgs e) {
			_mainConfiguration.StartAction = StartAction.StartPluginsSetup;
			_mainConfiguration.KrkalApplication.WantRestart = true;
			Close();
		}

		private void runideButton_Click(object sender, EventArgs e) {
			_mainConfiguration.StartAction = StartAction.StartIde;
			Close();
		}

		private void startGameButton_Click(object sender, EventArgs e) {
			if (_mainConfiguration.Game != null || _mainConfiguration.KrkalApplication.GameInfo != null) {
				_mainConfiguration.StartAction = StartAction.StartGame;
				Close();
			}
		}

		private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.AllowInternetLinks) != 0) {
				try {
					System.Diagnostics.Process.Start(@"http://www.krkal.org");
				}
				catch (System.ComponentModel.Win32Exception) { }
			}
		}

		private void webPageLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
			if ((_mainConfiguration.KrkalApplication.Behavior & KrkalAppBehavior.AllowInternetLinks) != 0) {
				try {
					String link = webPageLabel2.Text;
					if (!String.IsNullOrEmpty(link)) {
						if (!link.StartsWith("http", true, CultureInfo.CurrentCulture))
							link = "http://" + link;
						System.Diagnostics.Process.Start(link);
					}
				}
				catch (System.ComponentModel.Win32Exception) { }
			}

		}

		private void dontShowAgainCheckBox_CheckedChanged(object sender, EventArgs e) {
			_mainConfiguration.DontShowStartDialog = dontShowAgainCheckBox.Checked;
		}


		private void gameList_SelectedIndexChanged(object sender, EventArgs e) {
			if (gameList.SelectedIndices.Count > 0) {
				_selectedGameIndex = gameList.SelectedIndices[0];
				if (_mainConfiguration.Game != _startInformation.Games[_selectedGameIndex].Name) {
					_mainConfiguration.Game = _startInformation.Games[_selectedGameIndex].Name;
					_gameWasSelected = true;
					ShowGameDescription(_startInformation.Games[_selectedGameIndex]);
				}
			}
		}

		private void gameList_ItemActivate(object sender, EventArgs e) {
			startGameButton_Click(sender, e);
		}
	}
}
