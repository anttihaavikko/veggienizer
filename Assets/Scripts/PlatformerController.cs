﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlatformerController : MonoBehaviour {

	// configurables
	public float speed = 7.5f;
	public float acceleration = 0.7f;
	public float jump = 16f;
	public float inputBuffer = 0.05f;
	public bool canDoubleJump = false;
	public bool canWallJump = false;
	public bool mirrorWhenTurning = true;
    public ParticleSystem jumpTrail;

	// physics
	public Rigidbody2D body;
	public Transform[] groundChecks;
	public Transform[] wallChecks;
	public LayerMask groundLayer, canJumpLayers;
	public float groundCheckRadius = 0.05f;
	public bool checkForEdges = false;
	private float groundAngle = 0;
	private float gravity;
	private float walljumpCooldown = 0f;
	private float wallHugBuffer = 0f;
	private float walljumpDir = 0f;
    private int jumpFrames = 0;
    private int allowedJumpFrames = 5;
	public Transform feetPoint;
    private bool canJump = true;

	// flags
	public bool canControl = true;
	private bool running = false;
	private bool grounded = false;
	private bool doubleJumped = false;
    private bool respawning = false;
    private bool jumped = false;
    public bool demo = false;

	// misc
	private float jumpBufferedFor = 0;
	public Transform spriteObject;
	public Transform shadow;

	// animations
	public Animator anim;

    public Face face;
    public List<Transform> keepUnMirrored;

    private int stepSound, stepSoundDir = 1;

	// ###############################################################

	// Use this for initialization
	void Start () {
		gravity = body.gravityScale;
	}

    void IncreaseGravity()
    {
        body.gravityScale = gravity * 1.5f;
    }

    void EnableJump()
    {
        canJump = true;
    }

    // Update is called once per frame
    void FixedUpdate () {
		bool wasGrounded = grounded;
        bool leftInAir = false;
        bool rightInAir = false;

        Vector3 pos = body.transform.position + Vector3.down * 0.75f;
        var somethingAt = Physics2D.OverlapCircle(pos, 0.5f, canJumpLayers);

        if (!checkForEdges) {

			grounded = false;

			for (int i = 0; i < groundChecks.Length; i++) {

				Transform groundCheck = groundChecks [i];

                bool tempGrounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, canJumpLayers);

                if (i == 0)
                    leftInAir = !tempGrounded;
                else
                    rightInAir = !tempGrounded;

				grounded = tempGrounded ? tempGrounded : grounded;

				// draw debug lines
				Color debugLineColor = grounded ? Color.green : Color.red;
				Debug.DrawLine (pos, groundCheck.position, debugLineColor, 0.2f);
				Debug.DrawLine (groundCheck.position + Vector3.left * groundCheckRadius, groundCheck.position + Vector3.right * groundCheckRadius, debugLineColor, 0.2f);
			}

            anim.SetBool("grounded", grounded);

            if (grounded)
            {
                if (leftInAir)
                    anim.SetFloat("tiptoe", -1);

                if (rightInAir)
                    anim.SetFloat("tiptoe", 1);

                if (!leftInAir && !rightInAir)
                    anim.SetFloat("tiptoe", 0);
            }

            anim.SetBool("tiptoeing", (leftInAir || rightInAir) && grounded);

        } else {
			grounded = Physics2D.Raycast (transform.position, Vector2.down, 1f);

			// draw debug lines
			Color debugLineColor = grounded ? Color.green : Color.red;
			Debug.DrawRay (transform.position, Vector2.down, debugLineColor, 0.2f);
		}

        // double fall gravity
        Invoke("IncreaseGravity", 0.3f);
        if (!grounded && body.velocity.y <= 0.5f && jumped) {
			body.gravityScale = gravity * 2f;
		}

		// just landed
		if (!wasGrounded && grounded) {
			Land ();
		}

		// just left the ground
		if (wasGrounded && !grounded) {
			groundAngle = 0;
		}

		// jump buffer timing
		if (jumpBufferedFor > 0) {
			jumpBufferedFor -= Time.deltaTime;
		}

		if (shadow) {
            RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector2.down, 20f, groundLayer);
            shadow.position = hit.point;
		}

		// controls
		if (canControl) {

			float inputDirection = InputMagic.Instance.GetAxis (InputMagic.STICK_OR_DPAD_X);

            if (Mathf.Abs(inputDirection) < 0.1f) inputDirection = 0f;

            if (InputMagic.Instance.GetButtonUp(InputMagic.A))
            {
                jumpFrames = allowedJumpFrames + 1;
            }

			// jump
			if (canJump && (grounded || (canDoubleJump && !doubleJumped)) && (InputMagic.Instance.GetButton(InputMagic.A) || jumpBufferedFor > 0)) {

                canJump = false;
                CancelInvoke("EnableJump");
                Invoke("EnableJump", 0.5f);
                Jump();

			} else if (canControl && InputMagic.Instance.GetButton(InputMagic.A)) {
			
				// jump command buffering
				jumpBufferedFor = 0.2f;
			}

			// moving
			Vector2 moveVector = new Vector2 (speed * inputDirection, body.velocity.y);

			if (Mathf.Sign (body.velocity.x) == Mathf.Sign (moveVector.x) || walljumpCooldown > 0f) {
				body.velocity = Vector2.MoveTowards (body.velocity, moveVector, acceleration);
			} else {
				body.velocity = moveVector;
			}

			// direction
			if (mirrorWhenTurning && Mathf.Abs(inputDirection) > inputBuffer) {
				float dir = Mathf.Sign (inputDirection);
				spriteObject.localScale = new Vector2 (dir, 1);

                face.flipped = dir < 0;
                keepUnMirrored.ForEach(item => item.localScale = new Vector2(dir, 1));

//				Transform sprite = transform.Find("Character");
//				Vector3 scl = sprite.localScale;
//				scl.x = dir;
//				sprite.localScale = scl;

                //				transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 90f - dir * 90f, transform.localEulerAngles.z);
            }
				
			bool wallHug = false; 

			if (!checkForEdges) {
				for (int i = 0; i < wallChecks.Length; i++) {
					Vector2 p = wallChecks[i].position;
                    wallHug = Physics2D.OverlapCircle (p, groundCheckRadius, groundLayer) ? true : wallHug;
					Color hugLineColor = grounded ? Color.green : Color.red;
					Debug.DrawLine (transform.position, p, hugLineColor, 0.2f);
				}
			}

			if (wallHug) {
				walljumpDir = -transform.localScale.x;
                body.velocity = new Vector2(0, body.velocity.y);
			}

			if ((wallHug || wallHugBuffer > 0f) && !checkForEdges) {

				if (walljumpCooldown <= 0f) {
					float vertVel = (body.velocity.y > 0 || Mathf.Abs (inputDirection) < 0.5f || !canWallJump) ? body.velocity.y : Mathf.MoveTowards (body.velocity.y, 0, 0.5f);
					body.velocity = new Vector2 (0, vertVel);
				}

				if (wallHug && Mathf.Abs (inputDirection) > 0.5f) {
					wallHugBuffer = 0.2f;
				} else {
					wallHugBuffer -= Time.deltaTime;
				}

				if (canWallJump) {
					if ((Input.GetButtonDown ("Jump") || jumpBufferedFor > 0) && !grounded && walljumpCooldown <= 0f && (Mathf.Abs (inputDirection) > 0.5f || wallHugBuffer > 0f)) {

						jumpBufferedFor = 0;

						body.velocity = Vector2.zero;

						float dir = Mathf.Sign (walljumpDir);
						transform.localScale = new Vector2 (dir, 1);

						body.AddForce (Vector2.up * jump + Vector2.right * walljumpDir * jump * 0.5f, ForceMode2D.Impulse);
						walljumpCooldown = 0.5f;
					}
				}
			}

			if (walljumpCooldown > 0f) {
				walljumpCooldown -= Time.deltaTime;
			}

			running = inputDirection < -inputBuffer || inputDirection > inputBuffer;

            if (!grounded || wallHug) {
				running = false; 
			}

			if (anim) {
                anim.SetFloat("vertical", body.velocity.y);

				if (running) {
					anim.speed = Mathf.Abs (body.velocity.x * 0.2f);
					anim.SetFloat ("speed", Mathf.Abs(body.velocity.x));
				} else {
					anim.speed = 1f;
					anim.SetFloat ("speed", 0);
				}
			}
		}
	}

    private void FootStepSound() {
        AudioManager.Instance.PlayEffectAt(20 + stepSound, groundChecks[0].position, 1f, false);
        stepSound += stepSoundDir;

        if (stepSound == 0 || stepSound == 8)
            stepSoundDir = -stepSoundDir;
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, 0); // reset vertical speed

        body.AddForce(Vector2.up * jump * Time.deltaTime, ForceMode2D.Impulse);

        var vol = 0.7f;
        AudioManager.Instance.PlayEffectAt(42, feetPoint.position, 1f * vol);
        AudioManager.Instance.PlayEffectAt(44, feetPoint.position, 1f * vol);
        AudioManager.Instance.PlayEffectAt(45, feetPoint.position, 0.73f * vol);
        AudioManager.Instance.PlayEffectAt(54, feetPoint.position, 0.541f * vol);
        AudioManager.Instance.PlayEffectAt(55, feetPoint.position, 0.345f * vol);

        jumpTrail.Play();

        jumped = true;

        if (!grounded)
        {
            doubleJumped = true;
        }

        if (canDoubleJump && doubleJumped)
        {
            //EffectManager.Instance.AddEffect(7, transform.position + Vector3.down * 0.5f);
            //EffectManager.Instance.AddEffect(9, transform.position + Vector3.down * 0.5f);
        }

        jumpBufferedFor = 0;

		//AudioManager.Instance.PlayEffectAt (0, transform.position, 0.5f);

		//EffectManager.Instance.AddEffect(5, feetPoint.position + Vector3.up * 1f);

		// animation
		if (anim)
        {
            anim.speed = 1f;
            anim.SetTrigger("jump");
            anim.ResetTrigger("land");
            anim.ResetTrigger("land2");
        }

        body.gravityScale = gravity;

        jumpFrames = 0;
        //EnhanceJump();
    }

    private void EnhanceJump()
    {
        body.AddForce(Vector2.up * jump * 1.12f / allowedJumpFrames * Time.deltaTime, ForceMode2D.Impulse);
        jumpFrames++;
    }

	private void Land() {

        var vol = 0.5f;
        AudioManager.Instance.PlayEffectAt(31, feetPoint.position, 0.793f * vol);
        AudioManager.Instance.PlayEffectAt(32, feetPoint.position, 0.147f * vol);
        AudioManager.Instance.PlayEffectAt(33, feetPoint.position, 0.362f * vol);
        AudioManager.Instance.PlayEffectAt(40, feetPoint.position, 1.414f * vol);
        AudioManager.Instance.PlayEffectAt(45, feetPoint.position, 0.483f * vol);

        jumped = false;

		anim.ResetTrigger ("jump");

		doubleJumped = false;

		body.gravityScale = gravity;

        //AudioManager.Instance.PlayEffectAt (1, transform.position, 0.5f);

        EffectManager.Instance.AddEffect(3, feetPoint.position);
        EffectManager.Instance.AddEffect(0, feetPoint.position);

        // animation
        if (anim) {
			anim.speed = 1f;
			anim.SetTrigger ("land");
            anim.SetTrigger("land2");
        }
	}

	public bool IsGrounded() {
		return grounded;
	}

	public float GetGroundAngle() {
		if (Mathf.Abs (groundAngle) > 90) {
			groundAngle = 0;
		}
		return groundAngle;
	}
}
