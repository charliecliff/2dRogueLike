using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour {

	public GameObject gameHandler;

	void Awake () 
	{
		if (GameHandler.instance == null)
			Instantiate (gameHandler);	
	}
}
