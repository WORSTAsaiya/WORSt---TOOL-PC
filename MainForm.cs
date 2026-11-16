using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace WorstPC
{
    internal sealed class MainForm : Form
    {
        private readonly Settings _s = Settings.Load();
        private string _pendingSecret = "";
        private string _meName = "";

        private DataGridView _grid;
        private TextBox _placeBox;
        private TextBox _vipBox;
        private CheckBox _multi;
        private CheckBox _selAll;
        private Button _loginBtn, _launchBtn, _refreshBtn;
        private Label _status, _who;

        public MainForm()
        {
            Text = "WORST - Account Manager";
            BackColor = Theme.BG;
            ForeColor = Theme.TEXT;
            Font = new Font("Segoe UI", 9.5f);
            ClientSize = new Size(760, 560);
            MinimumSize = new Size(620, 440);
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            Launcher.SetMultiRoblox(_s.MultiRoblox);

            Load += delegate
            {
                if (!string.IsNullOrEmpty(_s.Wsid)) Refresh(false);
            };
        }

        // ---------------- UI ----------------

        private void BuildUi()
        {
            // header
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 58;
            header.BackColor = Theme.CARD;
            Controls.Add(header);

            Label logo = new Label();
            logo.Text = "W";
            logo.ForeColor = Theme.TEXT;
            logo.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            logo.AutoSize = true;
            logo.Location = new Point(16, 14);
            header.Controls.Add(logo);

            Label title = new Label();
            title.Text = "WORST  ·  ACCOUNT MANAGER";
            title.ForeColor = Theme.TEXT;
            title.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            title.AutoSize = true;
            title.Location = new Point(40, 20);
            header.Controls.Add(title);

            _who = new Label();
            _who.Text = "Not logged in";
            _who.ForeColor = Theme.SUB;
            _who.AutoSize = true;
            _who.Font = new Font("Segoe UI", 9f);
            header.Controls.Add(_who);

            _loginBtn = MakeButton("Login", Theme.ASH, Theme.INK);
            _loginBtn.Size = new Size(96, 32);
            _loginBtn.Click += delegate { if (string.IsNullOrEmpty(_s.Wsid)) StartLogin(); else Logout(); };
            header.Controls.Add(_loginBtn);

            header.Resize += delegate
            {
                _loginBtn.Location = new Point(header.Width - _loginBtn.Width - 14, 13);
                _who.Location = new Point(header.Width - _loginBtn.Width - 14 - _who.Width - 14, 21);
            };

            // bottom bar
            Panel bottom = new Panel();
            bottom.Dock = DockStyle.Bottom;
            bottom.Height = 126;
            bottom.BackColor = Theme.CARD;
            Controls.Add(bottom);

            Label gidLbl = new Label();
            gidLbl.Text = "Game ID";
            gidLbl.ForeColor = Theme.SUB;
            gidLbl.AutoSize = true;
            gidLbl.Location = new Point(16, 14);
            bottom.Controls.Add(gidLbl);

            _placeBox = MakeInput();
            _placeBox.Location = new Point(16, 34);
            _placeBox.Width = 240;
            _placeBox.Text = _s.PlaceId;
            bottom.Controls.Add(_placeBox);

            Label vipLbl = new Label();
            vipLbl.Text = "VIP key (for launching)";
            vipLbl.ForeColor = Theme.SUB;
            vipLbl.AutoSize = true;
            vipLbl.Location = new Point(16, 70);
            bottom.Controls.Add(vipLbl);

            _vipBox = MakeInput();
            _vipBox.Location = new Point(16, 90);
            _vipBox.Width = 240;
            _vipBox.Text = _s.VipKey;
            bottom.Controls.Add(_vipBox);

            _multi = new CheckBox();
            _multi.Text = "Allow multiple Roblox";
            _multi.ForeColor = Theme.TEXT;
            _multi.AutoSize = true;
            _multi.Checked = _s.MultiRoblox;
            _multi.Location = new Point(280, 36);
            _multi.FlatStyle = FlatStyle.Flat;
            _multi.CheckedChanged += delegate
            {
                _s.MultiRoblox = _multi.Checked;
                Launcher.SetMultiRoblox(_multi.Checked);
                _s.Save();
            };
            bottom.Controls.Add(_multi);

            _selAll = new CheckBox();
            _selAll.Text = "Select all";
            _selAll.ForeColor = Theme.TEXT;
            _selAll.AutoSize = true;
            _selAll.Location = new Point(280, 64);
            _selAll.FlatStyle = FlatStyle.Flat;
            _selAll.CheckedChanged += delegate { SetAllChecks(_selAll.Checked); };
            bottom.Controls.Add(_selAll);

            _refreshBtn = MakeButton("Refresh", Theme.RAISE, Theme.TEXT);
            _refreshBtn.Size = new Size(110, 34);
            _refreshBtn.Click += delegate { Refresh(true); };
            bottom.Controls.Add(_refreshBtn);

            _launchBtn = MakeButton("Launch selected", Theme.ASH, Theme.INK);
            _launchBtn.Size = new Size(170, 40);
            _launchBtn.Click += delegate { LaunchSelected(); };
            bottom.Controls.Add(_launchBtn);

            _status = new Label();
            _status.Text = "";
            _status.ForeColor = Theme.SUB;
            _status.AutoSize = false;
            _status.Height = 20;
            _status.TextAlign = ContentAlignment.MiddleLeft;
            bottom.Controls.Add(_status);

            bottom.Resize += delegate
            {
                _launchBtn.Location = new Point(bottom.Width - _launchBtn.Width - 16, 34);
                _refreshBtn.Location = new Point(bottom.Width - _launchBtn.Width - 16 - _refreshBtn.Width - 10, 37);
                _status.Location = new Point(16, bottom.Height - 24);
                _status.Width = bottom.Width - 32;
            };

            // account grid
            _grid = new DataGridView();
            _grid.Dock = DockStyle.Fill;
            _grid.BackgroundColor = Theme.BG;
            _grid.BorderStyle = BorderStyle.None;
            _grid.GridColor = Theme.LINE;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.AllowUserToResizeRows = false;
            _grid.RowHeadersVisible = false;
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.EnableHeadersVisualStyles = false;
            _grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            _grid.ColumnHeadersHeight = 34;
            _grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.CARD;
            _grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.SUB;
            _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.CARD;
            _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _grid.DefaultCellStyle.BackColor = Theme.CARD;
            _grid.DefaultCellStyle.ForeColor = Theme.TEXT;
            _grid.DefaultCellStyle.SelectionBackColor = Theme.RAISE;
            _grid.DefaultCellStyle.SelectionForeColor = Theme.TEXT;
            _grid.DefaultCellStyle.Padding = new Padding(4, 0, 0, 0);
            _grid.RowTemplate.Height = 32;
            _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;

            DataGridViewCheckBoxColumn cPick = new DataGridViewCheckBoxColumn();
            cPick.HeaderText = "";
            cPick.Width = 34;
            cPick.Name = "pick";
            DataGridViewTextBoxColumn cName = new DataGridViewTextBoxColumn();
            cName.HeaderText = "Account";
            cName.Name = "name";
            cName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DataGridViewTextBoxColumn cStat = new DataGridViewTextBoxColumn();
            cStat.HeaderText = "Status";
            cStat.Name = "status";
            cStat.Width = 110;
            _grid.Columns.Add(cPick);
            _grid.Columns.Add(cName);
            _grid.Columns.Add(cStat);

            // commit checkbox toggles immediately
            _grid.CurrentCellDirtyStateChanged += delegate
            {
                if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            _grid.CellDoubleClick += delegate (object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex >= 0) LaunchRow(e.RowIndex);
            };

            Controls.Add(_grid);
            _grid.BringToFront();
        }

        private Button MakeButton(string text, Color bg, Color fg)
        {
            Button b = new Button();
            b.Text = text;
            b.FlatStyle = FlatStyle.Flat;
            b.BackColor = bg;
            b.ForeColor = fg;
            b.FlatAppearance.BorderColor = Theme.LINE_HI;
            b.FlatAppearance.BorderSize = 1;
            b.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            return b;
        }

        private TextBox MakeInput()
        {
            TextBox t = new TextBox();
            t.BackColor = Theme.RAISE;
            t.ForeColor = Theme.TEXT;
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = new Font("Segoe UI", 9.5f);
            return t;
        }

        // ---------------- login ----------------

        private void StartLogin()
        {
            _pendingSecret = Api.RandomHex(24);
            string hash = Api.Sha256Hex(_pendingSecret);
            SetStatus("Opening Discord login in your browser...", Theme.SUB);
            Run(delegate
            {
                string url = Api.AuthStart(hash);
                OnUi(delegate
                {
                    if (url == null) { SetStatus(Api.LastError, Theme.REDSOFT); return; }
                    try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
                    catch { }
                    SetStatus("Approve in Discord. Waiting... (or click Login again, then 'Paste code')", Theme.SUB);
                    OfferPasteCode();
                });
            });
        }

        private void OfferPasteCode()
        {
            // fallback if the worstdash:// handoff doesn't fire
            string code = InputBox.Show(this, "Paste login code",
                "If the browser didn't return automatically, paste the code (or the whole worstdash:// link) here:");
            if (!string.IsNullOrEmpty(code)) HandleCode(ExtractCode(code));
        }

        public void QueueProtocol(string url)
        {
            HandleCode(ExtractCode(url));
        }

        private static string ExtractCode(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            int i = s.IndexOf("code=", StringComparison.OrdinalIgnoreCase);
            if (i < 0) return s.Trim();
            string rest = s.Substring(i + 5);
            int amp = rest.IndexOf('&');
            if (amp >= 0) rest = rest.Substring(0, amp);
            return rest.Trim();
        }

        private void HandleCode(string code)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(_pendingSecret)) return;
            SetStatus("Finishing login...", Theme.SUB);
            string secret = _pendingSecret;
            Run(delegate
            {
                string sid = Api.AuthClaim(code, secret);
                OnUi(delegate
                {
                    if (sid == null) { SetStatus(Api.LastError, Theme.REDSOFT); return; }
                    _pendingSecret = "";
                    _s.Wsid = sid; _s.Save();
                    SetStatus("Logged in.", Theme.GREEN);
                    SetLoginUi();
                    Refresh(false);
                });
            });
        }

        private void Logout()
        {
            _s.Wsid = ""; _s.Save();
            _grid.Rows.Clear();
            _meName = "";
            SetLoginUi();
            SetStatus("Logged out.", Theme.SUB);
        }

        private void SetLoginUi()
        {
            bool inp = !string.IsNullOrEmpty(_s.Wsid);
            _loginBtn.Text = inp ? "Log out" : "Login";
            _who.Text = inp ? (string.IsNullOrEmpty(_meName) ? "Logged in" : _meName) : "Not logged in";
            _who.ForeColor = inp ? Theme.TEXT : Theme.SUB;
        }

        // ---------------- bank ----------------

        private void Refresh(bool force)
        {
            if (string.IsNullOrEmpty(_s.Wsid)) { SetStatus("Log in first.", Theme.SUB); return; }
            SetStatus("Loading accounts...", Theme.SUB);
            Run(delegate
            {
                List<Account> list = Api.BankList(_s.Wsid);
                OnUi(delegate
                {
                    if (list == null)
                    {
                        SetStatus(Api.LastError, Theme.REDSOFT);
                        if (Api.LastError != null && Api.LastError.StartsWith("Log in")) Logout();
                        return;
                    }
                    Fill(list);
                    SetLoginUi();
                    SetStatus(list.Count + (list.Count == 1 ? " account." : " accounts."), Theme.SUB);
                });
            });
        }

        private void Fill(List<Account> list)
        {
            _grid.Rows.Clear();
            foreach (Account a in list)
            {
                int idx = _grid.Rows.Add(false, a.Display(), a.StatusText());
                DataGridViewRow row = _grid.Rows[idx];
                row.Tag = a;
                if (a.Active == true) row.Cells["status"].Style.ForeColor = Theme.GREEN;
                else if (a.Active == false) row.Cells["status"].Style.ForeColor = Theme.REDSOFT;
                else row.Cells["status"].Style.ForeColor = Theme.SUB;
            }
            _selAll.Checked = false;
        }

        private void SetAllChecks(bool on)
        {
            foreach (DataGridViewRow r in _grid.Rows) r.Cells["pick"].Value = on;
        }

        // ---------------- launch ----------------

        private void LaunchSelected()
        {
            string place = _placeBox.Text.Trim();
            if (place.Length == 0) { SetStatus("Enter a Game ID first.", Theme.REDSOFT); return; }
            _s.PlaceId = place; _s.VipKey = _vipBox.Text.Trim(); _s.Save();

            List<Account> picked = new List<Account>();
            foreach (DataGridViewRow r in _grid.Rows)
            {
                object v = r.Cells["pick"].Value;
                if (v is bool && (bool)v && r.Tag is Account) picked.Add((Account)r.Tag);
            }
            if (picked.Count == 0) { SetStatus("Tick at least one account.", Theme.REDSOFT); return; }

            if (_multi.Checked) Launcher.SetMultiRoblox(true);
            string token = _vipBox.Text.Trim();
            string device = _s.DeviceId;
            int delay = _s.JoinDelayMs;

            Run(delegate
            {
                int ok = 0;
                for (int i = 0; i < picked.Count; i++)
                {
                    Account a = picked[i];
                    OnUi(delegate { SetStatus("Launching " + a.Display() + "...", Theme.SUB); });
                    string cookie = Api.FetchCookie(a.Key, token, device);
                    if (cookie == null) { string er = Api.LastError; OnUi(delegate { SetStatus(a.Display() + ": " + er, Theme.REDSOFT); }); continue; }
                    string ticket = Api.GetTicket(cookie);
                    if (ticket == null) { string er = Api.LastError; OnUi(delegate { SetStatus(a.Display() + ": " + er, Theme.REDSOFT); }); continue; }
                    string err;
                    bool launched = Launcher.Launch(ticket, place, out err);
                    if (!launched) { OnUi(delegate { SetStatus(a.Display() + ": " + err, Theme.REDSOFT); }); continue; }
                    ok++;
                    if (i < picked.Count - 1) Thread.Sleep(delay);
                }
                int done = ok;
                OnUi(delegate { SetStatus("Launched " + done + " of " + picked.Count + ".", done > 0 ? Theme.GREEN : Theme.REDSOFT); });
            });
        }

        private void LaunchRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _grid.Rows.Count) return;
            Account a = _grid.Rows[rowIndex].Tag as Account;
            if (a == null) return;
            foreach (DataGridViewRow r in _grid.Rows) r.Cells["pick"].Value = false;
            _grid.Rows[rowIndex].Cells["pick"].Value = true;
            LaunchSelected();
        }

        // ---------------- helpers ----------------

        private void SetStatus(string text, Color color)
        {
            _status.Text = text;
            _status.ForeColor = color;
        }

        private void Run(Action work)
        {
            Thread t = new Thread(delegate () { try { work(); } catch { } });
            t.IsBackground = true;
            t.Start();
        }

        private void OnUi(Action a)
        {
            try { if (IsHandleCreated) BeginInvoke(a); } catch { }
        }
    }
}
