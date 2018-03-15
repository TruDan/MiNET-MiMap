using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Resources;
using System.Text.RegularExpressions;
using MiMap.ResourcePackLib.Json;
using MiMap.ResourcePackLib.Json.BlockStates;
using MiMap.ResourcePackLib.Json.Models;
using Newtonsoft.Json;

namespace MiMap.ResourcePackLib
{
    public class MinecraftResourcePack : IDisposable
    {
		
        public IReadOnlyDictionary<string, BlockState> BlockStates
        {
            get { return _blockStates; }
        }

        private ZipArchive _archive;

        private readonly Dictionary<uint, string> _blockStateIds = new Dictionary<uint, string>();
        private readonly Dictionary<string, BlockState> _blockStates = new Dictionary<string, BlockState>();
        private readonly Dictionary<string, BlockModel> _blockModelsCache = new Dictionary<string, BlockModel>();
        private readonly Dictionary<string, Bitmap> _textureCache = new Dictionary<string, Bitmap>();

        public MinecraftResourcePack() : this(new ZipArchive(
            typeof(MinecraftResourcePack).Assembly.GetManifestResourceStream(typeof(MinecraftResourcePack).Namespace + ".default.zip")))
        {
        }

	    public MinecraftResourcePack(string zipPath) : this(File.OpenRead(zipPath))
	    {
		}
	    public MinecraftResourcePack(Stream zipStream) : this(new ZipArchive(zipStream))
	    {
	    }
		public MinecraftResourcePack(ZipArchive archive)
        {
            _archive = archive;
            Load();
        }

        private void Load()
        {
            LoadBlockStates();
        }

        private void LoadBlockStates()
        {
            var idMapping = new Dictionary<uint, string>();
            using (var idMappingJsonFile =
                typeof(MinecraftResourcePack).Assembly.GetManifestResourceStream(
                    typeof(MinecraftResourcePack).Namespace + ".blockstate-ids.json"))
            {
                using (var reader = new StreamReader(idMappingJsonFile))
                {
                    var json = reader.ReadToEnd();
                    idMapping = MCJsonConvert.DeserializeObject<Dictionary<uint, string>>(json);
                }
            }

            foreach (var kvp in idMapping)
            {
                _blockStateIds.Add(kvp.Key, kvp.Value);
            }

            var jsonFiles = _archive.Entries
                .Where(e => e.FullName.StartsWith("assets/minecraft/blockstates/") && e.FullName.EndsWith(".json")).ToArray();

            foreach (var jsonFile in jsonFiles)
            {
                using (var r = new StreamReader(jsonFile.Open()))
                {
                    var blockState = MCJsonConvert.DeserializeObject<BlockState>(r.ReadToEnd());
                    ProcessBlockState(blockState);
                    _blockStates[jsonFile.Name.Replace(".json", "")] = blockState;
                }
            }
        }

        private bool TryGetBlockModel(string modelName, out BlockModel model)
        {
	        if (_blockModelsCache.TryGetValue(modelName, out model))
	        {
		        return true;
	        }

            model = null;

            var modelFile =
                _archive.Entries.FirstOrDefault(e => e.FullName.Equals("assets/minecraft/models/" + modelName + ".json"));
            if (modelFile == null)
            {
                Debug.WriteLine("Failed to load Block Model: File Not Found (" + "assets/minecraft/models/" + modelName + ".json)");
                return false;
            }


            using (var r = new StreamReader(modelFile.Open()))
            {
                var json = r.ReadToEnd();
                try
                {
                    var blockModel = MCJsonConvert.DeserializeObject<BlockModel>(json);
                    blockModel.Name = modelName;

                    ProcessBlockModel(blockModel);
                    _blockModelsCache[modelName] = blockModel;

                    model = blockModel;
                    return true;
                }
                catch (JsonException ex)
                {
                    Debug.WriteLine("Failed to load Block Model: " + modelName);
                    throw;
                }
            }
        }

