using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Creature : MovingObject 
{
	
	protected bool isMoving;				//The Moving State should trigger a change in animation
	protected Direction facingDirection;	//The Cardinal Direction that the Moving Object is Facing		


	/**
	 *The Facing Direction State should update Triggers for the Animation Controller of the Creature
	 **/
	protected void SetFacingDirection(Direction direction)
	{
		facingDirection = direction;

		animator.ResetTrigger ("FaceNorth");
		animator.ResetTrigger ("FaceEast");
		animator.ResetTrigger ("FaceWest");
		animator.ResetTrigger ("FaceSouth");

		switch (facingDirection)
		{
		case Direction.North:
			animator.SetTrigger ("FaceNorth");
			break;
		case Direction.East:
			animator.SetTrigger ("FaceEast");
			break;
		case Direction.West:
			animator.SetTrigger ("FaceWest");
			break;
		case Direction.South:
			animator.SetTrigger ("FaceSouth");
			break;
		}
	}


	/* 
	 *The Moving State should update Triggers for the Animation Controller of the Creature
	 */
	protected virtual void SetMoving(bool moving)
	{
		isMoving = moving;

		animator.ResetTrigger ("Moving");
		animator.ResetTrigger ("NotMoving");

		if(isMoving)
			animator.SetTrigger ("Moving");
		else
			animator.SetTrigger ("NotMoving");
	}
}
