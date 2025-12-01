namespace MiniGuys;

using Godot;

public partial class Hud : CanvasLayer
{
    private const float WarningStartSeconds = 10f;

    private Label? _stoneLabel;
    private Label? _waveLabel;
    private Label? _baseHpLabel;
    private Label? _countdownLabel;
    private Label? _warningLabel;
    private Button? _wallButton;
    private Button? _towerButton;

    private float _countdownRemaining;
    private bool _isPreparation;
    private float _warningBlinkTimer;

    [Signal]
    public delegate void WallButtonPressedEventHandler();

    [Signal]
    public delegate void TowerButtonPressedEventHandler();

    public override void _Ready()
    {
        _stoneLabel = GetNode<Label>("Panel/HBoxContainer/StoneLabel");
        _waveLabel = GetNode<Label>("Panel/HBoxContainer/WaveLabel");
        _baseHpLabel = GetNode<Label>("Panel/HBoxContainer/BaseHpLabel");
        _countdownLabel = GetNode<Label>("Panel/HBoxContainer/CountdownLabel");
        _wallButton = GetNode<Button>("Panel/HBoxContainer/WallButton");
        _towerButton = GetNode<Button>("Panel/HBoxContainer/TowerButton");
        _warningLabel = GetNode<Label>("WarningLabel");
        _warningLabel.Visible = false;

        _wallButton.Pressed += () => EmitSignal(SignalName.WallButtonPressed);
        _towerButton.Pressed += () => EmitSignal(SignalName.TowerButtonPressed);

        _wallButton.Disabled = true;
        _towerButton.Disabled = true;
    }

    public override void _Process(double delta)
    {
        if (_warningLabel is null || !_isPreparation)
            return;

        var showWarning = _countdownRemaining > 0f && _countdownRemaining <= WarningStartSeconds;
        if (showWarning)
        {
            _warningBlinkTimer += (float)delta;
            _warningLabel.Visible = ((int)(_warningBlinkTimer * 3f) % 2) == 0;
        }
        else
        {
            _warningLabel.Visible = false;
        }
    }

    public void OnBaseHealthChanged(int currentHealth, int maxHealth)
    {
        if (_baseHpLabel != null)
            _baseHpLabel.Text = $"Base HP: {currentHealth}/{maxHealth}";
    }

    public void OnPreparationTicked(float remaining, float total)
    {
        _countdownRemaining = remaining;
        _isPreparation = true;

        if (_wallButton != null) _wallButton.Disabled = false;
        if (_towerButton != null) _towerButton.Disabled = false;

        if (_countdownLabel != null)
            _countdownLabel.Text = $"Preparation: {remaining:F0}s";
    }

    public void OnWaveStarted(int waveNumber, int totalWaves)
    {
        _isPreparation = false;
        if (_warningLabel != null)
            _warningLabel.Visible = false;

        if (_wallButton != null) _wallButton.Disabled = true;
        if (_towerButton != null) _towerButton.Disabled = true;

        if (_waveLabel != null)
            _waveLabel.Text = $"Wave: {waveNumber}/{totalWaves}";

        if (_countdownLabel != null)
            _countdownLabel.Text = "Wave active";
    }

    public void OnWaveEnded(int nextWave, int totalWaves)
    {
        if (_countdownLabel != null)
            _countdownLabel.Text = "Preparing...";
    }

    public void OnStoneChanged(int amount)
    {
        if (_stoneLabel != null)
            _stoneLabel.Text = $"Stone: {amount}";
    }
}
