using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
    
namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public bool noCharge = false;
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7, multiplier = 1, maxCharge = 2, charge, chargeStart = .1f, runTime, timeBeforeRun = 3, runMultiplier = 1.25f, chargeAnim = 3, beforeRun=2, scalingOffset = 100;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump, chargingJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
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



        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            //spriteRenderer = GetComponent<SpriteRenderer>();
            //animator = GetComponent<Animator>();
            defaultColor = spriteRenderer.color;
            defaultMaxSpeed = maxSpeed;
            charge = chargeStart;
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                float vel=Mathf.Abs(velocity.y);
                
                spriteChar.localScale = new Vector3(1,1+vel/scalingOffset,1);

                float playerInput = Input.GetAxis("Horizontal");
                bool falling = Mathf.Abs(vel) != 0f;
                
                if(!noCharge){
                    if (chargingJump /* && !falling*/){
                        charge = charge >= maxCharge ? maxCharge : charge + multiplier * Time.deltaTime;
                        spriteRenderer.color = Color.Lerp(defaultColor, chargedColor, Mathf.PingPong(chargeAnim,(charge - chargeStart)/(maxCharge - chargeStart)));
                    }
                    else{
                        spriteRenderer.color = defaultColor;
                        charge = chargeStart;
                    }
                }

                move.x = playerInput;
                if (/*jumpState == JumpState.Grounded &&*/ Input.GetButton("Jump") /*&& falling*/)
                    chargingJump = true;
                if ((Input.GetButtonUp("Jump") && (chargingJump)) || (!falling && noCharge && Input.GetButton("Jump"))){
                    if(noCharge)
                        charge = 1;
                    if(!falling && jumpState == JumpState.Grounded ){
                        jumpState = JumpState.PrepareToJump;
                    }
                    else{
                        charge = chargeStart;
                    }
                    chargingJump = false;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
                
                
                if(Mathf.Abs(playerInput) >= 1 && IsGrounded && !falling){
                    if(runTime == 10){
                        runTime = 11;
                    }
                    else if (runTime == 11){
                        if(speedSmoke) 
                            speedClouds.Play();
                        //Nothing!
                    }
                    else if(runTime >= timeBeforeRun && runTime < 10){
                        maxSpeed = defaultMaxSpeed * runMultiplier;
                        runTime = 10;
                    }
                    else{
                        runTime += Time.deltaTime;
                    }

                    if(runTime >= timeBeforeRun/beforeRun && runTime < 10){
                        burstClouds.Play();
                        maxSpeed = Mathf.Lerp(maxSpeed, defaultMaxSpeed * runMultiplier, runTime-timeBeforeRun/beforeRun);
                    }
                }
                else if(Mathf.Abs(playerInput) >= 1 && (!IsGrounded || falling)){
                    if(runTime < 10){
                        runTime = 0;
                        maxSpeed = defaultMaxSpeed;
                    }
                    else{
                        if(speedSmoke) 
                            speedClouds.Play();
                    }
                }
                else{
                    if(IsGrounded || falling){
                        maxSpeed = defaultMaxSpeed;
                        runTime = 0;
                    }
                }
            }
            else
            {
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

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    //chargingJump = false;
                    //charge = chargeStart;
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier * charge;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;


            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}