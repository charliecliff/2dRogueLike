using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum MovementButton
{
	none, up, down, left, right,
}

public class Player : Creature 
{
	
	private int food;
	private bool hasAlredyReachedExit;

	public int wallDamage = 1;
	public int pointsPerFood = 20;
	public int pointsPerSoda = 10;
	public float restartLevelDelay = .2f;
	public Text foodText;
	public AudioClip moveSound1;
	public AudioClip moveSound2;
	public AudioClip eatSound1;
	public AudioClip eatSound2;
	public AudioClip drinkSound1;
	public AudioClip drinkSound2;
	public AudioClip gameOverSound;


	#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

	private float currentMoveDistance;
	private Vector2 currentMoveStartCoordinate;
	private MovementButton currentMoveButton;

	public float minimumMoveDistance = 1f;

	#else

	#endif


	private void OnDisable ()
	{
		GameHandler.instance.playerFoodPoints = food;
	}


	private void OnTriggerEnter2D (Collider2D other)
	{
		//Check if the tag of the trigger collided with is Exit.
		if(other.tag == "Exit")
		{
			if (hasAlredyReachedExit != true) 
			{
				//Disable the player object since level is over.
				enabled = false;

				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);
			}
			hasAlredyReachedExit = true;
		}

		//Check if the tag of the trigger collided with is Food.
		else if(other.tag == "Food")
		{
			//Add pointsPerFood to the players current food total.
			food += pointsPerFood;
			foodText.text = "+" + pointsPerFood + " Food: " + food;
			SoundHandler.instance.RandomizeSfx(eatSound1, eatSound2);

			//Disable the food object the player collided with.
			other.gameObject.SetActive (false);
		}

		//Check if the tag of the trigger collided with is Soda.
		else if(other.tag == "Soda")
		{
			//Add pointsPerSoda to players food points total
			food += pointsPerSoda;
			foodText.text = "+" + pointsPerSoda + " Food: " + food;
			SoundHandler.instance.RandomizeSfx(drinkSound1, drinkSound2);

			//Disable the soda object the player collided with.
			other.gameObject.SetActive (false);
		}
	}


	/*
	 * Load the last scene loaded, in this case Main, the only scene in the game.
	 */
	private void Restart ()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
	}


	private void CheckIfGameOver ()
	{
		if (food <= 0) 
		{
			SoundHandler.instance.PlaySingle(gameOverSound);
			SoundHandler.instance.musicSource.Stop ();

			GameHandler.instance.GameOver ();
		}
	}


	protected override void Start () 
	{
		animator = GetComponent<Animator> ();
		isMoving = false;

		food = GameHandler.instance.playerFoodPoints;

		foodText = GameObject.Find ("FoodText").GetComponent<Text>();;
		foodText.text = "Food: " + food;

		hasAlredyReachedExit = false;

		base.Start ();
	}

	/*
	protected override void AttemptMove <T> (float xDir, float yDir)
	{ 
		//Every time player moves, subtract from food points total.
		food--;
		foodText.text = "Food; " + food;

		//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
		base.AttemptMove <T> ((float)xDir, (float)yDir);

		//Hit allows us to reference the result of the Linecast done in Move.
		RaycastHit2D hit;

		//If Move returns true, meaning Player was able to move into an empty space.
		if (Move (xDir, yDir, out hit)) 
		{
			//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
			SoundHandler.instance.RandomizeSfx(moveSound1, moveSound2);
		}

		//Since the player has moved and lost food points, check if the game has ended.
		CheckIfGameOver ();

		//Set the playersTurn boolean of GameManager to false now that players turn is over.
		GameHandler.instance.playersTurn = false;
	}
	*/

	protected override void OnCantMove <T> (T component)
	{
		StopMoving ();
	}
		

	void StartMoving ()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

		SetMoving (true);
		currentMoveDistance = 1;
		currentMoveStartCoordinate = transform.position;

		#else

		#endif
	}


	#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

	void Move ()
	{
		//Update the MovementButton Distance in order to move smoothely
		currentMoveDistance = currentMoveDistance + Time.deltaTime * inverseMoveTime;				

		switch (currentMoveButton)
		{
		case MovementButton.up:
			base.AttemptMoveTo <Wall> (currentMoveStartCoordinate.x, currentMoveStartCoordinate.y + currentMoveDistance);
			break;
		case MovementButton.down:
			base.AttemptMoveTo <Wall> (currentMoveStartCoordinate.x, currentMoveStartCoordinate.y - currentMoveDistance);
			break;
		case MovementButton.right:
			base.AttemptMoveTo <Wall> (currentMoveStartCoordinate.x + currentMoveDistance, currentMoveStartCoordinate.y);
			break;
		case MovementButton.left:
			base.AttemptMoveTo <Wall> (currentMoveStartCoordinate.x - currentMoveDistance, currentMoveStartCoordinate.y);
			break;
		}
	}

	#endif


	void StopMoving ()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

		SetMoving (false);
		SnapToCoordinate ();

		#else

		#endif
	}


	/*
	 * Center the Player at the Coorindate it is moving towards
	 */
	void SnapToCoordinate ()
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

		Vector2 currentLocation = transform.position;

		switch (currentMoveButton)
		{
		case MovementButton.up:
			base.AttemptMoveTo <Wall> (currentLocation.x, Mathf.Ceil(currentLocation.y));
			break;
		case MovementButton.down:
			base.AttemptMoveTo <Wall> (currentLocation.x, Mathf.Floor(currentLocation.y));
			break;
		case MovementButton.right:
			base.AttemptMoveTo <Wall> (Mathf.Ceil(currentLocation.x), currentLocation.y);
			break;
		case MovementButton.left:
			base.AttemptMoveTo <Wall> (Mathf.Floor(currentLocation.x), currentLocation.y);
			break;
		}

		#else

		#endif
	}


	void Update () 
	{
		#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL

		if (Input.GetKeyDown ("up") && currentMoveButton == MovementButton.none)
		{
			currentMoveButton = MovementButton.up;
			SetFacingDirection(Direction.North);
			StartMoving();
		}
		else if (Input.GetKeyDown ("down") && currentMoveButton == MovementButton.none)
		{
			currentMoveButton = MovementButton.down;
			SetFacingDirection(Direction.South);
			StartMoving();
		}
		else if (Input.GetKeyDown ("left") && currentMoveButton == MovementButton.none)
		{
			currentMoveButton = MovementButton.left;
			SetFacingDirection(Direction.West);
			StartMoving();
		}
		else if (Input.GetKeyDown ("right") && currentMoveButton == MovementButton.none)
		{
			currentMoveButton = MovementButton.right;
			SetFacingDirection(Direction.East);
			StartMoving();
		}
		else if (Input.GetKeyUp ("right") || Input.GetKeyUp ("left") || Input.GetKeyUp ("down") || Input.GetKeyUp ("up"))
		{
			StopMoving ();
			currentMoveButton = MovementButton.none;
		}

		if (isMoving)
			Move ();
		
		#else

		#endif
	}


	public void LoseFood (int loss)
	{
		//Set the trigger for the player animator to transition to the playerHit animation.
		animator.SetTrigger ("Player_Hit");

		//Subtract lost food points from the players total.
		food -= loss;
		foodText.text = "-" + loss + " Food; " + food;

		//Check to see if game has ended.
		CheckIfGameOver ();
	}
}
