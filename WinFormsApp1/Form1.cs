using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using WinFormsApp1.Interop;

namespace WinFormsApp1;

public sealed class Form1 : Form
{
    private readonly TextBox _logTextBox;
    private readonly CheckBox _hookCheckBox;
    private readonly Label _statusLabel;
    private readonly Label _counterLabel;
    private readonly KeyboardHook _keyboardHook;
    private int _eventCount;

    public Form1()
    {
        Text = "Keyboard Hook Laboratory";
        Width = 850;
        Height = 500;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyPressed += OnHookKeyPressed;

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical
        };

        _hookCheckBox = new CheckBox
        {
            Text = "Включить low-level hook",
            AutoSize = true,
            Margin = new Padding(8)
        };

        var clearButton = new Button
        {
            Text = "Очистить журнал",
            AutoSize = true
        };

        _statusLabel = new Label
        {
            Text = "Hook не установлен",
            AutoSize = true,
            Margin = new Padding(8)
        };

        _counterLabel = new Label
        {
            Text = "Событий: 0",
            AutoSize = true,
            Margin = new Padding(8)
        };

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            WrapContents = false
        };

        toolbar.Controls.Add(_hookCheckBox);
        toolbar.Controls.Add(clearButton);
        toolbar.Controls.Add(_statusLabel);
        toolbar.Controls.Add(_counterLabel);

        Controls.Add(_logTextBox);
        Controls.Add(toolbar);

        KeyDown += OnLocalKeyDown;

        clearButton.Click += (_, _) =>
        {
            _logTextBox.Clear();
        };

        _hookCheckBox.CheckedChanged += OnHookCheckedChanged;
        Activated += OnFormActivated;
        Deactivate += OnFormDeactivated;
        FormClosing += OnFormClosing;
    }

    private void OnLocalKeyDown(object? sender, KeyEventArgs e)
    {
        CountEvent();
        AppendLog("WinForms", e.KeyCode.ToString());
    }

    private void OnHookKeyPressed(Keys key, bool isSystemKey)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (!IsDisposed)
                {
                    CountEvent();
                    AppendLog(
                        isSystemKey ? "Low-level hook WM_SYSKEYDOWN" : "Low-level hook",
                        key.ToString());
                }
            }));
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"Не удалось обновить журнал: {ex}");
        }
    }

    private void CountEvent()
    {
        _eventCount++;
        _counterLabel.Text = $"Событий: {_eventCount}";
    }

    private void AppendLog(string source, string key)
    {
        string timestamp = DateTimeOffset.Now.ToString("HH:mm:ss.fff");
        _logTextBox.AppendText($"[{timestamp}] [{source}] {key}{Environment.NewLine}");
    }

    private void OnHookCheckedChanged(object? sender, EventArgs e)
    {
        if (_hookCheckBox.Checked)
        {
            if (Form.ActiveForm == this)
                StartHook();
        }
        else
        {
            StopHook();
        }
    }

    private void OnFormActivated(object? sender, EventArgs e)
    {
        if (_hookCheckBox.Checked)
            StartHook();
    }

    private void OnFormDeactivated(object? sender, EventArgs e)
    {
        StopHook();
    }

    private void StartHook()
    {
        if (_keyboardHook.IsInstalled)
            return;

        try
        {
            _keyboardHook.Start();
            _statusLabel.Text = "Hook установлен";
            AppendLog("System", "Hook установлен");
        }
        catch (Win32Exception ex)
        {
            _statusLabel.Text = "Ошибка установки hook";
            AppendLog("Error", ex.Message);
            _hookCheckBox.Checked = false;
        }
    }

    private void StopHook()
    {
        if (!_keyboardHook.IsInstalled)
            return;

        try
        {
            _keyboardHook.Stop();
            _statusLabel.Text = "Hook не установлен";
            AppendLog("System", "Hook снят");
        }
        catch (Win32Exception ex)
        {
            _statusLabel.Text = "Ошибка снятия hook";
            AppendLog("Error", ex.Message);
        }
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _hookCheckBox.Enabled = false;
        StopHook();

        try
        {
            _keyboardHook.Dispose();
        }
        catch (Win32Exception ex)
        {
            Debug.WriteLine($"Ошибка освобождения hook: {ex}");
        }
    }
}