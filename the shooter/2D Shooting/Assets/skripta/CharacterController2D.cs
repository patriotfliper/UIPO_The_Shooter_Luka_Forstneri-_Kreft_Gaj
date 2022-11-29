using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;						//	Količina dodane sile, ko igralec skoči.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Količina največje hitrosti, ki se uporablja za gibanje v počepu. 1 = 100 %
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// Gibanje
	[SerializeField] private bool m_AirControl = false;							// Ali igralec lahko krmili med skokom ali ne;
	[SerializeField] private LayerMask m_WhatIsGround;							// Maska, ki določa, kaj je mleto liku.
	[SerializeField] private Transform m_GroundCheck;							// Oznaka položaja, kjer je treba preveriti, ali je igralec prizemljen.
	[SerializeField] private Transform m_CeilingCheck;							// Oznaka položaja, kjer je treba preveriti zgornje meje
	[SerializeField] private Collider2D m_CrouchDisableCollider;				// Trkalnik, ki bo onemogočen pri čepenju

	const float k_GroundedRadius = .2f; //Polmer kroga prekrivanja za določitev ozemljitve
	private bool m_Grounded;            //Ne glede na to, ali je igralec prizemljen ali ne.
	const float k_CeilingRadius = .2f; // Polmer prekrivajočega se kroga za določitev, ali lahko igralec vstane
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  //Za določitev, v katero smer je igralec trenutno obrnjen.
	private Vector3 m_Velocity = Vector3.zero;

	[Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();
	}

	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// Igralec je prizemljen, če krog zadane na položaj za preverjanje tal, zadene kar koli, kar je označeno kot tla
		// To lahko namesto tega storite s plastmi, vendar vzorčna sredstva ne bodo prepisala vaših nastavitev projekta.


		Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < colliders.Length; i++)
		{
			if (colliders[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}
	}


	public void Move(float move, bool crouch, bool jump)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			
// Če ima lik strop, ki mu preprečuje, da bi vstal, naj čepi
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//upravljajte predvajalnik samo, če je ozemljen ali je airControl vklopljen
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}

			
// Premaknite lika z iskanjem ciljne hitrosti
			Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			


			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

			
			if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// če igralec skoči 
		if (m_Grounded && jump)
		{
			// dodaj vertikalno silo na igralca - vertikal force 
			m_Grounded = false;
			m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
		}
	}


	private void Flip()
	{
		//smer igralca kamor gleda
		m_FacingRight = !m_FacingRight;

		transform.Rotate(0f, 180f, 0f);
	}
}