	    public bool TryGetTexture(BlockModel model, string textureName, out Bitmap texture)
        {
            if (textureName.StartsWith("#"))
            {
	            textureName = textureName.TrimStart('#');

				if (model.TextureDefinitions.TryGetValue(textureName, out textureName))
                {
                    return TryGetTexture(model, textureName, out texture);
                }

                texture = null;
                return false;
            }

            if (_textureCache.TryGetValue(textureName, out texture))
                return true;

            var textureFile =
                _archive.Entries.FirstOrDefault(e => e.FullName.Equals("assets/minecraft/textures/" + textureName + ".png"));
            if (textureFile == null) return false;

            using (var s = textureFile.Open())
            {
                var img = new Bitmap(s);

                _textureCache[textureName] = img;

                texture = img;
                return true;
            }
        }

        private void ProcessBlockModel(BlockModel model)
        {
            if (model.ParentName != null)
            {
                BlockModel parent;
                if (TryGetBlockModel(model.ParentName, out parent))
                {
                    model.Parent = parent;

                    if (model.Elements.Length == 0 && parent.Elements.Length > 0)
                    {
                        model.Elements = MCJsonConvert.DeserializeObject<BlockModelElement[]>(MCJsonConvert.SerializeObject(parent.Elements));
                    }

                    foreach (var kvp in parent.TextureDefinitions)
                    {
                        if (!model.TextureDefinitions.ContainsKey(kvp.Key))
                        {
                            model.TextureDefinitions.Add(kvp.Key, kvp.Value);
                        }
                    }

                   /* foreach (var kvp in parent.Textures)
                    {
                        if (!model.Textures.ContainsKey(kvp.Key))
                        {
                            model.Textures.Add(kvp.Key, kvp.Value);
                        }
                    }*/
                }
            }


            foreach (var textureDef in model.TextureDefinitions.OrderBy(e => e.Value.StartsWith("#")).ToArray())
            {
	            if (TryGetTexture(model, textureDef.Value, out _))
	            {

	            }
            }
        }

        /*private Bitmap GetBitmapFromBlockModel(BlockModel model, string textureRef)
        {
            if (textureRef.StartsWith("#"))
            {
                textureRef = textureRef.TrimStart('#');

                Bitmap texture;
                if (!model.Textures.TryGetValue(textureRef, out texture))
                {
                    string newRef;
                    if (model.TextureDefinitions.TryGetValue(textureRef, out newRef))
                    {
                        return GetBitmapFromBlockModel(model, newRef);
                    }
                    return null;
                }
                return texture;
            }
            else
            {
                Bitmap texture;
                if (!model.Textures.TryGetValue(textureRef, out texture))
                {
                    return null;
                }

                return texture;
            }
        }*/

        private void ProcessBlockState(BlockState blockState)
        {
            foreach (var variant in blockState.Variants)
            {
                foreach (var sVariant in variant.Value)
                {
                    BlockModel model;
                    if (!TryGetBlockModel("block/" + sVariant.ModelName, out model))
                    {
                        return;
                    }

                    sVariant.Model = model;
                }
            }

        }

        public bool TryGetBlockState(int blockId, byte blockMeta, out BlockStateVariant blockStateVariant)
        {
            if (TryGetBlockStateName(GetBlockStateId(blockId, blockMeta), out var blockStateName))
            {
	            blockStateName = blockStateName.Replace("minecraft:", "");
	            var split = blockStateName.Split('[', ']');

				//Debug.WriteLine("Val: " + blockStateName + " | Split: " + split.Length);
				if (split.Length > 0)
				{
					//var match = Regex.Match(blockStateName, @"^(?'state'.+)\[(?'variant'.+)\]$");
					string d = string.Empty;
					string name = split[0];
					if (split.Length > 1)
					{
						d = split[1];
						name = FixBlockStateNaming(name, ParseData(split[1]));
					}

					if (TryGetBlockState(name, out var blockState))
		            {
			            if (split.Length > 1)
			            {
				            if (blockState.Variants.TryGetValue(d, out blockStateVariant))
				            {
					            return true;
				            }
			            }

			            if (blockState.Variants.TryGetValue("normal", out blockStateVariant))
						{
							return true;
						}

			            if (blockState.Variants.Any())
			            {
				            blockStateVariant = blockState.Variants.Values.FirstOrDefault();
				            return true;
						}
					}
	            }
            }

	        blockStateVariant = null;
            return false;
        }

