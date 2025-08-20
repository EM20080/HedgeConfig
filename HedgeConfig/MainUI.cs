using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
namespace HedgeConfig
{
    public partial class MainUI : Form
    {
        private SplitContainer splitContainer;
        private TableLayoutPanel mainLayout;
        private Panel optionDetailPanel;
        private Label lblChoices;
        private ComboBox comboChoices;
        private TextBox txtFolderName;
        private Button btnAddOption;
        private Button btnRenameOption;
        private Button btnDeleteOption;
        private Button btnAddChoice;
        private Button btnRenameChoice;
        private Button btnDeleteChoice;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileMenuItem;
        private ToolStripMenuItem openConfigMenuItem;
        private ToolStripMenuItem selectModIniMenuItem;
        private ToolStripMenuItem exportConfigMenuItem;
        private ToolStripMenuItem settingsMenuItem;
        private ToolStripMenuItem themeMenuItem;
        private ToolStripMenuItem lightThemeMenuItem;
        private ToolStripMenuItem darkThemeMenuItem;

        private List<Option> options = new();
        private Dictionary<string, List<JsonEnumEntry>> enums = new();
        private string? selectedModIniPath = null;

        private FlowLayoutPanel previewPanel;

        private int selectedOptionIndex = -1;
        private bool isLightTheme = true;
        private CheckBox chkLoadRootFolder;  
        private bool loadRootFolder = false;

        public MainUI()
        {
            BuildUI();
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            SetTheme(false);
            UpdateOptionsList();
        }

