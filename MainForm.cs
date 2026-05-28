using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Media;           // SoundPlayer — available in .NET 8 on Windows
using System.Windows.Forms;

namespace CybersecurityChatBot
{
    public class MainForm : Form
    {
        // ── Engine ────────────────────────────────────────────────────────────
        private readonly ChatbotEngine _engine = new ChatbotEngine();
        private bool _nameEntered = false;

        // ── Asset path — matches your bin\Debug\net8.0-windows\Assets folder ──
        // On .NET 8 the binary lands in  bin\Debug\net8.0-windows\
        // but the assets folder was placed in  bin\Debug\assets\
        // So we walk up one level from BaseDirectory and look there first;
        // if not found we fall back to BaseDirectory itself (covers both layouts).
        private static readonly string AssetsDir = ResolveAssetsDir();
        private static string ResolveAssetsDir()
        {
            string baseDir  = AppDomain.CurrentDomain.BaseDirectory;
            string inBase   = Path.Combine(baseDir, "assets");
            if (Directory.Exists(inBase)) return inBase;

            // Walk up one level: bin\Debug\net8.0-windows\ → bin\Debug\
            string parentDir = Path.GetDirectoryName(baseDir.TrimEnd(Path.DirectorySeparatorChar)) ?? baseDir;
            string inParent  = Path.Combine(parentDir, "assets");
            if (Directory.Exists(inParent)) return inParent;

            return inBase; // default — let PlayAudio's File.Exists check handle missing files gracefully
        }

        // ── Controls ──────────────────────────────────────────────────────────
        private Panel       pnlHeader;
        private Label       lblTitle;
        private Label       lblSubtitle;
        private Label       lblOnline;
        private Panel       pnlMemBar;
        private Label       lblMemName;
        private Label       lblMemTopic;
        private Label       lblMemSentiment;
        private FlowLayoutPanel flpTopics;
        private Panel       pnlChat;        // holds the chat bubbles
        private Panel       pnlScroll;      // scrollable container
        private VScrollBar  vScroll;
        private Button      btnScrollBottom; // floating scroll-to-bottom arrow
        private Panel       pnlInput;
        private TextBox     txtInput;
        private Button      btnSend;
        private StatusStrip statusBar;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblClock;

        // ── Colours ───────────────────────────────────────────────────────────
        static Color C(int r, int g, int b) => Color.FromArgb(r, g, b);
        readonly Color BgApp      = C( 12,  22,  42);
        readonly Color BgHeader   = C( 16,  32,  62);
        readonly Color BgMemBar   = C(  8,  18,  38);
        readonly Color BgTopics   = C( 12,  26,  52);
        readonly Color BgInput    = C(  8,  18,  38);
        readonly Color BgBot      = C( 16,  40,  78);
        readonly Color BgUser     = C(  0,  72, 144);
        readonly Color BgChat     = C( 10,  20,  40);
        readonly Color AccBlue    = C(  0, 162, 255);
        readonly Color AccGreen   = C(  0, 210, 120);
        readonly Color AccWarn    = C(255, 165,  50);
        readonly Color TxtMain    = C(215, 235, 255);
        readonly Color TxtMuted   = C(100, 140, 185);
        readonly Color TxtBot     = C(200, 230, 255);
        readonly Color TxtUser    = C(230, 245, 255);
        readonly Color Border     = C( 28,  68, 120);

        // ── Fonts ─────────────────────────────────────────────────────────────
        readonly Font FChat  = new Font("Segoe UI",  10f);
        readonly Font FLabel = new Font("Segoe UI",   8f, FontStyle.Bold);
        readonly Font FInput = new Font("Segoe UI",  10f);
        readonly Font FBtn   = new Font("Segoe UI",   9f, FontStyle.Bold);
        readonly Font FTitle = new Font("Segoe UI",  16f, FontStyle.Bold);
        readonly Font FSub   = new Font("Segoe UI",   9f);
        readonly Font FMem   = new Font("Segoe UI",   8f);
        readonly Font FTopic = new Font("Segoe UI",   8f, FontStyle.Bold);

        // ── Quick topics ──────────────────────────────────────────────────────
        static readonly string[] Topics = {
            "password","phishing","scam","privacy",
            "malware","ransomware","vpn","two-factor"
        };
        static readonly Color[] TopicClr = {
            C(0,90,165), C(0,110,85), C(140,55,0), C(88,0,128),
            C(140,24,24),C(0,100,130),C(70,70,0),  C(0,72,90)
        };

