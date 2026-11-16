using System;
using System.Drawing;
using System.Windows.Forms;

namespace WorstPC
{
    internal static class InputBox
    {
        public static string Show(IWin32Window owner, string title, string prompt)
        {
            Form f = new Form();
            f.Text = title;
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MinimizeBox = false; f.MaximizeBox = false;
            f.ClientSize = new Size(460, 150);
            f.BackColor = Theme.BG;
            f.ForeColor = Theme.TEXT;
            f.Font = new Font("Segoe UI", 9.5f);

            Label l = new Label();
            l.Text = prompt;
            l.ForeColor = Theme.SUB;
            l.AutoSize = false;
            l.Location = new Point(16, 14);
            l.Size = new Size(428, 44);
            f.Controls.Add(l);

            TextBox t = new TextBox();
            t.BackColor = Theme.RAISE; t.ForeColor = Theme.TEXT; t.BorderStyle = BorderStyle.FixedSingle;
            t.Location = new Point(16, 66); t.Width = 428;
            f.Controls.Add(t);

            Button ok = new Button();
            ok.Text = "OK"; ok.FlatStyle = FlatStyle.Flat;
            ok.BackColor = Theme.ASH; ok.ForeColor = Theme.INK;
            ok.FlatAppearance.BorderColor = Theme.LINE_HI;
            ok.Size = new Size(96, 32); ok.Location = new Point(348, 104);
            ok.DialogResult = DialogResult.OK;
            f.Controls.Add(ok);

            Button cancel = new Button();
            cancel.Text = "Cancel"; cancel.FlatStyle = FlatStyle.Flat;
            cancel.BackColor = Theme.RAISE; cancel.ForeColor = Theme.TEXT;
            cancel.FlatAppearance.BorderColor = Theme.LINE_HI;
            cancel.Size = new Size(96, 32); cancel.Location = new Point(244, 104);
            cancel.DialogResult = DialogResult.Cancel;
            f.Controls.Add(cancel);

            f.AcceptButton = ok; f.CancelButton = cancel;

            return f.ShowDialog(owner) == DialogResult.OK ? t.Text.Trim() : "";
        }
    }
}