        private void BuildUI()
        {
            Text = "HedgeConfig";
            ClientSize = new System.Drawing.Size(900, 500);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new System.Drawing.Size(700, 400);
            BackColor = System.Drawing.Color.White;
            menuStrip = new MenuStrip { BackColor = System.Drawing.Color.WhiteSmoke, ForeColor = System.Drawing.Color.Black };
            fileMenuItem = new ToolStripMenuItem("File");
            openConfigMenuItem = new ToolStripMenuItem("Open Existing Config");
            selectModIniMenuItem = new ToolStripMenuItem("Select mod ini");
            exportConfigMenuItem = new ToolStripMenuItem("Export Config");
            openConfigMenuItem.Click += OpenConfigMenuItem_Click;
            selectModIniMenuItem.Click += BtnSelectModIni_Click;
            exportConfigMenuItem.Click += BtnExportJson_Click;
            fileMenuItem.DropDownItems.Add(openConfigMenuItem);
            fileMenuItem.DropDownItems.Add(selectModIniMenuItem);
            fileMenuItem.DropDownItems.Add(exportConfigMenuItem);
            menuStrip.Items.Add(fileMenuItem);
            settingsMenuItem = new ToolStripMenuItem("Settings");
            themeMenuItem = new ToolStripMenuItem("Theme");
            lightThemeMenuItem = new ToolStripMenuItem("Light");
            darkThemeMenuItem = new ToolStripMenuItem("Dark");
            lightThemeMenuItem.Click += (s, e) => SetTheme(true);
            darkThemeMenuItem.Click += (s, e) => SetTheme(false);
            themeMenuItem.DropDownItems.Add(lightThemeMenuItem);
            themeMenuItem.DropDownItems.Add(darkThemeMenuItem);
            settingsMenuItem.DropDownItems.Add(themeMenuItem);
            menuStrip.Items.Add(settingsMenuItem);
            MainMenuStrip = menuStrip;
            menuStrip.Dock = DockStyle.Top;
            Controls.Add(menuStrip);
            splitContainer = new SplitContainer { Dock = DockStyle.Fill, SplitterDistance = 320, IsSplitterFixed = false, BorderStyle = BorderStyle.None, BackColor = System.Drawing.Color.White };
            splitContainer.Panel1.BackColor = System.Drawing.Color.White;
            splitContainer.Panel2.BackColor = System.Drawing.Color.White;
            Controls.Add(splitContainer);
            previewPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = System.Drawing.Color.White, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Padding = new Padding(10) };
            splitContainer.Panel1.Controls.Add(previewPanel);
            mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(16), AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = System.Drawing.Color.White };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            splitContainer.Panel2.Controls.Add(mainLayout);
            int row = 0;
            var lblOptionSettings = new Label { Text = "Option Settings", Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.Black, BackColor = System.Drawing.Color.White, AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top, Margin = new Padding(0, 8, 0, 0) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.Controls.Add(lblOptionSettings, 0, row);
            mainLayout.SetColumnSpan(lblOptionSettings, 2);
            row++;
            var separator = new Label { BorderStyle = BorderStyle.Fixed3D, Height = 2, Dock = DockStyle.Top, BackColor = System.Drawing.Color.FromArgb(200, 200, 200), Margin = new Padding(0, 2, 0, 8) };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));
            mainLayout.Controls.Add(separator, 0, row);
            mainLayout.SetColumnSpan(separator, 2);
            row++;
            var optionButtonStack = new TableLayoutPanel { ColumnCount = 1, RowCount = 3, Dock = DockStyle.Top, AutoSize = true, BackColor = System.Drawing.Color.White, Margin = new Padding(0, 0, 0, 12) };
            optionButtonStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            optionButtonStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            optionButtonStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            btnDeleteOption = new Button { Font = new System.Drawing.Font("Segoe UI", 10), Text = "Delete Option", Width = 120, Height = 32 };
            btnDeleteOption.Click += BtnDeleteOption_Click;
            btnRenameOption = new Button { Font = new System.Drawing.Font("Segoe UI", 10), Text = "Rename Option", Width = 120, Height = 32 };
            btnRenameOption.Click += BtnRenameOption_Click;
            btnAddOption = new Button { Font = new System.Drawing.Font("Segoe UI", 10), Text = "Add Option", Width = 120, Height = 32 };
            btnAddOption.Click += BtnAddOption_Click;
            optionButtonStack.Controls.Add(btnDeleteOption, 0, 0);
            optionButtonStack.Controls.Add(btnRenameOption, 0, 1);
            optionButtonStack.Controls.Add(btnAddOption, 0, 2);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.Controls.Add(optionButtonStack, 0, row);
            mainLayout.SetColumnSpan(optionButtonStack, 2);
            row++;
            var choiceOptionsGroup = new GroupBox { Text = "Choice Options", Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold), ForeColor = System.Drawing.Color.Black, BackColor = System.Drawing.Color.White, Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10), Margin = new Padding(0, 12, 0, 0), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            var choiceOptionsLayout = new TableLayoutPanel { ColumnCount = 2, Dock = DockStyle.Fill, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, BackColor = System.Drawing.Color.White };
            choiceOptionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            choiceOptionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            choiceOptionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            choiceOptionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            choiceOptionsLayout.Controls.Add(new Label { Text = "Choices:", Anchor = AnchorStyles.Right, AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10), ForeColor = System.Drawing.Color.Black, BackColor = System.Drawing.Color.White }, 0, 0);
            comboChoices = new ComboBox { Dock = DockStyle.Fill, Font = new System.Drawing.Font("Segoe UI", 10) };
            comboChoices.SelectedIndexChanged += ListBoxChoices_SelectedIndexChanged;
            choiceOptionsLayout.Controls.Add(comboChoices, 1, 0);
            choiceOptionsLayout.Controls.Add(new Label { Text = "Folder Name:", Anchor = AnchorStyles.Right, AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10), ForeColor = System.Drawing.Color.Black, BackColor = System.Drawing.Color.White }, 0, 1);
            txtFolderName = new TextBox { Dock = DockStyle.Fill, Font = new System.Drawing.Font("Segoe UI", 10), PlaceholderText = "Type the Folder name this option will use." };
            txtFolderName.TextChanged += TxtFolderName_TextChanged;
            choiceOptionsLayout.Controls.Add(txtFolderName, 1, 1);
            var choiceButtonStack = new TableLayoutPanel { ColumnCount = 1, RowCount = 3, Dock = DockStyle.Top, AutoSize = true, BackColor = System.Drawing.Color.White, Margin = new Padding(0, 8, 0, 0) };
            choiceButtonStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            choiceButtonStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            choiceButtonStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            btnDeleteChoice = new Button { Font = new System.Drawing.Font("Segoe UI", 10), Text = "Delete Choice", Width = 120, Height = 32, Dock = DockStyle.Top };
            btnDeleteChoice.Click += BtnDeleteChoice_Click;
            btnRenameChoice = new Button { Font = new System.Drawing.Font("Segoe UI", 10), Text = "Rename Choice", Width = 120, Height = 32, Dock = DockStyle.Top };
            btnRenameChoice.Click += BtnRenameChoice_Click;
            btnAddChoice = new Button { Font = new System.Drawing.Font("Segoe UI", 10), Text = "Add Choice", Width = 120, Height = 32, Dock = DockStyle.Top };
            btnAddChoice.Click += BtnAddChoice_Click;
            choiceButtonStack.Controls.Add(btnDeleteChoice, 0, 0);
            choiceButtonStack.Controls.Add(btnRenameChoice, 0, 1);
            choiceButtonStack.Controls.Add(btnAddChoice, 0, 2);
            choiceOptionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
            choiceOptionsLayout.Controls.Add(choiceButtonStack, 0, 2);
            choiceOptionsLayout.SetColumnSpan(choiceButtonStack, 2);
            choiceOptionsGroup.Controls.Add(choiceOptionsLayout);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.Controls.Add(choiceOptionsGroup, 0, row);
            mainLayout.SetColumnSpan(choiceOptionsGroup, 2);
            row++;
            chkLoadRootFolder = new CheckBox { Text = "Load Root folder with options", AutoSize = true, Font = new System.Drawing.Font("Segoe UI", 10), ForeColor = System.Drawing.Color.Black, BackColor = System.Drawing.Color.White, Dock = DockStyle.Top, Margin = new Padding(0, 8, 0, 8) };
            chkLoadRootFolder.CheckedChanged += (s, e) => { loadRootFolder = chkLoadRootFolder.Checked; };
            mainLayout.Controls.Add(chkLoadRootFolder, 0, row);
            mainLayout.SetColumnSpan(chkLoadRootFolder, 2);
            foreach (Control ctrl in mainLayout.Controls)
            {
                if (ctrl is Label ll) { ll.ForeColor = System.Drawing.Color.Black; ll.BackColor = System.Drawing.Color.White; }
                else if (ctrl is TextBox tb) { tb.BackColor = System.Drawing.Color.White; tb.ForeColor = System.Drawing.Color.Black; tb.BorderStyle = BorderStyle.FixedSingle; }
                else if (ctrl is ComboBox cb) { cb.BackColor = System.Drawing.Color.White; cb.ForeColor = System.Drawing.Color.Black; cb.FlatStyle = FlatStyle.Flat; }
                else if (ctrl is Button bt) { bt.BackColor = System.Drawing.Color.Gainsboro; bt.ForeColor = System.Drawing.Color.Black; bt.FlatStyle = FlatStyle.Flat; bt.FlatAppearance.BorderColor = System.Drawing.Color.Silver; }
            }
        }

        private void UpdateOptionsList()
        {
            previewPanel.Controls.Clear();
            comboChoices.Items.Clear();
            txtFolderName.Text = "";
            if (options.Count > 0)
            {
                if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count)
                    selectedOptionIndex = 0;
                UpdateChoicesList();
            }
            else
            {
                selectedOptionIndex = -1;
                comboChoices.Items.Clear();
                txtFolderName.Text = "";
            }
            UpdatePreviewPanel();
        }

        private void UpdateChoicesList()
        {
            comboChoices.Items.Clear();
            txtFolderName.Text = "";
            if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count)
            {
                comboChoices.Items.Add("No choices available");
                comboChoices.SelectedIndex = 0;
                comboChoices.Enabled = false;
                return;
            }
            foreach (var choice in options[selectedOptionIndex].Choices)
                comboChoices.Items.Add(choice.Name);
            if (options[selectedOptionIndex].Choices.Count > 0)
            {
                comboChoices.SelectedIndex = 0;
                comboChoices.Enabled = true;
            }
            else
            {
                comboChoices.Items.Add("No choices available");
                comboChoices.SelectedIndex = 0;
                comboChoices.Enabled = false;
            }
        }

        private void ListBoxChoices_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int choiceIndex = comboChoices.SelectedIndex;
            if (selectedOptionIndex < 0 || choiceIndex < 0 || choiceIndex >= options[selectedOptionIndex].Choices.Count)
            {
                txtFolderName.Text = "";
                txtFolderName.Enabled = false;
                return;
            }

            txtFolderName.Enabled = true;
            txtFolderName.Text = options[selectedOptionIndex].Choices[choiceIndex].Folder;
        }

        private void TxtFolderName_TextChanged(object? sender, EventArgs e)
        {
            int choiceIndex = comboChoices.SelectedIndex;
            if (selectedOptionIndex < 0 || choiceIndex < 0) return;

            var folderName = txtFolderName.Text.Replace("/", "").Replace("\\", "");

            options[selectedOptionIndex].Choices[choiceIndex].Folder = folderName;
        }

        private void BtnAddOption_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedModIniPath))
            {
                MessageBox.Show("You must select a mod.ini file before creating a config option.", "Select mod.ini", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string? name = Prompt.ShowDialog("Enter new option name:", "Add Option");
            if (string.IsNullOrWhiteSpace(name)) return;

            int includeDirIndex = loadRootFolder ? options.Count + 1 : options.Count;

            var newJsonElement = new JsonElement
            {
                Name = $"IncludeDir{includeDirIndex}",
                DisplayName = name.Trim(),
                Description = new List<string> { $"Auto-generated option for {name.Trim()}" },
                Type = name.Trim().Replace(" ", ""),
                DefaultValue = null,
                MinValue = null,
                MaxValue = null,
                EnumName = null
            };

            options.Add(new Option { Name = name.Trim(), Choices = new List<Choice>(), OriginalJsonElement = newJsonElement });
            selectedOptionIndex = options.Count - 1;
            UpdateOptionsList();
            Functions.UpdateEnums(options, enums);
        }

        private void BtnRenameOption_Click(object? sender, EventArgs e)
        {
            if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count) return;

            string? newName = Prompt.ShowDialog("Rename option:", "Rename Option", options[selectedOptionIndex].Name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            options[selectedOptionIndex].Name = newName.Trim();
            if (options[selectedOptionIndex].OriginalJsonElement != null)
            {
                options[selectedOptionIndex].OriginalJsonElement.DisplayName = newName.Trim();
                options[selectedOptionIndex].OriginalJsonElement.Name = newName.Trim().Replace(" ", "");
                options[selectedOptionIndex].OriginalJsonElement.Type = newName.Trim().Replace(" ", "");
                if (options[selectedOptionIndex].OriginalJsonElement.EnumName != null)
                {
                    options[selectedOptionIndex].OriginalJsonElement.EnumName = newName.Trim().Replace(" ", "");
                }
            }

            UpdateOptionsList();
        }

        private void BtnDeleteOption_Click(object? sender, EventArgs e)
        {
            if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count) return;

            options.RemoveAt(selectedOptionIndex);
            if (selectedOptionIndex >= options.Count)
                selectedOptionIndex = options.Count - 1;
            UpdateOptionsList();
            Functions.UpdateEnums(options, enums);
        }

        private void BtnAddChoice_Click(object? sender, EventArgs e)
        {
            if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count) return;

            string? name = Prompt.ShowDialog("Enter new choice name:", "Add Choice");
            if (string.IsNullOrWhiteSpace(name)) return;

            options[selectedOptionIndex].Choices.Add(new Choice { Name = name.Trim(), Folder = "" });
            UpdateChoicesList();
            Functions.UpdateEnums(options, enums);
            comboChoices.SelectedIndex = options[selectedOptionIndex].Choices.Count - 1;
            UpdatePreviewPanel();
        }

        private void BtnRenameChoice_Click(object? sender, EventArgs e)
        {
            if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count) return;

            int choiceIdx = comboChoices.SelectedIndex;
            if (choiceIdx < 0) return;

            string? newName = Prompt.ShowDialog("Rename choice:", "Rename Choice", options[selectedOptionIndex].Choices[choiceIdx].Name);
            if (string.IsNullOrWhiteSpace(newName)) return;

            options[selectedOptionIndex].Choices[choiceIdx].Name = newName.Trim();
            UpdateChoicesList();
            comboChoices.SelectedIndex = choiceIdx;
            UpdatePreviewPanel();
        }

        private void BtnDeleteChoice_Click(object? sender, EventArgs e)
        {
            if (selectedOptionIndex < 0 || selectedOptionIndex >= options.Count) return;

            int choiceIdx = comboChoices.SelectedIndex;
            if (choiceIdx < 0) return;

            options[selectedOptionIndex].Choices.RemoveAt(choiceIdx);
            UpdateChoicesList();
            Functions.UpdateEnums(options, enums);
            UpdatePreviewPanel();
        }

        private void BtnSelectModIni_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "INI Files|mod.ini";
            ofd.Title = "Select Mod ini";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                selectedModIniPath = ofd.FileName;
                MessageBox.Show($"Selected mod.ini: {selectedModIniPath}", "mod.ini Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnExportJson_Click(object? sender, EventArgs e)
        {
            if (options.Count == 0)
            {
                MessageBox.Show("Add at least one option before exporting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(selectedModIniPath))
            {
                MessageBox.Show("Select a mod.ini file before exporting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var elements = new List<JsonElement>();
            foreach (var opt in options)
            {
                if (opt.OriginalJsonElement == null)
                {
                    opt.OriginalJsonElement = new JsonElement
                    {
                        Name = opt.Name.Replace(" ", ""),
                        DisplayName = opt.Name,
                        Description = new List<string> { "" },
                        Type = opt.Name.Replace(" ", "")
                    };
                }
                var el = opt.OriginalJsonElement;
                if (opt.Choices.Any())
                {
                    string enumKey = opt.Name.Replace(" ", "");
                    el.EnumName = enumKey;
                    el.Type = enumKey;
                    var first = opt.Choices.First();
                    string def = first.Folder;
                    if (!string.IsNullOrEmpty(def))
                    {
                        def = def.Replace("\\", "/");
                        if (!def.EndsWith("/")) def += "/";
                        def = "./" + def;
                    }
                    else def = "./";
                    el.DefaultValue = def;
                }
                else
                {
                    el.EnumName = null;
                    el.DefaultValue = "./"; // root
                }
                elements.Add(el);
            }
            Functions.UpdateEnums(options, enums);

            try
            {
                var iniLines = new List<string>();
                if (File.Exists(selectedModIniPath!))
                {
                    var existingLines = File.ReadAllLines(selectedModIniPath!);
                    foreach (var line in existingLines)
                        if (!line.TrimStart().StartsWith("IncludeDir", StringComparison.OrdinalIgnoreCase) &&
                            !line.TrimStart().StartsWith("IncludeDirCount", StringComparison.OrdinalIgnoreCase))
                            iniLines.Add(line);
                }

                if (loadRootFolder)
                    iniLines.Add("IncludeDir0=\".\"");

                for (int i = 0; i < options.Count; i++)
                {
                    int includeDirIndex = loadRootFolder ? i + 1 : i;
                    string folder = options[i].Choices.Count > 0 ? options[i].Choices[0].Folder.Trim() : "";
                    string includeValue;
                    if (string.IsNullOrEmpty(folder))
                    {
                        includeValue = ".";
                    }
                    else
                    {
                        folder = folder.Replace("\\", "/");
                        if (folder.StartsWith("./")) folder = folder[2..];
                        if (!folder.EndsWith("/")) folder += "/";
                        includeValue = "./" + folder;
                    }
                    iniLines.Add($"IncludeDir{includeDirIndex}=\"{includeValue}\"");
                }

                int includeDirCount = loadRootFolder ? options.Count + 1 : options.Count; // What this does is it loads the root folder by making it so that instead of starting off with includedir0 it makes the mod start as 1, then it writes A dummy IncludeDir0 with a ".", so that way it loads the root folder whilst loading the other options if existant.
                iniLines.Add($"IncludeDirCount={includeDirCount}");
                File.WriteAllLines(selectedModIniPath!, iniLines);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error writing mod.ini: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var exportObj = new RootObject
            {
                Groups = new List<Group>
                {
                    new Group
                    {
                        Name = "Main",
                        DisplayName = "Options",
                        Elements = elements
                    }
                },
                Enums = enums,
                IniFile = "mod.ini"
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            string json = JsonSerializer.Serialize(exportObj, jsonOptions);

            using var sfd = new SaveFileDialog();
            sfd.Filter = "JSON Files|*.json";
            sfd.FileName = "Config.json";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(sfd.FileName, json);
                    MessageBox.Show("Exported Config successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error writing file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenConfigMenuItem_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            ofd.Filter = "HMM Config|*.json";
            ofd.Title = "Open Config JSON";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = File.ReadAllText(ofd.FileName);
                    var jsonOptions = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    };
                    var config = JsonSerializer.Deserialize<RootObject>(json, jsonOptions);
                    if (config != null && config.Groups != null && config.Groups.Count > 0)
                    {
                        options.Clear();
                        enums = config.Enums ?? new Dictionary<string, List<JsonEnumEntry>>();
                        foreach (var group in config.Groups)
                        {
                            var elements = group.Elements ?? new List<JsonElement>();
                            foreach (var el in elements)
                            {
                                var opt = new Option
                                {
                                    Name = el.DisplayName ?? el.Name ?? "",
                                    Choices = new List<Choice>(),
                                    OriginalJsonElement = el
                                };
                                string enumKey = el.EnumName ?? el.Type ?? "";
                                if (!string.IsNullOrEmpty(enumKey) && enums.ContainsKey(enumKey))
                                {
                                    foreach (var enumEntry in enums[enumKey])
                                    {
                                        string folder = enumEntry.Value ?? "";
                                        if (folder.StartsWith("./")) folder = folder.Substring(2);
                                        if (folder.EndsWith("/")) folder = folder.Substring(0, folder.Length - 1);
                                        opt.Choices.Add(new Choice { Name = enumEntry.DisplayName ?? "", Folder = folder });
                                    }
                                }
                                options.Add(opt);
                            }
                        }
                        UpdateOptionsList();
                        MessageBox.Show("Config imported successfully.", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Invalid config file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error importing config: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeComponent()
        {

        }

        private void SetTheme(bool light)
        {
            isLightTheme = light;
            if (light)
            {
                BackColor = System.Drawing.Color.White;
                menuStrip.BackColor = System.Drawing.Color.WhiteSmoke;
                menuStrip.ForeColor = System.Drawing.Color.Black;
                menuStrip.Renderer = new ToolStripProfessionalRenderer();
                foreach (ToolStripMenuItem item in menuStrip.Items) { item.ForeColor = System.Drawing.Color.Black; item.BackColor = System.Drawing.Color.WhiteSmoke; }
                splitContainer.BackColor = System.Drawing.Color.White;
                splitContainer.Panel1.BackColor = System.Drawing.Color.White;
                splitContainer.Panel2.BackColor = System.Drawing.Color.White;
                mainLayout.BackColor = System.Drawing.Color.White;
                previewPanel.BackColor = System.Drawing.Color.White;
                ApplyThemeToControls(mainLayout, true);
                ApplyThemeToControls(previewPanel, true);
                chkLoadRootFolder.BackColor = System.Drawing.Color.White;
                chkLoadRootFolder.ForeColor = System.Drawing.Color.Black;
                SetWindowDarkMode(false);
            }
            else
            {
                var darkBack = System.Drawing.Color.FromArgb(32, 32, 32);
                var darkPanel = System.Drawing.Color.FromArgb(40, 40, 40);
                var darkText = System.Drawing.Color.White;
                menuStrip.BackColor = darkPanel;
                menuStrip.ForeColor = darkText;
                menuStrip.Renderer = new DarkMenuRenderer();
                foreach (ToolStripMenuItem item in menuStrip.Items) { item.ForeColor = darkText; item.BackColor = darkPanel; }
                BackColor = darkBack;
                splitContainer.BackColor = darkBack;
                splitContainer.Panel1.BackColor = darkBack;
                splitContainer.Panel2.BackColor = darkBack;
                mainLayout.BackColor = darkBack;
                previewPanel.BackColor = darkBack;
                ApplyThemeToControls(mainLayout, false);
                ApplyThemeToControls(previewPanel, false);
                chkLoadRootFolder.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
                chkLoadRootFolder.ForeColor = System.Drawing.Color.White;
                SetWindowDarkMode(true);
            }
            UpdatePreviewPanel();
        }

        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkMenuColors()) { }
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e) { e.TextColor = System.Drawing.Color.White; base.OnRenderItemText(e); }
        }
        private class DarkMenuColors : ProfessionalColorTable
        {
            public override System.Drawing.Color MenuItemSelected => System.Drawing.Color.FromArgb(60, 60, 60);
            public override System.Drawing.Color MenuItemBorder => System.Drawing.Color.FromArgb(80, 80, 80);
            public override System.Drawing.Color ToolStripDropDownBackground => System.Drawing.Color.FromArgb(32, 32, 32);
            public override System.Drawing.Color ImageMarginGradientBegin => System.Drawing.Color.FromArgb(32, 32, 32);
            public override System.Drawing.Color ImageMarginGradientMiddle => System.Drawing.Color.FromArgb(32, 32, 32);
            public override System.Drawing.Color ImageMarginGradientEnd => System.Drawing.Color.FromArgb(32, 32, 32);
            public override System.Drawing.Color MenuBorder => System.Drawing.Color.FromArgb(80, 80, 80);
            public override System.Drawing.Color MenuItemPressedGradientBegin => System.Drawing.Color.FromArgb(60, 60, 60);
            public override System.Drawing.Color MenuItemPressedGradientEnd => System.Drawing.Color.FromArgb(60, 60, 60);
            public override System.Drawing.Color MenuItemSelectedGradientBegin => System.Drawing.Color.FromArgb(60, 60, 60);
            public override System.Drawing.Color MenuItemSelectedGradientEnd => System.Drawing.Color.FromArgb(60, 60, 60);
            public override System.Drawing.Color SeparatorDark => System.Drawing.Color.FromArgb(80, 80, 80);
            public override System.Drawing.Color SeparatorLight => System.Drawing.Color.FromArgb(80, 80, 80);
        }

        private void UpdatePreviewPanel()
        {
            previewPanel.SuspendLayout();
            previewPanel.Controls.Clear();
            var selectedPanelColor = isLightTheme ? System.Drawing.Color.FromArgb(220, 235, 255) : System.Drawing.Color.FromArgb(50, 60, 80);
            var panelColor = isLightTheme ? System.Drawing.Color.WhiteSmoke : System.Drawing.Color.FromArgb(40, 40, 40);
            var labelColor = isLightTheme ? System.Drawing.Color.Black : System.Drawing.Color.White;
            var comboBackColor = isLightTheme ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(55, 55, 55);
            var comboForeColor = isLightTheme ? System.Drawing.Color.Black : System.Drawing.Color.White;
            for (int i = 0; i < options.Count; i++)
            {
                int optionIdx = i;
                var opt = options[i];
                var optPanel = new Panel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Margin = new Padding(0, 0, 0, 16), BackColor = (optionIdx == selectedOptionIndex) ? selectedPanelColor : panelColor, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(12, 10, 12, 10), MinimumSize = new System.Drawing.Size(previewPanel.ClientSize.Width - 40, 80), Cursor = Cursors.Hand };
                optPanel.Click += (s, e) => { selectedOptionIndex = optionIdx; UpdateChoicesList(); UpdatePreviewPanel(); };
                var headerPanel = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = System.Drawing.Color.Transparent };
                var optLabel = new Label { Text = opt.Name, Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold), Location = new System.Drawing.Point(10, 4), AutoSize = true, ForeColor = labelColor, BackColor = System.Drawing.Color.Transparent, Cursor = Cursors.Hand };
                optLabel.Click += (s, e) => { string? newName = Prompt.ShowDialog("Rename option:", "Rename Option", opt.Name); if (!string.IsNullOrWhiteSpace(newName)) { opt.Name = newName.Trim(); if (opt.OriginalJsonElement != null) { opt.OriginalJsonElement.DisplayName = newName.Trim(); opt.OriginalJsonElement.Name = newName.Trim().Replace(" ", ""); opt.OriginalJsonElement.Type = newName.Trim().Replace(" ", ""); if (opt.OriginalJsonElement.EnumName != null) opt.OriginalJsonElement.EnumName = newName.Trim().Replace(" ", ""); } UpdateOptionsList(); } };
                headerPanel.Controls.Add(optLabel);
                optPanel.Controls.Add(headerPanel);
                var choiceCombo = new ComboBox { Location = new System.Drawing.Point(10, 38), Width = Math.Max(220, previewPanel.ClientSize.Width - 80), Font = new System.Drawing.Font("Segoe UI", 11), Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, BackColor = comboBackColor, ForeColor = comboForeColor, FlatStyle = FlatStyle.Flat };
                if (opt.Choices.Count > 0) { foreach (var c in opt.Choices) choiceCombo.Items.Add(c.Name); choiceCombo.SelectedIndex = 0; choiceCombo.Enabled = true; } else { choiceCombo.Items.Add("No choices available"); choiceCombo.SelectedIndex = 0; choiceCombo.Enabled = false; }
                choiceCombo.SelectedIndexChanged += (s, e) => { if (optionIdx == selectedOptionIndex && choiceCombo.SelectedIndex >= 0 && choiceCombo.SelectedIndex < opt.Choices.Count) { comboChoices.SelectedIndex = choiceCombo.SelectedIndex; txtFolderName.Text = opt.Choices[choiceCombo.SelectedIndex].Folder; } };
                choiceCombo.MouseDoubleClick += (s, e) => { int idx = choiceCombo.SelectedIndex; if (idx >= 0 && idx < opt.Choices.Count) { string? newName = Prompt.ShowDialog("Rename choice:", "Rename Choice", opt.Choices[idx].Name); if (!string.IsNullOrWhiteSpace(newName)) { opt.Choices[idx].Name = newName.Trim(); UpdateOptionsList(); } } };
                optPanel.Controls.Add(choiceCombo);
                previewPanel.Controls.Add(optPanel);
            }
            previewPanel.ResumeLayout();
        }

        private void SetWindowDarkMode(bool enable)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                var attr = 19;
                int useDark = enable ? 1 : 0;
                DwmSetWindowAttribute(Handle, attr, ref useDark, sizeof(int));
            }
        }

        private void ApplyThemeToControls(Control parent, bool light)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is GroupBox gb)
                {
                    gb.BackColor = light ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(40, 40, 40);
                    gb.ForeColor = light ? System.Drawing.Color.Black : System.Drawing.Color.White;
                }
                else if (ctrl is Label lbl)
                {
                    lbl.BackColor = light ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(40, 40, 40);
                    lbl.ForeColor = light ? System.Drawing.Color.Black : System.Drawing.Color.White;
                }
                else if (ctrl is TextBox txt)
                {
                    txt.BackColor = light ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(55, 55, 55);
                    txt.ForeColor = light ? System.Drawing.Color.Black : System.Drawing.Color.White;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (ctrl is ComboBox cmb)
                {
                    cmb.BackColor = light ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(55, 55, 55);
                    cmb.ForeColor = light ? System.Drawing.Color.Black : System.Drawing.Color.White;
                    cmb.FlatStyle = FlatStyle.Flat;
                }
                else if (ctrl is Button btn)
                {
                    btn.BackColor = light ? System.Drawing.Color.Gainsboro : System.Drawing.Color.FromArgb(55, 55, 55);
                    btn.ForeColor = light ? System.Drawing.Color.Black : System.Drawing.Color.White;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderColor = light ? System.Drawing.Color.Silver : System.Drawing.Color.FromArgb(80, 80, 80);
                }
                else if (ctrl is Panel pnl)
                {
                    pnl.BackColor = light ? System.Drawing.Color.White : System.Drawing.Color.FromArgb(32, 32, 32);
                }
                if (ctrl.HasChildren) ApplyThemeToControls(ctrl, light);
            }
        }

        private static class Prompt
        {
            public static string? ShowDialog(string text, string caption, string defaultValue = "")
            {
                using Form prompt = new() { Width = 400, Height = 150, Text = caption, StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false };
                Label textLabel = new() { Left = 10, Top = 10, Text = text, AutoSize = true };
                TextBox textBox = new() { Left = 10, Top = 35, Width = 360, Text = defaultValue };
                Button confirmation = new() { Text = "Ok", Left = 290, Width = 80, Top = 70, DialogResult = DialogResult.OK };
                confirmation.Click += (sender, e) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;
                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : null;
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    }
}