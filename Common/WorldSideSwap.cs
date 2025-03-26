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
                tasks.Insert(SettleLiquidsAgainIndex - 1, new WorldSideSwapGenPass("World Side swap", 100f));
                tasks.Insert(SettleLiquidsAgainIndex, new BiggerCentralOceanGenPass("Bigger central ocean", 100f));
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
                var Y = spawnTileY - 50;
                for (int y = spawnTileY - 50; y < spawnTileY + 160; y++)
                {
                    if (tile[(maxTilesX) / 2 - 1, y].TileType == TileID.Sand)
                    {
                        Y = y - 190;
                    }
                }
                var leftSide = StructureData.FromWorld((maxTilesX) / 2 - 200, Y, 200, 190);

                for (int y = spawnTileY - 50; y < spawnTileY + 160; y++)
                {
                    if (tile[(maxTilesX) / 2 + 1, y].TileType == TileID.Sand)
                    {
                        Y = y - 190;
                    }
                }
                var rightSide = StructureData.FromWorld((maxTilesX / 2), Y, 200, 190);


                StructureHelper.API.Generator.GenerateFromData(leftSide, new Point16((maxTilesX) / 2 - 400, spawnTileY - 100));
                StructureHelper.API.Generator.GenerateFromData(rightSide, new Point16((maxTilesX) / 2 + 200, spawnTileY - 100));

                List<int> bottom = [];
                for (int x = 0; x < 500; x++)
                {
                    for (int y = 0; y < 160; y++)
                    {
                        if (tile[(maxTilesX) / 2 - 250 + x, spawnTileY + 90 - y].TileType == TileID.Sand)
                        {
                            if (bottom.Count <= x)
                                bottom.Add(y);
                            else
                                bottom[x] = y;
                        }
                    }
                }

                for (int i = 1; i < bottom.Count - 1; i++)
                {
                    bottom[i] = (bottom[i] + bottom[i - 1]) / 2;
                    for (int y = 0; y < bottom[i]; y++)
                    {
                        if (tile[(maxTilesX) / 2 - 250 + i, spawnTileY + 90 - y].TileType == TileID.Sand)
                            break;
                        else
                            tile[(maxTilesX) / 2 - 250 + i, spawnTileY + 90 - y].ResetToType(TileID.Sand);
                    }
                }

                for (int i = 1; i < bottom.Count - 1; i++)
                {
                    bottom[i] = (bottom[i] + bottom[(i + bottom.Count / 2) % bottom.Count]) / 2;
                    for (int y = 0; y < bottom[i]; y++)
                    {
                        if (tile[(maxTilesX) / 2 - 250 + i, spawnTileY + 90 - y].TileType == TileID.Sand)
                            break;
                        else
                            tile[(maxTilesX) / 2 - 250 + i, spawnTileY + 90 - y].ResetToType(TileID.Sand);
                    }
                }
            }
        }
    }
}