

using Microsoft.Xna.Framework.Graphics;

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
            npc.lifeMax = 100;
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

            for (int y = npc.height - 534; y < npc.height; y++)
            {
                // solid stop
                for (int X = -16; X == -16; X = npc.width + 16)
                {
                    var XY = new Vector2(X, y).ToTileCoordinates();

                    if (WorldGen.SolidOrSlopedTile(npc.position.ToTileCoordinates().X + XY.X, npc.position.ToTileCoordinates().Y + XY.Y))
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

                        WaterDepth = (534 - (y - (npc.height + 30 - 534))) / 534f;

                        return false;
                    }
                }
            }
            npc.velocity.Y += 1;


            return false;
        }
        public float WaterDepth = 0f;
        public override void DrawBehind(NPC npc, int index)
        {
            instance.DrawCacheNPCsOverPlayers.Add(index);
        }
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (!target.controlDownHold && target.velocity.Y > 0 && target.Center.Between(npc.position + new Vector2(0, -20 - target.height / 2), npc.position + new Vector2(npc.width, 0)))
            {
                npc.velocity.X = -7;//target.direction * 7;
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
                npc.velocity.X = -7;//target.direction * 7;
                target.velocity.Y *= 0;
                target.Center += new Vector2(npc.velocity.X * 0.97f / 2, 0);
            }

            return true;
        }
        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var texture = TextureAssets.Npc[npc.type].Value;
            var oarTex = TextureAssets.Extra[41].Value;

            npc.spriteDirection = (npc.direction - 1) / -2;

            var totalWidth = 590;
            var totalHeight = 534;
            ref var rect = ref npc.frame; //new Rectangle(0, 0, totalWidth, totalHeight);

            for (int Y = 0; Y < totalHeight; Y += 8)
            {
                rect.Height = 8;
                rect.Y = Y;


                drawColor = Color.White;

                if ((totalHeight - Y) * (1f / totalHeight) <= WaterDepth + 0.005f) 
                {
                    drawColor.R = 100;
                    drawColor.G = 150;
                }
                else
                    drawColor = Color.White;


                spriteBatch.Draw(texture, npc.Center - screenPos - new Vector2((npc.width - totalWidth) / 2 * (-npc.spriteDirection * 2 + 1) - 30 * (npc.spriteDirection * 2 - 1),(totalHeight - npc.height * 1.5f) / 2  - Y), rect, drawColor, npc.rotation, texture.Size() / 2, npc.scale, (SpriteEffects)npc.spriteDirection, 0);
            }

            // oar
            spriteBatch.Draw(oarTex, npc.Center - screenPos, drawColor);

            return false;

            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }
    }
}