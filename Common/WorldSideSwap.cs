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
                        if (tile[spawnTileX + x, y].TileType == TileID.Sand)
                            lowestSandY = y;
                    }
                }

                var width = maxTilesX / 4;
                for (int x = spawnTileX - width / 2; x < spawnTileX + width / 2; x++)
                {
                    for (int y = spawnTileY - 150; y < spawnTileY - 50 + width / 5; y++)
                    {
                        var funcY = int.Abs(y - (spawnTileY - 50 + width / 5));

                        if (tile[x, y].TileType != TileID.RainCloud && tile[x, y].TileType != TileID.SnowCloud && tile[x, y].TileType != TileID.Cloud && funcY.CheckBottomBowlFunc(x - spawnTileX, width))
                            tile[x, y].ClearEverything();

                        if (y >= spawnTileY - 50 && funcY.BowlFunc(x - spawnTileX, width))
                            tile[x, y].ResetToType(TileID.Sand);

                        else if (y >= spawnTileY - 50 && funcY * (1f / width) <= 0.18f && funcY.CheckTopBowlFunc(x - spawnTileX, width))
                        {
                            tile[x, y].LiquidAmount = 255;
                            WorldGen.PlaceLiquid(x, y, (byte)LiquidID.Water, 255);
                        }
                    }
                }

                List<int> ground = [];
                for (int a = 0; a < 110 + width / 5; a++)
                {
                    ground.Add(a);
                }

                for (int i = 0; i < 2; i++)
                {
                    for (int x = spawnTileX - width / 2; x > spawnTileX - width / 2 - 100; x--)
                    {
                        for (int y = spawnTileY - 50; y < spawnTileY + 50 + width / 5; y++)
                        {
                            if (i == 0)
                            {
                                if (y - spawnTileY + 50 > ground[int.Abs(x - spawnTileX - width / 2)] && WorldGen.TileEmpty(x, y))
                                {
                                    ground[int.Abs(x - spawnTileX - width / 2)] = y;
                                }
                            }
                            else
                            {
                                if (y - spawnTileY + 50 > int.Abs(x - spawnTileX - width / 2) && WorldGen.TileEmpty(x, y))
                                {
                                    tile[x, y].ResetToType(TileID.Sand);
                                }
                            }
                        }
                    }
                }
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
    }
}
