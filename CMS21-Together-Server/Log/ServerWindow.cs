using System;
using System.Linq;
using CMS21_Together_Server.Data;
using CMS21_Together_Server.Network;
using Terminal.Gui;

namespace CMS21_Together_Server.Log
{
    public class ServerWindow : Window
    {
        public static ColoredLogView LogView;
        private TextField CommandInput;
        private Label StatusLabel;
        private Label PlayersLabel;
        private Label GameStateLabel;

        public ServerWindow() : base($"CMS21 Together Server v{Program.SERVER_VERSION}")
        {
            // Dashboard (Top)
            var dashboardFrame = new FrameView("Dashboard")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 4
            };

            StatusLabel = new Label("Time: ") { X = 1, Y = 0 };
            PlayersLabel = new Label("Players: 0/0") { X = Pos.Right(StatusLabel) + 5, Y = 0 };
            GameStateLabel = new Label("Game State: Not Loaded") { X = 1, Y = 1 };

            dashboardFrame.Add(StatusLabel, PlayersLabel, GameStateLabel);
            Add(dashboardFrame);

            // Log View (Middle)
            var logFrame = new FrameView("Logs")
            {
                X = 0,
                Y = Pos.Bottom(dashboardFrame),
                Width = Dim.Fill(),
                Height = Dim.Fill() - 3
            };

            LogView = new ColoredLogView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = Dim.Fill()
            };

            logFrame.Add(LogView);
            Add(logFrame);

            // Input (Bottom)
            var inputFrame = new FrameView("Command")
            {
                X = 0,
                Y = Pos.Bottom(logFrame),
                Width = Dim.Fill(),
                Height = 3
            };

            CommandInput = new TextField("")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill()
            };

            CommandInput.KeyPress += (args) =>
            {
                if (args.KeyEvent.Key == Key.Enter)
                {
                    string cmd = CommandInput.Text.ToString();
                    CommandInput.Text = "";
                    if (!string.IsNullOrWhiteSpace(cmd))
                    {
                        CommandSystem.Execute(cmd);
                    }
                    args.Handled = true;
                }
            };

            inputFrame.Add(CommandInput);
            Add(inputFrame);

            // Timer for UI Update & Server Tick
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(10), (loop) =>
            {
                TickServer();
                UpdateDashboard();
                return true; // Keep repeating
            });
        }

        private void TickServer()
        {
            if (ServerTime.Time - GameDataManager.lastAutoSaveTime >= GameDataManager.AutoSaveInterval)
            {
                GameDataManager.lastAutoSaveTime = ServerTime.Time;
                GameDataManager.SaveSession();
            }
            Network.Server.Update();
        }

        private void UpdateDashboard()
        {
            StatusLabel.Text = $"Time: {DateTime.Now:HH:mm:ss}";
            PlayersLabel.Text = $"Players: {Network.Server.Clients.Count(c => c.Value.IsConnected)}/{Program.Config.MaxPlayers}";

            if (GameDataManager.CurrentState != null && GameDataManager.CurrentState.WorldState != null)
            {
                var state = GameDataManager.CurrentState.WorldState;
                GameStateLabel.Text = $"Gamemode: {state.Gamemode} | Level: {state.Level} | Exp: {state.Exp} | Money: {state.Money}$";
            }
            else
            {
                GameStateLabel.Text = "Game State: Not Loaded";
            }
        }
    }

    public class ColoredLogView : View
    {
        public class LogEntry
        {
            public string Message;
            public ConsoleColor Color;
        }

        private System.Collections.Generic.List<LogEntry> _logs = new System.Collections.Generic.List<LogEntry>();
        private int _maxLogs = 1000;
        private int _scrollOffset = 0;

        public ColoredLogView()
        {
            CanFocus = true;
        }

        public override bool ProcessKey(KeyEvent kb)
        {
            if (kb.Key == Key.CursorUp) { ScrollUp(1); return true; }
            if (kb.Key == Key.CursorDown) { ScrollDown(1); return true; }
            if (kb.Key == Key.PageUp) { ScrollUp(Bounds.Height); return true; }
            if (kb.Key == Key.PageDown) { ScrollDown(Bounds.Height); return true; }
            return base.ProcessKey(kb);
        }

        public override bool MouseEvent(MouseEvent mouseEvent)
        {
            if (mouseEvent.Flags.HasFlag(MouseFlags.WheeledUp)) { ScrollUp(3); return true; }
            if (mouseEvent.Flags.HasFlag(MouseFlags.WheeledDown)) { ScrollDown(3); return true; }
            return base.MouseEvent(mouseEvent);
        }

        private void ScrollUp(int lines)
        {
            lock (_logs)
            {
                int maxLines = Bounds.Height;
                int maxScroll = Math.Max(0, _logs.Count - maxLines);
                _scrollOffset = Math.Min(_scrollOffset + lines, maxScroll);
                SetNeedsDisplay();
            }
        }

        private void ScrollDown(int lines)
        {
            lock (_logs)
            {
                _scrollOffset = Math.Max(_scrollOffset - lines, 0);
                SetNeedsDisplay();
            }
        }

        public void AddLog(string message, ConsoleColor color)
        {
            lock (_logs)
            {
                _logs.Add(new LogEntry { Message = message, Color = color });
                if (_logs.Count > _maxLogs)
                {
                    _logs.RemoveAt(0);
                    if (_scrollOffset > 0) _scrollOffset--;
                }
            }
            Application.MainLoop.Invoke(() => SetNeedsDisplay());
        }

        public override void Redraw(Rect bounds)
        {
            Driver.SetAttribute(ColorScheme.Normal);
            Clear();
            
            lock (_logs)
            {
                int maxLines = bounds.Height;
                int startIdx = Math.Max(0, _logs.Count - maxLines - _scrollOffset);
                int count = Math.Min(maxLines, _logs.Count);

                for (int i = 0; i < count; i++)
                {
                    if (startIdx + i < _logs.Count)
                    {
                        var entry = _logs[startIdx + i];
                        var uiColor = (Color)(int)entry.Color;
                        if (uiColor == Color.Black) uiColor = Color.White;
                        
                        var attr = Application.Driver.MakeAttribute(uiColor, Color.Black);
                        Driver.SetAttribute(attr);
                        
                        Move(0, i);
                        string msg = entry.Message;
                        if (msg.Length > bounds.Width)
                        {
                            msg = msg.Substring(0, bounds.Width);
                        }
                        Driver.AddStr(msg);
                    }
                }
            }
        }
    }
}
