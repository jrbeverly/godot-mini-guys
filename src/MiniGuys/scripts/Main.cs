namespace MiniGuys;

using Godot;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        var label = new Label
        {
            Text = "MiniGuys: hello world",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        AddChild(label);
    }
}
