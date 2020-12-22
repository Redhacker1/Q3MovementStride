using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;

namespace MyFPSTest
{
    public class Trigger_ChangeLvl : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        bool isLoaded = false;
        PhysicsComponent GamePhysicsComponent;
        Scene Level;

        public override void Start()
        {
            Level = Content.Load<Scene>("ThirdPersonScene");
            GamePhysicsComponent = Entity.Get<PhysicsComponent>();
            isLoaded = true;
        }

        public override void Update()
        {
            DebugText.Print(string.Format("STATUS Loaded {0}", isLoaded), new Int2(100, 150));
            if(GamePhysicsComponent != null)
            {
                DebugText.Print(string.Format("STATUS: Physics Object Detected!", isLoaded), new Int2(100, 200));
            }
            if(GamePhysicsComponent.Collisions.Count > 0)
            {
                DebugText.Print("STATUS: Collided With Something", new Int2(100, 250));
                Entity.Scene = null;
                foreach(var Collision in GamePhysicsComponent.Collisions )
                {
                    Collision.ColliderA.Entity.Scene.Children.Add(Level);
                    Collision.ColliderA.Entity.Transform.Position = new Vector3(0, 0, 0);
                }

            }
            // Do stuff every new frame
        }
    }
}
