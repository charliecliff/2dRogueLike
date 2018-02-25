using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathingCost
{
	Blocked,
	Free,
}


public enum TileType
{
	Wall,
	Floor,
}


public enum PopulationType
{
	Unpopulated,
	Enemy,									// Represents the placement of a starting position of an Enemy
	Item,									// Represents the placement of an Item
	Exit,
	Entrance
}