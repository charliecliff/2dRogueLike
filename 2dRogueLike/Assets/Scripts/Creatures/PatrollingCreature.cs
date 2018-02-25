using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NesScripts.Controls.PathFind;

using Grid = NesScripts.Controls.PathFind.Grid;
using Point = NesScripts.Controls.PathFind.Point;

/*
 * This class represents a Creature that Patrols between Two points on the board
 */
public abstract class PatrollingCreature : Creature 
{		
	private bool onReturnLegOfPatrol;
	private Point patrolStart;
	private Point patrolEnd;
	private Grid pathingCosts;


	public int targXOffset;
	public int targYOffset;


	// Calculate the next point on the Creatures Patrol
	Point NextPointOnPatrol ()
	{
		// The Target Coord is determined by the Current Leg of the Creature's Patrol
		int targetXCoord = patrolEnd.x;
		int targetYCoord = patrolEnd.y;	

		if (onReturnLegOfPatrol) 
		{
			targetXCoord = patrolStart.x;
			targetYCoord = patrolStart.y;
		} 

		// Determine the Starting Position and the Target Position
		int currentX = (int) transform.position.x;
		int currentY = (int) transform.position.y;

		Point startPos = new Point(currentX, currentY);
		Point targetPos = new Point(targetXCoord, targetYCoord);

		// Calculate the Best Path to the Target Point
		List<Point> path = Pathfinding.FindPath(pathingCosts, startPos, targetPos);
		Point pointToMoveTo = path [0];

		// Update the Leg of the Patrol
		if (pointToMoveTo.x == targetPos.x && pointToMoveTo.y == targetPos.y) 
			onReturnLegOfPatrol = !onReturnLegOfPatrol;

		return pointToMoveTo;
	}


	float[,] GenerateFloatGridFromPathingCosts (PathingCost[,] pathingCostArray)
	{
		float[,] pathingCosts = new float[pathingCostArray.GetLength(0), pathingCostArray.GetLength(1)];

		// Go through all the tiles in the jagged array...
		for (int i = 0; i < pathingCostArray.GetLength(0); i++) 
		{
			for (int j = 0; j < pathingCostArray.GetLength(1); j++) 
			{
				// ... and add the pathing cost as a Float
				pathingCosts [i, j] = (float)pathingCostArray [i, j];
			}
		}

		return pathingCosts;
	}


	protected override void Start ()
	{
		onReturnLegOfPatrol = false;
		base.Start ();
	}


	public void SetPatrolStart (int x, int y)
	{
		patrolStart = new Point(x, y);
		patrolEnd = new Point(x + targXOffset, y + targYOffset);
	}


	public void SetPathingCosts (PathingCost[,] pathingCostArray)
	{
		// Create an Array of Floats from the Array of Pathing Costs
		float[,] tmpArray = GenerateFloatGridFromPathingCosts (pathingCostArray);

		// Convert that Array to a Grid in order to use our Pathing Library
		pathingCosts = new Grid(tmpArray.GetLength(0), tmpArray.GetLength(1), tmpArray);
	}


	//Move is called by the GameManger each turn to tell each Enemy to try to move towards the player.
	public void Move ()
	{
		Point nextPoint = NextPointOnPatrol ();
		MoveTo (nextPoint.x, nextPoint.y);
	}
}
