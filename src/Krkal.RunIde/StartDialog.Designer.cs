namespace Krkal.RunIde
{
	partial class StartDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartDialog));
			this.newProfileButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.languageLabel = new System.Windows.Forms.Label();
			this.userLabel = new System.Windows.Forms.Label();
			this.languageCombo = new System.Windows.Forms.ComboBox();
			this.profileCombo = new System.Windows.Forms.ComboBox();
			this.gameList = new System.Windows.Forms.ListView();
			this.gameIcons = new System.Windows.Forms.ImageList(this.components);
			this.gameNameLabel = new System.Windows.Forms.Label();
			this.authorLabel = new System.Windows.Forms.Label();
			this.webPageLabel = new System.Windows.Forms.Label();
			this.gameDescriptionBox = new System.Windows.Forms.TextBox();
			this.authorLabel2 = new System.Windows.Forms.Label();
			this.webPageLabel2 = new System.Windows.Forms.LinkLabel();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.dontShowAgainCheckBox = new System.Windows.Forms.CheckBox();
			this.exitButton = new System.Windows.Forms.Button();
			this.runideButton = new System.Windows.Forms.Button();
			this.startGameButton = new System.Windows.Forms.Button();
			this.setupButton = new System.Windows.Forms.Button();
			this.gameImage = new System.Windows.Forms.PictureBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.gameImage)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// newProfileButton
			// 
			this.newProfileButton.Location = new System.Drawing.Point(343, 214);
			this.newProfileButton.Name = "newProfileButton";
			this.newProfileButton.Size = new System.Drawing.Size(86, 23);
			this.newProfileButton.TabIndex = 6;
			this.newProfileButton.Text = "New User";
			this.newProfileButton.UseVisualStyleBackColor = true;
			this.newProfileButton.Click += new System.EventHandler(this.newProfileButton_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label1.Location = new System.Drawing.Point(130, 126);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(181, 20);
			this.label1.TabIndex = 0;
			this.label1.Text = "... play with objects ...";
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.linkLabel1.Location = new System.Drawing.Point(173, 146);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(94, 17);
			this.linkLabel1.TabIndex = 1;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "www.krkal.org";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// languageLabel
			// 
			this.languageLabel.AutoSize = true;
			this.languageLabel.Location = new System.Drawing.Point(12, 195);
			this.languageLabel.Name = "languageLabel";
			this.languageLabel.Size = new System.Drawing.Size(58, 13);
			this.languageLabel.TabIndex = 2;
			this.languageLabel.Text = "Language:";
			// 
			// userLabel
			// 
			this.userLabel.AutoSize = true;
			this.userLabel.Location = new System.Drawing.Point(12, 219);
			this.userLabel.Name = "userLabel";
			this.userLabel.Size = new System.Drawing.Size(64, 13);
			this.userLabel.TabIndex = 4;
			this.userLabel.Text = "User Profile:";
			// 
			// languageCombo
			// 
			this.languageCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.languageCombo.FormattingEnabled = true;
			this.languageCombo.Location = new System.Drawing.Point(121, 192);
			this.languageCombo.Name = "languageCombo";
			this.languageCombo.Size = new System.Drawing.Size(190, 21);
			this.languageCombo.TabIndex = 3;
			this.languageCombo.SelectedIndexChanged += new System.EventHandler(this.languageCombo_SelectedIndexChanged);
			// 
			// profileCombo
			// 
			this.profileCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.profileCombo.FormattingEnabled = true;
			this.profileCombo.Location = new System.Drawing.Point(121, 216);
			this.profileCombo.Name = "profileCombo";
			this.profileCombo.Size = new System.Drawing.Size(190, 21);
			this.profileCombo.TabIndex = 5;
			this.profileCombo.SelectedIndexChanged += new System.EventHandler(this.profileCombo_SelectedIndexChanged);
			// 
			// gameList
			// 
			this.gameList.Alignment = System.Windows.Forms.ListViewAlignment.Left;
			this.gameList.LargeImageList = this.gameIcons;
			this.gameList.Location = new System.Drawing.Point(15, 250);
			this.gameList.MultiSelect = false;
			this.gameList.Name = "gameList";
			this.gameList.Size = new System.Drawing.Size(414, 77);
			this.gameList.TabIndex = 7;
			this.gameList.UseCompatibleStateImageBehavior = false;
			this.gameList.ItemActivate += new System.EventHandler(this.gameList_ItemActivate);
			this.gameList.SelectedIndexChanged += new System.EventHandler(this.gameList_SelectedIndexChanged);
			// 
			// gameIcons
			// 
			this.gameIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("gameIcons.ImageStream")));
			this.gameIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.gameIcons.Images.SetKeyName(0, "defaultIcon");
			// 
			// gameNameLabel
			// 
			this.gameNameLabel.AutoSize = true;
			this.gameNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.gameNameLabel.Location = new System.Drawing.Point(116, 351);
			this.gameNameLabel.Name = "gameNameLabel";
			this.gameNameLabel.Size = new System.Drawing.Size(67, 26);
			this.gameNameLabel.TabIndex = 8;
			this.gameNameLabel.Text = "Krkal";
			// 
			// authorLabel
			// 
			this.authorLabel.AutoSize = true;
			this.authorLabel.Location = new System.Drawing.Point(12, 387);
			this.authorLabel.Name = "authorLabel";
			this.authorLabel.Size = new System.Drawing.Size(41, 13);
			this.authorLabel.TabIndex = 9;
			this.authorLabel.Text = "Author:";
			// 
			// webPageLabel
			// 
			this.webPageLabel.AutoSize = true;
			this.webPageLabel.Location = new System.Drawing.Point(12, 406);
			this.webPageLabel.Name = "webPageLabel";
			this.webPageLabel.Size = new System.Drawing.Size(61, 13);
			this.webPageLabel.TabIndex = 11;
			this.webPageLabel.Text = "Web Page:";
			// 
			// gameDescriptionBox
			// 
			this.gameDescriptionBox.BackColor = System.Drawing.SystemColors.Control;
			this.gameDescriptionBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.gameDescriptionBox.Location = new System.Drawing.Point(15, 431);
			this.gameDescriptionBox.Multiline = true;
			this.gameDescriptionBox.Name = "gameDescriptionBox";
			this.gameDescriptionBox.ReadOnly = true;
			this.gameDescriptionBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.gameDescriptionBox.Size = new System.Drawing.Size(414, 67);
			this.gameDescriptionBox.TabIndex = 13;
			this.gameDescriptionBox.Text = resources.GetString("gameDescriptionBox.Text");
			// 
			// authorLabel2
			// 
			this.authorLabel2.AutoSize = true;
			this.authorLabel2.Location = new System.Drawing.Point(118, 387);
			this.authorLabel2.Name = "authorLabel2";
			this.authorLabel2.Size = new System.Drawing.Size(24, 13);
			this.authorLabel2.TabIndex = 10;
			this.authorLabel2.Text = "MD";
			// 
			// webPageLabel2
			// 
			this.webPageLabel2.AutoSize = true;
			this.webPageLabel2.Location = new System.Drawing.Point(118, 406);
			this.webPageLabel2.Name = "webPageLabel2";
			this.webPageLabel2.Size = new System.Drawing.Size(75, 13);
			this.webPageLabel2.TabIndex = 12;
			this.webPageLabel2.TabStop = true;
			this.webPageLabel2.Text = "www.krkal.org";
			this.webPageLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.webPageLabel2_LinkClicked);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "install.ico");
			this.imageList1.Images.SetKeyName(1, "document.ico");
			this.imageList1.Images.SetKeyName(2, "gamecontroller.ico");
			this.imageList1.Images.SetKeyName(3, "delete.ico");
			// 
			// dontShowAgainCheckBox
			// 
			this.dontShowAgainCheckBox.AutoSize = true;
			this.dontShowAgainCheckBox.Location = new System.Drawing.Point(15, 568);
			this.dontShowAgainCheckBox.Name = "dontShowAgainCheckBox";
			this.dontShowAgainCheckBox.Size = new System.Drawing.Size(161, 17);
			this.dontShowAgainCheckBox.TabIndex = 18;
			this.dontShowAgainCheckBox.Text = "Don\'t show this dialog again.";
			this.dontShowAgainCheckBox.UseVisualStyleBackColor = true;
			this.dontShowAgainCheckBox.CheckedChanged += new System.EventHandler(this.dontShowAgainCheckBox_CheckedChanged);
			// 
			// exitButton
			// 
			this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.exitButton.ImageIndex = 3;
			this.exitButton.ImageList = this.imageList1;
			this.exitButton.Location = new System.Drawing.Point(333, 513);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(96, 42);
			this.exitButton.TabIndex = 17;
			this.exitButton.Text = "Exit";
			this.exitButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.exitButton.UseVisualStyleBackColor = true;
			this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
			// 
			// runideButton
			// 
			this.runideButton.ImageIndex = 1;
			this.runideButton.ImageList = this.imageList1;
			this.runideButton.Location = new System.Drawing.Point(121, 513);
			this.runideButton.Name = "runideButton";
			this.runideButton.Size = new System.Drawing.Size(96, 42);
			this.runideButton.TabIndex = 15;
			this.runideButton.Text = "Make Games";
			this.runideButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.runideButton.UseVisualStyleBackColor = true;
			this.runideButton.Click += new System.EventHandler(this.runideButton_Click);
			// 
			// startGameButton
			// 
			this.startGameButton.ImageIndex = 2;
			this.startGameButton.ImageList = this.imageList1;
			this.startGameButton.Location = new System.Drawing.Point(15, 513);
			this.startGameButton.Name = "startGameButton";
			this.startGameButton.Size = new System.Drawing.Size(96, 42);
			this.startGameButton.TabIndex = 14;
			this.startGameButton.Text = "Start Game";
			this.startGameButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.startGameButton.UseVisualStyleBackColor = true;
			this.startGameButton.Click += new System.EventHandler(this.startGameButton_Click);
			// 
			// setupButton
			// 
			this.setupButton.ImageIndex = 0;
			this.setupButton.ImageList = this.imageList1;
			this.setupButton.Location = new System.Drawing.Point(227, 513);
			this.setupButton.Name = "setupButton";
			this.setupButton.Size = new System.Drawing.Size(96, 42);
			this.setupButton.TabIndex = 16;
			this.setupButton.Text = "Install Uninstall";
			this.setupButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.setupButton.UseVisualStyleBackColor = true;
			this.setupButton.Click += new System.EventHandler(this.sutupButton_Click);
			// 
			// gameImage
			// 
			this.gameImage.Location = new System.Drawing.Point(15, 345);
			this.gameImage.Name = "gameImage";
			this.gameImage.Size = new System.Drawing.Size(32, 32);
			this.gameImage.TabIndex = 10;
			this.gameImage.TabStop = false;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = global::Krkal.RunIde.Properties.Resources.krkal2;
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(441, 120);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// StartDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.exitButton;
			this.ClientSize = new System.Drawing.Size(441, 596);
			this.Controls.Add(this.exitButton);
			this.Controls.Add(this.gameDescriptionBox);
			this.Controls.Add(this.dontShowAgainCheckBox);
			this.Controls.Add(this.runideButton);
			this.Controls.Add(this.startGameButton);
			this.Controls.Add(this.setupButton);
			this.Controls.Add(this.webPageLabel2);
			this.Controls.Add(this.authorLabel2);
			this.Controls.Add(this.webPageLabel);
			this.Controls.Add(this.authorLabel);
			this.Controls.Add(this.gameNameLabel);
			this.Controls.Add(this.gameImage);
			this.Controls.Add(this.gameList);
			this.Controls.Add(this.profileCombo);
			this.Controls.Add(this.languageCombo);
			this.Controls.Add(this.userLabel);
			this.Controls.Add(this.languageLabel);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.newProfileButton);
			this.Controls.Add(this.pictureBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StartDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Start Krkal";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartDialog_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.gameImage)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Button newProfileButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Label languageLabel;
		private System.Windows.Forms.Label userLabel;
		private System.Windows.Forms.ComboBox languageCombo;
		private System.Windows.Forms.ComboBox profileCombo;
		private System.Windows.Forms.ListView gameList;
		private System.Windows.Forms.PictureBox gameImage;
		private System.Windows.Forms.Label gameNameLabel;
		private System.Windows.Forms.Label authorLabel;
		private System.Windows.Forms.Label webPageLabel;
		private System.Windows.Forms.TextBox gameDescriptionBox;
		private System.Windows.Forms.Label authorLabel2;
		private System.Windows.Forms.LinkLabel webPageLabel2;
		private System.Windows.Forms.Button setupButton;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Button startGameButton;
		private System.Windows.Forms.Button runideButton;
		private System.Windows.Forms.CheckBox dontShowAgainCheckBox;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.ImageList gameIcons;
	}
}