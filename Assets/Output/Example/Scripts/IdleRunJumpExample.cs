using UnityEngine;
using System.Collections;

public class IdleRunJumpExample : MonoBehaviour {


	protected Animator animator;
	public float DirectionDampTime = .25f;
	public bool ApplyGravity = true; 

	// Use this for initialization
	void Start () 
	{
		animator = GetComponent<Animator>();
		
		if(animator.layerCount >= 2)
			animator.SetLayerWeight(1, 1);
	}
		
	// Update is called once per frame
	void Update () 
	{

		if (animator)
		{
			if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space)) 
			{
				animator.SetBool("Jump", true);                
			}

			if(Input.GetButtonDown("Fire2") && animator.layerCount >= 2)
			{
				animator.SetBool("Hi", !animator.GetBool("Hi"));
			}
			
		
      		float h = Input.GetAxis("Horizontal");
        	float v = Input.GetAxis("Vertical");
			
			animator.SetFloat("Speed", h*h+v*v);
            animator.SetFloat("Direction", h, DirectionDampTime, Time.deltaTime);	
		}   		  
	}
}
