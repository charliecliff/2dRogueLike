using UnityEngine;
using System.Collections;
	

public abstract class MovingObject : MonoBehaviour
{

	private BoxCollider2D boxCollider;      //The BoxCollider2D component attached to this object.
	private Rigidbody2D rb2D;               //The Rigidbody2D component attached to this object.
	private Vector3 moveDestinationCoord;	//Used for a smoothe movement to a destination endpoint


	protected Animator animator;			//Variable of type Animator to store a reference to the enemy's Animator component.
	protected float inverseMoveTime;		//Used to make movement more efficient.


	public float moveTime = 0.1f;           //Time it will take object to move, in seconds.
	public float collisionRadius = 0.2f;	//TODO
	public LayerMask blockingLayer;         //Layer on which collision will be checked.


	protected virtual void Start ()
	{
		//Turn off console warning on the animator
		animator.logWarnings = false;

		//Get a component reference to this object's BoxCollider2D
		boxCollider = GetComponent <BoxCollider2D> ();

		//Get a component reference to this object's Rigidbody2D
		rb2D = GetComponent <Rigidbody2D> ();

		//By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
		inverseMoveTime = 1f / moveTime;
	}


	/*
	 * Move returns true if it is able to move and false if not. 
	 * Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	 */
	protected bool AttemptMove (float xDir, float yDir)
	{
		// Calculate end position based on the direction parameters passed in when calling Move.
		float xCoord = transform.position.x + xDir;
		float yCoord = transform.position.y + yDir;

		// Move the object to the calculated coordinates
		return AttemptMoveTo (xCoord, yCoord);
	}


	/*
	 * Move returns true if it is able to move and false if not. 
	 * Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	 */
	protected bool AttemptMoveTo (float xCoordinate, float yCoordinate)
	{
		// Calculate end position based on the direction parameters passed in when calling Move.
		Vector2 start = transform.position;
		Vector2 end = new Vector2 (xCoordinate, yCoordinate);

		//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
		StartCoroutine (AttemptSmoothMovement (end));

		//Return true to say that Move was successful
		return true;
	}


	/*
	 * Move returns true if it is able to move and false if not. 
	 * Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	 */
	protected bool Move (float xDir, float yDir)
	{
		// Calculate end position based on the direction parameters passed in when calling Move.
		float xCoord = transform.position.x + xDir;
		float yCoord = transform.position.y + yDir;

		// Move the object to the calculated coordinates
		return MoveTo (xCoord, yCoord);
	}


	/*
	 * Move returns true if it is able to move and false if not. 
	 * Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	 */
	protected bool MoveTo (float xCoordinate, float yCoordinate)
	{
		// Calculate end position based on the direction parameters passed in when calling Move.
		Vector2 start = transform.position;
		Vector2 end = new Vector2 (xCoordinate, yCoordinate);

		//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
		StartCoroutine (SmoothMovement (end));

		//Return true to say that Move was successful
		return true;
	}


	protected bool DetectCollisionAt (float xCoordinate, float yCoordinate,  out RaycastHit2D hit)
	{
		//We need to caculate the LineCast from the Edge of the Objects
		Vector2 start = transform.position;
		Vector2 end = new Vector2 (xCoordinate, yCoordinate);
		Vector2 offsetVector = (end - start).normalized * collisionRadius;
		Vector2 offsetStart = start + offsetVector;
		Vector2 offsetEnd = end + offsetVector;

		//Disable the boxCollider so that linecast doesn't hit this object's own collider.
		boxCollider.enabled = false;

		//Cast a line from start point to end point checking collision on blockingLayer.
		hit = Physics2D.Linecast (offsetStart, offsetEnd, blockingLayer);

		//Re-enable boxCollider after linecast
		boxCollider.enabled = true;

		//If no object was collided with then we return false
		if(hit.transform == null)
			return false;

		//...otherwise return true
		return true;
	}


	//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
	protected IEnumerator SmoothMovement (Vector3 end)
	{
		// Captute the destination. This prevents an oscillation caused by starting several routines to move
		moveDestinationCoord = end;

		//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
		//Square magnitude is used instead of magnitude because it's computationally cheaper.
		float sqrRemainingDistance = (transform.position - moveDestinationCoord).sqrMagnitude;

		//While that distance is greater than a very small amount (Epsilon, almost zero):
		while(sqrRemainingDistance > float.Epsilon)
		{
			//Find a new position  proportionally closer to the end, based on the moveTime
			Vector3 newPostion = Vector3.MoveTowards(rb2D.position, moveDestinationCoord, inverseMoveTime * Time.deltaTime);

			//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
			rb2D.MovePosition (newPostion);

			//Recalculate the remaining distance after moving.
			sqrRemainingDistance = (transform.position - moveDestinationCoord).sqrMagnitude;

			//Return and loop until sqrRemainingDistance is close enough to zero to end the function
			yield return null;
		}
	}


	//Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
	protected IEnumerator AttemptSmoothMovement (Vector3 end)
	{
		// Captute the destination. This prevents an oscillation caused by starting several routines to move
		moveDestinationCoord = end;

		//Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
		//Square magnitude is used instead of magnitude because it's computationally cheaper.
		float sqrRemainingDistance = (transform.position - moveDestinationCoord).sqrMagnitude;

		//While that distance is greater than a very small amount (Epsilon, almost zero):
		while(sqrRemainingDistance > float.Epsilon)
		{
			//Find a new position  proportionally closer to the end, based on the moveTime
			Vector3 newPostion = Vector3.MoveTowards(rb2D.position, moveDestinationCoord, inverseMoveTime * Time.deltaTime);

			// Check for any Collisions in the new Position before moving
			RaycastHit2D hit;
			bool didCollide = DetectCollisionAt (newPostion.x, newPostion.y, out hit);

			//Check if nothing was hit by linecast
			if(didCollide)
			{
				//Call the OnCantMove function and pass it RaycastHit2D as a parameter.
				OnHit (hit);

				//Stop the Movement
				yield break;
			}

			//Call MovePosition on attached Rigidbody2D and move it to the calculated position.
			rb2D.MovePosition (newPostion);

			//Recalculate the remaining distance after moving.
			sqrRemainingDistance = (transform.position - moveDestinationCoord).sqrMagnitude;

			//Return and loop until sqrRemainingDistance is close enough to zero to end the function
			yield return null;
		}
	}


	/*
	 * OnCantMove will be overriden by functions in the inheriting classes.
	 */
	protected abstract void OnHit (RaycastHit2D hitObject);
}