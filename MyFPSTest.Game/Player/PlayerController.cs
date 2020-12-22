// Copyright (c) Stride contributors (https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Engine.Events;
using Stride.Physics;
using Stride.Audio;

namespace MyFPSTest.Player
{
    public class PlayerController : SyncScript
    {
        /*Frame occuring factors*/
        //public float gravity = 20f; //Currently broken and disabled

        public float friction = 6f; //Ground friction

        /* Movement stuff */
        public float moveSpeed = 10.0f;               // Ground move speed
        public float runAcceleration = 24.0f;         // Ground accel
        public float runDeacceleration = 1.0f;        // Deacceleration that occurs when running on the ground
        public float airAcceleration = 2.0f;          // Air accel
        public float airDecceleration = 2.0f;         // Deacceleration experienced when ooposite strafing
        public float airControl = 0.3f;               // How precise air control is
        public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
        public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
        public bool holdJumpToBhop = false;           // When enabled allows player to just hold jump button to keep on bhopping perfectly. Beware: smells like casual

        // Last projected Velocity
        public Vector3 PreviousVelocity = Vector3.Zero;
        //Projected Velocity
        private Vector3 playerVelocity = Vector3.Zero;

        // Q3: players can queue the next jump just before he hits the ground
        private bool wishJump = false;
        //Locks the jump to prevent holding down the spacebar from allowing you to continuously jumpS
        private bool JumpLock = false;

        [Display("Run Speed")]
        public float MaxRunSpeed { get; set; } = 5;

        public static readonly EventKey<float> RunSpeedEventKey = new EventKey<float>();

        // This component is the physics representation of a controllable character
        private CharacterComponent character;

        private readonly EventReceiver<Vector3> moveDirectionEvent = new EventReceiver<Vector3>(PlayerInput.MoveDirectionEventKey);
        public Vector3 Movedir = Vector3.Zero;

        private readonly EventReceiver<bool> QueueJump_Event = new EventReceiver<bool>(PlayerInput.QueueJump_Event);
        private readonly AudioEmitter JumpEmitter = new AudioEmitter();

        /// <summary>
        /// Called when the script is first initialized
        /// </summary>
        public override void Start()
        {
            Log.ActivateLog(Stride.Core.Diagnostics.LogMessageType.Debug);
            Log.Debug("Starting Log");
            base.Start();
            // Will search for an CharacterComponent within the same entity as this script
            character = Entity.Get<CharacterComponent>();
            if (character == null) throw new ArgumentException("Please add a CharacterComponent to the entity containing PlayerController!");
        }

        /// <summary>
        /// Called on every frame update
        /// </summary>
        public override void Update()
        {
            PreviousVelocity = playerVelocity;
            moveDirectionEvent.TryReceive(out Movedir);
            Console.WriteLine(playerVelocity);

            OnGUI();
            QueueJump();
            /* Movement, here's the important part */
            if (character.IsGrounded)
            {
                GroundMove();
                if(playerVelocity != Vector3.Zero || playerVelocity != Vector3.One)
                {
                    character.SetVelocity(playerVelocity);
                }
                else if (Math.Abs(playerVelocity.Z) == 0 && Math.Abs(playerVelocity.X) == 0 && Math.Abs(playerVelocity.Y) == 0)
                {

                }
                else
                {
                    playerVelocity -= float.Epsilon*2;
                    character.SetVelocity(playerVelocity);
                }
            }
            else if (!character.IsGrounded)
            {
                AirMove();
                if (playerVelocity != Vector3.Zero)
                {
                    character.SetVelocity(playerVelocity);
                }
            }
        }

        private void OnGUI()
        {
            var ups = PreviousVelocity;
            ups.Y = 0;

            DebugText.Print("Speed: " + Math.Round(ups.Length() * 100) / 100 + "ups", new Int2(100, 100));
        }

