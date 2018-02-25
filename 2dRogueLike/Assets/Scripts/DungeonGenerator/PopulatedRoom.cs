using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulatedRoom : Room 
{
	public int enemyCount;                      // The current number of enemies
	public PopulationType[][] populatedTiles;	// The jagged array of populated tiles


	private void SetupPopulatedTilesArray ()
	{
		populatedTiles = new PopulationType[roomWidth][];

		for (int i = 0; i < populatedTiles.Length; i++)
		{
			populatedTiles[i] = new PopulationType[roomHeight];
		}
	}


	// This is used to calculate the number of enemies in a Room based on the Difficulty Level
	protected int NumberOfEnemies (int difficultyLevel)
	{
		return 1;
	}


	// This is used to populate a Tile with an Enemy
	protected void PopulateEnemyAtTile (GameHandler gameHandler, int xCoord, int yCoord)
	{
		// Set the Population Type to an Enemy
		populatedTiles[xCoord][yCoord] = PopulationType.Enemy;
		enemyCount++;
	}


	// This is used to determine if an Enemy should be placed in a Tile
	protected bool ShouldPopulateEnemyAtTile (int xCoord, int yCoord)
	{
		return true;
	}


	// This is used to populate the Room with Enemies
	public void PopulateRoomWithEnemies (GameHandler gameHandler, int difficultyLevel)
	{
		// Set Up a blank array of Populated Tiles
		SetupPopulatedTilesArray ();

		// Calculate the number of total enemies that will be in this room
		enemyCount = 0;
		int _totalEnemyCount = NumberOfEnemies (difficultyLevel);

		// Go through all the tiles in the room
		for (int i = 0; i < populatedTiles.Length; i++)
		{
			for (int j = 0; j < populatedTiles[i].Length; j++)
			{
				// We determine whether or not the current tile need an Enemy to be placed...
				if (ShouldPopulateEnemyAtTile (i, j)) 
				{
					// ... and we add the enemy
					PopulateEnemyAtTile (gameHandler, i, j);
				}
					
				// When we reach our quota on enemies for this room, we stop the loop and end the Population Algorithm
				if (_totalEnemyCount == enemyCount) {
					return;
				}
			}
		}
	}
}
