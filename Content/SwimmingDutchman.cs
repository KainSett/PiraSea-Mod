

namespace PiraSea.Content
{
    public class SmallBoat : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
        {
            return entity.aiStyle == NPCAIStyleID.FlyingDutchman;
        }
        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            npc.width = 360;
            npc.height = 100;
            npc.damage = 1;
            npc.noTileCollide = false;
        }
        public override bool PreAI(NPC npc)
        {
            npc.velocity *= 0.97f;
            npc.direction = npc.velocity.X < 0 ? -1 : npc.velocity.X > 0 ? 1 : npc.direction;
            npc.spriteDirection = (npc.direction - 1) / -2;

            for (int y = 0; y < npc.height; y++)
            {
                // solid stop
                for (int X = -16; X == -16; X = npc.width + 16)
                {
                    var XY = new Vector2(X, y).ToTileCoordinates();
                    var solid = tile[npc.position.ToTileCoordinates().X + XY.X, npc.position.ToTileCoordinates().Y + XY.Y];

                    if (WorldGen.SolidOrSlopedTile(solid))
                    {
                        npc.velocity.X *= 0.5f;
                    }
                }

                // water "float"
                for (int x = npc.width; x > 0; x--)
                {
                    var XY = new Vector2(x, y).ToTileCoordinates();
                    var water = tile[npc.position.ToTileCoordinates().X + XY.X, npc.position.ToTileCoordinates().Y + XY.Y];

                    if (water.LiquidAmount > 0 && water.LiquidType == (byte)LiquidID.Water)
                    {
                        npc.velocity.Y += -(80 - y) * 0.002f;

                        return false;
                    }
                }
            }
            npc.velocity.Y += 1;


            return false;
        }
        public override void DrawBehind(NPC npc, int index)
        {
            instance.DrawCacheNPCsOverPlayers.Add(index);
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (!target.controlDownHold && target.velocity.Y > 0 && target.Center.Between(npc.position + new Vector2(0, -20 - target.height / 2), npc.position + new Vector2(npc.width, 0)))
            {
                npc.velocity.X = -7;
                target.velocity.Y *= 0;
                target.Center += new Vector2(npc.velocity.X * 0.97f / 2, 0);
            }
            modifiers.Cancel();
        }
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
            // cabin top
            if (!target.controlDownHold && target.velocity.Y > 0 && target.Center.Between(npc.position + new Vector2(30 * (npc.spriteDirection - 1) + npc.spriteDirection * npc.width / 2, 5 - npc.height - target.height / 2), npc.position + new Vector2(npc.width / 2 + npc.spriteDirection * npc.width / 2 + 30 * npc.spriteDirection, -npc.height)))
            {
                npc.velocity.X = -7;
                target.velocity.Y *= 0;
                target.Center += new Vector2(npc.velocity.X * 0.97f / 2, 0);
            }

            return true;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            //npc.spriteDirection = 1;
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}