        private void AirMove()
        {
            Vector3 wishdir;
            float accel;


            wishdir = new Vector3(Movedir.X, 0, Movedir.Z);

            float wishspeed = wishdir.Length();
            wishspeed *= moveSpeed;

            wishdir.Normalize();

            // CPM: Aircontrol
            float wishspeed2 = wishspeed;
            if (Vector3.Dot(playerVelocity, wishdir) < 0)
                accel = airDecceleration;
            else
                accel = airAcceleration;
            // If the player is ONLY strafing left or right
            if (Movedir.Z == 0 && Movedir.X != 0)
            {
                if (wishspeed > sideStrafeSpeed)
                    wishspeed = sideStrafeSpeed;
                accel = sideStrafeAcceleration;
            }

            Accelerate(wishdir, wishspeed, accel);
            if (airControl > 0)
                AirControl(wishdir, wishspeed2);
            // !CPM: Aircontrol

            // Apply gravity (Disabled)
            //playerVelocity.y -= gravity * Time.deltaTime;
        }

        /**
         * Air control occurs when the player is in the air, it allows
         * players to move side to side much faster rather than being
         * 'sluggish' when it comes to cornering.
         */
        private void AirControl(Vector3 wishdir, float wishspeed)
        {
            double zspeed;
            double speed;
            double dot;
            double k;

            // Can't control movement if not moving forward or backward
            if (Math.Abs(Movedir.Z) < 0.001 || Math.Abs(wishspeed) < 0.001)
                return;
            zspeed = playerVelocity.Y;
            playerVelocity.Y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            speed = playerVelocity.Length();
            playerVelocity.Normalize();

            dot = Vector3.Dot(playerVelocity, wishdir);
            k = 32;
            k *= airControl * Math.Pow(dot, 2) * (float)Game.DrawTime.TimePerFrame.TotalSeconds;

            // Change direction while slowing down
            if (dot > 0)
            {
                playerVelocity.X = (float)(playerVelocity.X * speed + wishdir.X * k);
                playerVelocity.Y = (float)(playerVelocity.Y * speed + wishdir.Y * k);
                playerVelocity.Z = (float)(playerVelocity.Z * speed + wishdir.Z * k);

                playerVelocity.Normalize();
            }

            playerVelocity.X *= (float)speed;
            playerVelocity.Y = (float)zspeed; // Note this line
            playerVelocity.Z *= (float)speed;

        }

        /**
    * Queues the next jump just like in Q3
    */
        private void QueueJump()
        {
            QueueJump_Event.TryReceive(out bool TryJump);
            if (holdJumpToBhop == true)
            {

                wishJump = TryJump;
                return;
            }
            else if (TryJump && !JumpLock)
            {
                wishJump = true;
                JumpLock = true;
            }
            else if (TryJump)
            {
                JumpLock = true;
                wishJump = false;
            }
            else
            {
                wishJump = false;
                JumpLock = false;
            }
            return;
        }

        private void GroundMove()
        {
            Vector3 wishdir;

            // Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                ApplyFriction(1.0f);
            else
                ApplyFriction(0);

            wishdir = new Vector3(Movedir.X, 0, Movedir.Z);
            DebugText.Print(Movedir.ToString(), new Int2(200, 400));
            wishdir.Normalize();

            var wishspeed = wishdir.Length();
            wishspeed *= moveSpeed;

            Accelerate(wishdir, wishspeed, runAcceleration);

            if (wishJump)
            {
                Sound JumpSound = new Sound();
                //JumpSound.CreateInstance(null, false, false, 0F, HrtfEnvironment.Small);
                character.Jump();
                wishJump = false;
            }
        }
        private void ApplyFriction(float t)
        {
            Vector3 vec = playerVelocity; // Equivalent to: VectorCopy();
            float speed;
            float newspeed;
            float control;
            float drop;

            vec.Y = 0.0f;
            speed = vec.Length();
            drop = 0.0f;

            /* Only if the player is on the ground then apply friction */
            if (character.IsGrounded)
            {
                control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = (float)(control * friction * Game.DrawTime.TimePerFrame.TotalSeconds * t);
            }

            newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;

            playerVelocity.X *= newspeed;
            playerVelocity.Z *= newspeed;
        }

        private void Accelerate(Vector3 wishdir, float wishspeed, float accel)
        {
            float addspeed;
            float accelspeed;
            float currentspeed;

            currentspeed = Vector3.Dot(playerVelocity, wishdir);
            addspeed = wishspeed - currentspeed;
            if (addspeed <= 0)
                return;
            accelspeed = accel * (float)Game.DrawTime.TimePerFrame.TotalSeconds * wishspeed;
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            playerVelocity.X += accelspeed * wishdir.X;
            playerVelocity.Z += accelspeed * wishdir.Z;
        }


    }
}
