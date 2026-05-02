using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AITrainingStudio
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += delegate(object sender, System.Threading.ThreadExceptionEventArgs e)
            {
                LogStartupError(e.Exception);
                MessageBox.Show("AI Training Studio failed to start.\n\nA log file was created in the logs folder.\n\n" + e.Exception.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                LogStartupError(e.ExceptionObject as Exception);
            };

            try
            {
                LogStartupInfo();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                LogStartupError(ex);
                MessageBox.Show("AI Training Studio failed to start.\n\nA log file was created in the logs folder.\n\n" + ex.Message, "Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void LogStartupInfo()
        {
            string folder = GetLogFolder();
            string errorPath = Path.Combine(folder, "startup_error.log");
            if (File.Exists(errorPath))
            {
                File.Delete(errorPath);
            }
            string path = Path.Combine(folder, "startup.log");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("AI Training Studio startup");
            sb.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("App: " + Application.ExecutablePath);
            sb.AppendLine("OS: " + Environment.OSVersion.ToString());
            sb.AppendLine(".NET: " + Environment.Version.ToString());
            sb.AppendLine("WorkingDirectory: " + Environment.CurrentDirectory);
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }

        private static void LogStartupError(Exception ex)
        {
            try
            {
                string folder = GetLogFolder();
                string path = Path.Combine(folder, "startup_error.log");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("AI Training Studio startup error");
                sb.AppendLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine("App: " + Application.ExecutablePath);
                sb.AppendLine("OS: " + Environment.OSVersion.ToString());
                sb.AppendLine(".NET: " + Environment.Version.ToString());
                sb.AppendLine("WorkingDirectory: " + Environment.CurrentDirectory);
                sb.AppendLine();
                if (ex == null)
                {
                    sb.AppendLine("Unknown non-Exception crash.");
                }
                else
                {
                    sb.AppendLine(ex.ToString());
                }
                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
            }
            catch
            {
            }
        }

        private static string GetLogFolder()
        {
            string baseFolder = Path.GetDirectoryName(Application.ExecutablePath);
            if (String.IsNullOrEmpty(baseFolder))
            {
                baseFolder = Environment.CurrentDirectory;
            }
            string folder = Path.Combine(baseFolder, "logs");
            Directory.CreateDirectory(folder);
            return folder;
        }
    }

    public class MainForm : Form
    {
        private TextBox txtProjectName;
        private TextBox txtOutputFolder;
        private TextBox txtRequirement;
        private TextBox txtPersonality;
        private TextBox txtUrlSources;
        private TextBox txtSearchKeywords;
        private TextBox txtNotes;
        private TextBox txtHelp;
        private TextBox txtLog;
        private ComboBox cboMode;
        private ComboBox cboTarget;
        private ComboBox cboOutputKind;
        private ComboBox cboModelLevel;
        private ComboBox cboTrainingBackend;
        private ListBox lstData;
        private TrackBar barCreativity;
        private TrackBar barConsistency;
        private TrackBar barMemory;
        private TrackBar barReaction;
        private TrackBar barSafety;
        private TrackBar barPasses;
        private Label valCreativity;
        private Label valConsistency;
        private Label valMemory;
        private Label valReaction;
        private Label valSafety;
        private Label valPasses;
        private CheckBox chkTutorial;
        private CheckBox chkAdapters;
        private CheckBox chkHardware;
        private CheckBox chkResourcePlan;
        private CheckBox chkAutoFindResources;
        private CheckBox chkInferActionsFromVideo;
        private CheckBox chkTextApp;
        private CheckBox chkSafetyNotes;
        private ToolTip tips;
        private readonly List<string> dataPaths;
        private readonly Dictionary<string, bool> dataPathSet;

        public MainForm()
        {
            dataPaths = new List<string>();
            dataPathSet = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            tips = new ToolTip();
            tips.AutoPopDelay = 16000;
            tips.InitialDelay = 250;
            tips.ReshowDelay = 100;

            Text = "AI Training Studio - 新手 AI 訓練與整合工具";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 620);
            Size = new Size(980, 680);
            Font = new Font("Microsoft JhengHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            BuildUi();
            RefreshTargets();
            ShowHelp("先從左邊的「1. 專案」開始：取名字、選你要做 NPC、遊戲操作、文字 AI，然後照分頁往右走。這個工具會把需求、資料、參數與整合方式整理成可保存、可交給程式讀取的 AI 行為包。");
            Log("程式已啟動。請先建立專案，或直接使用預設設定。");
        }

        private void BuildUi()
        {
            TableLayoutPanel root = new TableLayoutPanel();
            root.Dock = DockStyle.Fill;
            root.RowCount = 2;
            root.ColumnCount = 1;
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            Controls.Add(root);

            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = Color.FromArgb(32, 42, 52);
            header.Padding = new Padding(18, 10, 18, 8);
            root.Controls.Add(header, 0, 0);

            Label title = new Label();
            title.Text = "AI Training Studio";
            title.ForeColor = Color.White;
            title.Font = new Font(Font.FontFamily, 20F, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(18, 8);
            header.Controls.Add(title);

            Label subtitle = new Label();
            subtitle.Text = "把需求、資料、參數整理成 AI 訓練包，並產生 NPC、文字 AI App、硬體控制橋接的整合教學。";
            subtitle.ForeColor = Color.FromArgb(220, 226, 232);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(22, 44);
            header.Controls.Add(subtitle);

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            root.Controls.Add(split, 0, 1);

            Load += delegate
            {
                int desiredPanel2 = 300;
                int minPanel1 = 540;
                if (split.Width > minPanel1 + desiredPanel2)
                {
                    split.SplitterDistance = split.Width - desiredPanel2;
                }
                split.Panel1MinSize = 240;
                split.Panel2MinSize = 180;
            };

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill;
            tabs.Padding = new Point(16, 6);
            split.Panel1.Controls.Add(tabs);

            tabs.TabPages.Add(CreateProjectTab());
            tabs.TabPages.Add(CreateRequirementTab());
            tabs.TabPages.Add(CreateParameterTab());
            tabs.TabPages.Add(CreateBuildTab());

            split.Panel2.Controls.Add(CreateRightPanel());
        }

        private Control CreateRightPanel()
        {
            TableLayoutPanel panel = new TableLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(12);
            panel.RowCount = 4;
            panel.ColumnCount = 1;
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 48F));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            panel.RowStyles.Add(new RowStyle(SizeType.Percent, 52F));

            Label helpTitle = new Label();
            helpTitle.Text = "白話說明";
            helpTitle.Dock = DockStyle.Fill;
            helpTitle.Font = new Font(Font, FontStyle.Bold);
            panel.Controls.Add(helpTitle, 0, 0);

            txtHelp = new TextBox();
            txtHelp.Dock = DockStyle.Fill;
            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.ScrollBars = ScrollBars.Vertical;
            txtHelp.BackColor = Color.FromArgb(250, 250, 250);
            panel.Controls.Add(txtHelp, 0, 1);

            Label logTitle = new Label();
            logTitle.Text = "執行紀錄";
            logTitle.Dock = DockStyle.Fill;
            logTitle.Font = new Font(Font, FontStyle.Bold);
            panel.Controls.Add(logTitle, 0, 2);

            txtLog = new TextBox();
            txtLog.Dock = DockStyle.Fill;
            txtLog.Multiline = true;
            txtLog.ReadOnly = true;
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.BackColor = Color.FromArgb(250, 250, 250);
            panel.Controls.Add(txtLog, 0, 3);

            return panel;
        }

        private TabPage CreateProjectTab()
        {
            TabPage tab = new TabPage("1. 專案");
            Panel panel = CreateScrollPanel();
            TableLayoutPanel table = CreateFormTable();
            panel.Controls.Add(table);
            tab.Controls.Add(panel);

            txtProjectName = CreateTextBox("我的AI專案");
            AddRow(table, "專案名稱", txtProjectName, "幫這次訓練取一個名字。產出的資料夾和訓練檔會用這個名字，方便你之後分辨版本。");

            cboMode = new ComboBox();
            cboMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMode.Items.Add("NPC 智慧角色");
            cboMode.Items.Add("遊戲操作 / 控制策略");
            cboMode.Items.Add("文字型 AI / App");
            cboMode.Items.Add("自訂流程自動化");
            cboMode.SelectedIndex = 0;
            cboMode.SelectedIndexChanged += delegate { RefreshTargets(); };
            AddRow(table, "你想做什麼", cboMode, "選擇目標類型。NPC 會產生人格、對話與遊戲引擎整合範本；遊戲操作會產生影像資料與手把動作的對應表；文字 AI 會產生可做成 App 的提示詞與設定包。");

            cboTarget = new ComboBox();
            cboTarget.DropDownStyle = ComboBoxStyle.DropDownList;
            AddRow(table, "要接到哪裡", cboTarget, "選擇訓練完成後要放進哪種環境。這不會破解封閉遊戲；它會產生對應的教學、設定檔與程式範本，讓你接到自己能合法修改或控制的地方。");

            cboOutputKind = new ComboBox();
            cboOutputKind.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOutputKind.Items.Add("新手完整包");
            cboOutputKind.Items.Add("只要訓練檔");
            cboOutputKind.Items.Add("整合給工程師使用");
            cboOutputKind.Items.Add("教學優先");
            cboOutputKind.SelectedIndex = 0;
            AddRow(table, "輸出方式", cboOutputKind, "新手完整包會包含訓練檔、操作對應表、教學文件和範例程式。只要訓練檔適合你已經知道怎麼接入的情況。");

            cboModelLevel = new ComboBox();
            cboModelLevel.DropDownStyle = ComboBoxStyle.DropDownList;
            cboModelLevel.Items.Add("入門：需求整理與行為包");
            cboModelLevel.Items.Add("進階：可接 API / 本機模型");
            cboModelLevel.Items.Add("完整：輸出訓練資料格式與整合檔");
            cboModelLevel.SelectedIndex = 0;
            AddRow(table, "模型等級", cboModelLevel, "這裡只決定輸出檔案的完整程度，不決定要不要 GPU。入門會產生行為包；進階會加上 API / 本機模型設定；完整會再加上資料格式、標註表與整合檔。要不要用 GPU 請到「訓練方式」選。");

            Panel outputPanel = new Panel();
            outputPanel.Dock = DockStyle.Fill;
            outputPanel.Height = 34;
            txtOutputFolder = CreateTextBox(DefaultOutputFolder());
            txtOutputFolder.Left = 0;
            txtOutputFolder.Top = 0;
            Button btnBrowse = new Button();
            btnBrowse.Text = "選擇...";
            btnBrowse.Width = 78;
            btnBrowse.Top = 0;
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Click += delegate { BrowseOutputFolder(); };
            outputPanel.Resize += delegate
            {
                btnBrowse.Left = Math.Max(0, outputPanel.ClientSize.Width - btnBrowse.Width);
                txtOutputFolder.Width = Math.Max(180, btnBrowse.Left - 8);
            };
            outputPanel.Controls.Add(txtOutputFolder);
            outputPanel.Controls.Add(btnBrowse);
            RegisterHelp(btnBrowse, "選擇訓練包要存放的位置。建議放在文件資料夾，之後比較容易找到。");
            AddRow(table, "輸出資料夾", outputPanel, "程式會在這裡建立專案資料夾。你可以把整包複製到遊戲專案、開發板專案或其他電腦。");

            txtNotes = CreateMultiLineTextBox(90);
            AddRow(table, "備註", txtNotes, "寫下版本、限制、你想提醒自己的事情。這些會寫進 model_card，避免之後忘記當初怎麼訓練。");

            return tab;
        }

        private TabPage CreateRequirementTab()
        {
            TabPage tab = new TabPage("2. 需求與資料");
            Panel panel = CreateScrollPanel();
            TableLayoutPanel table = CreateFormTable();
            panel.Controls.Add(table);
            tab.Controls.Add(panel);

            txtRequirement = CreateMultiLineTextBox(150);
            txtRequirement.Text = "例：我要一個遊戲 NPC，他是沉著、會記住玩家選擇、遇到危險會先保護村民，說話簡短但有個性。";
            AddRow(table, "你的需求", txtRequirement, "用白話描述你要 AI 做什麼。你可以寫人格、任務、限制、口吻、不能做的事、成功標準。寫得越具體，產出的行為包越好用。");

            txtPersonality = CreateMultiLineTextBox(110);
            txtPersonality.Text = "例：冷靜、可靠、重視承諾，不主動透露秘密。";
            AddRow(table, "人格 / 風格", txtPersonality, "如果是 NPC 或文字 AI，這裡寫它的個性、說話方式和世界觀。如果是遊戲操作 AI，這裡可寫操作風格，例如保守、防守、積極超車。");

            Panel dataPanel = new Panel();
            dataPanel.Dock = DockStyle.Fill;
            dataPanel.Height = 280;
            lstData = new ListBox();
            lstData.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            lstData.Width = 520;
            lstData.Height = 210;
            Button btnAddFiles = new Button();
            btnAddFiles.Text = "加入檔案";
            btnAddFiles.Top = 220;
            btnAddFiles.Width = 88;
            btnAddFiles.Click += delegate { AddFiles(); };
            Button btnAddFolder = new Button();
            btnAddFolder.Text = "加入資料夾";
            btnAddFolder.Top = 220;
            btnAddFolder.Left = 96;
            btnAddFolder.Width = 96;
            btnAddFolder.Click += delegate { AddFolder(); };
            Button btnRemove = new Button();
            btnRemove.Text = "移除選取";
            btnRemove.Top = 220;
            btnRemove.Left = 200;
            btnRemove.Width = 96;
            btnRemove.Click += delegate { RemoveSelectedData(); };
            Button btnClear = new Button();
            btnClear.Text = "清空";
            btnClear.Top = 220;
            btnClear.Left = 304;
            btnClear.Width = 70;
            btnClear.Click += delegate { ClearData(); };
            dataPanel.Controls.Add(lstData);
            dataPanel.Controls.Add(btnAddFiles);
            dataPanel.Controls.Add(btnAddFolder);
            dataPanel.Controls.Add(btnRemove);
            dataPanel.Controls.Add(btnClear);
            RegisterHelp(dataPanel, "加入任何數量的圖片、影片、文字、CSV、設定檔或資料夾。程式不設定檔案數量上限，但速度會受電腦硬碟和記憶體影響。");
            RegisterHelp(btnAddFiles, "一次加入多個檔案，例如 NPC 設定文件、對話紀錄、遊玩影片、操作紀錄。");
            RegisterHelp(btnAddFolder, "加入整個資料夾。建立訓練包時會掃描裡面的所有檔案，包含子資料夾。");
            RegisterHelp(btnRemove, "只移除清單中的選取項目，不會刪除你電腦上的原始檔案。");
            RegisterHelp(btnClear, "清空資料清單，不會刪除原始檔案。");
            AddRow(table, "訓練資料", dataPanel, "把 AI 需要參考的資料放進來。NPC 可放劇情、角色設定、對話範例；遊戲操作可放影片、按鍵紀錄、地圖資料；文字 AI 可放 FAQ、文件、規則。");

            txtUrlSources = CreateMultiLineTextBox(90);
            txtUrlSources.Text = "";
            AddRow(table, "網址 / 影片連結", txtUrlSources, "可以貼 YouTube、影片網址、資料網站、文件網址或 GitHub 等來源，一行一個。程式會把它們寫進資料清單與搜尋清單；若要真正下載或爬網站，之後要接合法的下載器或 API，並遵守網站條款。");

            txtSearchKeywords = CreateMultiLineTextBox(80);
            txtSearchKeywords.Text = "";
            AddRow(table, "自動找資料關鍵字", txtSearchKeywords, "如果你沒有丟檔案，可以在這裡寫關鍵字，例如遊戲名稱、角色名稱、玩法、任務、控制方式。程式會產生可執行的資料搜尋清單，告訴你或後續爬蟲/API 該找哪些資料。");

            chkAutoFindResources = CreateCheckBox("沒有資料時，幫我產生自動找資料計畫", true, 0);
            AddRow(table, "沒有檔案時", chkAutoFindResources, "勾選後，即使沒有加入任何本機檔案，程式也會根據你的需求、關鍵字與網址產生 resource_discovery_queue.csv。它會列出該搜尋的網站、影片、文件、標註欄位與注意事項。這不是未授權爬取；真正下載前仍要確認授權與網站規則。");

            chkInferActionsFromVideo = CreateCheckBox("只有畫面時，幫我推測可能操作", true, 0);
            AddRow(table, "畫面推測操作", chkInferActionsFromVideo, "勾選後，若你只有遊戲畫面或影片，輸出會包含 action_inference_from_video_zh-TW.md，教你如何把畫面狀態推成操作標籤，例如看到彎道就推測左轉/右轉、直線就加速、接近障礙就煞車。這是標註與策略推測，不保證 100% 正確，正式訓練仍需要人工抽查。");

            return tab;
        }

        private TabPage CreateParameterTab()
        {
            TabPage tab = new TabPage("3. 參數");
            Panel panel = CreateScrollPanel();
            TableLayoutPanel table = CreateFormTable();
            panel.Controls.Add(table);
            tab.Controls.Add(panel);

            cboTrainingBackend = new ComboBox();
            cboTrainingBackend.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTrainingBackend.Items.Add("行為包 / 提示詞：不用 GPU，最快開始");
            cboTrainingBackend.Items.Add("大模型 API：使用雲端 LLM 或外部模型");
            cboTrainingBackend.Items.Add("本地 GPU 直接訓練：使用自己的顯卡開始訓練");
            cboTrainingBackend.Items.Add("混合：先用大模型整理，再用本地 GPU 訓練");
            cboTrainingBackend.SelectedIndex = 0;
            AddRow(table, "訓練方式", cboTrainingBackend, "這裡才決定要不要 GPU。行為包不用 GPU；大模型 API 用雲端模型；本地 GPU 直接訓練會產生 local_gpu_train 資料夾、訓練設定與啟動腳本；混合模式會先整理資料，再交給本地 GPU 訓練。");

            barCreativity = AddSliderRow(table, "創意程度", "越高越會嘗試不同回應或策略；越低越保守。NPC 想有個性可拉高，正式客服或穩定控制建議中低。", 0, 100, 45, out valCreativity);
            barConsistency = AddSliderRow(table, "穩定一致", "越高越遵守人格、規則和固定流程；越低越容易變化。想讓 NPC 不跑人設，這項要拉高。", 0, 100, 75, out valConsistency);
            barMemory = AddSliderRow(table, "記憶重視", "越高越重視過去事件、玩家選擇和長期狀態。這會寫入記憶策略，不代表程式會自動擁有無限記憶。", 0, 100, 60, out valMemory);
            barReaction = AddSliderRow(table, "反應速度", "越高越偏向快速做決定；越低越允許多想一下。接硬體控制時，速度高通常代表動作間隔更短。", 0, 100, 55, out valReaction);
            barSafety = AddSliderRow(table, "安全限制", "越高越保守，會更明確拒絕危險、違規或未授權操作。接真實設備、遊戲帳號或公開服務時建議拉高。", 0, 100, 85, out valSafety);
            barPasses = AddSliderRow(table, "整理輪數", "這是資料整理與規則萃取的輪數，不是大型神經網路訓練 epoch。數字越高，產出的文件會更詳細，但建立時間較久。", 1, 20, 5, out valPasses);

            Panel checks = new Panel();
            checks.Dock = DockStyle.Fill;
            checks.Height = 230;
            chkTutorial = CreateCheckBox("產生新手教學", true, 0);
            chkAdapters = CreateCheckBox("產生遊戲引擎整合範本", true, 28);
            chkHardware = CreateCheckBox("產生開發板 / 手把橋接範本", false, 56);
            chkResourcePlan = CreateCheckBox("產生找資料清單", true, 84);
            chkTextApp = CreateCheckBox("產生文字 AI App 範本", false, 112);
            chkSafetyNotes = CreateCheckBox("產生風險與限制說明", true, 140);
            checks.Controls.Add(chkTutorial);
            checks.Controls.Add(chkAdapters);
            checks.Controls.Add(chkHardware);
            checks.Controls.Add(chkResourcePlan);
            checks.Controls.Add(chkTextApp);
            checks.Controls.Add(chkSafetyNotes);
            RegisterHelp(chkTutorial, "產生新手教學：會把這次專案拆成步驟，告訴你先看哪些檔、如何測試、如何接到 NPC、文字 AI、影片操作或硬體。適合完全沒有程式背景的人，避免只拿到一堆檔案卻不知道下一步。");
            RegisterHelp(chkAdapters, "產生遊戲引擎整合範本：會輸出 Unity C#、Godot GDScript、Unreal Blueprint 接入說明。用途是把 AI 行為包接到你能修改的遊戲專案、官方 Mod SDK 或自製遊戲；不能用來破解封閉商業遊戲。");
            RegisterHelp(chkHardware, "產生開發板 / 手把橋接範本：會輸出序列埠指令、控制策略 JSON、影片標註 CSV 與接開發板的說明。適合 Arduino、RP2040、ESP32-S3 等硬體，再由你接合法的 HID / Gamepad 函式庫。");
            RegisterHelp(chkResourcePlan, "產生找資料清單：當你沒有檔案、只有網址或只有需求時，會建立 resource_discovery_queue.csv，列出要搜尋的關鍵字、來源類型、資料用途、授權注意事項與標註方式。它不會未授權下載內容。");
            RegisterHelp(chkTextApp, "產生文字 AI App 範本：會輸出可雙擊開啟的 HTML 介面與 app_config.json。它會放入你的系統提示詞、人格、資料限制與 API 預留位置，之後可接大模型 API 或本機 LLM。正式使用時 API Key 應放後端。");
            RegisterHelp(chkSafetyNotes, "產生風險與限制說明：會列出不能做的事，例如破解遊戲、繞過主機安全、違反多人遊戲服務條款、使用未授權資料訓練、直接把未測試 AI 接真實設備。這份文件是給使用者與工程師檢查風險用。");
            AddRow(table, "要產生哪些東西", checks, "勾選你希望輸出的額外內容。新手建議保留教學和整合範本，等熟悉後再只輸出訓練檔。");

            return tab;
        }

        private TabPage CreateBuildTab()
        {
            TabPage tab = new TabPage("4. 建立檔案");
            Panel panel = CreateScrollPanel();
            TableLayoutPanel table = CreateFormTable();
            panel.Controls.Add(table);
            tab.Controls.Add(panel);

            Label intro = new Label();
            intro.Text = "按下面的按鈕建立檔案。建立完成後，右側紀錄會顯示輸出位置。";
            intro.AutoSize = true;
            intro.MaximumSize = new Size(620, 0);
            AddRow(table, "準備建立", intro, "如果你還沒填需求也可以建立，程式會用預設架構產生空白範本，讓你之後補資料。");

            Panel buttons = new Panel();
            buttons.Dock = DockStyle.Fill;
            buttons.Height = 205;

            Button btnTraining = CreateBuildButton("建立訓練包", 0, 0);
            btnTraining.Click += delegate { CreatePackage(false, false); };
            Button btnIntegration = CreateBuildButton("建立整合包", 190, 0);
            btnIntegration.Click += delegate { CreatePackage(true, false); };
            Button btnTextApp = CreateBuildButton("建立文字AI App", 0, 58);
            btnTextApp.Click += delegate { CreatePackage(true, true); };
            Button btnOpen = CreateBuildButton("開啟輸出資料夾", 190, 58);
            btnOpen.Click += delegate { OpenOutputFolder(); };
            Button btnExampleNpc = CreateBuildButton("載入NPC範例", 0, 116);
            btnExampleNpc.Click += delegate { LoadNpcExample(); };
            Button btnExampleController = CreateBuildButton("載入遊戲操作範例", 190, 116);
            btnExampleController.Click += delegate { LoadControllerExample(); };

            buttons.Controls.Add(btnTraining);
            buttons.Controls.Add(btnIntegration);
            buttons.Controls.Add(btnTextApp);
            buttons.Controls.Add(btnOpen);
            buttons.Controls.Add(btnExampleNpc);
            buttons.Controls.Add(btnExampleController);

            RegisterHelp(btnTraining, "建立核心 AI 訓練包：.aipack、需求提示詞、資料清單、model card。這是最基本、最重要的輸出。");
            RegisterHelp(btnIntegration, "除了訓練包之外，再建立 Unity / Godot / Unreal、操作對應表、硬體橋接和新手教學。");
            RegisterHelp(btnTextApp, "建立訓練包、整合包，以及一個可雙擊開啟的文字 AI App HTML 範本。");
            RegisterHelp(btnOpen, "打開你設定的輸出資料夾。");
            RegisterHelp(btnExampleNpc, "自動填入 NPC 範例，適合第一次測試。");
            RegisterHelp(btnExampleController, "自動填入遊戲操作範例，適合了解影片資料、動作對應和開發板橋接的輸出內容。");
            AddRow(table, "操作", buttons, "通常先按「建立訓練包」確認內容，再按「建立整合包」。如果你只想做普通文字 AI，直接按「建立文字AI App」。");

            return tab;
        }

        private Panel CreateScrollPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            panel.Padding = new Padding(14);
            return panel;
        }

        private TableLayoutPanel CreateFormTable()
        {
            TableLayoutPanel table = new TableLayoutPanel();
            table.Dock = DockStyle.Top;
            table.AutoSize = true;
            table.ColumnCount = 2;
            table.RowCount = 0;
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.SizeChanged += delegate
            {
                foreach (Control control in table.Controls)
                {
                    Label label = control as Label;
                    if (label != null && label.ForeColor == Color.DimGray)
                    {
                        label.MaximumSize = new Size(Math.Max(300, table.ClientSize.Width - 180), 0);
                    }
                }
            };
            return table;
        }

        private TextBox CreateTextBox(string text)
        {
            TextBox box = new TextBox();
            box.Text = text;
            box.Width = 520;
            box.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            return box;
        }

        private TextBox CreateMultiLineTextBox(int height)
        {
            TextBox box = new TextBox();
            box.Multiline = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.Width = 520;
            box.Height = height;
            box.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            return box;
        }

        private CheckBox CreateCheckBox(string text, bool isChecked, int top)
        {
            CheckBox box = new CheckBox();
            box.Text = text;
            box.Checked = isChecked;
            box.Left = 0;
            box.Top = top;
            box.Width = 360;
            return box;
        }

        private Button CreateBuildButton(string text, int left, int top)
        {
            Button button = new Button();
            button.Text = text;
            button.Left = left;
            button.Top = top;
            button.Width = 176;
            button.Height = 42;
            return button;
        }

        private Label AddRow(TableLayoutPanel table, string title, Control input, string help)
        {
            int row = table.RowCount;
            table.RowCount = row + 2;
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font(Font, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Margin = new Padding(3, 10, 8, 8);

            input.Margin = new Padding(3, 7, 12, 10);
            input.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            if (input.Width < 520)
            {
                input.Width = 520;
            }

            ComboBox combo = input as ComboBox;
            if (combo != null)
            {
                combo.Width = 520;
                combo.DropDownWidth = 650;
            }

            Label helpLabel = new Label();
            helpLabel.Text = GetContextualHelp(input, help);
            helpLabel.AutoSize = true;
            helpLabel.ForeColor = Color.DimGray;
            helpLabel.MaximumSize = new Size(760, 0);
            helpLabel.Margin = new Padding(153, 0, 12, 16);

            if (combo != null)
            {
                combo.SelectedIndexChanged += delegate
                {
                    string selectedHelp = GetContextualHelp(combo, help);
                    helpLabel.Text = selectedHelp;
                    ShowHelp(selectedHelp);
                };
            }

            table.Controls.Add(titleLabel, 0, row);
            table.Controls.Add(input, 1, row);
            table.Controls.Add(helpLabel, 0, row + 1);
            table.SetColumnSpan(helpLabel, 2);

            RegisterHelp(titleLabel, help, helpLabel);
            RegisterHelp(input, help, helpLabel);
            RegisterHelp(helpLabel, help, helpLabel);
            return helpLabel;
        }

        private string GetContextualHelp(Control control, string fallback)
        {
            CheckBox check = control as CheckBox;
            if (check != null)
            {
                string state = check.Checked ? "目前狀態：已勾選。 " : "目前狀態：未勾選。 ";
                if (check == chkAutoFindResources)
                {
                    return state + (check.Checked ? "建立輸出包時會產生自動找資料計畫，包含關鍵字、網址、來源類型、授權注意事項與標註方式。" : "不會特別產生自動找資料計畫；若你沒有提供資料，輸出包可能只包含空白資料結構。");
                }
                if (check == chkInferActionsFromVideo)
                {
                    return state + (check.Checked ? "會產生只有畫面時推測操作的教學，協助把影片畫面標成加速、煞車、左右轉、使用道具等動作。" : "不會產生畫面推測操作教學；比較適合你已經有搖桿操作紀錄或人工標註資料。");
                }
                if (check == chkTutorial)
                {
                    return state + (check.Checked ? "會產生新手教學，逐步說明先看哪個檔案、怎麼測試、怎麼接到遊戲、App 或硬體。" : "不產生新手教學；適合你已經熟悉輸出檔案用途。");
                }
                if (check == chkAdapters)
                {
                    return state + (check.Checked ? "會產生 Unity、Godot、Unreal 等遊戲引擎接入範本。" : "不產生遊戲引擎接入範本；適合你只需要訓練檔或自己會寫接入程式。");
                }
                if (check == chkHardware)
                {
                    return state + (check.Checked ? "會產生開發板、序列埠、手把橋接與控制策略範本。" : "不產生硬體橋接範本；適合不需要接開發板或手把的專案。");
                }
                if (check == chkResourcePlan)
                {
                    return state + (check.Checked ? "會產生找資料清單，列出該找哪些資料、怎麼標註、怎麼避免授權問題。" : "不產生找資料清單；適合你已經準備好資料集。");
                }
                if (check == chkTextApp)
                {
                    return state + (check.Checked ? "會產生可雙擊開啟的 HTML 文字 AI App 範本與 app_config.json。" : "不產生文字 AI App 範本；適合只做 NPC、控制策略或資料包。");
                }
                if (check == chkSafetyNotes)
                {
                    return state + (check.Checked ? "會產生風險與限制說明，提醒授權、遊戲條款、真實設備與安全限制。" : "不產生風險說明；正式接遊戲、服務或硬體時仍建議保留。");
                }
                return state + fallback;
            }

            TrackBar track = control as TrackBar;
            if (track != null)
            {
                string level;
                if (track.Value <= track.Minimum + (track.Maximum - track.Minimum) / 3)
                {
                    level = "偏低";
                }
                else if (track.Value >= track.Minimum + (track.Maximum - track.Minimum) * 2 / 3)
                {
                    level = "偏高";
                }
                else
                {
                    level = "中等";
                }

                if (track == barCreativity)
                {
                    return "目前創意程度：" + track.Value.ToString() + "，屬於" + level + "。低代表穩定保守，高代表更會嘗試不同回答、人格表現或操作策略。";
                }
                if (track == barConsistency)
                {
                    return "目前穩定一致：" + track.Value.ToString() + "，屬於" + level + "。越高越遵守人格、規則與固定流程，越低越容易自由發揮。";
                }
                if (track == barMemory)
                {
                    return "目前記憶重視：" + track.Value.ToString() + "，屬於" + level + "。越高越重視玩家選擇、過去事件與長期狀態，但不代表自動擁有無限記憶。";
                }
                if (track == barReaction)
                {
                    return "目前反應速度：" + track.Value.ToString() + "，屬於" + level + "。越高越偏向快速決策；接硬體或遊戲控制時，通常代表動作間隔更短。";
                }
                if (track == barSafety)
                {
                    return "目前安全限制：" + track.Value.ToString() + "，屬於" + level + "。越高越保守，會更明確避免危險、違規、未授權或不適合自動化的操作。";
                }
                if (track == barPasses)
                {
                    return "目前整理輪數：" + track.Value.ToString() + "。這是資料整理與規則萃取的輪數，不是大型模型訓練 epoch；越高文件越詳細，但建立時間較久。";
                }
                return "目前數值：" + track.Value.ToString() + "。 " + fallback;
            }

            Button button = control as Button;
            if (button != null)
            {
                string text = button.Text;
                if (text.IndexOf("加入檔案", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：加入檔案。一次選多個圖片、影片、文字、CSV、設定檔或操作紀錄；不會移動或刪除原始檔。";
                }
                if (text.IndexOf("加入資料夾", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：加入資料夾。建立訓練包時會掃描資料夾與子資料夾中的檔案，適合大量資料。";
                }
                if (text.IndexOf("移除", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：移除選取。只從清單拿掉選取項目，不會刪除你電腦上的原始檔案。";
                }
                if (text.IndexOf("清空", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：清空。清空目前資料清單，不會刪除原始檔案。";
                }
                if (text.IndexOf("建立訓練包", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：建立訓練包。產生核心 .aipack、JSON、提示詞、資料清單、操作對應表與模型卡。";
                }
                if (text.IndexOf("建立整合包", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：建立整合包。除了訓練包，還會產生遊戲引擎、硬體橋接、新手教學與風險文件。";
                }
                if (text.IndexOf("文字AI", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：建立文字 AI App。產生可雙擊開啟的 HTML App 範本，之後可接大模型 API 或本機模型。";
                }
                if (text.IndexOf("開啟輸出", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：開啟輸出資料夾。打開目前設定的輸出位置，方便查看產生的訓練包。";
                }
                if (text.IndexOf("NPC", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：載入 NPC 範例。自動填入一個村莊守護者 NPC 的需求、人格與測試設定。";
                }
                if (text.IndexOf("遊戲操作", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：載入遊戲操作範例。自動填入賽車控制策略、影片標註與開發板橋接的測試設定。";
                }
                if (text.IndexOf("選擇", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "按鈕：選擇資料夾。指定訓練包要輸出的資料夾位置。";
                }
                return "按鈕：" + button.Text + "。 " + fallback;
            }

            ComboBox combo = control as ComboBox;
            if (combo == null)
            {
                return fallback;
            }

            string value = combo.Text;
            if (String.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            if (combo == cboMode)
            {
                if (value.IndexOf("NPC", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：NPC 智慧角色。適合做遊戲角色、村民、商人、敵人或隊友。輸出會重點整理人格、對話風格、記憶規則、任務反應，以及 Unity / Godot / Unreal 接入方式。";
                }
                if (value.IndexOf("遊戲", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：遊戲操作 / 控制策略。適合把影片、畫面狀態或操作紀錄整理成「看到什麼畫面 -> 做什麼操作」。如果只有畫面，會產生推測操作的標註教學。";
                }
                if (value.IndexOf("文字", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("App", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：文字型 AI / App。適合做客服、角色聊天、文件問答、個人助手。輸出會包含提示詞、資料規則、App 範本，以及大模型 API 或本機模型接法。";
                }
                return "目前選擇：自訂流程自動化。適合把 AI 輸出接到固定流程，例如讀檔、整理資料、呼叫 API、產生報告或送出硬體指令。";
            }

            if (combo == cboTarget)
            {
                if (value.IndexOf("Unity", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：Unity 遊戲專案。輸出會包含 Unity C# adapter 範本，建議把訓練包放在 StreamingAssets，再由角色腳本讀取。";
                }
                if (value.IndexOf("Godot", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：Godot 遊戲專案。輸出會包含 GDScript adapter 範本，適合把 NPC 狀態、玩家訊息與記憶資料傳給 AI 行為包。";
                }
                if (value.IndexOf("Unreal", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：Unreal Engine 專案。輸出會包含 Blueprint 接入說明，建議透過 AI Component 或後端服務處理模型呼叫。";
                }
                if (value.IndexOf("Mod", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("自訂遊戲引擎", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：自訂遊戲引擎 / Mod SDK。只有遊戲支援 Mod、SDK、腳本或你有原始碼時才適合；封閉遊戲不能靠複製檔案直接改 NPC。";
                }
                if (value.IndexOf("影片", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：影片資料到操作策略。適合丟遊玩影片、影片網址或畫面截圖，輸出會協助建立畫面狀態與操作標註。";
                }
                if (value.IndexOf("開發板", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("HID", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：開發板 / USB HID 控制。輸出會產生序列埠指令與控制策略範本，之後可接 Arduino、RP2040、ESP32 或其他支援 HID 的硬體。";
                }
                if (value.IndexOf("Switch", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：Nintendo Switch 手把橋接。只提供合法外部控制橋接概念，不提供破解、繞過安全或違反服務條款的方法。";
                }
                if (value.IndexOf("OpenAI", StringComparison.OrdinalIgnoreCase) >= 0 || value.IndexOf("LLM", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：OpenAI / 本機 LLM API。輸出會把提示詞、資料規則與 App 設定整理好；正式 API Key 不應放在前端。";
                }
                return "目前選擇：" + value + "。程式會依照這個目標產生對應的設定、教學與整合檔。";
            }

            if (combo == cboOutputKind)
            {
                if (value.IndexOf("完整", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：新手完整包。會輸出訓練包、教學、整合範本、風險說明、資料搜尋清單與操作對應表，適合第一次使用。";
                }
                if (value.IndexOf("訓練", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：只要訓練檔。會偏向輸出核心 .aipack、JSON、提示詞與資料清單，適合你已經知道怎麼接入的情況。";
                }
                if (value.IndexOf("工程師", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：整合給工程師使用。會保留較多設定檔、adapter、CSV 與技術文件，方便交給工程師接到遊戲、App 或硬體。";
                }
                return "目前選擇：教學優先。會加強新手步驟、限制說明、資料準備與整合流程，適合先理解再實作。";
            }

            if (combo == cboModelLevel)
            {
                if (value.IndexOf("入門", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：入門。只決定輸出較簡單，會重點產生需求整理、AI 行為包與基本提示詞；不代表要不要 GPU。";
                }
                if (value.IndexOf("進階", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：進階。會多輸出 API / 本機模型接入設定、資料規則與較完整的整合檔；是否用 GPU 仍由「訓練方式」決定。";
                }
                return "目前選擇：完整。會輸出訓練資料格式、標註表、整合檔與更多教學；是否用 GPU 仍由「訓練方式」決定。";
            }

            if (combo == cboTrainingBackend)
            {
                if (value.IndexOf("行為包", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：行為包 / 提示詞。不使用 GPU，最快開始。適合 NPC 人格、文字 AI、規則整理與先做可測試的原型。";
                }
                if (value.IndexOf("大模型", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "目前選擇：大模型 API。使用雲端或外部 LLM，不需要本地 GPU。適合資料整理、角色對話、問答與標註輔助；正式 API Key 建議放後端。";
                }
                if (value.IndexOf("GPU", StringComparison.OrdinalIgnoreCase) >= 0 && value.IndexOf("混合", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return "目前選擇：本地 GPU 直接訓練。會產生 local_gpu_train 資料夾與啟動腳本。GPU 不限 NVIDIA，也可以是 AMD、Intel、Apple Silicon 或其他加速器，但需要對應訓練框架。";
                }
                return "目前選擇：混合。先用大模型整理資料或產生標註草稿，再交給本地 GPU 訓練。適合資料很多、需要先清理再訓練的情況。";
            }

            return fallback;
        }

        private TrackBar AddSliderRow(TableLayoutPanel table, string title, string help, int min, int max, int value, out Label valueLabel)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.Height = 48;

            TrackBar bar = new TrackBar();
            bar.Minimum = min;
            bar.Maximum = max;
            bar.Value = value;
            bar.TickFrequency = Math.Max(1, (max - min) / 10);
            bar.Width = 320;
            bar.Left = 0;
            bar.Top = 0;

            Label label = new Label();
            label.Text = value.ToString();
            label.Left = 335;
            label.Top = 14;
            label.Width = 60;
            label.TextAlign = ContentAlignment.MiddleLeft;

            panel.Controls.Add(bar);
            panel.Controls.Add(label);
            panel.Resize += delegate
            {
                label.Left = Math.Max(0, panel.ClientSize.Width - label.Width);
                bar.Width = Math.Max(180, label.Left - 8);
            };
            Label helpLabel = AddRow(table, title, panel, help);
            bar.Scroll += delegate
            {
                label.Text = bar.Value.ToString();
                string sliderHelp = GetContextualHelp(bar, help);
                helpLabel.Text = sliderHelp;
                ShowHelp(sliderHelp);
            };
            valueLabel = label;
            return bar;
        }

        private void RegisterHelp(Control control, string help)
        {
            if (control == null)
            {
                return;
            }

            tips.SetToolTip(control, help);
            control.Enter += delegate { ShowHelp(GetContextualHelp(control, help)); };
            control.MouseEnter += delegate { ShowHelp(GetContextualHelp(control, help)); };

            foreach (Control child in control.Controls)
            {
                RegisterHelp(child, help);
            }
        }

        private void RegisterHelp(Control control, string help, Label rowHelpLabel)
        {
            if (control == null)
            {
                return;
            }

            tips.SetToolTip(control, help);
            EventHandler update = delegate
            {
                string text = GetContextualHelp(control, help);
                if (rowHelpLabel != null)
                {
                    rowHelpLabel.Text = text;
                }
                ShowHelp(text);
            };

            control.Enter += update;
            control.MouseEnter += update;

            CheckBox check = control as CheckBox;
            if (check != null)
            {
                check.CheckedChanged += update;
            }

            foreach (Control child in control.Controls)
            {
                RegisterHelp(child, help, rowHelpLabel);
            }
        }

        private void ShowHelp(string text)
        {
            if (txtHelp != null)
            {
                txtHelp.Text = text;
            }
        }

        private void Log(string message)
        {
            if (txtLog == null)
            {
                return;
            }

            txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + message + Environment.NewLine);
        }

        private string DefaultOutputFolder()
        {
            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (String.IsNullOrEmpty(documents))
            {
                documents = Application.StartupPath;
            }
            return Path.Combine(documents, "AITrainingStudio_Output");
        }

        private void RefreshTargets()
        {
            if (cboTarget == null || cboMode == null)
            {
                return;
            }

            string old = cboTarget.Text;
            cboTarget.Items.Clear();

            if (cboMode.SelectedIndex == 0)
            {
                cboTarget.Items.Add("Unity 遊戲專案");
                cboTarget.Items.Add("Godot 遊戲專案");
                cboTarget.Items.Add("Unreal Engine 專案");
                cboTarget.Items.Add("自訂遊戲引擎 / Mod SDK");
                ShowHelp("NPC 模式會把需求整理成人格、記憶規則、對話風格、世界觀限制和事件反應，並產生遊戲引擎接入範本。");
            }
            else if (cboMode.SelectedIndex == 1)
            {
                cboTarget.Items.Add("影片資料到操作策略");
                cboTarget.Items.Add("開發板 / USB HID 控制");
                cboTarget.Items.Add("Nintendo Switch 手把橋接");
                cboTarget.Items.Add("模擬器 / 自訂控制環境");
                ShowHelp("遊戲操作模式會建立「看到什麼狀態 -> 做什麼操作」的對應表。接真實主機時，只能使用你有權使用的硬體和方法，不能繞過主機安全機制。");
            }
            else if (cboMode.SelectedIndex == 2)
            {
                cboTarget.Items.Add("Windows 文字 AI App");
                cboTarget.Items.Add("網頁 App");
                cboTarget.Items.Add("OpenAI / 本機 LLM API");
                cboTarget.Items.Add("自訂應用程式");
                ShowHelp("文字 AI 模式會建立提示詞、人格、資料索引和 App 範本。你可以之後接 OpenAI API、公司內部模型或本機 LLM。");
            }
            else
            {
                cboTarget.Items.Add("Windows 操作流程");
                cboTarget.Items.Add("API / 網頁服務");
                cboTarget.Items.Add("機械 / 開發板");
                cboTarget.Items.Add("自訂流程");
                ShowHelp("自訂流程模式適合把 AI 輸出轉成固定步驟，例如讀檔、判斷、呼叫 API、發送序列埠指令。");
            }

            int found = -1;
            for (int i = 0; i < cboTarget.Items.Count; i++)
            {
                if (String.Equals(cboTarget.Items[i].ToString(), old, StringComparison.OrdinalIgnoreCase))
                {
                    found = i;
                    break;
                }
            }
            cboTarget.SelectedIndex = found >= 0 ? found : 0;
        }

        private void BrowseOutputFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "選擇 AI 訓練包輸出資料夾";
            if (Directory.Exists(txtOutputFolder.Text))
            {
                dialog.SelectedPath = txtOutputFolder.Text;
            }

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtOutputFolder.Text = dialog.SelectedPath;
                Log("輸出資料夾已設定：" + dialog.SelectedPath);
            }
        }

        private void AddFiles()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = "選擇訓練資料檔案";
            dialog.Filter = "所有檔案 (*.*)|*.*";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                AddDataPaths(dialog.FileNames);
            }
        }

        private void AddFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "選擇訓練資料資料夾";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                AddDataPaths(new string[] { dialog.SelectedPath });
            }
        }

        private void AddDataPaths(IEnumerable<string> paths)
        {
            int added = 0;
            foreach (string path in paths)
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(path);
                }
                catch
                {
                    fullPath = path;
                }

                if (!dataPathSet.ContainsKey(fullPath))
                {
                    dataPathSet[fullPath] = true;
                    dataPaths.Add(fullPath);
                    lstData.Items.Add(fullPath);
                    added++;
                }
            }

            Log("已加入 " + added.ToString() + " 個資料來源。");
        }

        private void RemoveSelectedData()
        {
            List<object> selected = new List<object>();
            foreach (object item in lstData.SelectedItems)
            {
                selected.Add(item);
            }

            foreach (object item in selected)
            {
                string path = item.ToString();
                lstData.Items.Remove(item);
                dataPaths.Remove(path);
                dataPathSet.Remove(path);
            }

            Log("已移除 " + selected.Count.ToString() + " 個資料來源。");
        }

        private void ClearData()
        {
            lstData.Items.Clear();
            dataPaths.Clear();
            dataPathSet.Clear();
            Log("訓練資料清單已清空。");
        }

        private void OpenOutputFolder()
        {
            string folder = txtOutputFolder.Text.Trim();
            if (!Directory.Exists(folder))
            {
                MessageBox.Show(this, "輸出資料夾還不存在，請先建立訓練包。", "尚未建立", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Process.Start(folder);
        }

        private void LoadNpcExample()
        {
            cboMode.SelectedIndex = 0;
            txtProjectName.Text = "VillageGuardian_NPC";
            txtRequirement.Text = "我要一個遊戲 NPC，名字叫艾琳。她是村莊守護者，會記住玩家是否幫助村民。玩家有危險時，她會先提醒、再給任務提示。她不能透露主線結局，也不能直接替玩家完成任務。";
            txtPersonality.Text = "沉著、可靠、說話簡短、有同理心。對陌生玩家保持禮貌，對守信玩家更願意分享情報。";
            txtSearchKeywords.Text = "fantasy village guardian npc dialogue\nbranching npc memory examples";
            txtNotes.Text = "第一次測試用，先接 Unity 或 Godot，確認對話和狀態記憶。";
            chkAdapters.Checked = true;
            chkTutorial.Checked = true;
            chkTextApp.Checked = false;
            chkHardware.Checked = false;
            Log("已載入 NPC 範例。");
        }

        private void LoadControllerExample()
        {
            cboMode.SelectedIndex = 1;
            txtProjectName.Text = "KartDriving_Controller";
            txtRequirement.Text = "我要訓練一個賽車遊戲操作策略。輸入是遊玩影片或畫面狀態，輸出是加速、煞車、左轉、右轉、使用道具。策略要保守，不要故意撞牆或逆向。";
            txtPersonality.Text = "操作風格：穩定、保守、優先保持賽道中央，有道具時先保留到彎道或被追擊時使用。";
            txtSearchKeywords.Text = "kart racing gameplay video\nkart racing controller mapping\nscreen to action labeling";
            txtNotes.Text = "接真實主機時，只使用合法購買且允許的控制硬體，不繞過主機安全機制。";
            chkAdapters.Checked = false;
            chkHardware.Checked = true;
            chkResourcePlan.Checked = true;
            chkSafetyNotes.Checked = true;
            Log("已載入遊戲操作範例。");
        }

        private void CreatePackage(bool forceIntegration, bool forceTextApp)
        {
            try
            {
                string projectName = CleanFileName(txtProjectName.Text);
                if (String.IsNullOrWhiteSpace(projectName))
                {
                    projectName = "AI_Project";
                }

                string baseFolder = txtOutputFolder.Text.Trim();
                if (String.IsNullOrWhiteSpace(baseFolder))
                {
                    baseFolder = DefaultOutputFolder();
                    txtOutputFolder.Text = baseFolder;
                }

                Directory.CreateDirectory(baseFolder);
                string projectFolder = Path.Combine(baseFolder, projectName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                Directory.CreateDirectory(projectFolder);

                Log("正在掃描資料來源...");
                List<string> warnings = new List<string>();
                List<string> files = CollectAllFiles(warnings);
                SortedDictionary<string, int> extCounts = CountExtensions(files);

                Log("正在建立訓練包...");
                WriteText(Path.Combine(projectFolder, projectName + ".aipack"), BuildAipack(files, extCounts));
                WriteText(Path.Combine(projectFolder, "training_project.json"), BuildTrainingJson(files, extCounts));
                WriteText(Path.Combine(projectFolder, "requirements_prompt.txt"), BuildRequirementsPrompt());
                WriteText(Path.Combine(projectFolder, "model_card_zh-TW.md"), BuildModelCard(files, extCounts));
                WriteDataManifest(Path.Combine(projectFolder, "data_manifest.csv"), files, warnings);
                WriteText(Path.Combine(projectFolder, "operation_mapping.csv"), BuildOperationMappingCsv());
                if (chkAutoFindResources.Checked || HasText(txtUrlSources.Text) || HasText(txtSearchKeywords.Text) || files.Count == 0)
                {
                    WriteText(Path.Combine(projectFolder, "resource_discovery_queue.csv"), BuildResourceDiscoveryQueue(files));
                }
                if (chkInferActionsFromVideo.Checked || cboMode.SelectedIndex == 1)
                {
                    WriteText(Path.Combine(projectFolder, "action_inference_from_video_zh-TW.md"), BuildActionInferenceGuide());
                }
                WriteText(Path.Combine(projectFolder, "training_backend_plan_zh-TW.md"), BuildTrainingBackendPlan());
                if (UsesLocalGpuTraining())
                {
                    WriteLocalGpuTrainingFiles(projectFolder);
                }

                if (chkTutorial.Checked || cboOutputKind.SelectedIndex == 0 || forceIntegration)
                {
                    WriteText(Path.Combine(projectFolder, "beginner_steps_zh-TW.md"), BuildBeginnerSteps());
                }

                if (chkResourcePlan.Checked || forceIntegration)
                {
                    WriteText(Path.Combine(projectFolder, "resource_plan_zh-TW.md"), BuildResourcePlan());
                }

                if (chkSafetyNotes.Checked || forceIntegration)
                {
                    WriteText(Path.Combine(projectFolder, "limits_and_safety_zh-TW.md"), BuildSafetyNotes());
                }

                if (forceIntegration || chkAdapters.Checked)
                {
                    WriteIntegrationFiles(projectFolder);
                }

                if (forceIntegration || chkHardware.Checked)
                {
                    WriteHardwareFiles(projectFolder);
                }

                if (forceTextApp || chkTextApp.Checked || cboMode.SelectedIndex == 2)
                {
                    WriteTextAppFiles(projectFolder);
                }

                Log("完成：" + projectFolder);
                MessageBox.Show(this, "已建立完成。\n\n輸出位置：\n" + projectFolder, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log("建立失敗：" + ex.Message);
                MessageBox.Show(this, "建立失敗：\n" + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<string> CollectAllFiles(List<string> warnings)
        {
            List<string> files = new List<string>();
            Dictionary<string, bool> seen = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            foreach (string source in dataPaths)
            {
                if (File.Exists(source))
                {
                    AddSeenFile(files, seen, source);
                }
                else if (Directory.Exists(source))
                {
                    CollectDirectory(source, files, seen, warnings);
                }
                else
                {
                    warnings.Add("找不到資料來源：" + source);
                }
            }

            return files;
        }

        private void CollectDirectory(string folder, List<string> files, Dictionary<string, bool> seen, List<string> warnings)
        {
            Stack<string> stack = new Stack<string>();
            stack.Push(folder);

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                string[] folderFiles = new string[0];
                string[] subFolders = new string[0];

                try
                {
                    folderFiles = Directory.GetFiles(current);
                }
                catch (Exception ex)
                {
                    warnings.Add("無法讀取檔案：" + current + "，原因：" + ex.Message);
                }

                for (int i = 0; i < folderFiles.Length; i++)
                {
                    AddSeenFile(files, seen, folderFiles[i]);
                }

                try
                {
                    subFolders = Directory.GetDirectories(current);
                }
                catch (Exception ex)
                {
                    warnings.Add("無法讀取子資料夾：" + current + "，原因：" + ex.Message);
                }

                for (int i = 0; i < subFolders.Length; i++)
                {
                    stack.Push(subFolders[i]);
                }
            }
        }

        private void AddSeenFile(List<string> files, Dictionary<string, bool> seen, string file)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(file);
            }
            catch
            {
                fullPath = file;
            }

            if (!seen.ContainsKey(fullPath))
            {
                seen[fullPath] = true;
                files.Add(fullPath);
            }
        }

        private SortedDictionary<string, int> CountExtensions(List<string> files)
        {
            SortedDictionary<string, int> counts = new SortedDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                if (String.IsNullOrEmpty(ext))
                {
                    ext = "(無副檔名)";
                }
                if (!counts.ContainsKey(ext))
                {
                    counts[ext] = 0;
                }
                counts[ext]++;
            }
            return counts;
        }

        private string BuildAipack(List<string> files, SortedDictionary<string, int> extCounts)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# AI Training Studio AIPACK");
            sb.AppendLine("# 這是可交給程式讀取的人類可讀訓練包。");
            sb.AppendLine();
            sb.AppendLine(BuildTrainingJson(files, extCounts));
            return sb.ToString();
        }

        private string BuildTrainingJson(List<string> files, SortedDictionary<string, int> extCounts)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            JsonProp(sb, "  ", "schema", "ai-training-studio.aipack.v1", true);
            JsonProp(sb, "  ", "project_name", txtProjectName.Text.Trim(), true);
            JsonProp(sb, "  ", "created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), true);
            JsonProp(sb, "  ", "mode", cboMode.Text, true);
            JsonProp(sb, "  ", "target", cboTarget.Text, true);
            JsonProp(sb, "  ", "output_kind", cboOutputKind.Text, true);
            JsonProp(sb, "  ", "model_level", cboModelLevel.Text, true);
            JsonProp(sb, "  ", "training_backend", cboTrainingBackend.Text, true);
            JsonProp(sb, "  ", "requirement", txtRequirement.Text, true);
            JsonProp(sb, "  ", "personality_or_style", txtPersonality.Text, true);
            JsonProp(sb, "  ", "notes", txtNotes.Text, true);

            sb.AppendLine("  \"parameters\": {");
            JsonNumberProp(sb, "    ", "creativity", barCreativity.Value, true);
            JsonNumberProp(sb, "    ", "consistency", barConsistency.Value, true);
            JsonNumberProp(sb, "    ", "memory_priority", barMemory.Value, true);
            JsonNumberProp(sb, "    ", "reaction_speed", barReaction.Value, true);
            JsonNumberProp(sb, "    ", "safety_limit", barSafety.Value, true);
            JsonNumberProp(sb, "    ", "organization_passes", barPasses.Value, false);
            sb.AppendLine("  },");

            sb.AppendLine("  \"generated_items\": {");
            JsonBoolProp(sb, "    ", "beginner_tutorial", chkTutorial.Checked, true);
            JsonBoolProp(sb, "    ", "game_engine_adapters", chkAdapters.Checked, true);
            JsonBoolProp(sb, "    ", "hardware_bridge", chkHardware.Checked, true);
            JsonBoolProp(sb, "    ", "resource_plan", chkResourcePlan.Checked, true);
            JsonBoolProp(sb, "    ", "auto_resource_discovery", chkAutoFindResources.Checked, true);
            JsonBoolProp(sb, "    ", "infer_actions_from_video", chkInferActionsFromVideo.Checked, true);
            JsonBoolProp(sb, "    ", "text_ai_app_template", chkTextApp.Checked, true);
            JsonBoolProp(sb, "    ", "safety_notes", chkSafetyNotes.Checked, false);
            sb.AppendLine("  },");

            sb.AppendLine("  \"data_sources\": [");
            for (int i = 0; i < dataPaths.Count; i++)
            {
                sb.Append("    \"").Append(JsonEscape(dataPaths[i])).Append("\"");
                if (i < dataPaths.Count - 1)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine("  ],");

            WriteJsonStringArray(sb, "  ", "url_sources", SplitLines(txtUrlSources.Text), true);
            WriteJsonStringArray(sb, "  ", "search_keywords", SplitLines(txtSearchKeywords.Text), true);

            sb.AppendLine("  \"dataset_summary\": {");
            JsonNumberProp(sb, "    ", "file_count", files.Count, true);
            sb.AppendLine("    \"extension_counts\": [");
            int index = 0;
            foreach (KeyValuePair<string, int> item in extCounts)
            {
                sb.Append("      { \"extension\": \"").Append(JsonEscape(item.Key)).Append("\", \"count\": ").Append(item.Value.ToString()).Append(" }");
                if (index < extCounts.Count - 1)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
                index++;
            }
            sb.AppendLine("    ]");
            sb.AppendLine("  },");

            sb.AppendLine("  \"behavior_contract\": {");
            JsonProp(sb, "    ", "plain_language", BuildBehaviorContract(), true);
            JsonProp(sb, "    ", "expected_input", BuildExpectedInput(), true);
            JsonProp(sb, "    ", "expected_output", BuildExpectedOutput(), false);
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildBehaviorContract()
        {
            if (cboMode.SelectedIndex == 0)
            {
                return "把玩家訊息、NPC狀態、世界事件轉成 NPC 的下一句話、下一個意圖和記憶更新。";
            }
            if (cboMode.SelectedIndex == 1)
            {
                return "把畫面狀態、影片標註或遊戲狀態轉成可執行的控制動作，例如加速、左轉、右轉、使用道具。";
            }
            if (cboMode.SelectedIndex == 2)
            {
                return "把使用者輸入和資料索引轉成文字回答，並遵守人格、格式和安全限制。";
            }
            return "把輸入事件轉成固定流程、API 呼叫、檔案輸出或硬體指令。";
        }

        private string BuildExpectedInput()
        {
            if (cboMode.SelectedIndex == 1)
            {
                return "frame_state, video_label, controller_feedback, current_goal";
            }
            if (cboMode.SelectedIndex == 0)
            {
                return "player_message, npc_state, world_event, memory";
            }
            return "user_message, context_files, memory";
        }

        private string BuildExpectedOutput()
        {
            if (cboMode.SelectedIndex == 1)
            {
                return "action_name, strength, duration_ms, reason";
            }
            if (cboMode.SelectedIndex == 0)
            {
                return "npc_reply, intent, emotion, memory_update";
            }
            return "reply_text, tool_action, reason, safety_note";
        }

        private void WriteDataManifest(string path, List<string> files, List<string> warnings)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("path,type,extension,size_bytes,last_write_utc");
            foreach (string file in files)
            {
                try
                {
                    FileInfo info = new FileInfo(file);
                    sb.Append(Csv(file)).Append(",");
                    sb.Append(Csv("file")).Append(",");
                    sb.Append(Csv(Path.GetExtension(file))).Append(",");
                    sb.Append(info.Length.ToString()).Append(",");
                    sb.AppendLine(Csv(info.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                catch
                {
                    sb.Append(Csv(file)).Append(",");
                    sb.Append(Csv("file")).Append(",");
                    sb.Append(Csv(Path.GetExtension(file))).Append(",");
                    sb.Append("0,");
                    sb.AppendLine(Csv(""));
                }
            }

            if (warnings.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("warnings");
                foreach (string warning in warnings)
                {
                    sb.AppendLine(Csv(warning));
                }
            }
            WriteText(path, sb.ToString());
        }

        private string BuildRequirementsPrompt()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("你是這個 AI 行為包的執行模型。請遵守以下設定。");
            sb.AppendLine();
            sb.AppendLine("目標類型：" + cboMode.Text);
            sb.AppendLine("接入目標：" + cboTarget.Text);
            sb.AppendLine("訓練方式：" + cboTrainingBackend.Text);
            sb.AppendLine();
            sb.AppendLine("使用者需求：");
            sb.AppendLine(EmptyToPlaceholder(txtRequirement.Text));
            sb.AppendLine();
            sb.AppendLine("人格 / 風格：");
            sb.AppendLine(EmptyToPlaceholder(txtPersonality.Text));
            sb.AppendLine();
            sb.AppendLine("外部網址 / 影片連結：");
            sb.AppendLine(EmptyToPlaceholder(txtUrlSources.Text));
            sb.AppendLine();
            sb.AppendLine("自動找資料關鍵字：");
            sb.AppendLine(EmptyToPlaceholder(txtSearchKeywords.Text));
            sb.AppendLine();
            sb.AppendLine("參數：");
            sb.AppendLine("- 創意程度：" + barCreativity.Value.ToString());
            sb.AppendLine("- 穩定一致：" + barConsistency.Value.ToString());
            sb.AppendLine("- 記憶重視：" + barMemory.Value.ToString());
            sb.AppendLine("- 反應速度：" + barReaction.Value.ToString());
            sb.AppendLine("- 安全限制：" + barSafety.Value.ToString());
            sb.AppendLine();
            sb.AppendLine("輸出格式：");
            sb.AppendLine(BuildExpectedOutput());
            return sb.ToString();
        }

        private string BuildModelCard(List<string> files, SortedDictionary<string, int> extCounts)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# " + txtProjectName.Text.Trim());
            sb.AppendLine();
            sb.AppendLine("## 這是什麼");
            sb.AppendLine("這是一個由 AI Training Studio 產生的 AI 行為包。它把需求、人格、資料來源、參數和整合方式整理成檔案，方便之後交給遊戲、App、API、本機模型或開發板使用。");
            sb.AppendLine();
            sb.AppendLine("重要：這不是已經完成大型神經網路訓練的權重檔。入門模式會產出可讀、可接程式的行為包；如果你要真正微調 LLM、視覺模型或強化學習模型，仍需要 GPU、雲端服務或專門訓練框架。");
            sb.AppendLine();
            sb.AppendLine("## 專案設定");
            sb.AppendLine("- 類型：" + cboMode.Text);
            sb.AppendLine("- 接入目標：" + cboTarget.Text);
            sb.AppendLine("- 模型等級：" + cboModelLevel.Text);
            sb.AppendLine("- 訓練方式：" + cboTrainingBackend.Text);
            sb.AppendLine("- 資料檔數：" + files.Count.ToString());
            sb.AppendLine();
            sb.AppendLine("## 副檔名統計");
            if (extCounts.Count == 0)
            {
                sb.AppendLine("- 尚未加入資料。");
            }
            else
            {
                foreach (KeyValuePair<string, int> item in extCounts)
                {
                    sb.AppendLine("- " + item.Key + "：" + item.Value.ToString());
                }
            }
            sb.AppendLine();
            sb.AppendLine("## 需求");
            sb.AppendLine(EmptyToPlaceholder(txtRequirement.Text));
            sb.AppendLine();
            sb.AppendLine("## 人格 / 風格");
            sb.AppendLine(EmptyToPlaceholder(txtPersonality.Text));
            sb.AppendLine();
            sb.AppendLine("## 使用提醒");
            sb.AppendLine("- 先看 `beginner_steps_zh-TW.md`。");
            sb.AppendLine("- 程式整合先從測試環境開始，不要直接接到正式帳號、正式遊戲伺服器或真實設備。");
            sb.AppendLine("- 封閉遊戲不能只靠複製檔案就讓 NPC 變聰明，必須有官方 Mod、SDK、遊戲原始碼或外部控制介面。");
            return sb.ToString();
        }

        private string BuildBeginnerSteps()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 新手操作教學");
            sb.AppendLine();
            sb.AppendLine("## 第 1 步：確認你產出的檔案");
            sb.AppendLine("- `" + CleanFileName(txtProjectName.Text) + ".aipack`：核心訓練包，包含需求、參數、資料來源。");
            sb.AppendLine("- `training_project.json`：給程式讀的設定檔。");
            sb.AppendLine("- `operation_mapping.csv`：AI 輸出如何對應到實際操作。");
            sb.AppendLine("- `data_manifest.csv`：你加入的訓練資料清單。");
            sb.AppendLine();
            sb.AppendLine("## 第 2 步：先在安全環境測試");
            sb.AppendLine("不要一開始就接真實遊戲帳號、正式伺服器或實體硬體。先用測試場景、測試角色、測試影片確認 AI 輸出合理。");
            sb.AppendLine();

            if (cboMode.SelectedIndex == 0)
            {
                sb.AppendLine("## NPC 接入流程");
                sb.AppendLine("1. 把整個輸出資料夾複製到你的遊戲專案，例如 Unity 的 `Assets/StreamingAssets/AITraining/`。");
                sb.AppendLine("2. 參考 `adapters/unity/NPCBrainAdapter.cs` 或 `adapters/godot/npc_brain_adapter.gd`。");
                sb.AppendLine("3. 在遊戲中把玩家說話、任務狀態、NPC 記憶傳給 adapter。");
                sb.AppendLine("4. 讓 adapter 回傳 `npc_reply`、`intent`、`emotion`、`memory_update`。");
                sb.AppendLine("5. 先用測試 NPC 確認不會劇透、不會亂接任務，再放到正式角色。");
            }
            else if (cboMode.SelectedIndex == 1)
            {
                sb.AppendLine("## 遊戲操作 / 控制策略流程");
                sb.AppendLine("1. 先把遊玩影片或操作紀錄加入資料清單。");
                sb.AppendLine("2. 用 `operation_mapping.csv` 定義每個 AI 動作對應的按鍵或搖桿輸出。");
                sb.AppendLine("3. 若接開發板，先用序列埠輸出文字指令，例如 `ACCELERATE`、`STEER_LEFT`。");
                sb.AppendLine("4. 在開發板端把文字指令轉成合法的 HID 手把輸入。");
                sb.AppendLine("5. 先接測試程式或模擬環境，再接真實主機。不要繞過主機安全機制或違反服務規範。");
            }
            else if (cboMode.SelectedIndex == 2)
            {
                sb.AppendLine("## 文字 AI App 流程");
                sb.AppendLine("1. 打開 `text_app/index.html` 看基本介面。");
                sb.AppendLine("2. 把 `requirements_prompt.txt` 的內容接到你使用的 AI API 或本機模型。");
                sb.AppendLine("3. 如果要做成正式 App，再把 `text_app/app_config.json` 交給工程師或匯入你的應用程式。");
            }
            else
            {
                sb.AppendLine("## 自訂流程");
                sb.AppendLine("1. 先看 `operation_mapping.csv`。");
                sb.AppendLine("2. 把每個 AI 輸出對應到你要執行的 API、檔案操作或硬體指令。");
                sb.AppendLine("3. 先在測試環境跑，再開到正式環境。");
            }

            sb.AppendLine();
            sb.AppendLine("## 第 3 步：真正訓練大型模型時需要什麼");
            sb.AppendLine("- 文字模型：乾淨的問答資料、角色規則、評測題目、API 或本機 LLM。");
            sb.AppendLine("- 影像操作模型：影片畫面、每一段對應的正確操作標註、測試關卡、失敗重播資料。");
            sb.AppendLine("- 強化學習：可重置的模擬環境、獎勵函數、狀態讀取、動作輸出，不建議一開始直接接真機。");
            return sb.ToString();
        }

        private string BuildResourcePlan()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 找資料與標註清單");
            sb.AppendLine();
            sb.AppendLine("這份清單不會自動抓取網路內容。請只使用你有權使用、自己產生、公開授權或獲得許可的資料。");
            sb.AppendLine();

            if (cboMode.SelectedIndex == 0)
            {
                sb.AppendLine("## NPC 建議資料");
                sb.AppendLine("- 角色背景、世界觀、任務規則。");
                sb.AppendLine("- 好的對話範例與不好的對話反例。");
                sb.AppendLine("- 玩家狀態欄位，例如好感度、任務進度、是否觸發過事件。");
                sb.AppendLine("- 不能說的內容，例如劇透、作弊提示、開發者秘密。");
            }
            else if (cboMode.SelectedIndex == 1)
            {
                sb.AppendLine("## 遊戲操作建議資料");
                sb.AppendLine("- 你自己錄製的遊玩影片。");
                sb.AppendLine("- 每段影片對應的操作標註，例如時間、畫面狀態、按鍵。");
                sb.AppendLine("- 失敗案例，例如撞牆、掉出賽道、錯過彎道。");
                sb.AppendLine("- 測試環境，最好先用模擬器、測試遊戲或自製小關卡。");
            }
            else
            {
                sb.AppendLine("## 文字 AI / 自訂流程建議資料");
                sb.AppendLine("- FAQ、產品文件、規則、範例輸入輸出。");
                sb.AppendLine("- 不能回答或要轉人工處理的情境。");
                sb.AppendLine("- 成功與失敗的評測題目。");
            }

            sb.AppendLine();
            sb.AppendLine("## 標註格式建議");
            sb.AppendLine("可以建立 CSV：`time_or_case,input,expected_output,reason,notes`。");
            sb.AppendLine("遊戲控制可使用：`time_ms,screen_state,action,strength,duration_ms,success`。");
            return sb.ToString();
        }

        private string BuildSafetyNotes()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 風險與限制說明");
            sb.AppendLine();
            sb.AppendLine("- 這個工具會產生 AI 行為包、整合範本和教學，不會破解遊戲、不會繞過主機或服務的安全機制。");
            sb.AppendLine("- 封閉遊戲通常不能把檔案放進資料夾就改變 NPC 行為，除非遊戲支援 Mod、官方 SDK、腳本、外掛或你有原始碼。");
            sb.AppendLine("- 接真實設備前，先用測試環境確認輸出不會造成危險、損壞或違反規範。");
            sb.AppendLine("- 使用影片、圖片、文字訓練前，請確認你有權使用這些資料。");
            sb.AppendLine("- 若要接 Nintendo Switch 或其他主機，只使用合法購買、合法授權且不繞過安全機制的控制硬體。");
            sb.AppendLine("- 若要用於公開服務或多人遊戲，先確認服務條款允許自動化。");
            return sb.ToString();
        }

        private string BuildResourceDiscoveryQueue(List<string> files)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("priority,source_type,query_or_url,why_needed,how_to_label,legal_note");

            List<string> urls = SplitLines(txtUrlSources.Text);
            for (int i = 0; i < urls.Count; i++)
            {
                sb.AppendLine(CsvLine("1", "url", urls[i], "使用者指定的外部來源", "先記錄標題、作者、授權、時間點，再標註可用片段", "先確認網站條款與授權，不能未授權下載或重上傳"));
            }

            List<string> keywords = SplitLines(txtSearchKeywords.Text);
            for (int i = 0; i < keywords.Count; i++)
            {
                sb.AppendLine(CsvLine("2", "search_keyword", keywords[i], "沒有足夠本機資料時用來找公開資料", "把找到的資料整理成 input,expected_output,reason,source", "只使用自己有權使用、公開授權或已取得許可的資料"));
            }

            if (files.Count == 0 && urls.Count == 0 && keywords.Count == 0)
            {
                string seed = txtRequirement.Text.Trim();
                if (String.IsNullOrWhiteSpace(seed))
                {
                    seed = txtProjectName.Text.Trim();
                }
                sb.AppendLine(CsvLine("1", "search_keyword", seed, "目前沒有本機檔案、網址或關鍵字，先從需求建立搜尋入口", "找到資料後補上來源、用途與標註", "不要抓取或訓練未授權內容"));
            }

            if (cboMode.SelectedIndex == 1)
            {
                sb.AppendLine(CsvLine("1", "video", "gameplay video with visible screen", "遊戲操作模型需要畫面狀態", "time_ms,screen_state,action,strength,duration_ms,success", "優先使用自己錄製或有授權影片"));
                sb.AppendLine(CsvLine("2", "reference", "controller mapping for target game", "需要知道每個按鍵代表什麼操作", "action_name,button_or_axis,meaning,notes", "只使用官方文件或你自己的設定"));
            }
            else if (cboMode.SelectedIndex == 0)
            {
                sb.AppendLine(CsvLine("2", "document", "character profile and dialogue examples", "NPC 需要人格、世界觀與對話範例", "player_input,npc_reply,intent,memory_update", "使用自己創作或有授權文本"));
            }
            else
            {
                sb.AppendLine(CsvLine("2", "document", "FAQ, rules, product docs", "文字 AI 需要可查詢知識", "question,answer,source,confidence", "確認資料可用於模型或 App"));
            }

            return sb.ToString();
        }

        private string BuildActionInferenceGuide()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 只有畫面時，如何推測操作");
            sb.AppendLine();
            sb.AppendLine("這份文件用於「沒有搖桿紀錄、只有遊玩畫面」的情況。它不會保證推測完全正確，但可以先建立可人工檢查的標註資料。");
            sb.AppendLine();
            sb.AppendLine("## 推測流程");
            sb.AppendLine("1. 先把影片切成片段，例如每 0.5 秒或每個明顯操作一段。");
            sb.AppendLine("2. 觀察畫面狀態：直線、彎道、障礙、敵人、道具、速度、角色位置。");
            sb.AppendLine("3. 根據畫面變化推測動作：直線通常加速；左彎通常左轉；右彎通常右轉；接近障礙可能煞車或閃避；道具出現可能保留或使用。");
            sb.AppendLine("4. 每個推測都加上 confidence。低於 70 的標註要人工檢查。");
            sb.AppendLine("5. 用 `hardware/video_labeling_template.csv` 或自己建立 CSV 記錄。");
            sb.AppendLine();
            sb.AppendLine("## 建議標註欄位");
            sb.AppendLine("`time_ms,screen_state,observed_change,inferred_action,strength,duration_ms,confidence,reason,needs_review`");
            sb.AppendLine();
            sb.AppendLine("## 範例");
            sb.AppendLine("- 畫面：車在直線中央，前方無障礙。推測：ACCELERATE，confidence 90。");
            sb.AppendLine("- 畫面：賽道即將左彎，車靠右側。推測：STEER_LEFT，confidence 80。");
            sb.AppendLine("- 畫面：前方牆壁快速接近。推測：BRAKE 或 STEER_AWAY，confidence 65，需人工檢查。");
            sb.AppendLine();
            sb.AppendLine("## 重要限制");
            sb.AppendLine("只看畫面通常無法知道玩家實際按了哪顆鍵，只能推測「合理動作」。正式訓練最好加入實際控制紀錄、遊戲狀態或人工標註。");
            return sb.ToString();
        }

        private string BuildTrainingBackendPlan()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 訓練方式計畫");
            sb.AppendLine();
            sb.AppendLine("目前選擇：" + cboTrainingBackend.Text);
            sb.AppendLine();
            sb.AppendLine("## 行為包 / 提示詞");
            sb.AppendLine("- 優點：不需要 GPU，最快能做出 NPC、文字 AI 或控制策略雛形。");
            sb.AppendLine("- 產物：`.aipack`、`training_project.json`、提示詞、操作對應表。");
            sb.AppendLine();
            sb.AppendLine("## 大模型 API");
            sb.AppendLine("- 適合：沒有本地 GPU，但想使用較強語言理解、資料整理、角色扮演或標註輔助。");
            sb.AppendLine("- 需要：API Key、後端服務、成本控制、資料授權檢查。");
            sb.AppendLine("- 建議：不要把 API Key 放在前端 HTML；放在後端或本機安全設定。");
            sb.AppendLine();
            sb.AppendLine("## 本地 GPU");
            sb.AppendLine("- 適合：你有可用 GPU 或 AI 加速器，例如 NVIDIA、AMD、Intel、Apple Silicon 或其他硬體，並有對應的訓練框架。");
            sb.AppendLine("- 選擇「本地 GPU 直接訓練」時，會輸出 `local_gpu_train` 資料夾。");
            sb.AppendLine("- 你可以從 `local_gpu_train\\start_local_gpu_training.ps1` 開始。若電腦沒有 Python、PyTorch 或可用加速後端，腳本會提示環境不足。");
            sb.AppendLine("- 遊戲操作模型建議先用模擬環境或離線影片資料，不要直接接真機。");
            sb.AppendLine();
            sb.AppendLine("## 混合");
            sb.AppendLine("- 流程：先用大模型幫你整理資料、產生標註草稿、檢查格式，再用本地 GPU 做微調或策略訓練。");
            sb.AppendLine("- 風險：大模型產生的標註可能錯，必須抽查。");
            return sb.ToString();
        }

        private bool UsesLocalGpuTraining()
        {
            return cboTrainingBackend != null && cboTrainingBackend.Text.IndexOf("GPU", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void WriteLocalGpuTrainingFiles(string projectFolder)
        {
            string folder = Path.Combine(projectFolder, "local_gpu_train");
            Directory.CreateDirectory(folder);
            WriteText(Path.Combine(folder, "README_zh-TW.md"), BuildLocalGpuReadme());
            WriteText(Path.Combine(folder, "train_config.json"), BuildLocalGpuConfig());
            WriteText(Path.Combine(folder, "dataset_template.csv"), BuildLocalGpuDatasetTemplate());
            WriteText(Path.Combine(folder, "start_local_gpu_training.ps1"), BuildLocalGpuStarterPs1());
            WriteText(Path.Combine(folder, "check_gpu.py"), BuildLocalGpuCheckPy());
            WriteText(Path.Combine(folder, "train_stub.py"), BuildLocalGpuTrainStubPy());
        }

        private string BuildLocalGpuReadme()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 本地 GPU 直接訓練");
            sb.AppendLine();
            sb.AppendLine("這個資料夾是給「本地 GPU 直接訓練」使用。它會從你的訓練包設定開始，檢查 Python / PyTorch / 可用 GPU 或加速後端，然後啟動訓練腳本。");
            sb.AppendLine();
            sb.AppendLine("## 需要先準備");
            sb.AppendLine("- 可用 GPU 或 AI 加速器，例如 NVIDIA、AMD、Intel、Apple Silicon 或其他硬體。");
            sb.AppendLine("- Python 3.10+。");
            sb.AppendLine("- 支援你硬體的訓練框架，例如 PyTorch CUDA、ROCm、DirectML、OpenVINO、MPS，或你自己的訓練框架。");
            sb.AppendLine("- 已標註資料，格式可參考 `dataset_template.csv`。");
            sb.AppendLine();
            sb.AppendLine("## 開始");
            sb.AppendLine("在 PowerShell 執行：");
            sb.AppendLine();
            sb.AppendLine("```powershell");
            sb.AppendLine(".\\start_local_gpu_training.ps1");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("如果你的電腦沒有 GPU 訓練環境，腳本會停下來並顯示缺什麼。");
            return sb.ToString();
        }

        private string BuildLocalGpuConfig()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            JsonProp(sb, "  ", "project_name", txtProjectName.Text.Trim(), true);
            JsonProp(sb, "  ", "mode", cboMode.Text, true);
            JsonProp(sb, "  ", "target", cboTarget.Text, true);
            JsonProp(sb, "  ", "training_backend", cboTrainingBackend.Text, true);
            JsonProp(sb, "  ", "dataset_csv", "dataset_template.csv", true);
            JsonProp(sb, "  ", "output_dir", "outputs", true);
            JsonNumberProp(sb, "  ", "epochs", Math.Max(1, barPasses.Value), true);
            JsonNumberProp(sb, "  ", "safety_limit", barSafety.Value, false);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildLocalGpuDatasetTemplate()
        {
            if (cboMode.SelectedIndex == 1)
            {
                return "input_path,time_ms,screen_state,expected_action,strength,duration_ms,confidence,notes\nsample.mp4,0,\"straight road\",ACCELERATE,80,500,90,\"example\"\n";
            }
            if (cboMode.SelectedIndex == 0)
            {
                return "context,player_input,expected_reply,intent,memory_update,notes\n\"village gate\",\"hello\",\"你好，旅人。\",reply,\"met_player=true\",\"example\"\n";
            }
            return "input,expected_output,source,notes\n\"question\",\"answer\",\"manual\",\"example\"\n";
        }

        private string BuildLocalGpuStarterPs1()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("$ErrorActionPreference = 'Stop'");
            sb.AppendLine("Write-Host 'AI Training Studio - Local GPU Training'");
            sb.AppendLine("if (-not (Get-Command python -ErrorAction SilentlyContinue)) {");
            sb.AppendLine("  Write-Host 'ERROR: Python was not found. Install Python 3.10+ first.'");
            sb.AppendLine("  Read-Host 'Press Enter to exit'");
            sb.AppendLine("  exit 1");
            sb.AppendLine("}");
            sb.AppendLine("python .\\check_gpu.py");
            sb.AppendLine("if ($LASTEXITCODE -ne 0) { Read-Host 'Press Enter to exit'; exit $LASTEXITCODE }");
            sb.AppendLine("python .\\train_stub.py --config .\\train_config.json");
            sb.AppendLine("Read-Host 'Training command finished. Press Enter to exit'");
            return sb.ToString();
        }

        private string BuildLocalGpuCheckPy()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("import sys");
            sb.AppendLine("try:");
            sb.AppendLine("    import torch");
            sb.AppendLine("except Exception as e:");
            sb.AppendLine("    print('ERROR: PyTorch is not installed:', e)");
            sb.AppendLine("    sys.exit(2)");
            sb.AppendLine("print('torch:', torch.__version__)");
            sb.AppendLine("backends = []");
            sb.AppendLine("if getattr(torch, 'cuda', None) is not None and torch.cuda.is_available():");
            sb.AppendLine("    backends.append('CUDA')");
            sb.AppendLine("mps = getattr(getattr(torch, 'backends', None), 'mps', None)");
            sb.AppendLine("if mps is not None and mps.is_available():");
            sb.AppendLine("    backends.append('MPS')");
            sb.AppendLine("if backends:");
            sb.AppendLine("    print('accelerator_available:', ', '.join(backends))");
            sb.AppendLine("else:");
            sb.AppendLine("    print('WARNING: PyTorch did not report CUDA or MPS acceleration.')");
            sb.AppendLine("    print('AMD/Intel/DirectML/OpenVINO or custom frameworks may still work, but this starter cannot auto-detect every backend.')");
            sb.AppendLine("    print('Edit train_stub.py or replace it with your backend-specific training script if needed.')");
            return sb.ToString();
        }

        private string BuildLocalGpuTrainStubPy()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("import argparse");
            sb.AppendLine("import csv");
            sb.AppendLine("import json");
            sb.AppendLine("import os");
            sb.AppendLine();
            sb.AppendLine("def main():");
            sb.AppendLine("    parser = argparse.ArgumentParser()");
            sb.AppendLine("    parser.add_argument('--config', required=True)");
            sb.AppendLine("    args = parser.parse_args()");
            sb.AppendLine("    with open(args.config, 'r', encoding='utf-8') as f:");
            sb.AppendLine("        cfg = json.load(f)");
            sb.AppendLine("    dataset = cfg.get('dataset_csv', 'dataset_template.csv')");
            sb.AppendLine("    if not os.path.exists(dataset):");
            sb.AppendLine("        raise SystemExit(f'Missing dataset: {dataset}')");
            sb.AppendLine("    with open(dataset, 'r', encoding='utf-8-sig', newline='') as f:");
            sb.AppendLine("        rows = list(csv.DictReader(f))");
            sb.AppendLine("    os.makedirs(cfg.get('output_dir', 'outputs'), exist_ok=True)");
            sb.AppendLine("    print('Loaded rows:', len(rows))");
            sb.AppendLine("    print('This is the local GPU training entry point.')");
            sb.AppendLine("    print('Replace train_stub.py with your real PyTorch / vision / LLM training code.')");
            sb.AppendLine("    with open(os.path.join(cfg.get('output_dir', 'outputs'), 'training_started.txt'), 'w', encoding='utf-8') as f:");
            sb.AppendLine("        f.write('Local GPU training entry point ran successfully.\\n')");
            sb.AppendLine();
            sb.AppendLine("if __name__ == '__main__':");
            sb.AppendLine("    main()");
            return sb.ToString();
        }

        private string BuildOperationMappingCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("model_signal,plain_meaning,real_world_action,where_to_connect,notes");
            if (cboMode.SelectedIndex == 0)
            {
                sb.AppendLine(CsvLine("npc_reply", "NPC 要說的文字", "顯示在對話框或語音系統", "遊戲對話系統", "接到 UI 文字或 TTS"));
                sb.AppendLine(CsvLine("intent", "NPC 下一步想做什麼", "切換動畫、任務、移動或互動", "NPC 狀態機", "例如 warn_player、offer_quest"));
                sb.AppendLine(CsvLine("emotion", "目前情緒", "切換表情或聲音", "角色動畫系統", "例如 calm、angry、worried"));
                sb.AppendLine(CsvLine("memory_update", "要記住的新事件", "寫入角色記憶檔或資料庫", "存檔系統", "例如 player_helped_villagers=true"));
            }
            else if (cboMode.SelectedIndex == 1)
            {
                sb.AppendLine(CsvLine("ACCELERATE", "加速", "按住 A / 油門", "控制器橋接", "duration_ms 控制按多久"));
                sb.AppendLine(CsvLine("BRAKE", "煞車或倒退", "按住 B / 煞車", "控制器橋接", "先測試遊戲設定"));
                sb.AppendLine(CsvLine("STEER_LEFT", "向左轉", "左搖桿 X 負值", "控制器橋接", "strength 控制轉向幅度"));
                sb.AppendLine(CsvLine("STEER_RIGHT", "向右轉", "左搖桿 X 正值", "控制器橋接", "strength 控制轉向幅度"));
                sb.AppendLine(CsvLine("USE_ITEM", "使用道具", "按下道具鍵", "控制器橋接", "只在策略判斷安全時使用"));
            }
            else
            {
                sb.AppendLine(CsvLine("reply_text", "AI 回答文字", "顯示在 App 聊天視窗", "文字 AI App", "接 API 或本機模型"));
                sb.AppendLine(CsvLine("tool_action", "AI 想執行的工具", "呼叫指定 API 或流程", "後端程式", "先做白名單限制"));
                sb.AppendLine(CsvLine("safety_note", "安全提醒", "顯示警告或轉人工", "App UI", "避免錯誤操作"));
            }
            return sb.ToString();
        }

        private void WriteIntegrationFiles(string projectFolder)
        {
            string adapters = Path.Combine(projectFolder, "adapters");
            Directory.CreateDirectory(adapters);

            string unity = Path.Combine(adapters, "unity");
            Directory.CreateDirectory(unity);
            WriteText(Path.Combine(unity, "NPCBrainAdapter.cs"), BuildUnityAdapter());

            string godot = Path.Combine(adapters, "godot");
            Directory.CreateDirectory(godot);
            WriteText(Path.Combine(godot, "npc_brain_adapter.gd"), BuildGodotAdapter());

            string unreal = Path.Combine(adapters, "unreal");
            Directory.CreateDirectory(unreal);
            WriteText(Path.Combine(unreal, "Unreal_Blueprint_Guide_zh-TW.md"), BuildUnrealGuide());

            WriteText(Path.Combine(adapters, "integration_overview_zh-TW.md"), BuildIntegrationOverview());
        }

        private string BuildUnityAdapter()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("// 新手說明：把這個檔案放到 Unity 專案的 Assets 資料夾。");
            sb.AppendLine("// 再把產出的 .aipack 或 training_project.json 放到 StreamingAssets。");
            sb.AppendLine("// 這個範本負責把玩家訊息整理成 NPC 決策格式；真正的 LLM/API 呼叫請接在 Decide 方法裡。");
            sb.AppendLine("public class NPCBrainAdapter : MonoBehaviour");
            sb.AppendLine("{");
            sb.AppendLine("    public TextAsset aiPackJson;");
            sb.AppendLine("    public string npcState = \"idle\";");
            sb.AppendLine();
            sb.AppendLine("    public NpcDecision Decide(string playerMessage)");
            sb.AppendLine("    {");
            sb.AppendLine("        NpcDecision decision = new NpcDecision();");
            sb.AppendLine("        decision.npc_reply = \"我聽到了：\" + playerMessage;");
            sb.AppendLine("        decision.intent = \"reply\";");
            sb.AppendLine("        decision.emotion = \"calm\";");
            sb.AppendLine("        decision.memory_update = \"last_player_message=\" + playerMessage;");
            sb.AppendLine("        return decision;");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("[System.Serializable]");
            sb.AppendLine("public class NpcDecision");
            sb.AppendLine("{");
            sb.AppendLine("    public string npc_reply;");
            sb.AppendLine("    public string intent;");
            sb.AppendLine("    public string emotion;");
            sb.AppendLine("    public string memory_update;");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildGodotAdapter()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("extends Node");
            sb.AppendLine();
            sb.AppendLine("# 新手說明：把這個檔案放進 Godot 專案。");
            sb.AppendLine("# 這是 NPC AI 行為包的接入範本；真正 API 或本機模型呼叫請接在 decide()。");
            sb.AppendLine();
            sb.AppendLine("var npc_state = \"idle\"");
            sb.AppendLine();
            sb.AppendLine("func decide(player_message: String) -> Dictionary:");
            sb.AppendLine("    return {");
            sb.AppendLine("        \"npc_reply\": \"我聽到了：\" + player_message,");
            sb.AppendLine("        \"intent\": \"reply\",");
            sb.AppendLine("        \"emotion\": \"calm\",");
            sb.AppendLine("        \"memory_update\": \"last_player_message=\" + player_message");
            sb.AppendLine("    }");
            return sb.ToString();
        }

        private string BuildUnrealGuide()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Unreal Engine 接入方式");
            sb.AppendLine();
            sb.AppendLine("1. 把 `training_project.json` 放進專案的 Content 或 Config 管理流程。");
            sb.AppendLine("2. 建立一個 NPC AI Component。");
            sb.AppendLine("3. Component 接收玩家訊息、NPC 狀態、世界事件。");
            sb.AppendLine("4. Component 輸出 `npc_reply`、`intent`、`emotion`、`memory_update`。");
            sb.AppendLine("5. Blueprint 中依照 `intent` 切換對話、動畫、任務或移動。");
            sb.AppendLine();
            sb.AppendLine("若要接大型語言模型，建議在後端服務呼叫 API，再把結果傳回 Unreal，避免 API Key 放在客戶端。");
            return sb.ToString();
        }

        private string BuildIntegrationOverview()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 整合總覽");
            sb.AppendLine();
            sb.AppendLine("核心觀念：AI 不會直接「附身」到任何角色或設備。你需要建立一個轉接層：");
            sb.AppendLine();
            sb.AppendLine("輸入資料 -> AI 行為包 / 模型 -> 標準輸出 -> 遊戲、App、API 或硬體");
            sb.AppendLine();
            sb.AppendLine("標準輸出請看 `operation_mapping.csv`。先讓輸出在測試畫面中可見，再接到真實功能。");
            return sb.ToString();
        }

        private void WriteHardwareFiles(string projectFolder)
        {
            string hardware = Path.Combine(projectFolder, "hardware");
            Directory.CreateDirectory(hardware);
            WriteText(Path.Combine(hardware, "serial_controller_bridge.ino"), BuildSerialBridgeArduino());
            WriteText(Path.Combine(hardware, "controller_policy.json"), BuildControllerPolicyJson());
            WriteText(Path.Combine(hardware, "switch_controller_connection_guide_zh-TW.md"), BuildSwitchGuide());
            WriteText(Path.Combine(hardware, "video_labeling_template.csv"), "time_ms,screen_state,action,strength,duration_ms,success,notes\n0,\"straight road\",ACCELERATE,80,500,true,\"example\"\n");
        }

        private string BuildSerialBridgeArduino()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// 通用序列埠 -> 手把指令橋接範本");
            sb.AppendLine("// 使用方式：電腦端送出文字，例如 ACCELERATE 或 STEER_LEFT。");
            sb.AppendLine("// 你需要依照自己的開發板安裝合法的 USB HID / Gamepad 函式庫，並把下面 TODO 補上。");
            sb.AppendLine();
            sb.AppendLine("String command = \"\";");
            sb.AppendLine();
            sb.AppendLine("void setup() {");
            sb.AppendLine("  Serial.begin(115200);");
            sb.AppendLine("  // TODO: 初始化你的 HID Gamepad 函式庫");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("void loop() {");
            sb.AppendLine("  if (Serial.available() > 0) {");
            sb.AppendLine("    command = Serial.readStringUntil('\\n');");
            sb.AppendLine("    command.trim();");
            sb.AppendLine("    handleCommand(command);");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("void handleCommand(String cmd) {");
            sb.AppendLine("  if (cmd == \"ACCELERATE\") {");
            sb.AppendLine("    // TODO: 按住加速鍵");
            sb.AppendLine("  } else if (cmd == \"BRAKE\") {");
            sb.AppendLine("    // TODO: 按住煞車鍵");
            sb.AppendLine("  } else if (cmd == \"STEER_LEFT\") {");
            sb.AppendLine("    // TODO: 左搖桿往左");
            sb.AppendLine("  } else if (cmd == \"STEER_RIGHT\") {");
            sb.AppendLine("    // TODO: 左搖桿往右");
            sb.AppendLine("  } else if (cmd == \"USE_ITEM\") {");
            sb.AppendLine("    // TODO: 按下道具鍵");
            sb.AppendLine("  } else if (cmd == \"RELEASE\") {");
            sb.AppendLine("    // TODO: 放開所有按鍵");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildControllerPolicyJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            JsonProp(sb, "  ", "schema", "ai-training-studio.controller-policy.v1", true);
            JsonProp(sb, "  ", "project_name", txtProjectName.Text.Trim(), true);
            JsonProp(sb, "  ", "input", "screen_state or video_label", true);
            JsonProp(sb, "  ", "output", "action_name, strength, duration_ms", true);
            sb.AppendLine("  \"actions\": [");
            sb.AppendLine("    { \"name\": \"ACCELERATE\", \"plain\": \"加速\", \"serial\": \"ACCELERATE\" },");
            sb.AppendLine("    { \"name\": \"BRAKE\", \"plain\": \"煞車\", \"serial\": \"BRAKE\" },");
            sb.AppendLine("    { \"name\": \"STEER_LEFT\", \"plain\": \"左轉\", \"serial\": \"STEER_LEFT\" },");
            sb.AppendLine("    { \"name\": \"STEER_RIGHT\", \"plain\": \"右轉\", \"serial\": \"STEER_RIGHT\" },");
            sb.AppendLine("    { \"name\": \"USE_ITEM\", \"plain\": \"使用道具\", \"serial\": \"USE_ITEM\" },");
            sb.AppendLine("    { \"name\": \"RELEASE\", \"plain\": \"放開按鍵\", \"serial\": \"RELEASE\" }");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"guardrails\": [");
            sb.AppendLine("    \"先在測試環境確認輸出\",");
            sb.AppendLine("    \"不要繞過主機安全機制\",");
            sb.AppendLine("    \"多人遊戲或公開服務請先確認服務條款\"");
            sb.AppendLine("  ]");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildSwitchGuide()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Switch / 手把橋接教學");
            sb.AppendLine();
            sb.AppendLine("這份教學只提供通用、合法的控制橋接概念，不提供破解、繞過安全機制或規避服務條款的方法。");
            sb.AppendLine();
            sb.AppendLine("## 基本架構");
            sb.AppendLine("1. 電腦端 AI 產生動作：`ACCELERATE`、`STEER_LEFT` 等。");
            sb.AppendLine("2. 電腦用 USB 序列埠把動作傳給開發板。");
            sb.AppendLine("3. 開發板用合法的 HID Gamepad 函式庫輸出手把按鍵。");
            sb.AppendLine("4. 主機或測試環境接收控制器輸入。");
            sb.AppendLine();
            sb.AppendLine("## 新手建議");
            sb.AppendLine("- 先接 Windows 測試程式或網頁 gamepad 測試器。");
            sb.AppendLine("- 確認每個指令都對應到正確按鍵，再接真實主機。");
            sb.AppendLine("- 先限制速度和動作頻率，避免 AI 連發指令造成不可控。");
            sb.AppendLine("- 使用自己的主機、自己的遊戲、自己的帳號，並確認規範允許。");
            return sb.ToString();
        }

        private void WriteTextAppFiles(string projectFolder)
        {
            string app = Path.Combine(projectFolder, "text_app");
            Directory.CreateDirectory(app);
            WriteText(Path.Combine(app, "app_config.json"), BuildTextAppConfig());
            WriteText(Path.Combine(app, "index.html"), BuildTextAppHtml());
            WriteText(Path.Combine(app, "README_zh-TW.md"), BuildTextAppReadme());
        }

        private string BuildTextAppConfig()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            JsonProp(sb, "  ", "app_name", txtProjectName.Text.Trim(), true);
            JsonProp(sb, "  ", "mode", cboMode.Text, true);
            JsonProp(sb, "  ", "system_prompt", BuildRequirementsPrompt(), true);
            JsonProp(sb, "  ", "api_placeholder", "把你的 OpenAI API、本機 LLM 或公司模型接在這裡。不要把正式 API Key 寫在前端公開檔案。", false);
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string BuildTextAppHtml()
        {
            string prompt = JsonEscape(BuildRequirementsPrompt());
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!doctype html>");
            sb.AppendLine("<html lang=\"zh-Hant\">");
            sb.AppendLine("<head>");
            sb.AppendLine("  <meta charset=\"utf-8\">");
            sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">");
            sb.AppendLine("  <title>" + Html(txtProjectName.Text.Trim()) + "</title>");
            sb.AppendLine("  <style>");
            sb.AppendLine("    body{font-family:'Microsoft JhengHei UI',Arial,sans-serif;margin:0;background:#f3f5f7;color:#1f2933;}");
            sb.AppendLine("    .wrap{max-width:900px;margin:0 auto;padding:20px;}");
            sb.AppendLine("    header{padding:18px 0 12px;border-bottom:1px solid #d8dee6;}");
            sb.AppendLine("    h1{font-size:24px;margin:0 0 6px;}");
            sb.AppendLine("    .chat{background:white;border:1px solid #d8dee6;border-radius:8px;margin-top:16px;min-height:360px;padding:14px;}");
            sb.AppendLine("    .msg{padding:10px 12px;border-radius:8px;margin:8px 0;line-height:1.55;}");
            sb.AppendLine("    .user{background:#e8f0fe;}");
            sb.AppendLine("    .bot{background:#f1f5f9;}");
            sb.AppendLine("    textarea{width:100%;min-height:90px;margin-top:14px;font:inherit;padding:10px;box-sizing:border-box;}");
            sb.AppendLine("    button{margin-top:10px;padding:10px 16px;font:inherit;cursor:pointer;}");
            sb.AppendLine("    details{margin-top:14px;}");
            sb.AppendLine("  </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class=\"wrap\">");
            sb.AppendLine("    <header>");
            sb.AppendLine("      <h1>" + Html(txtProjectName.Text.Trim()) + "</h1>");
            sb.AppendLine("      <div>這是可雙擊開啟的文字 AI App 範本。要變成真正 AI，請把 sendToModel() 接到你的 API 或本機模型。</div>");
            sb.AppendLine("    </header>");
            sb.AppendLine("    <div id=\"chat\" class=\"chat\"><div class=\"msg bot\">請輸入問題。範本會示範介面與提示詞串接位置。</div></div>");
            sb.AppendLine("    <textarea id=\"input\" placeholder=\"輸入你要問 AI 的內容\"></textarea>");
            sb.AppendLine("    <button onclick=\"send()\">送出</button>");
            sb.AppendLine("    <details><summary>查看系統提示詞</summary><pre id=\"prompt\"></pre></details>");
            sb.AppendLine("  </div>");
            sb.AppendLine("  <script>");
            sb.AppendLine("    const systemPrompt = \"" + prompt + "\";");
            sb.AppendLine("    document.getElementById('prompt').textContent = systemPrompt;");
            sb.AppendLine("    function add(role,text){const d=document.createElement('div');d.className='msg '+role;d.textContent=text;document.getElementById('chat').appendChild(d);}");
            sb.AppendLine("    function send(){const box=document.getElementById('input');const text=box.value.trim();if(!text)return;add('user',text);box.value='';sendToModel(text).then(r=>add('bot',r));}");
            sb.AppendLine("    async function sendToModel(userText){");
            sb.AppendLine("      return '範本回覆：我已收到「'+userText+'」。請把 sendToModel() 改成呼叫你的 AI API 或本機模型，並把 systemPrompt 一起送出。';");
            sb.AppendLine("    }");
            sb.AppendLine("  </script>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            return sb.ToString();
        }

        private string BuildTextAppReadme()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# 文字 AI App 範本");
            sb.AppendLine();
            sb.AppendLine("直接雙擊 `index.html` 可以開啟介面。");
            sb.AppendLine();
            sb.AppendLine("目前它是範本，不會真的呼叫大型模型。要變成真正 AI：");
            sb.AppendLine("1. 打開 `index.html`。");
            sb.AppendLine("2. 找到 `sendToModel(userText)`。");
            sb.AppendLine("3. 改成呼叫你的 OpenAI API、本機 LLM 或後端服務。");
            sb.AppendLine("4. 把 `systemPrompt` 一起送給模型。");
            sb.AppendLine();
            sb.AppendLine("正式 App 不建議把 API Key 放在 HTML 裡，應該放在後端。");
            return sb.ToString();
        }

        private string CleanFileName(string text)
        {
            if (text == null)
            {
                return "";
            }

            string name = text.Trim();
            char[] invalid = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalid.Length; i++)
            {
                name = name.Replace(invalid[i], '_');
            }
            if (String.IsNullOrWhiteSpace(name))
            {
                name = "AI_Project";
            }
            return name;
        }

        private void WriteText(string path, string content)
        {
            string folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }

        private string EmptyToPlaceholder(string text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                return "尚未填寫。";
            }
            return text.Trim();
        }

        private void JsonProp(StringBuilder sb, string indent, string name, string value, bool comma)
        {
            sb.Append(indent).Append("\"").Append(JsonEscape(name)).Append("\": \"").Append(JsonEscape(value)).Append("\"");
            if (comma)
            {
                sb.Append(",");
            }
            sb.AppendLine();
        }

        private void JsonNumberProp(StringBuilder sb, string indent, string name, int value, bool comma)
        {
            sb.Append(indent).Append("\"").Append(JsonEscape(name)).Append("\": ").Append(value.ToString());
            if (comma)
            {
                sb.Append(",");
            }
            sb.AppendLine();
        }

        private void JsonBoolProp(StringBuilder sb, string indent, string name, bool value, bool comma)
        {
            sb.Append(indent).Append("\"").Append(JsonEscape(name)).Append("\": ").Append(value ? "true" : "false");
            if (comma)
            {
                sb.Append(",");
            }
            sb.AppendLine();
        }

        private void WriteJsonStringArray(StringBuilder sb, string indent, string name, List<string> values, bool comma)
        {
            sb.Append(indent).Append("\"").Append(JsonEscape(name)).Append("\": [");
            if (values.Count == 0)
            {
                sb.Append("]");
                if (comma)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
                return;
            }

            sb.AppendLine();
            for (int i = 0; i < values.Count; i++)
            {
                sb.Append(indent).Append("  \"").Append(JsonEscape(values[i])).Append("\"");
                if (i < values.Count - 1)
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.Append(indent).Append("]");
            if (comma)
            {
                sb.Append(",");
            }
            sb.AppendLine();
        }

        private bool HasText(string text)
        {
            return !String.IsNullOrWhiteSpace(text);
        }

        private List<string> SplitLines(string text)
        {
            List<string> values = new List<string>();
            if (String.IsNullOrWhiteSpace(text))
            {
                return values;
            }

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string value = lines[i].Trim();
                if (value.Length > 0)
                {
                    values.Add(value);
                }
            }
            return values;
        }

        private string JsonEscape(string text)
        {
            if (text == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                switch (c)
                {
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '"':
                        sb.Append("\\\"");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (c < 32)
                        {
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            return sb.ToString();
        }

        private string CsvLine(string a, string b, string c, string d, string e)
        {
            return Csv(a) + "," + Csv(b) + "," + Csv(c) + "," + Csv(d) + "," + Csv(e);
        }

        private string CsvLine(string a, string b, string c, string d, string e, string f)
        {
            return Csv(a) + "," + Csv(b) + "," + Csv(c) + "," + Csv(d) + "," + Csv(e) + "," + Csv(f);
        }

        private string Csv(string text)
        {
            if (text == null)
            {
                text = "";
            }
            return "\"" + text.Replace("\"", "\"\"") + "\"";
        }

        private string Html(string text)
        {
            if (text == null)
            {
                return "";
            }
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }
    }
}