	    private static Dictionary<string, string> ParseData(string variant)
	    {
		    Dictionary<string, string> values = new Dictionary<string, string>();

		    string[] splitVariants = variant.Split(',');
		    foreach (var split in splitVariants)
		    {
			    string[] splitted = split.Split('=');
			    if (splitted.Length <= 1)
			    {
				    continue;
			    }

			    string key = splitted[0];
			    string value = splitted[1];

			    values.Add(key, value);
		    }

		    return values;
	    }

		private static string FixBlockStateNaming(string name, Dictionary<string, string> data)
		{
			string color = null;
			data.TryGetValue("color", out color);

			string variant = null;
			data.TryGetValue("variant", out variant);

			string type = null;
			data.TryGetValue("type", out type);
			int level = 8;
			if (data.TryGetValue("level", out string lvl))
			{
				int.TryParse(lvl, out level);
			}

			//string half = null;
			//data.TryGetValue("half", out half);

			if (name.Contains("wooden_slab") && !string.IsNullOrWhiteSpace(variant))
			{
				if (!string.IsNullOrWhiteSpace(variant))
				{
					name = $"{variant}_slab";
				}
			}
			else if (name.Contains("leaves") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_leaves";
			}
			else if (name.Contains("log") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_log";
			}
			else if (name.StartsWith("red_flower") && !string.IsNullOrWhiteSpace(type))
			{
				name = $"{type}";
			}
			else if (name.StartsWith("yellow_flower") && !string.IsNullOrWhiteSpace(type))
			{
				name = $"{type}";
			}
			else if (name.StartsWith("sapling") && !string.IsNullOrWhiteSpace(type))
			{
				name = $"{type}_sapling";
			}
			else if (name.StartsWith("planks") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_planks";
			}
			else if (name.StartsWith("double_stone_slab") && !string.IsNullOrWhiteSpace(variant))
			{
				name = $"{variant}_double_slab";
			}
			else if (name.StartsWith("double_plant") && !string.IsNullOrWhiteSpace(variant))
			{
				if (variant.Equals("sunflower", StringComparison.InvariantCultureIgnoreCase))
				{
					name = "sunflower";
				}
				else if (variant.Equals("paeonia", StringComparison.InvariantCultureIgnoreCase))
				{
					name = "paeonia";
				}
				else if (variant.Equals("syringa", StringComparison.InvariantCultureIgnoreCase))
				{
					name = "syringa";
				}
				else
				{
					name = $"double_{variant}";
				}
			}
			else if (name.StartsWith("deadbush"))
			{
				name = "dead_bush";
			}
			else if (name.StartsWith("tallgrass"))
			{
				name = "tall_grass";
			}
			else if (!string.IsNullOrWhiteSpace(color))
			{
				name = $"{color}_{name}";
			}

			/*if (name.Equals("water", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = StationairyWaterModel;
					w.Level = level;
					return w;
				};
			}
			else if (name.Equals("flowing_water", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = FlowingWaterModel;
					w.Level = level;
					return w;
				};
			}
			if (name.Equals("lava", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = StationairyLavaModel;
					w.Level = level;
					return w;
				};
			}
			else if (name.Equals("flowing_lava", StringComparison.InvariantCultureIgnoreCase))
			{
				return manager =>
				{
					var w = FlowingLavaModel;
					w.Level = level;
					return w;
				};
			}*/

			return name;
		}


		public bool TryGetBlockState(string blockStateId, out BlockState blockState)
        {
            return _blockStates.TryGetValue(blockStateId.ToLowerInvariant(), out blockState);
        }

        public bool TryGetBlockStateName(uint blockStateId, out string blockStateName)
        {
            return _blockStateIds.TryGetValue(blockStateId, out blockStateName);
        }
        
        public static uint GetBlockStateId(int id, byte meta)
        {
            if (id < 0) throw new ArgumentOutOfRangeException();

            return (uint)(id << 4 | meta);
        }

        public void Dispose()
        {
            _archive?.Dispose();
        }
    }
}
