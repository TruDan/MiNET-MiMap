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

        private bool TryGetTexture(BlockModel model, string textureName, out Bitmap texture)
        {
            if (textureName.StartsWith("#"))
            {
                if (model.TextureDefinitions.TryGetValue(textureName.TrimStart('#'), out textureName))
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

                    foreach (var kvp in parent.Textures)
                    {
                        if (!model.Textures.ContainsKey(kvp.Key))
                        {
                            model.Textures.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }


            foreach (var textureDef in model.TextureDefinitions.OrderBy(e => e.Value.StartsWith("#")).ToArray())
            {
                //Debug.WriteLine("Lookup Texture {2}: {0} {1}", textureDef.Key, textureDef.Value, model.Name);
                if (!model.Textures.ContainsKey(textureDef.Key))
                {
                    Bitmap texture;
                    if (TryGetTexture(model, textureDef.Value, out texture))
                    {
                        model.Textures[textureDef.Key] = texture;
                    }
                }
            }


            foreach (var element in model.Elements)
            {
                foreach (var face in element.Faces)
                {
                    face.Value.Texture = GetBitmapFromBlockModel(model, face.Value.TextureName);
                    if (face.Value.Texture == null)
                    {
                        //Debug.WriteLine("Failed to find texture for face {0} on block {1}", face.Key, model.Name);
                    }
                }
            }
        }

        private Bitmap GetBitmapFromBlockModel(BlockModel model, string textureRef)
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
        }

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
	            if (blockStateName.Contains("["))
	            {
		            var match = Regex.Match(blockStateName, @"^(?'state'.+)\[(?'variant'.+)\]$");
					
		            if (TryGetBlockState(match.Groups["state"].ToString(), out var blockState))
		            {
			            if (blockState.Variants.TryGetValue(match.Groups["variant"].ToString(), out blockStateVariant))
			            {
				            return true;
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
