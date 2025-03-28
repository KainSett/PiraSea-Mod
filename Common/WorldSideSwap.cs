using Terraria.Graphics.Light;
using Terraria.WorldBuilding;

namespace PiraSea.Common
{
    public class WorldSideSwap : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int SettleLiquidsAgainIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Settle Liquids Again"));
            if (SettleLiquidsAgainIndex != -1)
            {
                tasks.Insert(SettleLiquidsAgainIndex, new WorldSideSwapGenPass("World Side swap", 100f));
                tasks.Insert(SettleLiquidsAgainIndex + 1, new BiggerCentralOceanGenPass("Bigger central ocean", 100f));
            }
        }
        public class WorldSideSwapGenPass : GenPass
        {
            public WorldSideSwapGenPass(string name, double loadWeight) : base(name, loadWeight)
            {
            }
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                var leftSide = StructureData.FromWorld(40, 40, (maxTilesX) / 2 - 40, maxTilesY - 80);
                var rightSide = StructureData.FromWorld((maxTilesX / 2), 40, (maxTilesX) / 2 - 40, maxTilesY - 80);

                StructureHelper.API.Generator.GenerateFromData(leftSide, new Point16((maxTilesX) / 2, 40));
                StructureHelper.API.Generator.GenerateFromData(rightSide, new Point16(40, 40));
            }
        }
        public class BiggerCentralOceanGenPass : GenPass
        {
            public BiggerCentralOceanGenPass(string name, double loadWeight) : base(name, loadWeight)
            {
            }
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                int lowestSandY = spawnTileY;

                for (int x = -1; x != 1; x += 2)
                {
                    for (int y = lowestSandY; y < spawnTileY + 200; y++)
                    {
                        if (tile[spawnTileX + x, y].TileType == Sand)
                            lowestSandY = y;
                    }
                }

                HashSet<ushort> blackList = [RainCloud, SnowCloud, TileID.Cloud, BlueDungeonBrick, CrackedBlueDungeonBrick, CrackedGreenDungeonBrick, CrackedPinkDungeonBrick, GreenDungeonBrick, PinkDungeonBrick];

                var width = maxTilesX / 4;
                for (int x = spawnTileX - width / 2; x < spawnTileX + width / 2; x++)
                {
                    for (int y = spawnTileY - 150; y < spawnTileY - 50 + width / 5; y++)
                    {
                        if (blackList.Contains(tile[x, y].TileType))
                            continue;

                        var funcY = int.Abs(y - (spawnTileY - 50 + width / 5));

                        if (funcY.CheckBottomBowlFunc(x - spawnTileX, width))
                            tile[x, y].ClearEverything();

                        if (y >= spawnTileY - 50 && funcY.BowlFunc(x - spawnTileX, width))
                            tile[x, y].ResetToType(Sand);

                        else if (y >= spawnTileY - 50 && funcY * (1f / width) <= 0.18f && funcY.CheckTopBowlFunc(x - spawnTileX, width))
                        {
                            tile[x, y].LiquidAmount = 255;
                            WorldGen.PlaceLiquid(x, y, (byte)LiquidID.Water, 255);
                        }
                    }
                }

                return;

                var leftPeak = new Point(spawnTileX + 50 - width / 2, spawnTileY - 50);
                var leftTarget = new Point(spawnTileX - 100 - width / 2, spawnTileY - 50);

                var rightPeak = new Point(spawnTileX - 50 + width / 2, spawnTileY - 50);
                var rightTarget = new Point(spawnTileX + 100 + width / 2, spawnTileY - 50);


                for (int y = spawnTileY - 50; y < spawnTileY + 100; y++)
                {
                    if (leftPeak.Y == spawnTileY - 50 && tile[leftPeak.X, y].TileType == Sand)
                    {
                        leftPeak.Y = y;
                    }

                    if (rightPeak.Y == spawnTileY - 50 && tile[rightPeak.X, y].TileType == Sand)
                    {
                        rightPeak.Y = y;
                    }

                    if (leftTarget.Y == spawnTileY - 50 && !WorldGen.TileEmpty(leftTarget.X, y))
                    {
                        leftTarget.Y = y;
                    }

                    if (rightTarget.Y == spawnTileY - 50 && !WorldGen.TileEmpty(rightTarget.X, y))
                    {
                        rightTarget.Y = y;
                    }
                }

                var leftHeight = leftPeak.Y - leftTarget.Y;
                for (int x = spawnTileX - 150 - width / 2; x < spawnTileX + 50 - width / 2; x++)
                {
                    for (int y = spawnTileY - 50; y < spawnTileY + 100; y++)
                    {
                        if (WorldGen.TileEmpty(x, y) && int.Abs(y - spawnTileY + 50).CheckQuadraticConnectionFunc(int.Abs(x - spawnTileX + 50 + width / 2), 150, int.Abs(leftHeight)))
                        {
                            tile[x, y].ResetToType(Sand);
                        }
                        else if (!WorldGen.TileEmpty(x, y))
                            continue;
                    }
                }

                var rightHeight = rightPeak.Y - rightTarget.Y;
            }
        }
    }
    public static class Calc
    {
        public static bool BowlFunc(this int y, int x, float width)
        {
            var Y = y * (1f / width);
            var X = x * (1f / width);


            float bottom = float.Pow(X, 4) / 0.31f;
            if (Y <= bottom)
                return false;

            float top = float.Pow(X, 4) / 0.3f + 0.05f;
            if (Y >= top)
                return false;

            float sidePeaks = -float.Pow(X, 2) * 0.6f + 0.33f;
            if (Y >= sidePeaks)
                return false;

            return true;
        }
        public static bool CheckBottomBowlFunc(this int y, int x, float width)
        {
            var Y = y * (1f / width);
            var X = x * (1f / width);

            float bottom = float.Pow(X, 4) / 0.31f;
            if (Y <= bottom)
                return false;

            return true;
        }
        public static bool CheckTopBowlFunc(this int y, int x, float width)
        {
            var Y = y * (1f / width);
            var X = x * (1f / width);

            float top = float.Pow(X, 4) / 0.3f + 0.05f;
            if (Y >= top)
                return true;

            return false;
        }
        public static bool CheckQuadraticConnectionFunc(this int y, int x, float width, float height)
        {
            var Y = y * (1f / width);
            var X = x * (1f / width);
            var Height = height * (1f / width);

            float top = Height - float.Pow(X, 2) * Height;
            if (Y <= top)
                return false;

            return true;
        }
    }
}