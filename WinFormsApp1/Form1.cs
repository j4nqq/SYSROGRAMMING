namespace WinFormsApp1;

public sealed class Form1 : Form
{
    private readonly TextBox _logTextBox;

    public Form1()
    {
        Text = "Keyboard Events";
        Width = 700;
        Height = 450;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        _logTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical
        };
        Controls.Add(_logTextBox);

        KeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            _logTextBox.Clear();
            return;
        }

        _logTextBox.AppendText($"Нажата клавиша: {e.KeyCode}{Environment.NewLine}");
    }

    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _logTextBox.AppendText($"Отпущена клавиша: {e.KeyCode}{Environment.NewLine}");
    }
}