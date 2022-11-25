using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Cinemachine;
using Platformer.Core;

namespace Platformer.Mechanics {
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject {
        public bool noCharge = false;
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public bool LimitVelocity = false;

        public CinemachineVirtualCamera vcam;
        public float camZoomA = 3.7f, camZoomB = 3.8f;
        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float flipSpeed = 10, maxSpeed = 7, multiplier = 1, maxCharge = 2, charge, chargeStart = .1f, runTime, timeBeforeRun = 3, runMultiplier = 1.25f, chargeAnim = 3, beforeRun = 2, scalingOffset = 100;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7, currentFlip = 1;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump, chargingJump;
        /*internal new*/
        public Collider2D collider2d;
        /*internal new*/
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true, speedSmoke;

        private Color defaultColor;
        public Color chargedColor = Color.cyan;

        public ParticleSystem speedClouds, burstClouds;

        private float defaultMaxSpeed;

        public Transform spriteChar;
        private bool jump;
        Vector2 move;
        public SpriteRenderer spriteRenderer;
        public Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        public void MoveCharacter(GameObject target) {
            transform.position = target.transform.position;
        }

        float coyoteTime, maxCoyotime = 2;

        void Awake() {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            //spriteRenderer = GetComponent<SpriteRenderer>();
            //animator = GetComponent<Animator>();
            defaultColor = spriteRenderer.color;
            defaultMaxSpeed = maxSpeed;
            charge = chargeStart;
        }

        protected override void Update() {
            if (!Application.isFocused) 
                Time.timeScale = 0;
            else 
                Time.timeScale = 1;

            if (LimitVelocity&& velocity.y < -14)
                    velocity.y = -14;

            float vel = Mathf.Abs(velocity.y);
            spriteChar.localScale = new Vector3(spriteChar.localScale.x, 1 + vel / scalingOffset, 1);
            if (controlEnabled) {
                float playerInput = Input.GetAxis("Horizontal");
                bool falling = Mathf.Abs(vel) != 0f;

                if (!noCharge) {
                    if (chargingJump /* && !falling*/) {
                        charge = charge >= maxCharge ? maxCharge : charge + multiplier * Time.deltaTime;
                        spriteRenderer.color = Color.Lerp(defaultColor, chargedColor, Mathf.PingPong(chargeAnim, (charge - chargeStart) / (maxCharge - chargeStart)));
                    }
                    else {
                        spriteRenderer.color = defaultColor;
                        charge = chargeStart;
                    }
                }

                move.x = playerInput;
                if (/*jumpState == JumpState.Grounded &&*/ Input.GetButton("Jump") /*&& falling*/)
                    chargingJump = true;
                if ((Input.GetButtonUp("Jump") && (chargingJump)) || (!falling && noCharge && Input.GetButton("Jump"))) {
                    if (noCharge)
                        charge = 1;
                    if (!falling && jumpState == JumpState.Grounded) {
                        jumpState = JumpState.PrepareToJump;
                    }
                    else {
                        charge = chargeStart;
                    }
                    chargingJump = false;
                }
                else if (Input.GetButtonUp("Jump")) {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }


                if (Mathf.Abs(playerInput) >= 1 && (IsGrounded && !falling)) {
                    if (runTime == 10) {
                        runTime = 11;
                    }
                    else if (runTime == 11) {
                        if (speedSmoke)
                            speedClouds.Play();
                        vcam.m_Lens.OrthographicSize = Mathf.Lerp(vcam.m_Lens.OrthographicSize, camZoomB, Time.deltaTime);
                    }
                    else if (runTime >= timeBeforeRun && runTime < 10) {
                        maxSpeed = defaultMaxSpeed * runMultiplier;
                        runTime = 10;
                    }
                    else {
                        runTime += Time.deltaTime;
                    }

                    if (runTime >= timeBeforeRun / beforeRun && runTime < 10) {
                        burstClouds.Play();
                        maxSpeed = Mathf.Lerp(maxSpeed, defaultMaxSpeed * runMultiplier, runTime - timeBeforeRun / beforeRun);
                    }
                }
                else if (Mathf.Abs(playerInput) >= 1 && (!IsGrounded || falling)) {
                    if (runTime < 10) {
                        vcam.m_Lens.OrthographicSize = Mathf.Lerp(vcam.m_Lens.OrthographicSize, camZoomA, Time.deltaTime);
                        runTime = 0;
                        maxSpeed = defaultMaxSpeed;
                    }
                    else {
                        if (speedSmoke)
                            speedClouds.Play();
                    }
                }
                else {
                    if (IsGrounded || falling) {
                        maxSpeed = defaultMaxSpeed;
                        runTime = 0;
                        vcam.m_Lens.OrthographicSize = Mathf.Lerp(vcam.m_Lens.OrthographicSize, camZoomA, Time.deltaTime);
                    }
                }
            }
            else {
                vcam.m_Lens.OrthographicSize = Mathf.Lerp(vcam.m_Lens.OrthographicSize, camZoomA, Time.deltaTime);
                chargingJump = false;
                charge = chargeStart;
                spriteRenderer.color = defaultColor;
                maxSpeed = defaultMaxSpeed;
                runTime = 0;
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState() {
            jump = false;
            switch (jumpState) {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded) {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    coyoteTime += Time.deltaTime;
                    if (IsGrounded) {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    coyoteTime = 0;
                    //chargingJump = false;
                    //charge = chargeStart;
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity() {
            if (jump && IsGrounded) {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier * charge;
                jump = false;
            }
            else if (stopJump) {
                stopJump = false;
                if (velocity.y > 0) {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                currentFlip = 1;
            else if (move.x < -0.01f)
                currentFlip = -1;

            spriteChar.localScale = Vector3.Lerp(spriteChar.localScale, new Vector3(currentFlip, spriteChar.localScale.y, spriteChar.localScale.z), flipSpeed * Time.deltaTime);

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}