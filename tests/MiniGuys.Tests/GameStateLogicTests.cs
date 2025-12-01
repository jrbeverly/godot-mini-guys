namespace MiniGuys.Tests;

using FluentAssertions;
using Xunit;

public class GameStateLogicTests
{
    [Fact]
    public void Initial_state_is_setup()
    {
        var logic = new GameStateLogic();
        logic.CurrentPhase.Should().Be(GamePhase.Setup);
    }

    [Fact]
    public void IsValidTransition_returns_true_for_documented_transitions()
    {
        GameStateLogic.IsValidTransition(GamePhase.Setup, GamePhase.Preparation).Should().BeTrue();
        GameStateLogic.IsValidTransition(GamePhase.Preparation, GamePhase.Wave).Should().BeTrue();
        GameStateLogic.IsValidTransition(GamePhase.Wave, GamePhase.Preparation).Should().BeTrue();
        GameStateLogic.IsValidTransition(GamePhase.Wave, GamePhase.Won).Should().BeTrue();
        GameStateLogic.IsValidTransition(GamePhase.Setup, GamePhase.Lost).Should().BeTrue();
        GameStateLogic.IsValidTransition(GamePhase.Preparation, GamePhase.Lost).Should().BeTrue();
        GameStateLogic.IsValidTransition(GamePhase.Wave, GamePhase.Lost).Should().BeTrue();
    }

    [Fact]
    public void IsValidTransition_returns_false_for_disallowed_transitions()
    {
        GameStateLogic.IsValidTransition(GamePhase.Setup, GamePhase.Wave).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Setup, GamePhase.Won).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Preparation, GamePhase.Setup).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Preparation, GamePhase.Won).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Wave, GamePhase.Setup).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Won, GamePhase.Preparation).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Won, GamePhase.Lost).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Lost, GamePhase.Setup).Should().BeFalse();
        GameStateLogic.IsValidTransition(GamePhase.Lost, GamePhase.Preparation).Should().BeFalse();
    }

    [Fact]
    public void Full_happy_path_transitions_through_all_phases()
    {
        var logic = new GameStateLogic();

        logic.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        logic.CurrentPhase.Should().Be(GamePhase.Preparation);

        logic.TryTransitionTo(GamePhase.Wave).Should().BeTrue();
        logic.CurrentPhase.Should().Be(GamePhase.Wave);

        logic.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        logic.CurrentPhase.Should().Be(GamePhase.Preparation);

        logic.TryTransitionTo(GamePhase.Wave).Should().BeTrue();
        logic.CurrentPhase.Should().Be(GamePhase.Wave);

        logic.TryTransitionTo(GamePhase.Won).Should().BeTrue();
        logic.CurrentPhase.Should().Be(GamePhase.Won);
    }

    [Fact]
    public void Losing_from_preparation_phase_is_valid()
    {
        var logic = new GameStateLogic();

        logic.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        logic.TryTransitionTo(GamePhase.Lost).Should().BeTrue();
        logic.CurrentPhase.Should().Be(GamePhase.Lost);
    }

    [Fact]
    public void Invalid_transition_does_not_change_state()
    {
        var logic = new GameStateLogic();

        logic.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        logic.TryTransitionTo(GamePhase.Won).Should().BeFalse();
        logic.CurrentPhase.Should().Be(GamePhase.Preparation);
    }

    [Fact]
    public void Invalid_transition_sets_last_error()
    {
        var logic = new GameStateLogic();

        logic.TryTransitionTo(GamePhase.Wave).Should().BeFalse();
        logic.LastError.Should().NotBeNull();
        logic.LastError.Should().Contain("Invalid transition");
    }

    [Fact]
    public void Reset_returns_to_setup_and_clears_error()
    {
        var logic = new GameStateLogic();
        logic.TryTransitionTo(GamePhase.Preparation);
        logic.TryTransitionTo(GamePhase.Wave);
        logic.TryTransitionTo(GamePhase.Won);

        logic.Reset();

        logic.CurrentPhase.Should().Be(GamePhase.Setup);
        logic.LastError.Should().BeNull();
    }

    [Fact]
    public void After_reset_valid_transitions_work_again()
    {
        var logic = new GameStateLogic();
        logic.TryTransitionTo(GamePhase.Preparation);
        logic.Reset();

        logic.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        logic.TryTransitionTo(GamePhase.Wave).Should().BeTrue();
        logic.TryTransitionTo(GamePhase.Won).Should().BeTrue();
    }

    [Fact]
    public void After_reset_invalid_transition_from_setup_still_fails()
    {
        var logic = new GameStateLogic();
        logic.TryTransitionTo(GamePhase.Preparation);
        logic.Reset();

        logic.TryTransitionTo(GamePhase.Wave).Should().BeFalse();
    }

    [Fact]
    public void Terminal_states_accept_no_outgoing_transitions()
    {
        var wonLogic = new GameStateLogic();
        wonLogic.TryTransitionTo(GamePhase.Preparation).Should().BeTrue();
        wonLogic.TryTransitionTo(GamePhase.Wave).Should().BeTrue();
        wonLogic.TryTransitionTo(GamePhase.Won).Should().BeTrue();

        wonLogic.TryTransitionTo(GamePhase.Setup).Should().BeFalse();
        wonLogic.TryTransitionTo(GamePhase.Preparation).Should().BeFalse();
        wonLogic.TryTransitionTo(GamePhase.Wave).Should().BeFalse();
        wonLogic.TryTransitionTo(GamePhase.Lost).Should().BeFalse();

        var lostLogic = new GameStateLogic();
        lostLogic.TryTransitionTo(GamePhase.Lost).Should().BeTrue();

        lostLogic.TryTransitionTo(GamePhase.Setup).Should().BeFalse();
        lostLogic.TryTransitionTo(GamePhase.Preparation).Should().BeFalse();
        lostLogic.TryTransitionTo(GamePhase.Wave).Should().BeFalse();
        lostLogic.TryTransitionTo(GamePhase.Won).Should().BeFalse();
    }
}
