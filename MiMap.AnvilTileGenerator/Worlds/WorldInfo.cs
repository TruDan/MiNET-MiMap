using fNbt;
using MiNET.Worlds;

namespace MiMap.AnvilTileGenerator.Worlds
{
	public class WorldInfo : LevelInfo
	{
        public WorldInfoGameRules GameRules { get; private set; } = new WorldInfoGameRules();

	    public WorldInfo()
	    {
	        
	    }

	    public WorldInfo(NbtTag dataTag) : base()
	    {
            LoadFromNbtExtended(dataTag);
	    }
        /*

		public T SetValue<T>(NbtTag tag, Expression<Func<T>> property, bool upperFirst = true)
		{
			var propertyInfo = ((MemberExpression) property.Body).Member as PropertyInfo;
			if (propertyInfo == null)
			{
				throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
			}

			NbtTag nbtTag = tag[propertyInfo.Name];
			if (nbtTag == null)
			{
				nbtTag = tag[LowercaseFirst(propertyInfo.Name)];
			}

			if (nbtTag == null)
			{
				return default(T);
			}

			var mex = property.Body as MemberExpression;
			object target = Expression.Lambda(mex.Expression).Compile().DynamicInvoke();

			switch (nbtTag.TagType)
			{
				case NbtTagType.Unknown:
					break;
				case NbtTagType.End:
					break;
				case NbtTagType.Byte:
					if (propertyInfo.PropertyType == typeof (bool))
					{
						tag[nbtTag.Name] = new NbtByte(nbtTag.Name, (byte) ((bool) propertyInfo.GetValue(target) ? 1 : 0));
					}
					else
					{
						tag[nbtTag.Name] = new NbtByte(nbtTag.Name, (byte) propertyInfo.GetValue(target));
					}
					break;
				case NbtTagType.Short:
					tag[nbtTag.Name] = new NbtShort(nbtTag.Name, (short) propertyInfo.GetValue(target));
					break;
				case NbtTagType.Int:
					if (propertyInfo.PropertyType == typeof (bool))
					{
						tag[nbtTag.Name] = new NbtInt(nbtTag.Name, (bool) propertyInfo.GetValue(target) ? 1 : 0);
					}
					else
					{
						tag[nbtTag.Name] = new NbtInt(nbtTag.Name, (int) propertyInfo.GetValue(target));
					}
					break;
				case NbtTagType.Long:
					tag[nbtTag.Name] = new NbtLong(nbtTag.Name, (long) propertyInfo.GetValue(target));
					break;
				case NbtTagType.Float:
					tag[nbtTag.Name] = new NbtFloat(nbtTag.Name, (float) propertyInfo.GetValue(target));
					break;
				case NbtTagType.Double:
					tag[nbtTag.Name] = new NbtDouble(nbtTag.Name, (double) propertyInfo.GetValue(target));
					break;
				case NbtTagType.ByteArray:
					tag[nbtTag.Name] = new NbtByteArray(nbtTag.Name, (byte[]) propertyInfo.GetValue(target));
					break;
				case NbtTagType.String:
					tag[nbtTag.Name] = new NbtString(nbtTag.Name, (string) propertyInfo.GetValue(target));
					break;
				case NbtTagType.List:
					break;
				case NbtTagType.Compound:
					break;
				case NbtTagType.IntArray:
					tag[nbtTag.Name] = new NbtIntArray(nbtTag.Name, (int[]) propertyInfo.GetValue(target));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return (T) propertyInfo.GetValue(target);
		}


		private static string LowercaseFirst(string s)
		{
			// Check for empty string.
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			// Return char and concat substring.
			return char.ToLower(s[0]) + s.Substring(1);
		}*/

	    public void LoadFromNbtExtended(NbtTag dataTag)
	    {
	        LoadFromNbt(dataTag);

	        GameRules.LoadGameRules(dataTag);
	    }

		public void SaveToNbtExtended(NbtTag dataTag)
		{
            SaveToNbt(dataTag);

            GameRules.SaveGameRules(dataTag);
        }
	}

    public class WorldInfoGameRules : LevelInfo
    {
        private string randomTickSpeed { get; set; } = "3";
        private string commandBlockOutput { get; set; } = "true";
        private string disableElytraMovementCheck { get; set; } = "false";
        private string doDaylightCycle { get; set; } = "true";
        private string doFireTick { get; set; } = "true";
        private string doMobLoot { get; set; } = "true";
        private string doMobSpawning { get; set; } = "true";
        private string doTileDrops { get; set; } = "true";
        private string keepInventory { get; set; } = "false";
        private string logAdminCommands { get; set; } = "true";
        private string mobGriefing { get; set; } = "true";
        private string naturalRegeneration { get; set; } = "true";
        private string sendCommandFeedback { get; set; } = "true";
        private string showDeathMessages { get; set; } = "true";

