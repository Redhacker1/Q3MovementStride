using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using MyFPSTest.Player;

namespace MyFPSTest
{
    public class Info_Player_Start_Async : AsyncScript
    {
        // Declared public member fields and properties will show in the game studio
        bool FoundPlayer = false;
        bool SpawnPlayer = true;
        Entity Player;
        public override async Task Execute()
        {
            foreach (var entity in Entity.Scene.Entities)
            {
                bool isPlayer = false;
                if (entity.Name.ToLower().Contains("player") && entity.Name.ToLower() != "Info_Player_Start".ToLower())
                {
                    isPlayer = true;
                    Player = entity;
                }
                if (isPlayer)
                {
                    Player = entity;
                    FoundPlayer = true;
                }
            }

            while (Game.IsRunning)
            {
                Console.WriteLine(Entity.Transform.Position);
                // Do stuff every new frame
                //var Hello = Player.Get<PlayerController>();
                //Console.WriteLine(Hello == null);
            }
        }
    }
}
