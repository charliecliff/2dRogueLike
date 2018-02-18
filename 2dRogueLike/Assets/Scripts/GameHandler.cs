using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour 
{
	private int level = 1;
	private Text levelText;
	private GameObject levelImage;

	private List<Enemy> enemies;
	private bool enemiesMoving;
	private bool doingSetup;

	public GameObject player;								  

	public float levelStartDelay = 0.4f;
	public float turnDelay = .1f;
	public static GameHandler instance = null;
	public int playerFoodPoints = 100;
	[HideInInspector] public bool playersTurn = true;

	void Awake () 
	{
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		enemies = new List<Enemy> ();

		InitGame ();
	}

	void InitGame ()
	{
		doingSetup = true;

		levelImage = GameObject.Find ("LevelImage");
		levelText = GameObject.Find ("LevelText").GetComponent<Text>();;
		levelText.text = "Day " + level;
		levelImage.SetActive (true);
		Invoke ("HideLevelImage", levelStartDelay);

		enemies.Clear ();
	}

	void HideLevelImage ()
	{
		levelImage.SetActive(false);
		doingSetup = false;
	}

	//Update is called every frame.
	void Update()
	{
		//Check that playersTurn or enemiesMoving or doingSetup are not currently true.
		if(playersTurn || enemiesMoving || doingSetup)

			//If any of these are true, return and do not start MoveEnemies.
			return;

		//Start moving enemies.
		StartCoroutine (MoveEnemies ());
	}

	//Call this to add the passed in Enemy to the List of Enemy objects.
	public void AddEnemyToList(Enemy script)
	{
		//Add Enemy to List enemies.
		enemies.Add(script);
	}

	public void GameOver ()
	{
		levelText.text = "After " + level + " days, you starved";
		levelImage.SetActive (true);

		enabled = false;
	}

	IEnumerator MoveEnemies()
	{
		//While enemiesMoving is true player is unable to move.
		enemiesMoving = true;

		//Wait for turnDelay seconds, defaults to .1 (100 ms).
		yield return new WaitForSeconds(turnDelay);

		//If there are no enemies spawned (IE in first level):
		if (enemies.Count == 0) 
		{
			//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
			yield return new WaitForSeconds(turnDelay);
		}

		//Loop through List of Enemy objects.
		for (int i = 0; i < enemies.Count; i++)
		{
			//Call the MoveEnemy function of Enemy at index i in the enemies List.
			enemies[i].MoveEnemy ();

			//Wait for Enemy's moveTime before moving next Enemy, 
			yield return new WaitForSeconds(enemies[i].moveTime);
		}
		//Once Enemies are done moving, set playersTurn to true so player can move.
		playersTurn = true;

		//Enemies are done moving, set enemiesMoving to false.
		enemiesMoving = false;
	}

	//This is called each time a scene is loaded.
	void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		//Add one to our level number.
		level++;

		//Call InitGame to initialize our level.
		InitGame();
	}

	void OnEnable()
	{
		//Tell our ‘OnLevelFinishedLoading’ function to start listening for a scene change event as soon as this script is enabled.
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	void OnDisable()
	{
		//Tell our ‘OnLevelFinishedLoading’ function to stop listening for a scene change event as soon as this script is disabled.
		//Remember to always have an unsubscription for every delegate you subscribe to!
		SceneManager.sceneLoaded -= OnLevelFinishedLoading;
	}
}