        public bool CommandBlockOutput
        {
            get { return commandBlockOutput == "true"; }
            set { commandBlockOutput = value.ToString(); }
        }

        public bool DisableElytraMovementCheck
        {
            get { return disableElytraMovementCheck == "true"; }
            set { disableElytraMovementCheck = value.ToString(); }
        }

        public bool DoDaylightCycle
        {
            get { return doDaylightCycle == "true"; }
            set { doDaylightCycle = value.ToString(); }
        }

        public bool DoFireTick
        {
            get { return doFireTick == "true"; }
            set { doFireTick = value.ToString(); }
        }

        public bool DoMobLoot
        {
            get { return doMobLoot == "true"; }
            set { doMobLoot = value.ToString(); }
        }

        public bool DoMobSpawning
        {
            get { return doMobSpawning == "true"; }
            set { doMobSpawning = value.ToString(); }
        }

        public bool DoTileDrops
        {
            get { return doTileDrops == "true"; }
            set { doTileDrops = value.ToString(); }
        }

        public bool KeepInventory
        {
            get { return keepInventory == "true"; }
            set { keepInventory = value.ToString(); }
        }

        public bool LogAdminCommands
        {
            get { return logAdminCommands == "true"; }
            set { logAdminCommands = value.ToString(); }
        }

        public bool MobGriefing
        {
            get { return mobGriefing == "true"; }
            set { mobGriefing = value.ToString(); }
        }

        public bool NaturalRegeneration
        {
            get { return naturalRegeneration == "true"; }
            set { naturalRegeneration = value.ToString(); }
        }

        public int RandomTickSpeed
        {
            get { return int.Parse(randomTickSpeed); }
            set { randomTickSpeed = value.ToString(); }
        }

        public bool SendCommandFeedback
        {
            get { return sendCommandFeedback == "true"; }
            set { sendCommandFeedback = value.ToString(); }
        }

        public bool ShowDeathMessages
        {
            get { return showDeathMessages == "true"; }
            set { showDeathMessages = value.ToString(); }
        }

        internal void LoadGameRules(NbtTag dataTag)
        {
            NbtTag gamerulesNbtTag = dataTag["GameRules"];
            if (gamerulesNbtTag == null)
            {
                return;
            }

            GetPropertyValue(gamerulesNbtTag, () => commandBlockOutput);
            GetPropertyValue(gamerulesNbtTag, () => disableElytraMovementCheck);
            GetPropertyValue(gamerulesNbtTag, () => doDaylightCycle);
            GetPropertyValue(gamerulesNbtTag, () => doFireTick);
            GetPropertyValue(gamerulesNbtTag, () => doMobLoot);
            GetPropertyValue(gamerulesNbtTag, () => doMobSpawning);
            GetPropertyValue(gamerulesNbtTag, () => doTileDrops);
            GetPropertyValue(gamerulesNbtTag, () => keepInventory);
            GetPropertyValue(gamerulesNbtTag, () => logAdminCommands);
            GetPropertyValue(gamerulesNbtTag, () => mobGriefing);
            GetPropertyValue(gamerulesNbtTag, () => randomTickSpeed);
            GetPropertyValue(gamerulesNbtTag, () => sendCommandFeedback);
            GetPropertyValue(gamerulesNbtTag, () => showDeathMessages);

        }

        internal void SaveGameRules(NbtTag dataTag)
        {
            NbtTag gamerulesNbtTag = dataTag["GameRules"];
            if (gamerulesNbtTag == null)
            {
                gamerulesNbtTag = new NbtCompound("GameRules");
                dataTag["GameRules"] = gamerulesNbtTag;
            }

            SetPropertyValue(gamerulesNbtTag, () => commandBlockOutput);
            SetPropertyValue(gamerulesNbtTag, () => disableElytraMovementCheck);
            SetPropertyValue(gamerulesNbtTag, () => doDaylightCycle);
            SetPropertyValue(gamerulesNbtTag, () => doFireTick);
            SetPropertyValue(gamerulesNbtTag, () => doMobLoot);
            SetPropertyValue(gamerulesNbtTag, () => doMobSpawning);
            SetPropertyValue(gamerulesNbtTag, () => doTileDrops);
            SetPropertyValue(gamerulesNbtTag, () => keepInventory);
            SetPropertyValue(gamerulesNbtTag, () => logAdminCommands);
            SetPropertyValue(gamerulesNbtTag, () => mobGriefing);
            SetPropertyValue(gamerulesNbtTag, () => randomTickSpeed);
            SetPropertyValue(gamerulesNbtTag, () => sendCommandFeedback);
            SetPropertyValue(gamerulesNbtTag, () => showDeathMessages);

        }
    }
}