        // ── Chat panel content ─────────────────────────────────────────────
        private int _chatY = 8;   // next bubble Y inside pnlChat

        public MainForm()
        {
            Build();
            WireEngine();
            PlayAudio("greeting.wav");
            ShowWelcome();
        }

        // ── Wire delegate events ──────────────────────────────────────────────
        private void WireEngine()
        {
            _engine.OnSentimentDetected += (s, _) =>
            {
                lblMemSentiment.Text      = $"  |  Mood: {s}";
                lblMemSentiment.ForeColor = AccWarn;
                SetStatus($"Mood detected: {s}", AccWarn);
            };
            _engine.OnMemoryUpdated += (k, v) =>
            {
                RefreshMemBar();
                SetStatus(k == "name" ? $"Name remembered: {v}" : $"Interest remembered: {v}", AccGreen);
            };
        }

        // ═════════════════════════════════════════════════════════════════════
        // BUILD UI
        // ═════════════════════════════════════════════════════════════════════
        private void Build()
        {
            Text          = "Cybersecurity Awareness Chatbot";
            Size          = new Size(960, 660);
            MinimumSize   = new Size(720, 600);
            BackColor     = BgApp;
            ForeColor     = TxtMain;
            StartPosition = FormStartPosition.CenterScreen;
            Icon          = SystemIcons.Shield;

            BuildHeader();
            BuildMemBar();
            BuildTopicsBar();
            BuildChatArea();
            BuildInputBar();
            BuildStatusBar();

            // Dock order (bottom-up for DockStyle stacking)
            statusBar.Dock  = DockStyle.Bottom;
            pnlInput.Dock   = DockStyle.Bottom;
            pnlChat.Dock    = DockStyle.Fill;
            flpTopics.Dock  = DockStyle.Top;
            pnlMemBar.Dock  = DockStyle.Top;
            pnlHeader.Dock  = DockStyle.Top;

            Controls.Add(pnlChat);
            Controls.Add(flpTopics);
            Controls.Add(pnlMemBar);
            Controls.Add(pnlHeader);
            Controls.Add(pnlInput);
            Controls.Add(statusBar);
        }

