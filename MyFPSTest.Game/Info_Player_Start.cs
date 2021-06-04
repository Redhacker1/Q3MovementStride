using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using MyFPSTest.Player;
using Stride.Engine.Processors;

namespace MyFPSTest
{
    public class Info_Player_Start : SyncScript
    {
        // Declared public member fields and properties will show in the game studio
        bool FoundPlayer = false;
        bool SpawnPlayer = true;
        Entity entity_root;
        Entity Player;
        public override void Start()
        {
            entity_root = Entity.FindRoot();
            Log.ActivateLog(Stride.Core.Diagnostics.LogMessageType.Debug);
            Log.Debug("Running, Info Player Start");
            foreach (var entity in Entity.Scene.Entities)
            {
                bool isPlayer = false;
                if(entity.Name.ToLower().Contains("player") && entity.Name.ToLower() != "Info_Player_Start".ToLower())
                {
                    isPlayer = true;
                }
                if (isPlayer )
                {
                    Player = entity;
                    FoundPlayer = true;
                }
            }
        }
        public override void Update()
        {
            Player.Transform.Position = entity_root.Transform.Position;
            //Script.NextFrame();
            if (Player.Transform.Position != entity_root.Transform.Position)
            {
                Update();
            }
            else
            {
                SpawnPlayer = false;
            }
        }
    }
}
