﻿using Assets.SiliconSocial;
using FF9;
using Memoria;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Prime;
using Memoria.Prime.CSV;
using Memoria.Prime.Text;
using System;
using System.Collections.Generic;
using System.IO;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable RedundantExplicitArraySize

public class ff9item
{
    // Each item (regular/important/card) has 2 IDs:
    // - Regular items have a RegularItem (any number) and an integer ID (such that 0 <= (ID % 1000) < 256)
    // - Important items have an integer ID (any number) and a second integer ID (such that 256 <= (ID % 1000) < 512)
    // - Cards have an integer ID (any number) and a second integer ID (such that 512 <= (ID % 1000) < 612)
    // Thus that second ID can represent any kind of item; it is used when something can refer to either one (eg. event scripts or NCalc formulas)
    // while the first ID represent an item for situations in which there is no ambiguity on the type of item (eg. an equipment piece can only be a regular item)

    //public const int FF9ITEM_MAX = 256;
    //public const int FF9ITEM_RARE_MAX = 256;
    //public const int FF9ITEM_RARE_SIZE = 64;
    //public const int FF9ITEM_RARE_BIT = 2;
    //public const int FF9ITEM_ABILITY_MAX = 3;
    //public const int FF9ITEM_NONE = 255;
    //public const int FF9ITEM_INFO_START = 224;
    //public const int FF9ITEM_NAME_SIZE = 2048;
    //public const int FF9ITEM_HELP_SIZE = 10240;
    //public const int FF9ITEM_IMP_NAME_SIZE = 3072;

    public const Int32 FF9ITEM_COUNT_MAX = 99;
    public const Int32 EFFECT_START = 224;
    public const Int32 EFFECT_COUNT = 32;

    public static Dictionary<RegularItem, FF9ITEM_DATA> _FF9Item_Data;
    public static Dictionary<Int32, ITEM_DATA> _FF9Item_Info;

    static ff9item()
    {
        _FF9Item_Data = LoadItems();
        _FF9Item_Info = LoadItemEffects();
        PatchItemEquip();
        PatchItemAbility();
    }

    public static void FF9Item_Init()
    {
        FF9Item_InitNormal();
        FF9Item_InitImportant();
        LoadInitialItems();
    }

