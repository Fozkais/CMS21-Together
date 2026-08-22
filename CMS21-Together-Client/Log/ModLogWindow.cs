using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS21Together.Log
{
	public class ModLogWindow : Form
	{
		private readonly RichTextBox _textBox;

		public ModLogWindow()
		{
			Text = "CMS21-Together - Mod Console";
			Width = 900;
			Height = 500;
			BackColor = Color.Black;

			_textBox = new RichTextBox
			{
				Dock = DockStyle.Fill,
				ReadOnly = true,
				BackColor = Color.Black,
				ForeColor = Color.Gainsboro,
				Font = new Font("Consolas", 9.5f),
				BorderStyle = BorderStyle.None
			};
			Controls.Add(_textBox);

			// Hide instead of closing: the mod keeps logging for the whole game session.
			FormClosing += (sender, args) =>
			{
				if (args.CloseReason == CloseReason.UserClosing)
				{
					args.Cancel = true;
					Hide();
				}
			};
		}

		public void AppendLog(string line, Color color)
		{
			if (_textBox.IsDisposed) return;

			if (_textBox.InvokeRequired)
			{
				_textBox.BeginInvoke(new Action(() => AppendLog(line, color)));
				return;
			}

			_textBox.SelectionStart = _textBox.TextLength;
			_textBox.SelectionLength = 0;
			_textBox.SelectionColor = color;
			_textBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {line}{Environment.NewLine}");
			_textBox.ScrollToCaret();
		}
	}
}
