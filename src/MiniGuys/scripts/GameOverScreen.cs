namespace MiniGuys;

using Godot;

public partial class GameOverScreen : CanvasLayer
{
    private Button? _restartButton;

    public override void _Ready()
    {
        _restartButton = GetNode<Button>("VBoxContainer/RestartButton");
        _restartButton.Pressed += OnRestartPressed;
    }

    private void OnRestartPressed()
    {
        var gameState = GetNode<GameState>("/root/GameState");
        gameState.Reset();
        GetTree().ReloadCurrentScene();
    }
}
