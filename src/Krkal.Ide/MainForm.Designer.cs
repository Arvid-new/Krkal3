namespace Krkal.Ide
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.mainMenu = new System.Windows.Forms.MenuStrip();
			this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemNew = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.openRegisterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemClose = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemCloseAll = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemCloseAllButThisOne = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemSave = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSaveAll = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemExit = new System.Windows.Forms.ToolStripMenuItem();
			this.exitWithoutSavingLayout = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemView = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemSolutionExplorer = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemTaskList = new System.Windows.Forms.ToolStripMenuItem();
			this.classViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nameViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItemToolBar = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemStatusBar = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemProject = new System.Windows.Forms.ToolStripMenuItem();
			this.chooseProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.compileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.rebuildAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.compileInBackgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemHelp = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
			this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.statusBar = new System.Windows.Forms.StatusStrip();
			this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
			this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.toolBar = new System.Windows.Forms.ToolStrip();
			this.toolBarButtonNew = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolBarButtonOpen = new System.Windows.Forms.ToolStripButton();
			this.toolStripButtonOpenRegister = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
			this.toolBarButtonSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolBarButtonSolutionExplorer = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
			this.toolBarButtonTaskList = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
			this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
			this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
			this.backgroundCompilation = new System.ComponentModel.BackgroundWorker();
			this.backgroundCompilationTimer = new System.Windows.Forms.Timer(this.components);
			this.mainMenu.SuspendLayout();
			this.statusBar.SuspendLayout();
			this.toolBar.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFile,
            this.menuItemView,
            this.menuItemProject,
            this.testToolStripMenuItem,
            this.configurationToolStripMenuItem,
            this.menuItemHelp});
			this.mainMenu.Location = new System.Drawing.Point(0, 0);
			this.mainMenu.Name = "mainMenu";
			this.mainMenu.Size = new System.Drawing.Size(579, 24);
			this.mainMenu.TabIndex = 7;
			// 
			// menuItemFile
			// 
			this.menuItemFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemNew,
            this.menuItemOpen,
            this.openRegisterToolStripMenuItem,
            this.menuItemClose,
            this.menuItemCloseAll,
            this.menuItemCloseAllButThisOne,
            this.menuItem4,
            this.menuItemSave,
            this.menuItemSaveAll,
            this.menuItemSaveAs,
            this.toolStripSeparator2,
            this.menuItemExit,
            this.exitWithoutSavingLayout});
			this.menuItemFile.Name = "menuItemFile";
			this.menuItemFile.Size = new System.Drawing.Size(35, 20);
			this.menuItemFile.Text = "&File";
			this.menuItemFile.DropDownOpening += new System.EventHandler(this.menuItemFile_Popup);
			// 
			// menuItemNew
			// 
			this.menuItemNew.Image = ((System.Drawing.Image)(resources.GetObject("menuItemNew.Image")));
			this.menuItemNew.Name = "menuItemNew";
			this.menuItemNew.Size = new System.Drawing.Size(215, 22);
			this.menuItemNew.Text = "&New";
			// 
			// menuItemOpen
			// 
			this.menuItemOpen.Image = ((System.Drawing.Image)(resources.GetObject("menuItemOpen.Image")));
			this.menuItemOpen.Name = "menuItemOpen";
			this.menuItemOpen.Size = new System.Drawing.Size(215, 22);
			this.menuItemOpen.Text = "&Open...";
			this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
			// 
			// openRegisterToolStripMenuItem
			// 
			this.openRegisterToolStripMenuItem.Name = "openRegisterToolStripMenuItem";
			this.openRegisterToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
			this.openRegisterToolStripMenuItem.Text = "Open &Register...";
			this.openRegisterToolStripMenuItem.Click += new System.EventHandler(this.toolStripButtonOpenRegister_Click);
			// 
			// menuItemClose
			// 
			this.menuItemClose.Name = "menuItemClose";
			this.menuItemClose.Size = new System.Drawing.Size(215, 22);
			this.menuItemClose.Text = "&Close";
			this.menuItemClose.Click += new System.EventHandler(this.menuItemClose_Click);
			// 
			// menuItemCloseAll
			// 
			this.menuItemCloseAll.Name = "menuItemCloseAll";
			this.menuItemCloseAll.Size = new System.Drawing.Size(215, 22);
			this.menuItemCloseAll.Text = "Close &All";
			this.menuItemCloseAll.Click += new System.EventHandler(this.menuItemCloseAll_Click);
			// 
			// menuItemCloseAllButThisOne
			// 
			this.menuItemCloseAllButThisOne.Name = "menuItemCloseAllButThisOne";
			this.menuItemCloseAllButThisOne.Size = new System.Drawing.Size(215, 22);
			this.menuItemCloseAllButThisOne.Text = "Close All &But This One";
			this.menuItemCloseAllButThisOne.Click += new System.EventHandler(this.menuItemCloseAllButThisOne_Click);
			// 
			// menuItem4
			// 
			this.menuItem4.Name = "menuItem4";
			this.menuItem4.Size = new System.Drawing.Size(212, 6);
			// 
			// menuItemSave
			// 
			this.menuItemSave.Image = ((System.Drawing.Image)(resources.GetObject("menuItemSave.Image")));
			this.menuItemSave.Name = "menuItemSave";
			this.menuItemSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.menuItemSave.Size = new System.Drawing.Size(215, 22);
			this.menuItemSave.Text = "Save";
			this.menuItemSave.Click += new System.EventHandler(this.menuItemSave_Click);
			// 
			// menuItemSaveAll
			// 
			this.menuItemSaveAll.Image = ((System.Drawing.Image)(resources.GetObject("menuItemSaveAll.Image")));
			this.menuItemSaveAll.Name = "menuItemSaveAll";
			this.menuItemSaveAll.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
						| System.Windows.Forms.Keys.S)));
			this.menuItemSaveAll.Size = new System.Drawing.Size(215, 22);
			this.menuItemSaveAll.Text = "Save All";
			this.menuItemSaveAll.Click += new System.EventHandler(this.menuItemSaveAll_Click);
			// 
			// menuItemSaveAs
			// 
			this.menuItemSaveAs.Name = "menuItemSaveAs";
			this.menuItemSaveAs.Size = new System.Drawing.Size(215, 22);
			this.menuItemSaveAs.Text = "Save As...";
			this.menuItemSaveAs.Click += new System.EventHandler(this.menuItemSaveAs_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(212, 6);
			// 
			// menuItemExit
			// 
			this.menuItemExit.Name = "menuItemExit";
			this.menuItemExit.Size = new System.Drawing.Size(215, 22);
			this.menuItemExit.Text = "&Exit";
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
			// 
			// exitWithoutSavingLayout
			// 
			this.exitWithoutSavingLayout.Name = "exitWithoutSavingLayout";
			this.exitWithoutSavingLayout.Size = new System.Drawing.Size(215, 22);
			this.exitWithoutSavingLayout.Text = "Exit &Without Saving Layout";
			this.exitWithoutSavingLayout.Click += new System.EventHandler(this.exitWithoutSavingLayout_Click);
			// 
			// menuItemView
			// 
			this.menuItemView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemSolutionExplorer,
            this.menuItemTaskList,
            this.classViewToolStripMenuItem,
            this.nameViewToolStripMenuItem,
            this.menuItem1,
            this.menuItemToolBar,
            this.menuItemStatusBar});
			this.menuItemView.MergeIndex = 1;
			this.menuItemView.Name = "menuItemView";
			this.menuItemView.Size = new System.Drawing.Size(41, 20);
			this.menuItemView.Text = "&View";
			// 
			// menuItemSolutionExplorer
			// 
			this.menuItemSolutionExplorer.Name = "menuItemSolutionExplorer";
			this.menuItemSolutionExplorer.Size = new System.Drawing.Size(166, 22);
			this.menuItemSolutionExplorer.Text = "&Solution Explorer";
			this.menuItemSolutionExplorer.Click += new System.EventHandler(this.menuItemSolutionExplorer_Click);
			// 
			// menuItemTaskList
			// 
			this.menuItemTaskList.Name = "menuItemTaskList";
			this.menuItemTaskList.Size = new System.Drawing.Size(166, 22);
			this.menuItemTaskList.Text = "&Error List";
			this.menuItemTaskList.Click += new System.EventHandler(this.menuItemErrorListWindow_Click);
			// 
			// classViewToolStripMenuItem
			// 
			this.classViewToolStripMenuItem.Name = "classViewToolStripMenuItem";
			this.classViewToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
			this.classViewToolStripMenuItem.Text = "&Class View";
			this.classViewToolStripMenuItem.Click += new System.EventHandler(this.classViewToolStripMenuItem_Click);
			// 
			// nameViewToolStripMenuItem
			// 
			this.nameViewToolStripMenuItem.Name = "nameViewToolStripMenuItem";
			this.nameViewToolStripMenuItem.Size = new System.Drawing.Size(166, 22);
			this.nameViewToolStripMenuItem.Text = "&Name View";
			this.nameViewToolStripMenuItem.Click += new System.EventHandler(this.nameViewToolStripMenuItem_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Name = "menuItem1";
			this.menuItem1.Size = new System.Drawing.Size(163, 6);
			// 
			// menuItemToolBar
			// 
			this.menuItemToolBar.Checked = true;
			this.menuItemToolBar.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItemToolBar.Name = "menuItemToolBar";
			this.menuItemToolBar.Size = new System.Drawing.Size(166, 22);
			this.menuItemToolBar.Text = "Tool &Bar";
			this.menuItemToolBar.Click += new System.EventHandler(this.menuItemToolBar_Click);
			// 
			// menuItemStatusBar
			// 
			this.menuItemStatusBar.Checked = true;
			this.menuItemStatusBar.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItemStatusBar.Name = "menuItemStatusBar";
			this.menuItemStatusBar.Size = new System.Drawing.Size(166, 22);
			this.menuItemStatusBar.Text = "Status B&ar";
			this.menuItemStatusBar.Click += new System.EventHandler(this.menuItemStatusBar_Click);
			// 
			// menuItemProject
			// 
			this.menuItemProject.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.chooseProjectToolStripMenuItem,
            this.compileToolStripMenuItem,
            this.rebuildAllToolStripMenuItem});
			this.menuItemProject.Name = "menuItemProject";
			this.menuItemProject.Size = new System.Drawing.Size(53, 20);
			this.menuItemProject.Text = "&Project";
			// 
			// chooseProjectToolStripMenuItem
			// 
			this.chooseProjectToolStripMenuItem.Image = global::Krkal.Ide.Properties.Resources.openfolderHS;
			this.chooseProjectToolStripMenuItem.Name = "chooseProjectToolStripMenuItem";
			this.chooseProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.chooseProjectToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.chooseProjectToolStripMenuItem.Text = "Ch&oose project...";
			this.chooseProjectToolStripMenuItem.Click += new System.EventHandler(this.chooseProjectToolStripMenuItem_Click);
			// 
			// compileToolStripMenuItem
			// 
			this.compileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("compileToolStripMenuItem.Image")));
			this.compileToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.compileToolStripMenuItem.Name = "compileToolStripMenuItem";
			this.compileToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F7;
			this.compileToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.compileToolStripMenuItem.Text = "&Compile";
			this.compileToolStripMenuItem.Click += new System.EventHandler(this.compileToolStripMenuItem_Click);
			// 
			// rebuildAllToolStripMenuItem
			// 
			this.rebuildAllToolStripMenuItem.Name = "rebuildAllToolStripMenuItem";
			this.rebuildAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Alt)
						| System.Windows.Forms.Keys.F7)));
			this.rebuildAllToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
			this.rebuildAllToolStripMenuItem.Text = "Re&build All";
			this.rebuildAllToolStripMenuItem.Click += new System.EventHandler(this.rebuildAllToolStripMenuItem_Click);
			// 
			// testToolStripMenuItem
			// 
			this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1});
			this.testToolStripMenuItem.Name = "testToolStripMenuItem";
			this.testToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
			this.testToolStripMenuItem.Text = "Test";
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
			this.toolStripMenuItem1.Text = "Test Form";
			this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
			// 
			// configurationToolStripMenuItem
			// 
			this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compileInBackgroundToolStripMenuItem});
			this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
			this.configurationToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
			this.configurationToolStripMenuItem.Text = "&Configuration";
			// 
			// compileInBackgroundToolStripMenuItem
			// 
			this.compileInBackgroundToolStripMenuItem.Checked = true;
			this.compileInBackgroundToolStripMenuItem.CheckOnClick = true;
			this.compileInBackgroundToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.compileInBackgroundToolStripMenuItem.Name = "compileInBackgroundToolStripMenuItem";
			this.compileInBackgroundToolStripMenuItem.Size = new System.Drawing.Size(192, 22);
			this.compileInBackgroundToolStripMenuItem.Text = "Compile in Background";
			this.compileInBackgroundToolStripMenuItem.CheckStateChanged += new System.EventHandler(this.compileInBackgroundToolStripMenuItem_CheckStateChanged);
			// 
			// menuItemHelp
			// 
			this.menuItemHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemAbout});
			this.menuItemHelp.MergeIndex = 3;
			this.menuItemHelp.Name = "menuItemHelp";
			this.menuItemHelp.Size = new System.Drawing.Size(40, 20);
			this.menuItemHelp.Text = "&Help";
			// 
			// menuItemAbout
			// 
			this.menuItemAbout.Name = "menuItemAbout";
			this.menuItemAbout.Size = new System.Drawing.Size(152, 22);
			this.menuItemAbout.Text = "&About Krkal...";
			this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
			// 
			// BottomToolStripPanel
			// 
			this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.BottomToolStripPanel.Name = "BottomToolStripPanel";
			this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// statusBar
			// 
			this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
			this.statusBar.Location = new System.Drawing.Point(0, 387);
			this.statusBar.Name = "statusBar";
			this.statusBar.Size = new System.Drawing.Size(579, 22);
			this.statusBar.TabIndex = 4;
			// 
			// statusText
			// 
			this.statusText.AutoSize = false;
			this.statusText.Name = "statusText";
			this.statusText.Size = new System.Drawing.Size(200, 17);
			this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// TopToolStripPanel
			// 
			this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.TopToolStripPanel.Name = "TopToolStripPanel";
			this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// toolBar
			// 
			this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBarButtonNew,
            this.toolBarButtonOpen,
            this.toolStripButtonOpenRegister,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolBarButtonSeparator1,
            this.toolBarButtonSolutionExplorer,
            this.toolStripButton5,
            this.toolStripButton6,
            this.toolBarButtonTaskList,
            this.toolStripSeparator1,
            this.toolStripButton1,
            this.toolStripButton2});
			this.toolBar.Location = new System.Drawing.Point(0, 24);
			this.toolBar.Name = "toolBar";
			this.toolBar.Size = new System.Drawing.Size(579, 25);
			this.toolBar.TabIndex = 6;
			// 
			// toolBarButtonNew
			// 
			this.toolBarButtonNew.Image = global::Krkal.Ide.Properties.Resources.NewDocumentHS;
			this.toolBarButtonNew.Name = "toolBarButtonNew";
			this.toolBarButtonNew.Size = new System.Drawing.Size(29, 22);
			this.toolBarButtonNew.ToolTipText = "New";
			// 
			// toolBarButtonOpen
			// 
			this.toolBarButtonOpen.Image = ((System.Drawing.Image)(resources.GetObject("toolBarButtonOpen.Image")));
			this.toolBarButtonOpen.Name = "toolBarButtonOpen";
			this.toolBarButtonOpen.Size = new System.Drawing.Size(23, 22);
			this.toolBarButtonOpen.ToolTipText = "Open Text";
			this.toolBarButtonOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
			// 
			// toolStripButtonOpenRegister
			// 
			this.toolStripButtonOpenRegister.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButtonOpenRegister.Image = global::Krkal.Ide.Properties.Resources.VSProject_database;
			this.toolStripButtonOpenRegister.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButtonOpenRegister.Name = "toolStripButtonOpenRegister";
			this.toolStripButtonOpenRegister.Size = new System.Drawing.Size(23, 22);
			this.toolStripButtonOpenRegister.Text = "Open Register";
			this.toolStripButtonOpenRegister.ToolTipText = "Open Register";
			this.toolStripButtonOpenRegister.Click += new System.EventHandler(this.toolStripButtonOpenRegister_Click);
			// 
			// toolStripButton3
			// 
			this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
			this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton3.Name = "toolStripButton3";
			this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton3.Text = "Save";
			this.toolStripButton3.Click += new System.EventHandler(this.menuItemSave_Click);
			// 
			// toolStripButton4
			// 
			this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
			this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton4.Name = "toolStripButton4";
			this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton4.Text = "Save All";
			this.toolStripButton4.Click += new System.EventHandler(this.menuItemSaveAll_Click);
			// 
			// toolBarButtonSeparator1
			// 
			this.toolBarButtonSeparator1.Name = "toolBarButtonSeparator1";
			this.toolBarButtonSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolBarButtonSolutionExplorer
			// 
			this.toolBarButtonSolutionExplorer.Image = global::Krkal.Ide.Properties.Resources.VSProject_genericproject_small;
			this.toolBarButtonSolutionExplorer.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.toolBarButtonSolutionExplorer.Name = "toolBarButtonSolutionExplorer";
			this.toolBarButtonSolutionExplorer.Size = new System.Drawing.Size(23, 22);
			this.toolBarButtonSolutionExplorer.ToolTipText = "Solution Explorer";
			this.toolBarButtonSolutionExplorer.Click += new System.EventHandler(this.menuItemSolutionExplorer_Click);
			// 
			// toolStripButton5
			// 
			this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton5.Image = global::Krkal.Ide.Properties.Resources.VSObject_Class;
			this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton5.Name = "toolStripButton5";
			this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton5.Text = "Class View";
			this.toolStripButton5.Click += new System.EventHandler(this.classViewToolStripMenuItem_Click);
			// 
			// toolStripButton6
			// 
			this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton6.Image = global::Krkal.Ide.Properties.Resources.OrgChartHS;
			this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton6.Name = "toolStripButton6";
			this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton6.Text = "Name View";
			this.toolStripButton6.Click += new System.EventHandler(this.nameViewToolStripMenuItem_Click);
			// 
			// toolBarButtonTaskList
			// 
			this.toolBarButtonTaskList.Image = global::Krkal.Ide.Properties.Resources.CheckGrammarHS;
			this.toolBarButtonTaskList.Name = "toolBarButtonTaskList";
			this.toolBarButtonTaskList.Size = new System.Drawing.Size(23, 22);
			this.toolBarButtonTaskList.ToolTipText = "Error List";
			this.toolBarButtonTaskList.Click += new System.EventHandler(this.menuItemErrorListWindow_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripButton1
			// 
			this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton1.Image = global::Krkal.Ide.Properties.Resources.openfolderHS;
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton1.Text = "Choose Project";
			this.toolStripButton1.Click += new System.EventHandler(this.chooseProjectToolStripMenuItem_Click);
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.toolStripButton2.Image = global::Krkal.Ide.Properties.Resources.VSProject_generatedfile;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
			this.toolStripButton2.Text = "Compile";
			this.toolStripButton2.Click += new System.EventHandler(this.compileToolStripMenuItem_Click);
			// 
			// RightToolStripPanel
			// 
			this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.RightToolStripPanel.Name = "RightToolStripPanel";
			this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// LeftToolStripPanel
			// 
			this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
			this.LeftToolStripPanel.Name = "LeftToolStripPanel";
			this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
			this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
			// 
			// ContentPanel
			// 
			this.ContentPanel.Size = new System.Drawing.Size(579, 338);
			// 
			// dockPanel
			// 
			this.dockPanel.ActiveAutoHideContent = null;
			this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dockPanel.DockBottomPortion = 150;
			this.dockPanel.DockLeftPortion = 200;
			this.dockPanel.DockRightPortion = 200;
			this.dockPanel.DockTopPortion = 150;
			this.dockPanel.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
			this.dockPanel.Location = new System.Drawing.Point(0, 24);
			this.dockPanel.Name = "dockPanel";
			this.dockPanel.RightToLeftLayout = true;
			this.dockPanel.Size = new System.Drawing.Size(579, 385);
			this.dockPanel.TabIndex = 0;
			// 
			// backgroundCompilation
			// 
			this.backgroundCompilation.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundCompilation_DoWork);
			this.backgroundCompilation.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundCompilation_RunWorkerCompleted);
			// 
			// backgroundCompilationTimer
			// 
			this.backgroundCompilationTimer.Enabled = true;
			this.backgroundCompilationTimer.Interval = 1500;
			this.backgroundCompilationTimer.Tick += new System.EventHandler(this.backgroundCompilationTimer_Tick);
			// 
			// MainForm
			// 
			this.ClientSize = new System.Drawing.Size(579, 409);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.toolBar);
			this.Controls.Add(this.dockPanel);
			this.Controls.Add(this.mainMenu);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.IsMdiContainer = true;
			this.MainMenuStrip = this.mainMenu;
			this.Name = "MainForm";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.mainMenu.ResumeLayout(false);
			this.mainMenu.PerformLayout();
			this.statusBar.ResumeLayout(false);
			this.statusBar.PerformLayout();
			this.toolBar.ResumeLayout(false);
			this.toolBar.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		private System.Windows.Forms.ToolStripMenuItem menuItemFile;
        private System.Windows.Forms.ToolStripMenuItem menuItemNew;
        private System.Windows.Forms.ToolStripMenuItem menuItemOpen;
        private System.Windows.Forms.ToolStripMenuItem menuItemClose;
        private System.Windows.Forms.ToolStripMenuItem menuItemCloseAll;
        private System.Windows.Forms.ToolStripMenuItem menuItemCloseAllButThisOne;
        private System.Windows.Forms.ToolStripSeparator menuItem4;
        private System.Windows.Forms.ToolStripMenuItem menuItemExit;
        private System.Windows.Forms.ToolStripMenuItem menuItemView;
		private System.Windows.Forms.ToolStripMenuItem menuItemSolutionExplorer;
        private System.Windows.Forms.ToolStripMenuItem menuItemTaskList;
        private System.Windows.Forms.ToolStripSeparator menuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuItemToolBar;
		private System.Windows.Forms.ToolStripMenuItem menuItemStatusBar;
        private System.Windows.Forms.ToolStripMenuItem menuItemHelp;
		private System.Windows.Forms.ToolStripMenuItem menuItemAbout;
        private System.Windows.Forms.ToolStripMenuItem exitWithoutSavingLayout;
		private System.Windows.Forms.ToolStripMenuItem menuItemProject;
		private System.Windows.Forms.ToolStripMenuItem chooseProjectToolStripMenuItem;
		private System.Windows.Forms.ToolStripButton toolBarButtonOpen;
		private System.Windows.Forms.ToolStripSeparator toolBarButtonSeparator1;
		private System.Windows.Forms.ToolStripButton toolBarButtonSolutionExplorer;
		private System.Windows.Forms.ToolStripButton toolBarButtonTaskList;
		private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
		private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
		private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
		private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
		private System.Windows.Forms.ToolStripContentPanel ContentPanel;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripMenuItem compileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem menuItemSave;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveAll;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem classViewToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItemSaveAs;
		private System.Windows.Forms.ToolStripButton toolStripButton3;
		private System.Windows.Forms.ToolStripButton toolStripButton4;
		private System.Windows.Forms.ToolStripButton toolStripButton5;
		private System.Windows.Forms.ToolStripButton toolStripButton6;
		private System.Windows.Forms.ToolStripMenuItem nameViewToolStripMenuItem;
		private System.Windows.Forms.ToolStripDropDownButton toolBarButtonNew;
		protected System.Windows.Forms.StatusStrip statusBar;
		protected System.Windows.Forms.ToolStrip toolBar;
		protected WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
		protected System.Windows.Forms.MenuStrip mainMenu;
		internal System.ComponentModel.BackgroundWorker backgroundCompilation;
		private System.Windows.Forms.Timer backgroundCompilationTimer;
		private System.Windows.Forms.ToolStripMenuItem configurationToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem compileInBackgroundToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem rebuildAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripStatusLabel statusText;
		private System.Windows.Forms.ToolStripButton toolStripButtonOpenRegister;
		private System.Windows.Forms.ToolStripMenuItem openRegisterToolStripMenuItem;
    }
}