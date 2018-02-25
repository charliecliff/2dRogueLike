using System.Collections;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
	private TileType[][] tiles;                          	    // A jagged array of tile types representing the board, like a grid.
	private PathingCost[,] pathingCosts;                   		// A jagged array of pathing tile types representing the board, like a grid, for use by the Pathing Algorithms.
	private PopulationType[][] populatedTiles;                  // 


	private PopulatedRoom[] rooms;                          	// All the rooms that are created for this board.
	private Corridor[] corridors;                             	// All the corridors that connect the rooms.
	private GameObject boardHolder;                           	// GameObject that acts as a container for all other tiles.


	public int columns = 100;                                 	// The number of columns on the board (how wide it will be).
	public int rows = 100;                                    	// The number of rows on the board (how tall it will be).
	public IntRange numRooms = new IntRange (15, 20);         	// The range of the number of rooms there can be.
	public IntRange roomWidth = new IntRange (3, 10);         	// The range of widths rooms can have.
	public IntRange roomHeight = new IntRange (3, 10);        	// The range of heights rooms can have.
	public IntRange corridorLength = new IntRange (6, 10);    	// The range of lengths corridors between rooms can have.

	public GameObject[] floorTiles;                           	// An array of floor tile prefabs.
	public GameObject[] innerWallTiles;                       	// An array of wall tile prefabs.
	public GameObject[] outerWallTiles;                       	// An array of outer wall tile prefabs.
	public GameObject[] enemyPrefabs;                       	// An array of enemy prefabs.


	private void Start ()
	{
		boardHolder = new GameObject("BoardHolder");

		SetupTilesArray ();
		SetupPopulatedTilesArray ();
		SetupPathingCostsArray ();

		CreateRoomsAndCorridors ();
		PopulateRooms (GameHandler.instance, 1);

		SetTilesValues ();
		SetPopulatedTilesValues ();

		InstantiateTiles ();
		InstantiatePopulatedTiles ();

		InstantiateOuterWalls ();
	}
		

	void SetupTilesArray ()
	{
		tiles = new TileType[columns][];
		for (int i = 0; i < tiles.Length; i++) {
			tiles[i] = new TileType[rows];
		}
	}


	void SetupPopulatedTilesArray ()
	{
		populatedTiles = new PopulationType[columns][];
		for (int i = 0; i < populatedTiles.Length; i++) {
			populatedTiles[i] = new PopulationType[rows];
		}
	}


	void SetupPathingCostsArray ()
	{
		pathingCosts = new PathingCost[columns, rows];
	}


	void CreateRoomsAndCorridors ()
	{
		// Create the rooms array with a random size.
		rooms = new PopulatedRoom[numRooms.Random];

		// There should be one less corridor than there is rooms.
		corridors = new Corridor[rooms.Length - 1];

		// Create the first room and corridor.
		rooms[0] = new PopulatedRoom ();
		corridors[0] = new Corridor ();

		// Setup the first room, there is no previous corridor so we do not use one.
		rooms[0].SetupFirstRoom(roomWidth, roomHeight, columns, rows);

		// Setup the first corridor using the first room.
		corridors[0].SetupCorridor(rooms[0], corridorLength, roomWidth, roomHeight, columns, rows, true);

		for (int i = 1; i < rooms.Length; i++)
		{
			// Create a room.
			rooms[i] = new PopulatedRoom ();

			// Setup the room based on the previous corridor.
			rooms[i].SetupRoom (roomWidth, roomHeight, columns, rows, corridors[i - 1]);

			// If we haven't reached the end of the corridors array...
			if (i < corridors.Length)
			{
				// ... create a corridor.
				corridors[i] = new Corridor ();

				// Setup the corridor based on the room that was just created.
				corridors[i].SetupCorridor(rooms[i], corridorLength, roomWidth, roomHeight, columns, rows, false);
			}



			if (i == 1)
			{
				Vector3 playerPos = new Vector3 (rooms[i].xPos, rooms[i].yPos, 0);
				GameObject player = GameHandler.instance.player;								  
				player.transform.position = playerPos;
			}



		}

	}


	void SetTilesValues ()
	{
		SetTilesValuesForRooms ();
		SetTilesValuesForCorridors ();
	}


	void SetTilesValuesForRooms ()
	{
		// Go through all the rooms...
		for (int i = 0; i < rooms.Length; i++)
		{
			Room currentRoom = rooms[i];

			// ... and for each room go through it's width.
			for (int j = 0; j < currentRoom.roomWidth; j++)
			{
				int xCoord = currentRoom.xPos + j;

				// For each horizontal tile, go up vertically through the room's height.
				for (int k = 0; k < currentRoom.roomHeight; k++)
				{
					int yCoord = currentRoom.yPos + k;

					// The coordinates in the jagged array are based on the room's position and it's width and height.
					tiles[xCoord][yCoord] = TileType.Floor;

					// Set the Pathing Costs for the Floor Tiles in the Room
					pathingCosts [xCoord, yCoord] = PathingCost.Free;
				}
			}
		}
	}


	void SetTilesValuesForCorridors ()
	{
		// Go through every corridor...
		for (int i = 0; i < corridors.Length; i++)
		{
			Corridor currentCorridor = corridors[i];

			// and go through it's length.
			for (int j = 0; j < currentCorridor.corridorLength; j++)
			{
				// Start the coordinates at the start of the corridor.
				int xCoord = currentCorridor.startXPos;
				int yCoord = currentCorridor.startYPos;

				// Depending on the direction, add or subtract from the appropriate
				// coordinate based on how far through the length the loop is.
				switch (currentCorridor.direction)
				{
				case Direction.North:
					yCoord += j;
					break;
				case Direction.East:
					xCoord += j;
					break;
				case Direction.South:
					yCoord -= j;
					break;
				case Direction.West:
					xCoord -= j;
					break;
				}

				// Set the tile at these coordinates to Floor.
				tiles[xCoord][yCoord] = TileType.Floor;
			}
		}
	}


	void SetPopulatedTilesValues ()
	{
		// Go through all the rooms...
		for (int n = 0; n < rooms.Length; n++)
		{
			PopulatedRoom currentRoom = rooms[n];

			// ... and for each room go through it's width.
			for (int i = 0; i < currentRoom.roomWidth; i++)
			{
				int xCoord = currentRoom.xPos + i;

				// For each horizontal tile, go up vertically through the room's height.
				for (int j = 0; j < currentRoom.roomHeight; j++)
				{
					int yCoord = currentRoom.yPos + j;

					// The coordinates in the jagged array are based on the room's position and it's width and height.
					populatedTiles[xCoord][yCoord] = currentRoom.populatedTiles[i][j];
				}
			}
		}
	}


	void InstantiateTiles ()
	{
		// Go through all the tiles in the jagged array...
		for (int i = 0; i < tiles.Length; i++)
		{
			for (int j = 0; j < tiles[i].Length; j++)
			{
				// ... and instantiate a floor tile for it.
				InstantiateFromArray (floorTiles, i, j);

				// If the tile type is Wall...
				if (tiles[i][j] == TileType.Wall)
				{
					// ... instantiate a wall over the top.
					InstantiateFromArray (innerWallTiles, i, j);
				}
			}
		}
	}


	void InstantiatePopulatedTiles ()
	{
		// Go through all the tiles in the jagged array...
		for (int i = 0; i < populatedTiles.Length; i++)
		{
			for (int j = 0; j < populatedTiles[i].Length; j++)
			{
				// If the tile type is Enemy...
				if (populatedTiles[i][j] == PopulationType.Enemy)
				{
					// ... and instantiate an enemy...
					GameObject prefab = InstantiateFromArray (enemyPrefabs, i, j);

					// ... and set the Attributes for the Enemy.
					Enemy enemyScript = prefab.GetComponent<Enemy>();
					enemyScript.SetPatrolStart (i, j);
					enemyScript.SetPathingCosts (pathingCosts);
				}
			}
		}
	}


	void InstantiateOuterWalls ()
	{
		// The outer walls are one unit left, right, up and down from the board.
		float leftEdgeX = -1f;
		float rightEdgeX = columns + 0f;
		float bottomEdgeY = -1f;
		float topEdgeY = rows + 0f;

		// Instantiate both vertical walls (one on each side).
		InstantiateVerticalOuterWall (leftEdgeX, bottomEdgeY, topEdgeY);
		InstantiateVerticalOuterWall (rightEdgeX, bottomEdgeY, topEdgeY);

		// Instantiate both horizontal walls, these are one in left and right from the outer walls.
		InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, bottomEdgeY);
		InstantiateHorizontalOuterWall(leftEdgeX + 1f, rightEdgeX - 1f, topEdgeY);
	}


	void InstantiateVerticalOuterWall (float xCoord, float startingY, float endingY)
	{
		// Start the loop at the starting value for Y.
		float currentY = startingY;

		// While the value for Y is less than the end value...
		while (currentY <= endingY)
		{
			// ... and instantiate an outer wall tile at the x coordinate and the current y coordinate.
			InstantiateFromArray(outerWallTiles, xCoord, currentY);

			currentY++;
		}
	}


	void InstantiateHorizontalOuterWall (float startingX, float endingX, float yCoord)
	{
		// Start the loop at the starting value for X.
		float currentX = startingX;

		// While the value for X is less than the end value...
		while (currentX <= endingX)
		{
			// ... and instantiate an outer wall tile at the y coordinate and the current x coordinate.
			InstantiateFromArray (outerWallTiles, currentX, yCoord);

			currentX++;
		}
	}


	GameObject InstantiateFromArray (GameObject[] prefabs, float xCoord, float yCoord)
	{
		// Create a random index for the array.
		int randomIndex = Random.Range(0, prefabs.Length);

		// The position to be instantiated at is based on the coordinates.
		Vector3 position = new Vector3(xCoord, yCoord, 0f);

		// Create an instance of the prefab from the random index of the array.
		GameObject tileInstance = Instantiate(prefabs[randomIndex], position, Quaternion.identity) as GameObject;

		// Set the tile's parent to the board holder.
		tileInstance.transform.parent = boardHolder.transform;

		return tileInstance;
	}


	void PopulateRooms (GameHandler gameHandler, int difficultyLevel)
	{
		// Go through all the rooms...
		for (int i = 0; i < rooms.Length; i++) 
		{
			// ... and Populate each Room with Enemies
			PopulatedRoom room = rooms [i];
			room.PopulateRoomWithEnemies (gameHandler, difficultyLevel);
		}
	}
}