    private static Dictionary<RegularItem, FF9ITEM_DATA> LoadItems()
    {
        try
        {
            String inputPath = DataResources.Items.PureDirectory + DataResources.Items.ItemsFile;
            Dictionary<RegularItem, FF9ITEM_DATA> result = new Dictionary<RegularItem, FF9ITEM_DATA>();
            foreach (ItemInfo[] infos in AssetManager.EnumerateCsvFromLowToHigh<ItemInfo>(inputPath))
            {
                for (Int32 i = 0; i < infos.Length; i++)
                {
                    if (infos[i].Id < 0)
                    {
                        infos[i].Id = (RegularItem)i;
                        infos[i].WeaponId = i < ff9weap.WEAPON_COUNT ? i : -1;
                        infos[i].ArmorId = i >= ff9armor.ARMOR_START && i < ff9armor.ARMOR_START + ff9armor.ARMOR_COUNT ? i - ff9armor.ARMOR_START : -1;
                        infos[i].EffectId = i >= EFFECT_START && i < EFFECT_START + EFFECT_COUNT ? i - EFFECT_START : -1;
                    }
                }
                foreach (ItemInfo info in infos)
                    result[info.Id] = info.ToItemData();
            }
            if (result.Count == 0)
                throw new FileNotFoundException($"Cannot load items because a file does not exist: [{DataResources.Items.Directory + DataResources.Items.ItemsFile}].", DataResources.Items.Directory + DataResources.Items.ItemsFile);
            for (Int32 i = 0; i < 256; i++)
                if (!result.ContainsKey((RegularItem)i))
                    throw new NotSupportedException($"You must define at least the 256 base items, with IDs between 0 and 255.");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ff9item] Load items failed.");
            UIManager.Input.ConfirmQuit();
            return null;
        }
    }

    private static void PatchItemEquip()
    {
        try
        {
            String inputPath = DataResources.Items.PureDirectory + DataResources.Items.ItemEquipPatchFile;
            foreach (AssetManager.AssetFolder folder in AssetManager.FolderLowToHigh)
                if (folder.TryFindAssetInModOnDisc(inputPath, out String fullPath, AssetManagerUtil.GetStreamingAssetsPath() + "/"))
                    ApplyItemEquipPatchFile(File.ReadAllLines(fullPath));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ff9item] Patch item equipability failed.");
        }
    }

    private static void PatchItemAbility()
    {
        try
        {
            String inputPath = DataResources.Items.PureDirectory + DataResources.Items.ItemAbilityPatchFile;
            foreach (AssetManager.AssetFolder folder in AssetManager.FolderLowToHigh)
                if (folder.TryFindAssetInModOnDisc(inputPath, out String fullPath, AssetManagerUtil.GetStreamingAssetsPath() + "/"))
                    ApplyItemAbilityPatchFile(File.ReadAllLines(fullPath));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ff9item] Patch item equipability failed.");
        }
    }

    private static Dictionary<Int32, ITEM_DATA> LoadItemEffects()
    {
        try
        {
            String inputPath = DataResources.Items.PureDirectory + DataResources.Items.ItemEffectsFile;
            Dictionary<Int32, ITEM_DATA> result = new Dictionary<Int32, ITEM_DATA>();
            foreach (ItemEffect[] effects in AssetManager.EnumerateCsvFromLowToHigh<ItemEffect>(inputPath))
            {
                for (Int32 i = 0; i < effects.Length; i++)
                    if (effects[i].Id < 0)
                        effects[i].Id = i;
                foreach (ItemEffect effect in effects)
                    result[effect.Id] = effect.ToItemData();
            }
            if (result.Count == 0)
                throw new FileNotFoundException($"Cannot load item effects because a file does not exist: [{DataResources.Items.Directory + DataResources.Items.ItemEffectsFile}].", DataResources.Items.Directory + DataResources.Items.ItemEffectsFile);
            for (Int32 i = 0; i < EFFECT_COUNT; i++)
                if (!result.ContainsKey(i))
                    throw new NotSupportedException($"You must define at least the 32 base item effects, with IDs between 0 and 31.");
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ff9item] Load item effects failed.");
            UIManager.Input.ConfirmQuit();
            return null;
        }
    }

    private static void LoadInitialItems()
    {
        try
        {
            String inputPath = DataResources.Items.PureDirectory + DataResources.Items.InitialItemsFile;
            FF9ITEM[] items = AssetManager.GetCsvWithHighestPriority<FF9ITEM>(inputPath);
            if (items == null)
                return;
            foreach (FF9ITEM item in items)
                FF9Item_Add(item.id, item.count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ff9item] Load initial items failed.");
        }
    }

    public static void FF9Item_InitNormal()
    {
        FF9StateSystem.Common.FF9.item.Clear();
    }

    public static void FF9Item_InitImportant()
    {
        FF9StateSystem.Common.FF9.rare_item_obtained.Clear();
        FF9StateSystem.Common.FF9.rare_item_used.Clear();
    }

    public static Int32 FF9Item_Add_Generic(Int32 id, Int32 count)
    {
        if (IsItemRegular(id))
            return FF9Item_Add(GetRegularIdFromItemId(id), count);
        if (IsItemImportant(id))
        {
            if (count <= 0)
                return 0;
            FF9Item_AddImportant(GetImportantIdFromItemId(id));
            return 1;
        }
        if (IsItemCard(id))
        {
            Int32 countAdded = 0;
            while (count-- > 0)
                countAdded += QuadMistDatabase.MiniGame_SetCard(GetCardIdFromItemId(id));
            return countAdded;
        }
        return 0;
    }

    public static Int32 FF9Item_Remove_Generic(Int32 id, Int32 count)
    {
        if (IsItemRegular(id))
            return FF9Item_Remove(GetRegularIdFromItemId(id), count);
        if (IsItemImportant(id))
        {
            if (count > 0 && FF9Item_IsExistImportant(GetImportantIdFromItemId(id)))
            {
                FF9Item_RemoveImportant(GetImportantIdFromItemId(id));
                return 1;
            }
            return 0;
        }
        if (IsItemCard(id))
            return QuadMistDatabase.Remove(GetCardIdFromItemId(id), count);
        return 0;
    }

    public static Int32 FF9Item_GetCount_Generic(Int32 id)
    {
        if (IsItemRegular(id))
            return FF9Item_GetCount(GetRegularIdFromItemId(id));
        if (IsItemImportant(id))
            return FF9Item_IsExistImportant(GetImportantIdFromItemId(id)) ? 1 : 0;
        if (IsItemCard(id))
            return QuadMistDatabase.MiniGame_GetCardCount(GetCardIdFromItemId(id));
        return 0;
    }

    public static FF9ITEM FF9Item_GetPtr(RegularItem id)
    {
        return FF9StateSystem.Common.FF9.item.Find(item => item.count > 0 && item.id == id);
    }

    public static Int32 FF9Item_GetCount(RegularItem id)
    {
        FF9ITEM ptr = FF9Item_GetPtr(id);
        if (ptr == null)
            return 0;
        return ptr.count;
    }

    public static Int32 FF9Item_Add(RegularItem id, Int32 count)
    {
        if (id == RegularItem.NoItem)
            return 0;
        count = Math.Min(FF9ITEM_COUNT_MAX, count);
        FF9ITEM existingItem = FF9Item_GetPtr(id);
        if (existingItem != null)
        {
            if (existingItem.count + count > FF9ITEM_COUNT_MAX)
                count = FF9ITEM_COUNT_MAX - existingItem.count;
            existingItem.count += (Byte)count;
            FF9Item_Achievement(existingItem.id, existingItem.count);
            return count;
        }
        FF9ITEM emptyItem = FF9StateSystem.Common.FF9.item.Find(item => item.count <= 0);
        if (emptyItem != null)
        {
            emptyItem.id = id;
            emptyItem.count = (Byte)count;
            FF9Item_Achievement(id, count);
            return count;
        }
        FF9StateSystem.Common.FF9.item.Add(new FF9ITEM(id, (Byte)count));
        FF9Item_Achievement(id, count);
        return count;
    }

    public static Int32 FF9Item_Remove(RegularItem id, Int32 count)
    {
        FF9ITEM existingItem = FF9Item_GetPtr(id);
        if (existingItem != null)
        {
            if (existingItem.count < count)
                count = existingItem.count;
            existingItem.count -= (Byte)count;
            return count;
        }
        return 0;
    }

    public static Int32 FF9Item_GetEquipPart(RegularItem id)
    {
        FF9ITEM_DATA itemData = _FF9Item_Data[id];
        ItemType[] partMask = new ItemType[]
        {
            ItemType.Weapon,
            ItemType.Helmet,
            ItemType.Armlet,
            ItemType.Armor,
            ItemType.Accessory
        };
        for (Int32 i = 0; i < 5; ++i)
            if ((itemData.type & partMask[i]) != 0)
                return i;
        return -1;
    }

    public static Int32 FF9Item_GetEquipCount(RegularItem id)
    {
        Int32 count = 0;
        foreach (PLAYER p in FF9StateSystem.Common.FF9.PlayerList)
            if (p.info.party != 0)
                for (Int32 i = 0; i < 5; ++i)
                    if (id == p.equip[i])
                        ++count;
        return count;
    }

    public static Int32 FF9Item_GetAnyCount(RegularItem id)
    {
        return FF9Item_GetCount(id) + FF9Item_GetEquipCount(id);
    }

    public static void FF9Item_AddImportant(Int32 id)
    {
        FF9StateSystem.Common.FF9.rare_item_obtained.Add(id);
        FF9StateSystem.Common.FF9.rare_item_used.Remove(id);
    }

    public static void FF9Item_RemoveImportant(Int32 id)
    {
        FF9StateSystem.Common.FF9.rare_item_obtained.Remove(id);
        FF9StateSystem.Common.FF9.rare_item_used.Remove(id);
    }

    public static void FF9Item_UseImportant(Int32 id)
    {
        FF9StateSystem.Common.FF9.rare_item_used.Add(id);
    }

    public static void FF9Item_UnuseImportant(Int32 id)
    {
        FF9StateSystem.Common.FF9.rare_item_used.Remove(id);
    }

    public static Boolean FF9Item_IsExistImportant(Int32 id)
    {
        return FF9StateSystem.Common.FF9.rare_item_obtained.Contains(id);
    }

    public static Boolean FF9Item_IsUsedImportant(Int32 id)
    {
        return FF9StateSystem.Common.FF9.rare_item_used.Contains(id);
    }

    private static void FF9Item_Achievement(RegularItem id, Int32 count)
    {
        switch (id)
        {
            case RegularItem.Excalibur:
                AchievementManager.ReportAchievement(AcheivementKey.Excalibur, count);
                break;
            case RegularItem.ExcaliburII:
                AchievementManager.ReportAchievement(AcheivementKey.ExcaliburII, count);
                break;
            case RegularItem.TheTower:
                AchievementManager.ReportAchievement(AcheivementKey.TheTower, count);
                break;
            case RegularItem.UltimaWeapon:
                AchievementManager.ReportAchievement(AcheivementKey.UltimaWeapon, count);
                break;
            case RegularItem.Hammer:
                AchievementManager.ReportAchievement(AcheivementKey.Hammer, count);
                break;
            case RegularItem.KainLance:
                AchievementManager.ReportAchievement(AcheivementKey.KainLance, count);
                break;
            case RegularItem.RuneClaws:
                AchievementManager.ReportAchievement(AcheivementKey.RuneClaws, count);
                break;
            case RegularItem.TigerRacket:
                AchievementManager.ReportAchievement(AcheivementKey.TigerHands, count);
                break;
            case RegularItem.WhaleWhisker:
                AchievementManager.ReportAchievement(AcheivementKey.WhaleWhisker, count);
                break;
            case RegularItem.AngelFlute:
                AchievementManager.ReportAchievement(AcheivementKey.AngelFlute, count);
                break;
            case RegularItem.MaceOfZeus:
                AchievementManager.ReportAchievement(AcheivementKey.MaceOfZeus, count);
                break;
            case RegularItem.GastroFork:
                AchievementManager.ReportAchievement(AcheivementKey.GastroFork, count);
                break;
            case RegularItem.Moonstone:
                AchievementManager.ReportAchievement(AcheivementKey.Moonstone4, IncreaseMoonStoneCount());
                break;
            case RegularItem.GenjiGloves:
            case RegularItem.GenjiHelmet:
            case RegularItem.GenjiArmor:
                if (FF9Item_GetAnyCount(RegularItem.GenjiGloves) <= 0 || FF9Item_GetAnyCount(RegularItem.GenjiHelmet) <= 0 || FF9Item_GetAnyCount(RegularItem.GenjiArmor) <= 0)
                    break;
                count = FF9Item_GetAnyCount(RegularItem.GenjiGloves) + FF9Item_GetAnyCount(RegularItem.GenjiHelmet) + FF9Item_GetAnyCount(RegularItem.GenjiArmor);
                AchievementManager.ReportAchievement(AcheivementKey.GenjiSet, count);
                break;
        }
    }

    public static Int32 IncreaseMoonStoneCount()
    {
        return ++FF9StateSystem.Achievement.EvtReservedArray[0];
    }

    public static Int32 DecreaseMoonStoneCount()
    {
        return --FF9StateSystem.Achievement.EvtReservedArray[0];
    }

    public static Boolean IsItemRegular(Int32 itemId)
    {
        return GetItemModuledId(itemId) < 256;
    }

    public static Boolean IsItemImportant(Int32 itemId)
    {
        Int32 modId = GetItemModuledId(itemId);
        return modId >= 256 && modId < 512;
    }

    public static Boolean IsItemCard(Int32 itemId)
    {
        Int32 modId = GetItemModuledId(itemId);
        return modId >= 512 && modId < 612;
    }

    public static RegularItem GetRegularIdFromItemId(Int32 itemId)
    {
        if (!IsItemRegular(itemId))
            return RegularItem.NoItem;
        Int32 poolNum = itemId / 1000;
        Int32 idInPool = itemId % 1000;
        return (RegularItem)(poolNum * 256 + idInPool);
    }

    public static Int32 GetItemIdFromRegularId(RegularItem itemId)
    {
        Int32 poolNum = (Int32)itemId / 256;
        Int32 idInPool = (Int32)itemId % 256;
        return poolNum * 1000 + idInPool;
    }

    public static Int32 GetImportantIdFromItemId(Int32 itemId)
    {
        if (!IsItemImportant(itemId))
            return -1;
        Int32 poolNum = itemId / 1000;
        Int32 idInPool = itemId % 1000 - 256;
        return poolNum * 256 + idInPool;
    }

    public static Int32 GetItemIdFromImportantId(Int32 importantId)
    {
        Int32 poolNum = importantId / 256;
        Int32 idInPool = importantId % 256;
        return poolNum * 1000 + idInPool + 256;
    }

    public static TetraMasterCardId GetCardIdFromItemId(Int32 itemId)
    {
        if (!IsItemCard(itemId))
            return TetraMasterCardId.NONE;
        Int32 poolNum = itemId / 1000;
        Int32 idInPool = itemId % 1000 - 512;
        return (TetraMasterCardId)(poolNum * 100 + idInPool);
    }

    public static Int32 GetItemIdFromCardId(TetraMasterCardId cardId)
    {
        Int32 poolNum = (Int32)cardId / 100;
        Int32 idInPool = (Int32)cardId % 100;
        return poolNum * 1000 + idInPool + 512;
    }

    public static Boolean HasItemWeapon(RegularItem itemId)
    {
        return _FF9Item_Data[itemId].weapon_id >= 0;
    }

    public static Boolean HasItemArmor(RegularItem itemId)
    {
        return _FF9Item_Data[itemId].armor_id >= 0;
    }

    public static Boolean HasItemEffect(RegularItem itemId)
    {
        return _FF9Item_Data[itemId].effect_id >= 0;
    }

    public static Boolean CanThrowItem(RegularItem itemId)
    {
        return ff9item.CanThrowItem(_FF9Item_Data[itemId]);
    }
    public static Boolean CanThrowItem(FF9ITEM_DATA itemData)
    {
        if (!ff9weap.WeaponData.TryGetValue(itemData.weapon_id, out ItemAttack weapData))
            return false;
        return (weapData.Category & WeaponCategory.Throw) != 0;
    }

    public static ItemAttack GetItemWeapon(RegularItem itemId)
    {
        FF9ITEM_DATA item = _FF9Item_Data[itemId];
        if (item.weapon_id < 0 || !ff9weap.WeaponData.TryGetValue(item.weapon_id, out ItemAttack result))
        {
            Log.Error($"[ff9item] Trying to use the weapon data of {itemId} which has no valid weapon data");
            return ff9weap.WeaponData[0];
        }
        return result;
    }

    public static ItemDefence GetItemArmor(RegularItem itemId)
    {
        FF9ITEM_DATA item = _FF9Item_Data[itemId];
        if (item.armor_id < 0 || !ff9armor.ArmorData.TryGetValue(item.armor_id, out ItemDefence result))
        {
            Log.Error($"[ff9item] Trying to use the armor data of {itemId} which has no valid armor data");
            return ff9armor.ArmorData[0];
        }
        return result;
    }

    public static ITEM_DATA GetItemEffect(RegularItem itemId)
    {
        FF9ITEM_DATA item = _FF9Item_Data[itemId];
        if (item.effect_id < 0 || !_FF9Item_Info.TryGetValue(item.effect_id, out ITEM_DATA result))
        {
            Log.Error($"[ff9item] Trying to use the effect data of {itemId} which has no valid effect data");
            return _FF9Item_Info[0];
        }
        return result;
    }

    private static Int32 GetItemModuledId(Int32 itemId)
    {
        return itemId % 1000;
    }

    public static Object GetItemProperty(RegularItem itemId, String propertyName)
    {
        if (!_FF9Item_Data.TryGetValue(itemId, out FF9ITEM_DATA item))
        {
            Log.Error($"[ff9item] Unrecognized item \"{itemId}\"");
            return -1;
        }
        Boolean hasWeapon = HasItemWeapon(itemId);
        Boolean hasArmor = HasItemArmor(itemId);
        Boolean hasEffect = HasItemEffect(itemId);
        switch (propertyName)
        {
            case "Price": return item.price;
            case "SellingPrice": return item.selling_price;
            case "Shape": return item.shape;
            case "Color": return item.color;
            case "EquipLevel": return item.eq_lv;
            case "SortOrder": return item.sort;
            case "Type": return (Int32)item.type;
            case "WeaponCategory": return hasWeapon ? (Int32)GetItemWeapon(itemId).Category : 0;
            case "WeaponStatus": return hasWeapon ? (UInt64)FF9BattleDB.StatusSets[GetItemWeapon(itemId).StatusIndex].Value : 0;
            case "WeaponModelId": return hasWeapon ? (Int32)GetItemWeapon(itemId).ModelId : -1;
            case "WeaponScriptId": return hasWeapon ? GetItemWeapon(itemId).Ref.ScriptId : 0;
            case "WeaponPower": return hasWeapon ? GetItemWeapon(itemId).Ref.Power : 0;
            case "WeaponElement": return hasWeapon ? (Int32)GetItemWeapon(itemId).Ref.Elements : 0;
            case "WeaponStatusRate": return hasWeapon ? GetItemWeapon(itemId).Ref.Rate : 0;
            case "WeaponOffset1": return hasWeapon ? (Int32)GetItemWeapon(itemId).Offset1 : 0;
            case "WeaponOffset2": return hasWeapon ? (Int32)GetItemWeapon(itemId).Offset2 : 0;
            case "WeaponHitSfx": return hasWeapon ? (Int32)GetItemWeapon(itemId).HitSfx : 0;
            case "ArmorDefence": return hasArmor ? GetItemArmor(itemId).PhysicalDefence : 0;
            case "ArmorEvade": return hasArmor ? GetItemArmor(itemId).PhysicalEvade : 0;
            case "ArmorMagicDefence": return hasArmor ? GetItemArmor(itemId).MagicalDefence : 0;
            case "ArmorMagicEvade": return hasArmor ? GetItemArmor(itemId).MagicalEvade : 0;
            case "EffectTargetType": return hasEffect ? (Int32)GetItemEffect(itemId).info.Target : 0;
            case "EffectDefaultAlly": return hasEffect ? GetItemEffect(itemId).info.DefaultAlly : false;
            case "EffectDisplayStats": return hasEffect ? (Int32)GetItemEffect(itemId).info.DisplayStats : 0;
            case "EffectVfxIndex": return hasEffect ? (Int32)GetItemEffect(itemId).info.VfxIndex : -1;
            case "EffectForDead": return hasEffect ? GetItemEffect(itemId).info.ForDead : false;
            case "EffectDefaultCamera": return hasEffect ? GetItemEffect(itemId).info.DefaultCamera : false;
            case "EffectDefaultOnDead": return hasEffect ? GetItemEffect(itemId).info.DefaultOnDead : false;
            case "EffectScriptId": return hasEffect ? GetItemEffect(itemId).Ref.ScriptId : 0;
            case "EffectPower": return hasEffect ? GetItemEffect(itemId).Ref.Power : 0;
            case "EffectElement": return hasEffect ? (Int32)GetItemEffect(itemId).Ref.Elements : 0;
            case "EffectRate": return hasEffect ? GetItemEffect(itemId).Ref.Rate : 0;
            case "EffectStatus": return hasEffect ? (UInt64)GetItemEffect(itemId).status : 0;
        }
        if (propertyName.StartsWith("Ability ") && Int32.TryParse(propertyName.Substring("Ability ".Length), out Int32 index))
            return index >= 0 && index < item.ability.Length ? item.ability[index] : -1;
        if (propertyName.StartsWith("HasActiveAbility ") && Int32.TryParse(propertyName.Substring("HasActiveAbility ".Length), out Int32 abilId))
            return new List<Int32>(item.ability).Contains(ff9abil.GetAbilityIdFromActiveAbility((BattleAbilityId)abilId));
        if (propertyName.StartsWith("HasSupportAbility ") && Int32.TryParse(propertyName.Substring("HasSupportAbility ".Length), out Int32 supportId))
            return new List<Int32>(item.ability).Contains(ff9abil.GetAbilityIdFromSupportAbility((SupportAbility)supportId));
        Log.Error($"[ff9item] Unrecognized item property \"{propertyName}\"");
        return -1;
    }

    private static void ApplyItemEquipPatchFile(String[] allLines)
    {
        foreach (String line in allLines)
        {
            // eg.: Allow Garnet to equip Dagger and Mage Masher (1 2) but disallows her to equip Tiger Racket (56)
            // Garnet Add 1 2 Remove 56
            if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("//"))
                continue;
            String[] allWords = line.Trim().Split(DataPatchers.SpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (allWords.Length < 3)
                continue;
            if (!Int32.TryParse(allWords[0], out Int32 charId))
            {
                if (!allWords[0].TryEnumParse(out CharacterId charIdAsStr))
                    continue;
                charId = (Int32)charIdAsStr;
            }
            Int32 currentOperation = -1;
            UInt64 charMask = ff9feqp.GetCharacterEquipMaskFromId((CharacterId)charId);
            for (Int32 wordIndex = 1; wordIndex < allWords.Length; wordIndex++)
            {
                String word = allWords[wordIndex].Trim();
                if (String.Equals(word, "Add"))
                    currentOperation = 0;
                else if (String.Equals(word, "Remove"))
                    currentOperation = 1;
                else
                {
                    if (!Int32.TryParse(word, out Int32 itemId) || !_FF9Item_Data.TryGetValue((RegularItem)itemId, out FF9ITEM_DATA item))
                        continue;
                    if (currentOperation == 0)
                        item.equip |= charMask;
                    else if (currentOperation == 1)
                        item.equip &= ~charMask;
                }
            }
        }
    }

    private static void ApplyItemAbilityPatchFile(String[] allLines)
    {
        foreach (String line in allLines)
        {
            // eg.: AA:6 Add 51 57 Remove 68
            // Make it so Full-Life (AA:6) is taught by the Air Racket and Rod (51 57) but not by Siren's Flute (68)
            if (String.IsNullOrEmpty(line) || line.Trim().StartsWith("//"))
                continue;
            String[] allWords = line.Trim().Split(DataPatchers.SpaceSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (allWords.Length < 3)
                continue;
            Int32 abilId = 0;
            try
            {
                abilId = CsvParser.AnyAbility(allWords[0].Trim());
            }
            catch (Exception)
            {
                Log.Warning($"[ff9item] Could not parse '{allWords[0]}' as an abilty ID");
            }
            if (abilId == 0)
                continue;
            Int32 currentOperation = -1;
            for (Int32 wordIndex = 1; wordIndex < allWords.Length; wordIndex++)
            {
                String word = allWords[wordIndex].Trim();
                if (String.Equals(word, "Add"))
                    currentOperation = 0;
                else if (String.Equals(word, "Remove"))
                    currentOperation = 1;
                else
                {
                    if (!Int32.TryParse(word, out Int32 itemId) || !_FF9Item_Data.TryGetValue((RegularItem)itemId, out FF9ITEM_DATA item))
                        continue;
                    List<Int32> abilList = new List<Int32>(item.ability);
                    if (currentOperation == 0)
                        abilList.Add(abilId);
                    else if (currentOperation == 1)
                        abilList.Remove(abilId);
                    item.ability = abilList.ToArray();
                }
            }
        }
    }
}
