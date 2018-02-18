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
	 * AttemptMove takes a generic parameter T to specify the type of component we expect our unit to interact 
	 * with if blocked (Player for Enemies, Wall for Player).
	 */
	protected virtual void AttemptMove <T> (float xDir, float yDir) where T : Component
	{
		//Calculate end position based on the direction parameters passed in when calling Move.
		float xCoord = transform.position.x + xDir;
		float yCoord = transform.position.y + yDir;

		//Attempt to move the object to the calculated coordinates
		AttemptMoveTo <T> (xCoord, yCoord);
	}


	/*
	 */
	protected virtual void AttemptMoveTo <T> (float xCoordinate, float yCoordinate) where T : Component
	{
		//Hit will store whatever our linecast hits when Move is called.
		RaycastHit2D hit;

		//Set canMove to true if Move was successful, false if failed.
		bool canMove = MoveTo (xCoordinate, yCoordinate, out hit);

		//Check if nothing was hit by linecast
		if(hit.transform == null)
			//If nothing was hit, return and don't execute further code.
			return;
		
		//Get a component reference to the component of type T attached to the object that was hit
		T hitComponent = hit.transform.GetComponent <T> ();

		//If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
		if(!canMove && hitComponent != null)

			//Call the OnCantMove function and pass it hitComponent as a parameter.
			OnCantMove (hitComponent);
	}


	/*
	 * Move returns true if it is able to move and false if not. 
	 * Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
	 */
	protected bool Move (float xDir, float yDir, out RaycastHit2D hit)
	{
		// Calculate end position based on the direction parameters passed in when calling Move.
		float xCoord = transform.position.x + xDir;
		float yCoord = transform.position.y + yDir;

		// Move the object to the calculated coordinates
		return MoveTo (xCoord, yCoord, out hit);
	}


	//
	protected bool MoveTo (float xCoordinate, float yCoordinate, out RaycastHit2D hit)
	{
		// Calculate end position based on the direction parameters passed in when calling Move.
		Vector2 start = transform.position;
		Vector2 end = new Vector2 (xCoordinate, yCoordinate);

		//Disable the boxCollider so that linecast doesn't hit this object's own collider.
		boxCollider.enabled = false;

		//Cast a line from start point to end point checking collision on blockingLayer.
		hit = Physics2D.Linecast (start, end, blockingLayer);

		//Re-enable boxCollider after linecast
		boxCollider.enabled = true;

		//Check if anything was hit
		if(hit.transform == null)
		{
			//If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
			StartCoroutine (SmoothMovement (end));

			//Return true to say that Move was successful
			return true;
		}

		//If something was hit, return false, Move was unsuccesful.
		return false;
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


	/*
	 * OnCantMove will be overriden by functions in the inheriting classes.
	 */
	protected abstract void OnCantMove <T> (T component) where T : Component;
}