        // ── Header ────────────────────────────────────────────────────────────
        private void BuildHeader()
        {
            pnlHeader = new Panel { Height = 90, BackColor = BgHeader };
            pnlHeader.Paint += (s, e) =>
            {
                var g = e.Graphics;
                using (var b = new LinearGradientBrush(new Rectangle(0,0,6,pnlHeader.Height),
                    AccBlue, BgHeader, LinearGradientMode.Horizontal))
                    g.FillRectangle(b, 0, 0, 6, pnlHeader.Height);
                using (var p = new Pen(Border))
                    g.DrawLine(p, 0, pnlHeader.Height-1, pnlHeader.Width, pnlHeader.Height-1);
            };

            lblTitle = new Label
            {
                Text      = "CYBERSECURITY AWARENESS CHATBOT",
                Font      = FTitle,
                ForeColor = AccBlue,
                Location  = new Point(18, 16),
                AutoSize  = true
            };
            lblSubtitle = new Label
            {
                Text      = "Keyword recognition  •  Sentiment detection  •  Memory recall  •  Random responses",
                Font      = FSub,
                ForeColor = TxtMuted,
                Location  = new Point(20, 50),
                AutoSize  = true
            };
            lblOnline = new Label
            {
                Text      = "●  SECURE SESSION",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AccGreen,
                Location  = new Point(20, 68),
                AutoSize  = true
            };

            var blinkT = new System.Windows.Forms.Timer { Interval = 900 };
            blinkT.Tick += (s, e) =>
                lblOnline.ForeColor = lblOnline.ForeColor == AccGreen ? BgHeader : AccGreen;
            blinkT.Start();

            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSubtitle, lblOnline });
        }

        // ── Memory bar ────────────────────────────────────────────────────────
        private void BuildMemBar()
        {
            pnlMemBar = new Panel { Height = 28, BackColor = BgMemBar, Visible = false };
            pnlMemBar.Paint += (s, e) =>
            {
                using (var p = new Pen(Border))
                {
                    e.Graphics.DrawLine(p, 0, 0, pnlMemBar.Width, 0);
                    e.Graphics.DrawLine(p, 0, pnlMemBar.Height-1, pnlMemBar.Width, pnlMemBar.Height-1);
                }
            };

            var ico = new Label { Text="🧠  MEMORY:", Font=new Font("Segoe UI",8f,FontStyle.Bold),
                                  ForeColor=AccBlue, Location=new Point(10,7), AutoSize=true };
            lblMemName      = new Label { Text="", Font=FMem, ForeColor=TxtMain,   Location=new Point(96,7),  AutoSize=true };
            lblMemTopic     = new Label { Text="", Font=FMem, ForeColor=AccGreen,  Location=new Point(260,7), AutoSize=true };
            lblMemSentiment = new Label { Text="", Font=FMem, ForeColor=AccWarn,   Location=new Point(440,7), AutoSize=true };
            pnlMemBar.Controls.AddRange(new Control[]{ ico, lblMemName, lblMemTopic, lblMemSentiment });
        }

        // ── Quick-topic buttons ───────────────────────────────────────────────
        private void BuildTopicsBar()
        {
            flpTopics = new FlowLayoutPanel
            {
                Height        = 40,
                BackColor     = BgTopics,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(8, 6, 8, 0),
                AutoScroll    = false
            };
            flpTopics.Paint += (s, e) =>
            {
                using (var p = new Pen(Border))
                    e.Graphics.DrawLine(p, 0, flpTopics.Height-1, flpTopics.Width, flpTopics.Height-1);
            };

            var lbl = new Label { Text="Quick Topics:", Font=new Font("Segoe UI",8f,FontStyle.Bold),
                                  ForeColor=TxtMuted, AutoSize=true, Margin=new Padding(2,4,8,0) };
            flpTopics.Controls.Add(lbl);

            for (int i = 0; i < Topics.Length; i++)
            {
                string topic = Topics[i];
                Color  clr   = TopicClr[i];
                var btn = new Button
                {
                    Text      = topic,
                    Font      = FTopic,
                    ForeColor = Color.White,
                    BackColor = clr,
                    FlatStyle = FlatStyle.Flat,
                    Height    = 26,
                    AutoSize  = true,
                    Cursor    = Cursors.Hand,
                    Margin    = new Padding(2, 0, 2, 0),
                    Tag       = topic
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor =
                    C(Math.Min(clr.R+40,255), Math.Min(clr.G+40,255), Math.Min(clr.B+40,255));
                btn.Click += (s, e) =>
                {
                    txtInput.Text = $"What is {(string)((Button)s).Tag}?";
                    DoSend();
                };
                flpTopics.Controls.Add(btn);
            }
        }

        // ── Chat area ─────────────────────────────────────────────────────────
        private void BuildChatArea()
        {
            pnlChat = new Panel { BackColor = BgChat, Padding = new Padding(0) };
            pnlChat.Resize += (s, e) => pnlChat.Invalidate();

            pnlScroll = new Panel
            {
                BackColor  = BgChat,
                Left       = 0,
                Top        = 0,
                Width      = 900,
                AutoScroll = false
            };

            vScroll = new VScrollBar
            {
                Dock    = DockStyle.Right,
                Minimum = 0,
                Maximum = 0,
                Value   = 0
            };
            vScroll.Scroll += (s, e) =>
            {
                pnlScroll.Top = -vScroll.Value;
                bool atBottom = vScroll.Value >= vScroll.Maximum - vScroll.LargeChange;
                btnScrollBottom.Visible = !atBottom;
                PositionScrollButton();
            };

            btnScrollBottom = new Button
            {
                Text      = "▼",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = C(0, 100, 200),
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(38, 38),
                Cursor    = Cursors.Hand,
                Visible   = false,
                TabStop   = false
            };
            btnScrollBottom.FlatAppearance.BorderSize        = 0;
            btnScrollBottom.FlatAppearance.MouseOverBackColor = C(0, 130, 240);
            btnScrollBottom.Paint += (s, e) =>
            {
                var btn  = (Button)s;
                var g    = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var b = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    g.FillEllipse(b, 2, 4, btn.Width - 4, btn.Height - 4);
                using (var b = new SolidBrush(btn.BackColor))
                    g.FillEllipse(b, 0, 0, btn.Width - 2, btn.Height - 2);
                var sf = new StringFormat { Alignment = StringAlignment.Center,
                                            LineAlignment = StringAlignment.Center };
                using (var b = new SolidBrush(Color.White))
                    g.DrawString("▼", btn.Font, b,
                        new RectangleF(0, 0, btn.Width - 2, btn.Height - 2), sf);
            };
            btnScrollBottom.Click += (s, e) => ScrollToBottom();

            pnlChat.Controls.Add(pnlScroll);
            pnlChat.Controls.Add(vScroll);
            pnlChat.Controls.Add(btnScrollBottom);

            pnlChat.Resize += (s, e) =>
            {
                pnlScroll.Width = pnlChat.Width - vScroll.Width;
                PositionScrollButton();
                UpdateScrollbar();
            };
        }

        private void PositionScrollButton()
        {
            int rightEdge = pnlChat.Width - (vScroll.Visible ? vScroll.Width : 0) - 14;
            btnScrollBottom.Left = rightEdge - btnScrollBottom.Width;
            btnScrollBottom.Top  = pnlChat.Height - btnScrollBottom.Height - 14;
            btnScrollBottom.BringToFront();
        }

        // ── Input bar ─────────────────────────────────────────────────────────
        private void BuildInputBar()
        {
            pnlInput = new Panel { Height = 56, BackColor = BgInput };
            pnlInput.Paint += (s, e) =>
            {
                using (var p = new Pen(Border))
                    e.Graphics.DrawLine(p, 0, 0, pnlInput.Width, 0);
            };

            txtInput = new TextBox
            {
                Font        = FInput,
                BackColor   = BgHeader,
                ForeColor   = TxtMain,
                BorderStyle = BorderStyle.FixedSingle,
                Location    = new Point(12, 14),
                Height      = 30,
                Anchor      = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            txtInput.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; DoSend(); }
            };

            btnSend = new Button
            {
                Text      = "SEND  ►",
                Font      = FBtn,
                BackColor = AccBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(105, 30),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                Cursor    = Cursors.Hand
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += (s, e) => DoSend();

            pnlInput.Controls.AddRange(new Control[] { txtInput, btnSend });
            pnlInput.Resize += (s, e) =>
            {
                txtInput.Width = pnlInput.Width - 132;
                btnSend.Left   = pnlInput.Width - 117;
                btnSend.Top    = 14;
            };
        }

        // ── Status bar ────────────────────────────────────────────────────────
        private void BuildStatusBar()
        {
            statusBar = new StatusStrip { BackColor = C(6,12,28), SizingGrip = false };
            lblStatus = new ToolStripStatusLabel("Ready")
                { Font = new Font("Segoe UI",8f), ForeColor = TxtMuted };
            var spring = new ToolStripStatusLabel { Spring = true };
            lblClock   = new ToolStripStatusLabel(DateTime.Now.ToString("HH:mm:ss"))
                { Font = new Font("Segoe UI",8f), ForeColor = C(45,75,115) };
            statusBar.Items.AddRange(new ToolStripItem[]{ lblStatus, spring, lblClock });
            var t = new System.Windows.Forms.Timer { Interval = 1000 };
            t.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss  |  dd MMM yyyy");
            t.Start();
        }

        // ═════════════════════════════════════════════════════════════════════
        // BUBBLE FACTORIES
        // ═════════════════════════════════════════════════════════════════════
        private void AddBotBubble(string text)
        {
            int margin = 14;
            int maxW   = pnlScroll.Width - margin * 2 - 10;

            var bubble = new Panel
            {
                BackColor   = BgBot,
                Left        = margin,
                Top         = _chatY,
                Width       = maxW,
                BorderStyle = BorderStyle.None,
                Padding     = new Padding(14, 10, 14, 12),
                Cursor      = Cursors.Default
            };
            bubble.Paint += BubblePaint_Bot;

            var lblName = new Label
            {
                Text      = "🤖   CYBER-BOT",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AccBlue,
                AutoSize  = true,
                Location  = new Point(14, 10)
            };
            bubble.Controls.Add(lblName);

            var divider = new Panel
            {
                BackColor = Border,
                Height    = 1,
                Left      = 14,
                Top       = 30,
                Width     = maxW - 28
            };
            bubble.Controls.Add(divider);

            var rtb = new RichTextBox
            {
                Text        = text,
                Font        = FChat,
                ForeColor   = TxtBot,
                BackColor   = BgBot,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.None,
                WordWrap    = true,
                DetectUrls  = false,
                Left        = 14,
                Top         = 38,
                Width       = maxW - 28
            };
            rtb.Height = MeasureRtbHeight(rtb, text);
            bubble.Controls.Add(rtb);
            bubble.Height = rtb.Top + rtb.Height + 16;

            pnlScroll.Controls.Add(bubble);
            _chatY += bubble.Height + 10;
            pnlScroll.Height = Math.Max(pnlChat.Height, _chatY + 10);
            UpdateScrollbar();
            ScrollToBottom();
        }

        private void AddUserBubble(string text)
        {
            int margin = 14;
            int maxW   = (int)(pnlScroll.Width * 0.70);
            int left   = pnlScroll.Width - maxW - margin;

            var bubble = new Panel
            {
                BackColor   = BgUser,
                Left        = left,
                Top         = _chatY,
                Width       = maxW,
                BorderStyle = BorderStyle.None,
                Padding     = new Padding(14, 10, 14, 12)
            };
            bubble.Paint += BubblePaint_User;

            var lblYou = new Label
            {
                Text      = "YOU  👤",
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = AccGreen,
                AutoSize  = true,
                Location  = new Point(14, 10)
            };
            bubble.Controls.Add(lblYou);

            var divider = new Panel
            {
                BackColor = C(0, 100, 180),
                Height    = 1,
                Left      = 14,
                Top       = 30,
                Width     = maxW - 28
            };
            bubble.Controls.Add(divider);

            var rtb = new RichTextBox
            {
                Text        = text,
                Font        = FChat,
                ForeColor   = TxtUser,
                BackColor   = BgUser,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.None,
                WordWrap    = true,
                DetectUrls  = false,
                Left        = 14,
                Top         = 38,
                Width       = maxW - 28
            };
            rtb.Height = MeasureRtbHeight(rtb, text);
            bubble.Controls.Add(rtb);
            bubble.Height = rtb.Top + rtb.Height + 16;

            pnlScroll.Controls.Add(bubble);
            _chatY += bubble.Height + 10;
            pnlScroll.Height = Math.Max(pnlChat.Height, _chatY + 10);
            UpdateScrollbar();
            ScrollToBottom();
        }

        // ── Rounded-rect bubble painting ──────────────────────────────────────
        private void BubblePaint_Bot(object sender, PaintEventArgs e)
        {
            PaintBubble(sender as Panel, e, BgBot, AccBlue, false);
        }
        private void BubblePaint_User(object sender, PaintEventArgs e)
        {
            PaintBubble(sender as Panel, e, BgUser, C(0,120,210), true);
        }
        private void PaintBubble(Panel p, PaintEventArgs e, Color bg, Color border, bool right)
        {
            if (p == null) return;
            var g   = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, p.Width - 1, p.Height - 1);
            int r    = 12;

            using (var b = new SolidBrush(bg))
            using (var path = RoundedRect(rect, r))
                g.FillPath(b, path);

            using (var pen = new Pen(border, 1))
            using (var path = RoundedRect(rect, r))
                g.DrawPath(pen, path);

            using (var b = new SolidBrush(border))
            {
                if (!right)
                    g.FillRectangle(b, 0, 0, 4, p.Height);
                else
                    g.FillRectangle(b, p.Width - 4, 0, 4, p.Height);
            }
        }
        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, radius*2, radius*2, 180, 90);
            path.AddArc(r.Right - radius*2, r.Y, radius*2, radius*2, 270, 90);
            path.AddArc(r.Right - radius*2, r.Bottom - radius*2, radius*2, radius*2, 0, 90);
            path.AddArc(r.X, r.Bottom - radius*2, radius*2, radius*2, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ── Scrollbar helpers ─────────────────────────────────────────────────
        private void UpdateScrollbar()
        {
            int content = _chatY + 10;
            int visible = pnlChat.Height;
            if (content <= visible)
            {
                vScroll.Visible         = false;
                pnlScroll.Top           = 0;
                btnScrollBottom.Visible = false;
            }
            else
            {
                vScroll.Visible = true;
                vScroll.Maximum = content - visible + vScroll.LargeChange;
                bool atBottom = vScroll.Value >= vScroll.Maximum - vScroll.LargeChange;
                btnScrollBottom.Visible = !atBottom;
                PositionScrollButton();
            }
        }
        private void ScrollToBottom()
        {
            if (vScroll.Visible)
            {
                int max = Math.Max(0, _chatY + 10 - pnlChat.Height);
                vScroll.Value           = Math.Min(max, vScroll.Maximum - vScroll.LargeChange + 1);
                pnlScroll.Top           = -vScroll.Value;
                btnScrollBottom.Visible = false;
            }
        }

        // ── RichTextBox auto-height ───────────────────────────────────────────
        private int MeasureRtbHeight(RichTextBox rtb, string _)
        {
            pnlScroll.Controls.Add(rtb);
            rtb.Height = 10;
            rtb.ScrollBars = RichTextBoxScrollBars.None;
            var size = rtb.GetPreferredSize(new Size(rtb.Width, 0));
            pnlScroll.Controls.Remove(rtb);
            return Math.Max(size.Height + 10, 24);
        }

        // ═════════════════════════════════════════════════════════════════════
        // ── ASCII art converted from cyber-lock.jpg ──────────────────────────
        private static readonly string[] AsciiArt = new[]
        {
            "  .... .... ....  .:.  .:.  .:.  .:.. .... .... ....  .",
            ".......................................................",
            "..........................::...........................",
            "...... .... .... .::..::=*++*=-:..::. .... .... ..:....",
            "... ..:.  ......-***+:-:+#+=**-----++-....... .:.. ....",
            ".............-::-+=+*:---++*+=:::=*+*+:-::.............",
            "..........=+++------::-:::::::::------=-=+++:..........",
            "........ +#+++#=-:-::------------::-:--*+=++*:.........",
            "  .... ::=*+=+*-.:---::........::---::.+*++++::..... ..",
            ".......=-.-++--:---.  .:=+##*=-.. .:--:--===::-:.......",
            ".....-*=+=-=-.:---++*#####++#####*++=--:::--:--=:......",
            ".....=*-*+:--.:=.-@#*+++=++=+==++**@* --.:-:--=--:.....",
            "...  :-+-:---:=: -@+==++*+++=*+++==%*..=--=--:---. ....",
            "....-*****-.--=. -@*++=++=*++=*+=++%+  :=-::+****=.....",
            "....=###*#-.--- .:@*=++*:.-=:.+++++@= .:---.*#++#*.....",
            ".....=+**+---:=...*%+++*. =#  =+=+#%:. -----=**++:.....",
            "  ...::--::---=- .:%%++*-::-::+*=*@=  .=-:-::--:::.....",
            ".....-+**=::-.:--. :#%*+++++++=+#%=  .=:::-:-***=......",
            ".....=*+*+-:::-:--. .+%%*==+=+#%*: .:-:::::---==:......",
            ".......--:-====:::--:..=*%#*%#+: .:--::-==+=-:-........",
            "... ...:.=***++=:------::-++-:::--:-:-:**++**:::.. ....",
            "  .......=+***+=:.-:::----::----:::-.:-**==**:. .... ..",
            "..........-===---=+==:-.::---:::--=*+--:=++=...........",
            ".............:::=#=+*-::-+***=-::+%#%*:-::.............",
            ".........:......:=++=.--**++*#=-::+++-.:....... .......",
            "... ..:.  .... .... .::::++++-:-:.  ... ....  .... ....",
            ".......................................................",
            ".......................................................",
        };

        // ═════════════════════════════════════════════════════════════════════
        // LOGIC
        // ═════════════════════════════════════════════════════════════════════
        private void ShowWelcome()
        {
            int panelW = Math.Max(400, pnlScroll.Width - 28);

            var asciiPnl = new Panel
            {
                BackColor = C(8, 18, 38),
                Left      = 14,
                Top       = _chatY,
                Width     = panelW,
                Height    = 1
            };

            var titleLbl = new Label
            {
                Text      = "  CYBERSECURITY AWARENESS CHATBOT",
                Font      = new Font("Consolas", 9f, FontStyle.Bold),
                ForeColor = AccBlue,
                AutoSize  = true,
                Location  = new Point(12, 10)
            };
            asciiPnl.Controls.Add(titleLbl);

            var sep1 = new Panel { BackColor = AccBlue, Left = 12, Top = 30, Height = 1, Width = panelW - 24 };
            asciiPnl.Controls.Add(sep1);

            var artLbl = new Label
            {
                Text      = string.Join("\r\n", AsciiArt),
                Font      = new Font("Consolas", 7f),
                ForeColor = AccBlue,
                AutoSize  = true,
                Location  = new Point(12, 38)
            };
            asciiPnl.Controls.Add(artLbl);

            asciiPnl.Controls.Add(new Label()); // dummy to force layout
            int artBottom = artLbl.Location.Y + artLbl.PreferredHeight + 10;

            var sep2 = new Panel { BackColor = Border, Left = 12, Top = artBottom, Height = 1, Width = panelW - 24 };
            asciiPnl.Controls.Add(sep2);

            var subLbl = new Label
            {
                Text      = "  Keyword recognition  •  Sentiment detection  •  Memory recall  •  Random responses",
                Font      = new Font("Consolas", 7.5f),
                ForeColor = TxtMuted,
                AutoSize  = true,
                Location  = new Point(12, artBottom + 6)
            };
            asciiPnl.Controls.Add(subLbl);

            asciiPnl.Height = artBottom + subLbl.PreferredHeight + 18;

            asciiPnl.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(AccBlue, 1))
                using (var path = RoundedRect(
                    new Rectangle(0, 0, asciiPnl.Width - 1, asciiPnl.Height - 1), 8))
                    e.Graphics.DrawPath(pen, path);
            };

            pnlScroll.Controls.Add(asciiPnl);
            _chatY += asciiPnl.Height + 10;
            pnlScroll.Height = Math.Max(pnlChat.Height, _chatY + 10);
            UpdateScrollbar();
            ScrollToBottom();

            AddBotBubble(
                "Welcome to the Cybersecurity Awareness Chatbot!\n\n" +
                "I am here to help you stay safe online.\n\n" +
                "Hello! What is your name?");

            SetStatus("Waiting for your name...", AccBlue);
        }

        private void DoSend()
        {
            string raw = txtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(raw)) return;
            txtInput.Clear();
            txtInput.Focus();

            if (!_nameEntered)
            {
                string name = raw.Trim().Split(' ')[0];
                if (name.Length < 1) return;
                name = char.ToUpper(name[0]) + name.Substring(1).ToLower();
                _engine.UserName = name;
                _nameEntered = true;
                pnlMemBar.Visible = true;
                RefreshMemBar();

                AddUserBubble(raw);
                string greet = _engine.GetResponse("hello");
                AddBotBubble(greet);
                SetStatus($"Session started — {name}", AccGreen);
                return;
            }

            string low = raw.ToLower().Trim();
            if (low == "exit" || low == "quit" || low == "bye" || low == "goodbye")
            {
                AddUserBubble(raw);
                string farewell = $"Thank you{(_engine.UserName != "" ? ", " + _engine.UserName : "")} for using the Cybersecurity Awareness Chatbot!\n\nStay safe online and remember: a little knowledge goes a long way. 🛡️";
                AddBotBubble(farewell);
                PlayAudio("closing.wav");
                SetStatus("Session ended.", C(210, 70, 70));
                btnSend.Enabled  = false;
                txtInput.Enabled = false;
                return;
            }

            AddUserBubble(raw);
            string response = _engine.GetResponse(raw);
            AddBotBubble(response);
            PlayAudio("question.wav");
            SetStatus("Response delivered.", AccGreen);
        }

        private void RefreshMemBar()
        {
            lblMemName.Text  = _engine.UserName       != "" ? $"Name: {_engine.UserName}" : "";
            lblMemTopic.Text = _engine.FavouriteTopic != "" ? $"  |  Interest: {_engine.FavouriteTopic}" : "";
        }

        private void SetStatus(string msg, Color color)
        {
            if (statusBar.InvokeRequired)
                statusBar.Invoke(new Action(() => SetStatus(msg, color)));
            else { lblStatus.Text = msg; lblStatus.ForeColor = color; }
        }

        // ── Audio — System.Media.SoundPlayer (.NET 8 Windows) ─────────────────
        // SoundPlayer plays WAV files asynchronously on Windows.
        // It does not support MP3; keep your assets as .wav files.
        private void PlayAudio(string fileName)
        {
            try
            {
                string path = Path.Combine(AssetsDir, fileName);
                if (!File.Exists(path)) return;

                // Each call creates a fresh SoundPlayer so overlapping calls
                // do not block each other.
                var player = new SoundPlayer(path);
                player.Play(); // async — does not block the UI thread
            }
            catch { /* Audio failure is non-fatal */ }
        }
    }
}
