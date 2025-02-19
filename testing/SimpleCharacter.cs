using System;
using System.Collections.Generic;
using Godot;

namespace Flamme.testing;

public partial class SimpleCharacter : Node2D
{
	[Export] public float SpeedMultiplier = 25.0f;
	[Export] public float FrictionMultiplier = .1f;
	[Export] public float MaxSpeed = 100.0f;
	
	[ExportCategory("Meta")] 
	[Export] public CharacterBody2D Body;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ExportMetaNonNull.Check(this);
	}
	
	public enum Actions
	{
		MoveUp,
		MoveDown,
		MoveLeft,
		MoveRight
	}

	public List<Actions> CurrentActions = new List<Actions>();
	
	public override void  _UnhandledKeyInput(InputEvent @event)
	{
		var handled = false;
		if (@event.IsActionPressed(Const.InputMap.MoveUp))
		{
			CurrentActions.Add(Actions.MoveUp);
			handled = true;
		}
		if (@event.IsActionPressed(Const.InputMap.MoveDown))
		{
			CurrentActions.Add(Actions.MoveDown);
			handled = true;
		}
		if (@event.IsActionPressed(Const.InputMap.MoveLeft))
		{
			CurrentActions.Add(Actions.MoveLeft);
			handled = true;
		}
		if (@event.IsActionPressed(Const.InputMap.MoveRight))
		{
			CurrentActions.Add(Actions.MoveRight);
			handled = true;
		}
		
		if (@event.IsActionReleased(Const.InputMap.MoveUp))
		{
			CurrentActions.Remove(Actions.MoveUp);
			handled = true;
		}
		if (@event.IsActionReleased(Const.InputMap.MoveDown))
		{
			CurrentActions.Remove(Actions.MoveDown);
			handled = true;
		}
		if (@event.IsActionReleased(Const.InputMap.MoveLeft))
		{
			CurrentActions.Remove(Actions.MoveLeft);
			handled = true;
		}
		if (@event.IsActionReleased(Const.InputMap.MoveRight))
		{
			CurrentActions.Remove(Actions.MoveRight);
			handled = true;
		}
		

		if (handled)
		{
			GetViewport().SetInputAsHandled();
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		foreach (var action in CurrentActions)
		{
			switch(action)
			{
				case Actions.MoveUp:
					Body.Velocity += Vector2.Up * SpeedMultiplier;
					break;
				case Actions.MoveDown:
					Body.Velocity += Vector2.Down * SpeedMultiplier;
					break;
				case Actions.MoveLeft:
					Body.Velocity += Vector2.Left * SpeedMultiplier;
					break;
				case Actions.MoveRight:
					Body.Velocity += Vector2.Right * SpeedMultiplier;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		Body.Velocity = Body.Velocity.LimitLength(MaxSpeed);
		Body.Velocity = Body.Velocity.Lerp(Vector2.Zero, FrictionMultiplier);
		Body.MoveAndSlide();
	}
}