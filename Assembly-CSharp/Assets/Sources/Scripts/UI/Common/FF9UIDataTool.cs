﻿using FF9;
using Memoria;
using Memoria.Assets;
using Memoria.Data;
using Memoria.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XInputDotNetPure;

namespace Assets.Sources.Scripts.UI.Common
{
    public static class FF9UIDataTool
    {
        public static void DisplayItem(RegularItem itemId, UISprite itemIcon, UILabel itemName, Boolean isEnable, Boolean displayStock = false)
        {
            if (itemId != RegularItem.NoItem)
            {
                FF9ITEM_DATA item = ff9item._FF9Item_Data[itemId];
                Int32 colorIndex = isEnable ? item.color : 15;
                if (itemIcon != null)
                {
                    itemIcon.spriteName = $"item{item.shape:0#}_{colorIndex:0#}";
                    itemIcon.alpha = isEnable ? 1f : 0.5f;
                }
                if (itemName != null)
                {
                    String itemLabel = FF9TextTool.ItemName(itemId);
                    if (displayStock && Configuration.Interface.SynthIngredientStockDisplayed)
                    {
                        Int32 itemAmount = ff9item.FF9Item_GetCount(itemId);
                        if (itemAmount > 0)
                            itemLabel += $" ({itemAmount})";
                    }
                    itemName.rawText = itemLabel;
                    itemName.color = isEnable ? FF9TextTool.White : FF9TextTool.Gray;
                    itemName.multiLine = true;
                }
            }
            else
            {
                if (itemIcon != null)
                    itemIcon.spriteName = String.Empty;
                if (itemName != null)
                    itemName.rawText = String.Empty;
            }
        }

        public static void DisplayMultipleItems(UILabel label, Dictionary<RegularItem, Int32> items, Func<KeyValuePair<RegularItem, Int32>, Boolean> enabledCheck, Boolean displayStock = false)
        {
            Int32 validItemCount = 0;
            foreach (KeyValuePair<RegularItem, Int32> kvp in items)
            {
                if (kvp.Key == RegularItem.NoItem || kvp.Value <= 0)
                    continue;
                validItemCount++;
            }
            Int32 columnCount = validItemCount > 4 ? 2 : 1;
            Single shrinkFactor = validItemCount <= 2 ? 1f
                : validItemCount <= 4 ? 2f / validItemCount
                : validItemCount <= 8 ? 2f / 4
                : 2f / ((validItemCount + 1) / 2);
            Single spacingY = 10f * shrinkFactor;
            String itemLabel = String.Empty;
            Int32 columnIndex = 0;
            IEnumerable<KeyValuePair<RegularItem, Int32>> enumerator = items;
            if (items.Count == 1 && items.First().Value == 2)
                enumerator = new List<KeyValuePair<RegularItem, Int32>>([new(items.First().Key, 1), new(items.First().Key, 1)]);
            foreach (KeyValuePair<RegularItem, Int32> kvp in enumerator)
            {
                if (kvp.Key == RegularItem.NoItem || kvp.Value <= 0)
                    continue;
                FF9ITEM_DATA item = ff9item._FF9Item_Data[kvp.Key];
                Boolean enabled = enabledCheck(kvp);
                String itemSpriteName = $"item{item.shape:0#}_{(enabled ? item.color : 15):0#}";
                String labelColor = NGUIText.EncodeColor(enabled ? FF9TextTool.White : FF9TextTool.Gray);
                if (itemLabel.Length == 0)
                    itemLabel += $"[YADD={spacingY}]";
                else if (columnIndex > 0)
                    itemLabel += $"[YADD={2f * shrinkFactor}][XTAB={106f * columnIndex / columnCount}]";
                else
                    itemLabel += $"\n[YADD={spacingY}]";
                itemLabel += $"{labelColor}[SPRT={itemSpriteName},{64f * shrinkFactor},{64f * shrinkFactor}]  [FEED=1][YSUB={2f * shrinkFactor}]";
                if (kvp.Value > 1)
                    itemLabel += $"{kvp.Value} × ";
                itemLabel += FF9TextTool.ItemName(kvp.Key);
                if (displayStock && Configuration.Interface.SynthIngredientStockDisplayed)
                {
                    Int32 itemAmount = ff9item.FF9Item_GetCount(kvp.Key);
                    if (itemAmount > 0)
                        itemLabel += $" ({itemAmount})";
                }
                columnIndex++;
                if (columnIndex >= columnCount)
                {
                    columnIndex = 0;
                    spacingY += 10f * shrinkFactor;
                }
            }
            label.fontSize = Mathf.RoundToInt(36 * shrinkFactor);
            label.rawText = itemLabel;
        }

        public static void DisplayCharacterDetail(PLAYER player, CharacterDetailHUD charHud)
        {
            IOverloadPlayerUIScript overloadedMethod = ScriptsLoader.GetOverloadedMethod(typeof(IOverloadPlayerUIScript)) as IOverloadPlayerUIScript;
            if (overloadedMethod != null)
            {
                IOverloadPlayerUIScript.Result ui = overloadedMethod.UpdatePointStatus(player);
                charHud.HPTextColor = ui.ColorHP;
                charHud.MPTextColor = ui.ColorMP;
                if (charHud.MagicStoneLabel != null)
                    charHud.MagicStoneTextColor = ui.ColorMagicStone;
            }
            else
            {
                // Default method
                charHud.HPTextColor = (player.cur.hp == 0) ? FF9TextTool.Red
                                    : (player.cur.hp <= player.max.hp / 6) ? FF9TextTool.Yellow : FF9TextTool.White;
                charHud.MPTextColor = (player.cur.mp <= player.max.mp / 6) ? FF9TextTool.Yellow : FF9TextTool.White;
                if (charHud.MagicStoneLabel != null)
                    charHud.MagicStoneTextColor = (player.cur.capa == 0) ? FF9TextTool.Yellow : FF9TextTool.White;
            }
            charHud.Self.SetActive(true);
            charHud.NameLabel.SetText(player.NameTag);
            charHud.LvLabel.rawText = player.level.ToString();
            charHud.HPLabel.rawText = player.cur.hp.ToString();
            charHud.HPMaxLabel.rawText = player.max.hp.ToString();
            charHud.MPLabel.rawText = player.cur.mp.ToString();
            charHud.MPMaxLabel.rawText = player.max.mp.ToString();
            if (charHud.MagicStoneLabel != null)
            {
                if (player.max.capa == UInt32.MaxValue)
                {
                    charHud.MagicStoneLabel.rawText = "∞";
                    charHud.MagicStoneMaxLabel.rawText = "∞";
                }
                else
                {
                    charHud.MagicStoneLabel.rawText = player.cur.capa.ToString();
                    charHud.MagicStoneMaxLabel.rawText = player.max.capa.ToString();
                }
            }
            if (charHud.StatusesSpriteList != null)
            {
                UISprite[] statusesSpriteList = charHud.StatusesSpriteList;
                foreach (UISprite statusSprite in statusesSpriteList)
                    statusSprite.alpha = 0f;
                foreach (BattleStatusId statusId in player.status.ToStatusList())
                {
                    if (!BattleHUD.DebuffIconNames.TryGetValue(statusId, out String spriteName))
                        continue;
                    Int32 spriteSlot = (Int32)statusId;
                    UISprite statusSprite = null;
                    if (spriteSlot >= statusesSpriteList.Length || statusesSpriteList[spriteSlot].alpha == 1f)
                        statusSprite = statusesSpriteList.FirstOrDefault(sprite => sprite.alpha == 0f);
                    else
                        statusSprite = statusesSpriteList[spriteSlot];
                    // TODO Add more UISprite if the limit is reached?
                    if (statusSprite == null)
                        break;
                    statusSprite.spriteName = spriteName;
                    statusSprite.alpha = 1f;
                }
            }
        }

        public static void DisplayCharacterAvatar(PLAYER player, Vector3 frontPos, Vector3 backPos, UISprite avatarSprite, Boolean rowUpdate)
        {
            avatarSprite.spriteName = FF9UIDataTool.AvatarSpriteName(player.info.serial_no);
            avatarSprite.alpha = (player.cur.hp != 0) ? 1f : 0.5f;
            if (rowUpdate)
            {
                if (player.info.row == 1)
                    avatarSprite.transform.localPosition = frontPos;
                else if (player.info.row == 0)
                    avatarSprite.transform.localPosition = backPos;
            }
        }

        public static void DisplayCharacterAvatar(CharacterSerialNumber serialId, Vector3 frontPos, Vector3 backPos, UISprite avatarSprite, Boolean rowUpdate)
        {
            avatarSprite.spriteName = FF9UIDataTool.AvatarSpriteName(serialId);
        }

        public static void DisplayCard(QuadMistCard card, CardDetailHUD cardHud, Boolean subCard = false)
        {
            for (Int32 i = 0; i < 8; i++)
                cardHud.CardArrowList[i].SetActive((Configuration.TetraMaster.TripleTriad <= 1) ? ((card.arrow & (1 << i)) != 0 && !subCard) : false); // HIDE ARROW
            cardHud.CardImageSprite.spriteName = "card_" + ((Int32)card.id).ToString("0#");
            if (subCard)
            {
                cardHud.AtkParamSprite.gameObject.SetActive(false);
                cardHud.PhysicDefParamSprite.gameObject.SetActive(false);
                cardHud.MagicDefParamSprite.gameObject.SetActive(false);
                cardHud.AtkTypeParamSprite.gameObject.SetActive(false);
                return;
            }
            cardHud.AtkParamSprite.gameObject.SetActive(true);
            cardHud.PhysicDefParamSprite.gameObject.SetActive(true);
            cardHud.MagicDefParamSprite.gameObject.SetActive(true);
            cardHud.AtkTypeParamSprite.gameObject.SetActive(true);
            if (Configuration.TetraMaster.TripleTriad > 0)
            {
                TripleTriadCard baseCard = TripleTriad.TripleTriadCardStats[card.id];
                cardHud.AtkParamSprite.spriteName = "card_point_" + baseCard.atk.ToString("x");
                cardHud.PhysicDefParamSprite.spriteName = "card_point_" + baseCard.pdef.ToString("x");
                cardHud.MagicDefParamSprite.spriteName = "card_point_" + baseCard.mdef.ToString("x");
                cardHud.AtkTypeParamSprite.spriteName = "card_point_" + baseCard.matk.ToString("x");
            }
            else
            {
                cardHud.AtkParamSprite.spriteName = "card_point_" + (card.atk >> 4).ToString("x");
                cardHud.PhysicDefParamSprite.spriteName = "card_point_" + (card.pdef >> 4).ToString("x");
                cardHud.MagicDefParamSprite.spriteName = "card_point_" + (card.mdef >> 4).ToString("x");
                switch (card.type)
                {
                    case QuadMistCard.Type.PHYSICAL:
                        cardHud.AtkTypeParamSprite.spriteName = "card_point_p";
                        break;
                    case QuadMistCard.Type.MAGIC:
                        cardHud.AtkTypeParamSprite.spriteName = "card_point_m";
                        break;
                    case QuadMistCard.Type.FLEXIABLE:
                        cardHud.AtkTypeParamSprite.spriteName = "card_point_x";
                        break;
                    case QuadMistCard.Type.ASSAULT:
                        cardHud.AtkTypeParamSprite.spriteName = "card_point_a";
                        break;
                }
            }
            if (card.arrow == Byte.MaxValue && QuadMistResourceManager.UseArrowGoldenFrame)
                cardHud.CardBorderSprite.spriteName = "goldenbluecardframe";
            else
                cardHud.CardBorderSprite.spriteName = "card_player_frame";
        }

        public static void DisplayAPBar(PLAYER player, Int32 abilityId, Boolean isShowText, APBarHUD apBar)
        {
            Int32 curAP = ff9abil.FF9Abil_GetAp(player, abilityId);
            Int32 maxAP = ff9abil.FF9Abil_GetMax(player, abilityId);
            if (curAP >= maxAP)
            {
                apBar.TextPanel.SetActive(false);
                apBar.APLabel.rawText = curAP.ToString();
                apBar.APMaxLabel.rawText = maxAP.ToString();
                apBar.ForegroundSprite.spriteName = "ap_bar_complete";
                apBar.MasterSprite.spriteName = "ap_bar_complete_star";
                apBar.Slider.value = 1f;
            }
            else
            {
                apBar.TextPanel.SetActive(isShowText);
                apBar.APLabel.rawText = curAP.ToString();
                apBar.APMaxLabel.rawText = maxAP.ToString();
                apBar.ForegroundSprite.spriteName = "ap_bar_progress";
                apBar.MasterSprite.spriteName = String.Empty;
                apBar.Slider.value = (Single)curAP / (Single)maxAP;
            }
        }

        public static void DisplayTextLocalize(GameObject go, String key)
        {
            go.GetComponent<UILocalize>().key = key;
            go.GetComponent<UILabel>().rawText = Localization.Get(key);
        }

        public static UIAtlas WindowAtlas
        {
            get
            {
                if (FF9StateSystem.Settings.cfg.win_type == 0UL)
                {
                    if (FF9UIDataTool.grayAtlas == null)
                        FF9UIDataTool.grayAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/Gray Atlas", false);
                    return FF9UIDataTool.grayAtlas;
                }
                if (FF9UIDataTool.blueAtlas == null)
                    FF9UIDataTool.blueAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/Blue Atlas", false);
                return FF9UIDataTool.blueAtlas;
            }
        }

        public static UIAtlas GrayAtlas
        {
            get
            {
                if (FF9UIDataTool.grayAtlas == null)
                    FF9UIDataTool.grayAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/Gray Atlas", false);
                return FF9UIDataTool.grayAtlas;
            }
        }

        public static UIAtlas BlueAtlas
        {
            get
            {
                if (FF9UIDataTool.blueAtlas == null)
                    FF9UIDataTool.blueAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/Blue Atlas", false);
                return FF9UIDataTool.blueAtlas;
            }
        }

        public static UIAtlas IconAtlas
        {
            get
            {
                if (FF9UIDataTool.iconAtlas == null)
                    FF9UIDataTool.iconAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/Icon Atlas", false);
                return FF9UIDataTool.iconAtlas;
            }
        }

        public static UIAtlas GeneralAtlas
        {
            get
            {
                if (FF9UIDataTool.generalAtlas == null)
                    FF9UIDataTool.generalAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/General Atlas", false);
                return FF9UIDataTool.generalAtlas;
            }
        }

        public static UIAtlas ScreenButtonAtlas
        {
            get
            {
                if (FF9UIDataTool.screenButtonAtlas == null)
                    FF9UIDataTool.screenButtonAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/Screen Button Atlas", false);
                return FF9UIDataTool.screenButtonAtlas;
            }
        }

        public static UIAtlas TutorialAtlas
        {
            get
            {
                if (FF9UIDataTool.tutorialAtlas == null)
                    FF9UIDataTool.tutorialAtlas = AssetManager.Load<UIAtlas>("EmbeddedAsset/UI/Atlas/TutorialUI Atlas", false);
                return FF9UIDataTool.tutorialAtlas;
            }
        }

        public static UIAtlas ChocographAtlas => PersistenSingleton<UIManager>.Instance.ChocographScene.HintMap.atlas;
        public static UIAtlas FaceAtlas => PersistenSingleton<UIManager>.Instance.StatusScene.CharacterDetailPanel.GetChild(0).GetChild(0).GetComponent<UISprite>().atlas;
        public static UIAtlas MovieGalleryAtlas => PersistenSingleton<UIManager>.Instance.TitleScene.MoviePageGrid.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<UISprite>().atlas;
        public static UIAtlas QuadMistImageAtlas => PersistenSingleton<UIManager>.Instance.CardScene.CardInfoPanel.GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetComponent<UISprite>().atlas;
        public static UIAtlas QuadMistCardAtlas => PersistenSingleton<UIManager>.Instance.CardScene.CardInfoPanel.GetChild(0).GetChild(0).GetChild(0).GetChild(3).GetComponent<UISprite>().atlas;

        public static GameObject IconGameObject(Int32 id)
        {
            GameObject result = null;
            if (id == FF9UIDataTool.NewIconId)
                result = FF9UIDataTool.DrawButton(BitmapIconType.New);
            else if (FF9UIDataTool.TutorialIconSpriteName.ContainsKey(id))
                result = FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.TutorialAtlas, FF9UIDataTool.TutorialIconSpriteName[id]);
            else if (FF9UIDataTool.IconSpriteName.ContainsKey(id))
                result = FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, FF9UIDataTool.IconSpriteName[id]);
            return result;
        }

        public static Vector2 GetIconSize(Int32 id)
        {
            if (id == FF9UIDataTool.NewIconId)
                return new Vector2(115f, 64f);
            if (FF9UIDataTool.IconSpriteName.ContainsKey(id))
                return FF9UIDataTool.GetSpriteSize("IconAtlas", FF9UIDataTool.IconSpriteName[id]);
            return Vector2.zero;
        }

        public static GameObject ButtonGameObject(Control key, Boolean checkFromConfig, String tag)
        {
            if (tag == NGUIText.JoyStickButtonIcon || (PersistenSingleton<HonoInputManager>.Instance.IsControllerConnect && tag != NGUIText.KeyboardButtonIcon))
            {
                return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, FF9UIDataTool.DialogButtonSpriteName(key, checkFromConfig, tag));
            }
            else
            {
                Int32 memoriaKeyIndex = -1;
                switch (key)
                {
                    case Control.Confirm:
                    case Control.Cancel:
                    case Control.Menu:
                    case Control.Special:
                    case Control.LeftBumper:
                    case Control.RightBumper:
                    case Control.LeftTrigger:
                    case Control.RightTrigger:
                    {
                        Int32 keyIndex = (Int32)key;
                        if (!checkFromConfig)
                            keyIndex = (Int32)PersistenSingleton<HonoInputManager>.Instance.PhysicalButtonToLogicalControl(keyIndex);
                        return FF9UIDataTool.DrawButton(BitmapIconType.Keyboard, PersistenSingleton<HonoInputManager>.Instance.InputKeysPrimary[keyIndex]);
                    }
                    case Control.Pause:
                        memoriaKeyIndex = 4;
                        break;
                    case Control.Select:
                        memoriaKeyIndex = 5;
                        break;
                    case Control.Up:
                        //memoriaKeyIndex = 0;
                        return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, "keyboard_button_arrow_up");
                    case Control.Down:
                        //memoriaKeyIndex = 2;
                        return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, "keyboard_button_arrow_down");
                    case Control.Left:
                        //memoriaKeyIndex = 1;
                        return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, "keyboard_button_arrow_left");
                    case Control.Right:
                        //memoriaKeyIndex = 3;
                        return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, "keyboard_button_arrow_right");
                    case Control.DPad:
                        return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, "ps_dpad");
                }
                if (memoriaKeyIndex >= 0)
                {
                    if (HonoInputManager.MemoriaKeyBindings[memoriaKeyIndex] == KeyCode.Backspace)
                        return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, "keyboard_button_backspace");
                    else
                        return FF9UIDataTool.DrawButton(BitmapIconType.Keyboard, HonoInputManager.MemoriaKeyBindings[memoriaKeyIndex]);
                }
            }
            return null;
        }

        private static GameObject GetMobileButtonGameObject(Control key)
        {
            GameObject result = null;
            Int32 iconIndex = 0;
            Int32 mesID = EventEngineUtils.eventIDToMESID[FF9StateSystem.Common.FF9.fldMapNo];
            if (mesID == 2 && key == Control.Up) // Prima Vista
                iconIndex = 268; // icon_up
            FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, FF9UIDataTool.IconSpriteName[iconIndex]);
            return result;
        }

        public static Vector2 GetButtonSize(Control key, Boolean checkFromConfig, String tag)
        {
            String spriteName = String.Empty;
            if (tag == NGUIText.JoyStickButtonIcon || (PersistenSingleton<HonoInputManager>.Instance.IsControllerConnect && tag != NGUIText.KeyboardButtonIcon))
            {
                spriteName = FF9UIDataTool.DialogButtonSpriteName(key, checkFromConfig, tag);
            }
            else
            {
                switch (key)
                {
                    case Control.Confirm:
                    case Control.Cancel:
                    case Control.Menu:
                    case Control.Special:
                    case Control.LeftBumper:
                    case Control.RightBumper:
                    case Control.LeftTrigger:
                    case Control.RightTrigger:
                    case Control.Pause:
                    case Control.Select:
                    case Control.Up:
                    case Control.Down:
                    case Control.Left:
                    case Control.Right:
                        spriteName = "keyboard_button";
                        break;
                    case Control.DPad:
                        spriteName = "ps_dpad";
                        break;
                }
            }
            if (!checkFromConfig && key == Control.Pause && (FF9StateSystem.PCPlatform || FF9StateSystem.AndroidPlatform))
                if (!global::GamePad.GetState(PlayerIndex.One).IsConnected)
                    spriteName = "keyboard_button_backspace";
            return FF9UIDataTool.GetSpriteSize("IconAtlas", spriteName);
        }

        public static GameObject SpriteGameObject(String atlasName, String spriteName)
        {
            switch (atlasName)
            {
                default:
                case "IconAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.IconAtlas, spriteName);
                case "WindowAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.WindowAtlas, spriteName);
                case "GrayAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.GrayAtlas, spriteName);
                case "BlueAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.BlueAtlas, spriteName);
                case "GeneralAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.GeneralAtlas, spriteName);
                case "ScreenButtonAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.ScreenButtonAtlas, spriteName);
                case "TutorialAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.TutorialAtlas, spriteName);
                case "ChocographAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.ChocographAtlas, spriteName);
                case "FaceAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.FaceAtlas, spriteName);
                case "MovieGalleryAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.MovieGalleryAtlas, spriteName);
                case "QuadMistImageAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.QuadMistImageAtlas, spriteName);
                case "QuadMistCardAtlas": return FF9UIDataTool.DrawButton(BitmapIconType.Sprite, FF9UIDataTool.QuadMistCardAtlas, spriteName);
            }
        }

        public static Vector2 GetSpriteSize(String atlasName, String spriteName)
        {
            UISpriteData spriteData;
            switch (atlasName)
            {
                default:
                case "IconAtlas": spriteData = FF9UIDataTool.IconAtlas.GetSprite(spriteName); break;
                case "WindowAtlas": spriteData = FF9UIDataTool.WindowAtlas.GetSprite(spriteName); break;
                case "GrayAtlas": spriteData = FF9UIDataTool.GrayAtlas.GetSprite(spriteName); break;
                case "BlueAtlas": spriteData = FF9UIDataTool.BlueAtlas.GetSprite(spriteName); break;
                case "GeneralAtlas": spriteData = FF9UIDataTool.GeneralAtlas.GetSprite(spriteName); break;
                case "ScreenButtonAtlas": spriteData = FF9UIDataTool.ScreenButtonAtlas.GetSprite(spriteName); break;
                case "TutorialAtlas": spriteData = FF9UIDataTool.TutorialAtlas.GetSprite(spriteName); break;
                case "ChocographAtlas": spriteData = FF9UIDataTool.ChocographAtlas.GetSprite(spriteName); break;
                case "FaceAtlas": spriteData = FF9UIDataTool.FaceAtlas.GetSprite(spriteName); break;
                case "MovieGalleryAtlas": spriteData = FF9UIDataTool.MovieGalleryAtlas.GetSprite(spriteName); break;
                case "QuadMistImageAtlas": spriteData = FF9UIDataTool.QuadMistImageAtlas.GetSprite(spriteName); break;
                case "QuadMistCardAtlas": spriteData = FF9UIDataTool.QuadMistCardAtlas.GetSprite(spriteName); break;
            }
            if (spriteData == null)
                return Vector2.zero;
            return new Vector2(spriteData.width + spriteData.paddingLeft + spriteData.paddingRight, spriteData.height + spriteData.paddingTop + spriteData.paddingBottom);
        }

        private static GameObject DrawButton(BitmapIconType bitmapIconType)
        {
            return FF9UIDataTool.GetControllerGameObject(bitmapIconType);
        }

        private static GameObject DrawButton(BitmapIconType bitmapIconType, UIAtlas atlas, String spriteName)
        {
            spriteName = FF9UIDataTool.CheckIconLocalize(spriteName);
            GameObject controllerGo = FF9UIDataTool.GetControllerGameObject(bitmapIconType);
            FF9UIDataTool.DrawSprite(controllerGo, atlas, spriteName);
            return controllerGo;
        }

        private static GameObject DrawButton(BitmapIconType bitmapIconType, KeyCode keycode)
        {
            GameObject controllerGo = FF9UIDataTool.GetControllerGameObject(bitmapIconType);
            FF9UIDataTool.DrawLabel(controllerGo.GetChild(0), keycode);
            return controllerGo;
        }

        private static GameObject GetControllerGameObject(BitmapIconType bitmapIconType)
        {
            GameObject iconObject = null;
            switch (bitmapIconType)
            {
                case BitmapIconType.Sprite:
                    iconObject = FF9UIDataTool.GetGameObjectFromPool(FF9UIDataTool.bitmapSpritePool);
                    if (iconObject == null)
                    {
                        if (FF9UIDataTool.controllerSpritePrefab == null)
                            FF9UIDataTool.controllerSpritePrefab = Resources.Load("EmbeddedAsset/UI/Prefabs/Controller Sprite") as GameObject;
                        iconObject = UnityEngine.Object.Instantiate(FF9UIDataTool.controllerSpritePrefab);
                        iconObject.tag = "BitmapSprite";
                    }
                    iconObject.SetActive(false);
                    FF9UIDataTool.activeBitmapSpriteList.Push(iconObject);
                    break;
                case BitmapIconType.Keyboard:
                    iconObject = FF9UIDataTool.GetGameObjectFromPool(FF9UIDataTool.bitmapKeyboardPool);
                    if (iconObject == null)
                    {
                        if (FF9UIDataTool.controllerKeyboardPrefab == null)
                            FF9UIDataTool.controllerKeyboardPrefab = Resources.Load("EmbeddedAsset/UI/Prefabs/Controller Keyboard") as GameObject;
                        iconObject = UnityEngine.Object.Instantiate(FF9UIDataTool.controllerKeyboardPrefab);
                        iconObject.tag = "BitmapKeyboard";
                    }
                    iconObject.SetActive(false);
                    FF9UIDataTool.activeBitmapKeyboardList.Push(iconObject);
                    break;
                case BitmapIconType.New:
                    iconObject = FF9UIDataTool.GetGameObjectFromPool(FF9UIDataTool.bitmapNewIconPool);
                    if (iconObject == null)
                    {
                        if (FF9UIDataTool.newIconPrefab == null)
                            FF9UIDataTool.newIconPrefab = Resources.Load("EmbeddedAsset/UI/Prefabs/New Icon") as GameObject;
                        iconObject = UnityEngine.Object.Instantiate(FF9UIDataTool.newIconPrefab);
                        iconObject.tag = "BitmapNewIcon";
                    }
                    iconObject.SetActive(false);
                    FF9UIDataTool.activeBitmapNewIconList.Push(iconObject);
                    break;
            }
            return iconObject;
        }

        private static GameObject GetGameObjectFromPool(List<GameObject> currentPool)
        {
            GameObject gameObject = null;
            if (currentPool.Count > 0)
            {
                gameObject = currentPool.Pop();
                gameObject.SetActive(false);
            }
            return gameObject;
        }

        private static String CheckIconLocalize(String spriteName)
        {
            String key = spriteName + "#" + Localization.CurrentDisplayLanguage;
            if (FF9UIDataTool.iconLocalizeList.TryGetValue(key, out String localSpriteName))
                return localSpriteName;
            return spriteName;
        }

        /// <summary>Take the ownership of a sprite, so FF9UIDataTool will never release it back to its sprite pool and the calling class is in charge of releasing it</summary>
        public static void TakeBitmapOwnership(GameObject gameObject)
        {
            FF9UIDataTool.activeBitmapKeyboardList.Remove(gameObject);
            FF9UIDataTool.activeBitmapSpriteList.Remove(gameObject);
            FF9UIDataTool.activeBitmapNewIconList.Remove(gameObject);
        }

        public static void ReleaseBitmapIconToPool(GameObject bitmap)
        {
            FF9UIDataTool.GetCurrentPool(bitmap.tag, out List<GameObject> inactivePool, out List<GameObject> activePool);
            bitmap.transform.parent = PersistenSingleton<UIManager>.Instance.transform;
            bitmap.SetActive(false);
            activePool.Remove(bitmap);
            inactivePool.Push(bitmap);
        }

        private static void GetCurrentPool(String tag, out List<GameObject> currentPool, out List<GameObject> currentActivePool)
        {
            switch (tag)
            {
                case "BitmapKeyboard":
                    currentPool = FF9UIDataTool.bitmapKeyboardPool;
                    currentActivePool = FF9UIDataTool.activeBitmapKeyboardList;
                    return;
                case "BitmapSprite":
                    currentPool = FF9UIDataTool.bitmapSpritePool;
                    currentActivePool = FF9UIDataTool.activeBitmapSpriteList;
                    return;
                case "BitmapNewIcon":
                    currentPool = FF9UIDataTool.bitmapNewIconPool;
                    currentActivePool = FF9UIDataTool.activeBitmapNewIconList;
                    return;
            }
            currentPool = null;
            currentActivePool = null;
        }

        private static void ReleaseAllBitmapIconsToPool(List<GameObject> currentPool, List<GameObject> currentActiveList)
        {
            foreach (GameObject gameObject in currentActiveList)
            {
                gameObject.transform.parent = PersistenSingleton<UIManager>.Instance.transform;
                gameObject.SetActive(false);
            }
            currentPool.AddRange(currentActiveList);
            currentActiveList.Clear();
        }

        public static void ReleaseAllTypeBitmapIconsToPool()
        {
            if (FF9UIDataTool.activeBitmapKeyboardList.Count != 0)
                FF9UIDataTool.ReleaseAllBitmapIconsToPool(FF9UIDataTool.bitmapKeyboardPool, FF9UIDataTool.activeBitmapKeyboardList);
            if (FF9UIDataTool.activeBitmapSpriteList.Count != 0)
                FF9UIDataTool.ReleaseAllBitmapIconsToPool(FF9UIDataTool.bitmapSpritePool, FF9UIDataTool.activeBitmapSpriteList);
            if (FF9UIDataTool.activeBitmapNewIconList.Count != 0)
                FF9UIDataTool.ReleaseAllBitmapIconsToPool(FF9UIDataTool.bitmapNewIconPool, FF9UIDataTool.activeBitmapNewIconList);
        }

        public static void ClearAllPool()
        {
            FF9UIDataTool.bitmapKeyboardPool.Clear();
            FF9UIDataTool.bitmapSpritePool.Clear();
            FF9UIDataTool.bitmapNewIconPool.Clear();
        }

        public static void DrawSprite(GameObject go, UIAtlas atlas, String spriteName)
        {
            UISprite component = go.GetComponent<UISprite>();
            component.atlas = atlas;
            component.spriteName = spriteName;
            component.MakePixelPerfect();
        }

        public static void DrawLabel(GameObject go, KeyCode keycode)
        {
            go.GetComponent<UILabel>().rawText = FF9UIDataTool.KeyboardIconLabel.ContainsKey(keycode) ? FF9UIDataTool.KeyboardIconLabel[keycode] : String.Empty;
            if (keycode >= KeyCode.Keypad0 && keycode <= KeyCode.KeypadPlus)
            {
                go.transform.localPosition = new Vector3(go.transform.localPosition.x, -37f, go.transform.localPosition.z);
                FF9UIDataTool.DrawSprite(go.GetParent(), FF9UIDataTool.IconAtlas, "keyboard_button_num");
            }
            else
            {
                go.transform.localPosition = new Vector3(go.transform.localPosition.x, -31f, go.transform.localPosition.z);
                FF9UIDataTool.DrawSprite(go.GetParent(), FF9UIDataTool.IconAtlas, "keyboard_button");
            }
        }

        public static String DialogButtonSpriteName(Control key, Boolean checkFromConfig, String tag)
        {
            HonoInputManager inputManager = PersistenSingleton<HonoInputManager>.Instance;
            String result = String.Empty;
            if (inputManager.IsControllerConnect || tag == NGUIText.JoyStickButtonIcon)
            {
                Dictionary<String, String> buttonSpriteDictionary;
                if (Application.platform == RuntimePlatform.Android)
                    buttonSpriteDictionary = FF9UIDataTool.buttonSpriteNameAndroidJoystick;
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    buttonSpriteDictionary = FF9UIDataTool.buttonSpriteNameiOSJoystick;
                else
                    buttonSpriteDictionary = FF9UIDataTool.buttonSpriteNameJoystick;
                switch (key)
                {
                    case Control.Confirm:
                    case Control.Cancel:
                    case Control.Menu:
                    case Control.Special:
                    case Control.LeftBumper:
                    case Control.RightBumper:
                    case Control.LeftTrigger:
                    case Control.RightTrigger:
                    {
                        Int32 keyIndex = (Int32)key;
                        if (!checkFromConfig && EventInput.isJapaneseLayout)
                        {
                            if (keyIndex == 0)
                                keyIndex = 1;
                            else if (keyIndex == 1)
                                keyIndex = 0;
                        }
                        String primaryKey = checkFromConfig ? inputManager.JoystickKeysPrimary[keyIndex] : inputManager.DefaultJoystickInputKeys[keyIndex];
                        if (!buttonSpriteDictionary.TryGetValue(primaryKey, out result))
                            result = String.Empty;
                        break;
                    }
                    case Control.Pause:
                        result = FF9UIDataTool.buttonSpriteNameJoystick["JoystickButton6"];
                        break;
                    case Control.Select:
                        result = FF9UIDataTool.buttonSpriteNameJoystick["JoystickButton7"];
                        break;
                    case Control.Up:
                        result = buttonSpriteDictionary["Up"];
                        break;
                    case Control.Down:
                        result = buttonSpriteDictionary["Down"];
                        break;
                    case Control.Left:
                        result = buttonSpriteDictionary["Left"];
                        break;
                    case Control.Right:
                        result = buttonSpriteDictionary["Right"];
                        break;
                    case Control.DPad:
                        result = buttonSpriteDictionary["DPad"];
                        break;
                }
            }
            return result;
        }

        public static String GetJoystickSpriteByName(String key)
        {
            String result = String.Empty;
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (FF9UIDataTool.buttonSpriteNameiOSJoystick.ContainsKey(key))
                    result = FF9UIDataTool.buttonSpriteNameiOSJoystick[key];
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                if (FF9UIDataTool.buttonSpriteNameAndroidJoystick.ContainsKey(key))
                    result = FF9UIDataTool.buttonSpriteNameAndroidJoystick[key];
            }
            else if (FF9UIDataTool.buttonSpriteNameJoystick.ContainsKey(key))
            {
                result = FF9UIDataTool.buttonSpriteNameJoystick[key];
            }
            return result;
        }

        public static String AvatarSpriteName(CharacterSerialNumber serialNo)
        {
            if (serialNo >= CharacterSerialNumber.GARNET_LH_ROD && serialNo <= CharacterSerialNumber.GARNET_SH_RACKET)
            {
                if (Configuration.Graphics.GarnetHair == 1)
                    return "face02";
                if (Configuration.Graphics.GarnetHair == 2)
                    return "face03";
            }
            return btl_mot.BattleParameterList.TryGetValue(serialNo, out CharacterBattleParameter param) ? param.AvatarSprite : "face_unkown";
        }

        public static Sprite LoadWorldTitle(SByte titleId, Boolean isShadow)
        {
            String langSymbol = Localization.CurrentDisplaySymbol;
            String spriteName = GetWorldTitleSpriteName(titleId, isShadow, langSymbol);
            Sprite sprite;
            if (FF9UIDataTool.worldTitleSpriteList.ContainsKey(spriteName))
            {
                sprite = FF9UIDataTool.worldTitleSpriteList[spriteName];
            }
            else
            {
                String path = "EmbeddedAsset/UI/Sprites/" + langSymbol + "/" + spriteName;
                sprite = AssetManager.Load<Sprite>(path, false);
                // Don't use that dictionary at all to store the sprites:
                // they are not requested often (normally not more than once)
                // and storing them could make the wrong sprite appears if the player changes the language
                //FF9UIDataTool.worldTitleSpriteList.Add(spriteName, sprite);
            }
            return sprite;
        }

        public static String GetWorldTitleSpriteName(SByte titleId, Boolean isShadow, String langSymbol)
        {
            if (langSymbol == "UK")
                langSymbol = "US";
            String spriteName;
            if (titleId == FF9UIDataTool.WorldTitleMistContinent)
            {
                spriteName = "title_world_mist";
            }
            else if (titleId == FF9UIDataTool.WorldTitleOuterContinent)
            {
                spriteName = "title_world_outer";
            }
            else if (titleId == FF9UIDataTool.WorldTitleForgottenContinent)
            {
                spriteName = "title_world_forgotten";
            }
            else if (titleId == FF9UIDataTool.WorldTitleLostContinent)
            {
                spriteName = "title_world_lost";
            }
            else
            {
                global::Debug.LogError("World Continent Title: Could not found resource from titleId:" + titleId);
                return null;
            }
            spriteName += isShadow ? "_shadow_" + langSymbol.ToLower() : "_" + langSymbol.ToLower();
            return spriteName;
        }

        public const Int32 NewIconId = 400;

        private static UIAtlas generalAtlas;
        private static UIAtlas iconAtlas;
        private static UIAtlas grayAtlas;
        private static UIAtlas blueAtlas;
        private static UIAtlas screenButtonAtlas;
        private static UIAtlas tutorialAtlas;

        private static GameObject controllerSpritePrefab = null;
        private static GameObject controllerKeyboardPrefab = null;
        private static GameObject newIconPrefab = null;

        private static List<GameObject> bitmapKeyboardPool = new List<GameObject>();
        private static List<GameObject> bitmapSpritePool = new List<GameObject>();
        private static List<GameObject> bitmapNewIconPool = new List<GameObject>();
        private static List<GameObject> activeBitmapKeyboardList = new List<GameObject>();
        private static List<GameObject> activeBitmapSpriteList = new List<GameObject>();
        private static List<GameObject> activeBitmapNewIconList = new List<GameObject>();

        private static Dictionary<String, String> buttonSpriteNameiOSJoystick = new Dictionary<String, String>
        {
            { "JoystickButton14",   "joystick_button_a" },
            { "JoystickButton13",   "joystick_button_b" },
            { "JoystickButton15",   "joystick_button_x" },
            { "JoystickButton12",   "joystick_button_y" },
            { "JoystickButton8",    "joystick_l1" },
            { "JoystickButton9",    "joystick_r1" },
            { "JoystickButton10",   "joystick_l2" },
            { "JoystickButton11",   "joystick_r2" },
            { "JoystickButton0",    "joystick_start" },
            { "Empty",              "joystick_analog_r" },
            { "Up",                 "ps_dpad_up" },
            { "Down",               "ps_dpad_down" },
            { "Left",               "ps_dpad_left" },
            { "Right",              "ps_dpad_right" },
            { "DPad",               "ps_dpad" }
        };

        private static Dictionary<String, String> buttonSpriteNameAndroidJoystick = new Dictionary<String, String>
        {
            { "JoystickButton0",        "joystick_button_a" },
            { "JoystickButton1",        "joystick_button_b" },
            { "JoystickButton2",        "joystick_button_x" },
            { "JoystickButton3",        "joystick_button_y" },
            { "JoystickButton4",        "joystick_l1" },
            { "JoystickButton5",        "joystick_r1" },
            { "LeftTrigger Android",    "joystick_l2" },
            { "RightTrigger Android",   "joystick_r2" },
            { "JoystickButton10",       "joystick_start" },
            { "Empty",                  "joystick_analog_r" },
            { "Up",                     "ps_dpad_up" },
            { "Down",                   "ps_dpad_down" },
            { "Left",                   "ps_dpad_left" },
            { "Right",                  "ps_dpad_right" },
            { "DPad",                   "ps_dpad" }
        };

        private static Dictionary<String, String> buttonSpriteNameJoystick = new Dictionary<String, String>
        {
            { "JoystickButton0",    "joystick_button_a" },
            { "JoystickButton1",    "joystick_button_b" },
            { "JoystickButton2",    "joystick_button_x" },
            { "JoystickButton3",    "joystick_button_y" },
            { "JoystickButton4",    "joystick_l1" },
            { "JoystickButton5",    "joystick_r1" },
            { "LeftTrigger",        "joystick_l2" },
            { "RightTrigger",       "joystick_r2" },
            { "JoystickButton6",    "joystick_start" },
            { "JoystickButton7",    "joystick_select" },
            { "Up",                 "ps_dpad_up" },
            { "Down",               "ps_dpad_down" },
            { "Left",               "ps_dpad_left" },
            { "Right",              "ps_dpad_right" },
            { "DPad",               "ps_dpad" }
        };

        public static readonly Dictionary<Int32, String> IconSpriteName = new Dictionary<Int32, String>
        {
            { 1,    "cursor_hand_choice" },
            { 2,    "cursor_hand_here" },
            { 17,   "arrow_left" },
            { 18,   "arrow_right" },
            { 19,   "arrow_up" },
            { 20,   "arrow_down" },
            { 27,   "help_mog_dialog" },
            { 28,   "help_mog_hand_1" },
            { 29,   "help_mog_hand_2" },
            { 92,   "item00_00" },
            { 93,   "item01_00" },
            { 94,   "item02_00" },
            { 95,   "item03_00" },
            { 96,   "item04_00" },
            { 97,   "item05_00" },
            { 98,   "item06_00" },
            { 99,   "item07_00" },
            { 100,  "item08_00" },
            { 101,  "item09_00" },
            { 102,  "item10_00" },
            { 103,  "item11_00" },
            { 104,  "item12_00" },
            { 105,  "item13_01" },
            { 106,  "item14_02" },
            { 107,  "item15_01" },
            { 108,  "item16_01" },
            { 109,  "item17_01" },
            { 110,  "item18_00" },
            { 111,  "item19_02" },
            { 112,  "item20_01" },
            { 113,  "item21_00" },
            { 114,  "item22_02" },
            { 115,  "item23_00" },
            { 116,  "item24_00" },
            { 117,  "item25_03" },
            { 118,  "item26_09" },
            { 119,  "item27_01" },
            { 120,  "item28_03" },
            { 121,  "item29_01" },
            { 122,  "item30_00" },
            { 123,  "item31_08" },
            { 124,  "item32_02" },
            { 125,  "item33_01" },
            { 126,  "item34_02" },
            { 127,  "item35_03" },
            { 131,  "icon_status_22" },
            { 132,  "icon_status_23" },
            { 133,  "icon_status_11" },
            { 134,  "icon_status_00" },
            { 135,  "icon_status_01" },
            { 136,  "icon_status_02" },
            { 137,  "icon_status_03" },
            { 138,  "icon_status_04" },
            { 139,  "icon_status_05" },
            { 140,  "icon_status_06" },
            { 141,  "icon_status_07" },
            { 142,  "icon_status_08" },
            { 143,  "icon_status_09" },
            { 144,  "icon_status_10" },
            { 145,  "icon_status_12" },
            { 146,  "icon_status_13" },
            { 147,  "icon_status_14" },
            { 148,  "icon_status_15" },
            { 149,  "icon_status_16" },
            { 150,  "icon_status_17" },
            { 151,  "icon_status_18" },
            { 152,  "icon_status_19" },
            { 153,  "icon_status_20" },
            { 154,  "icon_status_21" },
            { 155,  "icon_status_04_es" },
            { 180,  "text_lv_us_uk_jp_gr_it" },
            { 188,  "ability_stone" },
            { 189,  "skill_stone_on" },
            { 190,  "ability_stone_null" },
            { 191,  "skill_stone_null" },
            { 192,  "ap_bar_complete_star" },
            { 193,  "skill_stone_gem" },
            { 194,  "skill_stone_gem_00" },
            { 195,  "skill_stone_gem_01" },
            { 196,  "skill_stone_gem_02" },
            { 197,  "skill_stone_gem_03" },
            { 198,  "skill_stone_gem_04" },
            { 199,  "skill_stone_gem_05" },
            { 200,  "skill_stone_gem_06" },
            { 244,  "skill_stone_off" },
            { 245,  "config_arrow" },
            { 246,  "balloon_beach_act" },
            { 247,  "balloon_exclamation_act" },
            { 248,  "balloon_question_act" },
            { 249,  "balloon_card_act" },
            { 250,  "virtual_map" },
            { 254,  "ap_bar_full" },
            { 255,  "ap_bar_half" },
            { 256,  "balloon_beach" },
            { 257,  "balloon_exclamation" },
            { 258,  "balloon_question" },
            { 259,  "balloon_card" },
            { 260,  "icon_action" },
            { 261,  "icon_back" },
            { 262,  "icon_analog" },
            { 263,  "virtual_up" },
            { 264,  "virtual_down" },
            { 265,  "virtual_left" },
            { 266,  "virtual_right" },
            { 267,  "icon_left" },
            { 268,  "icon_up" },
            { 269,  "icon_right" },
            { 270,  "icon_down" },
            { 271,  "icon_x" },
            { 272,  "icon_y" },
            { 273,  "icon_b" },
            { 274,  "icon_a" },
            { 275,  "icon_minus" },
            { 276,  "icon_plus" },
            { 277,  "icon_racing_01" },
            { 278,  "icon_racing_02" },
            { 279,  "icon_help" },
            { 280,  "virtual_aside" },
            { 281,  "icon_world_map" },
            { 282,  "icon_world_mog" },
            { 283,  "icon_world_map" },
            { 284,  "icon_cam_rotate" },
            { 285,  "icon_cam_perspective" },
            { 286,  "icon_world_dismount" },
            { 287,  "icon_cam_align" },
            { 288,  "icon_menu_jp" },
            { 289,  "icon_menu_us" },
            { 290,  "icon_menu_fr" },
            { 291,  "icon_menu_es" },
            { 292,  "icon_menu_gr" },
            { 293,  "icon_menu_it" },
            { 294,  "icon_menu_uk" },
            { 295,  "icon_deck_jp" },
            { 296,  "icon_deck_us" },
            { 297,  "icon_deck_fr" },
            { 298,  "icon_deck_es" },
            { 299,  "icon_deck_gr" },
            { 300,  "icon_deck_it" },
            { 301,  "icon_deck_uk" },
            { 302,  "icon_battle_run" },
            { 303,  "icon_battle_all" },
            { 304,  "icon_pause" },
            { 305,  "icon_bubble_question" },
            { 306,  "text_touch_us_uk_fr_gr_it_es" },
            { 307,  "text_touchconfirm_us_uk_fr_gr_it_es" },
            { 308,  "text_touchscreen_us_uk_fr_gr_it_es" },
            { 309,  "text_characterpanel_us_uk_fr_gr_it_es" },
            { 310,  "icon_bubble_card" },
            { 311,  "icon_chocobo_dig" },
            { 312,  "icon_bubble_question" },
            { 313,  "icon_ate_us_uk_jp" },
            { 314,  "icon_ate_es_fr" },
            { 315,  "icon_ate_gr_it" },
            { 316,  "text_touch_jp" },
            { 317,  "text_touch_us_uk" },
            { 318,  "text_touch_fr" },
            { 319,  "text_touch_es" },
            { 320,  "text_touch_gr" },
            { 321,  "text_touch_it" },
            { 322,  "text_touchconfirm_jp" },
            { 323,  "text_touchconfirm_us_uk" },
            { 324,  "text_touchconfirm_fr" },
            { 325,  "text_touchconfirm_es" },
            { 326,  "text_touchconfirm_gr" },
            { 327,  "text_touchconfirm_it" },
            { 328,  "text_touchscreen_jp" },
            { 329,  "text_touchscreen_us_uk" },
            { 330,  "text_touchscreen_fr" },
            { 331,  "text_touchscreen_es" },
            { 332,  "text_touchscreen_gr" },
            { 333,  "text_touchscreen_it" },
            { 334,  "text_characterpanel_jp" },
            { 335,  "text_characterpanel_us_uk" },
            { 336,  "text_characterpanel_fr" },
            { 337,  "text_characterpanel_es" },
            { 338,  "text_characterpanel_gr" },
            { 339,  "text_characterpanel_it" },
            { 340,  "icon_ate_us_uk_jp" },
            { 341,  "icon_ate_es_fr" },
            { 342,  "icon_ate_gr_it" },
            { 343,  "icon_beach" },
            { 350,  "icon_ff" },
            { 351,  "icon_battle" },
            { 352,  "icon_9999" },
            { 353,  "icon_encounter" },
            { 354,  "icon_gil" },
            { 355,  "icon_masterskill" },
            { 360,  "icon_bubble" },
            { 361,  "icon_card" },
            { 362,  "icon_new_exclamation" },
            { 363,  "icon_new_text" },
            { 364,  "icon_time" },
            { 450,  "item00_15" },
            { 451,  "item01_15" },
            { 452,  "item02_15" },
            { 453,  "item03_15" },
            { 454,  "item04_15" },
            { 455,  "item05_15" },
            { 456,  "item06_15" },
            { 457,  "item07_15" },
            { 458,  "item08_15" },
            { 459,  "item09_15" },
            { 460,  "item10_15" },
            { 461,  "item11_15" },
            { 462,  "item12_15" },
            { 463,  "item13_15" },
            { 464,  "item14_15" },
            { 465,  "item15_15" },
            { 466,  "item16_15" },
            { 467,  "item17_15" },
            { 468,  "item18_15" },
            { 469,  "item19_15" },
            { 470,  "item20_15" },
            { 471,  "item21_15" },
            { 472,  "item22_15" },
            { 473,  "item23_15" },
            { 474,  "item24_15" },
            { 475,  "item25_15" },
            { 476,  "item26_15" },
            { 477,  "item27_15" },
            { 478,  "item28_15" },
            { 479,  "item29_15" },
            { 480,  "item30_15" },
            { 481,  "item31_15" },
            { 482,  "item32_15" },
            { 483,  "item33_15" },
            { 484,  "item34_15" },
            { 485,  "item35_15" },
            { 512,  "item01_06" },
            { 513,  "item01_12" },
            { 514,  "item03_03" },
            { 515,  "item03_05" },
            { 516,  "item03_09" },
            { 517,  "item03_11" },
            { 518,  "item03_13" },
            { 519,  "item04_03" },
            { 520,  "item05_03" },
            { 521,  "item05_05" },
            { 522,  "item05_06" },
            { 523,  "item05_12" },
            { 524,  "item06_03" },
            { 525,  "item06_06" },
            { 526,  "item06_13" },
            { 527,  "item08_03" },
            { 528,  "item08_04" },
            { 529,  "item08_12" },
            { 530,  "item09_02" },
            { 531,  "item09_03" },
            { 532,  "item09_07" },
            { 533,  "item10_02" },
            { 534,  "item10_05" },
            { 535,  "item10_09" },
            { 536,  "item10_11" },
            { 537,  "item11_02" },
            { 538,  "item11_04" },
            { 539,  "item13_02" },
            { 540,  "item13_03" },
            { 541,  "item13_07" },
            { 542,  "item13_14" },
            { 543,  "item14_08" },
            { 544,  "item14_09" },
            { 545,  "item14_11" },
            { 546,  "item15_02" },
            { 547,  "item15_04" },
            { 548,  "item15_07" },
            { 549,  "item15_08" },
            { 550,  "item15_09" },
            { 551,  "item15_12" },
            { 552,  "item16_03" },
            { 553,  "item16_08" },
            { 554,  "item16_09" },
            { 555,  "item16_12" },
            { 556,  "item17_02" },
            { 557,  "item17_03" },
            { 558,  "item17_08" },
            { 559,  "item17_14" },
            { 560,  "item18_06" },
            { 561,  "item18_08" },
            { 562,  "item18_09" },
            { 563,  "item18_10" },
            { 564,  "item18_11" },
            { 565,  "item18_14" },
            { 566,  "item19_03" },
            { 567,  "item19_06" },
            { 568,  "item19_08" },
            { 569,  "item19_11" },
            { 570,  "item19_12" },
            { 571,  "item20_02" },
            { 572,  "item20_03" },
            { 573,  "item20_06" },
            { 574,  "item20_10" },
            { 575,  "item20_11" },
            { 576,  "item20_14" },
            { 577,  "item21_01" },
            { 578,  "item21_02" },
            { 579,  "item21_03" },
            { 580,  "item21_04" },
            { 581,  "item21_05" },
            { 582,  "item21_06" },
            { 583,  "item21_07" },
            { 584,  "item21_08" },
            { 585,  "item21_10" },
            { 586,  "item21_12" },
            { 587,  "item21_13" },
            { 588,  "item21_14" },
            { 589,  "item22_06" },
            { 590,  "item22_08" },
            { 591,  "item22_09" },
            { 592,  "item23_03" },
            { 593,  "item23_05" },
            { 594,  "item23_06" },
            { 595,  "item23_07" },
            { 596,  "item23_09" },
            { 597,  "item23_10" },
            { 598,  "item24_02" },
            { 599,  "item24_03" },
            { 600,  "item24_04" },
            { 601,  "item24_06" },
            { 602,  "item24_08" },
            { 603,  "item24_10" },
            { 604,  "item24_11" },
            { 605,  "item25_09" },
            { 606,  "item27_09" },
            { 607,  "item28_08" },
            { 608,  "item29_02" },
            { 609,  "item29_03" },
            { 610,  "item29_04" },
            { 611,  "item29_08" },
            { 612,  "item29_10" },
            { 613,  "item29_12" },
            { 614,  "item29_14" },
            { 615,  "item31_09" },
            { 616,  "item31_14" },
            { 617,  "item32_03" },
            { 618,  "item33_02" },
            { 619,  "item33_03" },
            { 620,  "item33_04" },
            { 621,  "item33_05" },
            { 622,  "item33_11" },
            { 623,  "item33_14" },
            { 624,  "item35_12" },
            { 625,  "icon_equip_0" },
            { 626,  "icon_equip_1" },
            { 627,  "icon_equip_2" },
            { 628,  "icon_equip_3" },
            { 629,  "icon_equip_4" },
            { 630,  "keyboard_button_enter" },
            { 631,  "keyboard_button_esc" },
            { 632,  "keyboard_button_arrow_up" },
            { 633,  "keyboard_button_arrow_left" },
            { 634,  "keyboard_button_arrow_right" },
            { 635,  "keyboard_button_arrow_down" },
            { 636,  "text_lv_es" },
            { 637,  "text_lv_fr" },
            { 638,  "joystick_start" },
            { 639,  "joystick_l2" },
            { 640,  "joystick_r2" },
            { 641,  "joystick_button_y" },
            { 642,  "joystick_button_x" },
            { 643,  "joystick_button_a" },
            { 644,  "joystick_button_b" },
            { 645,  "joystick_analog_r" },
            { 646,  "joystick_select" },
            { 647,  "joystick_l1" },
            { 648,  "joystick_r1" },
            { 649,  "keyboard_button" },
            { 650,  "keyboard_button_backspace" },
            { 651,  "keyboard_button_backspace_fr_gr_it" },
            { 652,  "keyboard_button_enter_fr_gr" },
            { 653,  "keyboard_button_enter_it" },
            { 654,  "keyboard_button_num" },
            { 655,  "ps_dpad" },
            { 656,  "ps_dpad_down" },
            { 657,  "ps_dpad_left" },
            { 658,  "ps_dpad_right" },
            { 659,  "ps_dpad_up" },
            { 700,  "shop_icon_part_0" },
            { 701,  "shop_icon_part_1" },
            { 702,  "shop_icon_part_2" },
            { 703,  "shop_icon_part_3" },
            { 704,  "shop_icon_part_4" },
            { 710,  "status_arrow" },
            { 720,  "text_hp_es" },
            { 721,  "text_hp_us_uk_jp_fr_gr_it" },
            { 722,  "text_mp_es" },
            { 723,  "text_mp_us_uk_jp_fr_gr_it" },
            { 724,  "text_sp_us_uk_jp_es_fr_gr_it" },
        };

        public static readonly Dictionary<Int32, String> TutorialIconSpriteName = new Dictionary<Int32, String>
        {
            { 769, "tutorial_quadmist_2" },
            { 770, "tutorial_quadmist_3" }
        };

        public static readonly Dictionary<KeyCode, String> KeyboardIconLabel = new Dictionary<KeyCode, String>
        {
            { KeyCode.Backspace,    "BS" },
            { KeyCode.Exclaim,      "!" },
            { KeyCode.DoubleQuote,  "\"" },
            { KeyCode.Hash,         "#" },
            { KeyCode.Dollar,       "$" },
            { KeyCode.Ampersand,    "&" },
            { KeyCode.Quote,        "'" },
            { KeyCode.LeftParen,    "(" },
            { KeyCode.RightParen,   ")" },
            { KeyCode.Asterisk,     "*" },
            { KeyCode.Plus,         "+" },
            { KeyCode.Comma,        "," },
            { KeyCode.Minus,        "-" },
            { KeyCode.Period,       "." },
            { KeyCode.Slash,        "/" },
            { KeyCode.Alpha0,       "0" },
            { KeyCode.Alpha1,       "1" },
            { KeyCode.Alpha2,       "2" },
            { KeyCode.Alpha3,       "3" },
            { KeyCode.Alpha4,       "4" },
            { KeyCode.Alpha5,       "5" },
            { KeyCode.Alpha6,       "6" },
            { KeyCode.Alpha7,       "7" },
            { KeyCode.Alpha8,       "8" },
            { KeyCode.Alpha9,       "9" },
            { KeyCode.Colon,        ":" },
            { KeyCode.Semicolon,    ";" },
            { KeyCode.Less,         "<" },
            { KeyCode.Equals,       "=" },
            { KeyCode.Greater,      ">" },
            { KeyCode.Question,     "?" },
            { KeyCode.At,           "@" },
            { KeyCode.LeftBracket,  "[" },
            { KeyCode.Backslash,    "\\" },
            { KeyCode.RightBracket, "]" },
            { KeyCode.Caret,        "^" },
            { KeyCode.Underscore,   "_" },
            { KeyCode.BackQuote,    "`" },
            { KeyCode.A,            "A" },
            { KeyCode.B,            "B" },
            { KeyCode.C,            "C" },
            { KeyCode.D,            "D" },
            { KeyCode.E,            "E" },
            { KeyCode.F,            "F" },
            { KeyCode.G,            "G" },
            { KeyCode.H,            "H" },
            { KeyCode.I,            "I" },
            { KeyCode.J,            "J" },
            { KeyCode.K,            "K" },
            { KeyCode.L,            "L" },
            { KeyCode.M,            "M" },
            { KeyCode.N,            "N" },
            { KeyCode.O,            "O" },
            { KeyCode.P,            "P" },
            { KeyCode.Q,            "Q" },
            { KeyCode.R,            "R" },
            { KeyCode.S,            "S" },
            { KeyCode.T,            "T" },
            { KeyCode.U,            "U" },
            { KeyCode.V,            "V" },
            { KeyCode.W,            "W" },
            { KeyCode.X,            "X" },
            { KeyCode.Y,            "Y" },
            { KeyCode.Z,            "Z" },
            { KeyCode.Keypad0,      "0" },
            { KeyCode.Keypad1,      "1" },
            { KeyCode.Keypad2,      "2" },
            { KeyCode.Keypad3,      "3" },
            { KeyCode.Keypad4,      "4" },
            { KeyCode.Keypad5,      "5" },
            { KeyCode.Keypad6,      "6" },
            { KeyCode.Keypad7,      "7" },
            { KeyCode.Keypad8,      "8" },
            { KeyCode.Keypad9,      "9" },
            { KeyCode.KeypadDivide, "/" },
            { KeyCode.KeypadPeriod, "." },
            { KeyCode.KeypadMultiply, "*" },
            { KeyCode.KeypadPlus,   "+" },
            { KeyCode.KeypadMinus,  "-" },
            { KeyCode.F1,           "F1" },
            { KeyCode.F2,           "F2" },
            { KeyCode.F3,           "F3" },
            { KeyCode.F4,           "F4" },
            { KeyCode.F5,           "F5" },
            { KeyCode.F6,           "F6" },
            { KeyCode.F7,           "F7" },
            { KeyCode.F8,           "F8" },
            { KeyCode.F9,           "F9" },
            { KeyCode.F10,          "F10" },
            { KeyCode.F11,          "F11" },
            { KeyCode.F12,          "F12" }
        };

        private static readonly SByte WorldTitleMistContinent = 0;
        private static readonly SByte WorldTitleOuterContinent = 1;
        private static readonly SByte WorldTitleForgottenContinent = 2;
        private static readonly SByte WorldTitleLostContinent = 3;

        private static Dictionary<String, Sprite> worldTitleSpriteList = new Dictionary<String, Sprite>();

        private static readonly Dictionary<String, String> iconLocalizeList = new Dictionary<String, String>
        {
            { "keyboard_button_enter#French",       "keyboard_button_enter_fr_gr" },
            { "keyboard_button_enter#German",       "keyboard_button_enter_fr_gr" },
            { "keyboard_button_enter#Italian",      "keyboard_button_enter_it" },
            { "keyboard_button_backspace#French",   "keyboard_button_backspace_fr_gr_it" },
            { "keyboard_button_backspace#German",   "keyboard_button_backspace_fr_gr_it" },
            { "keyboard_button_backspace#Italian",  "keyboard_button_backspace_fr_gr_it" }
        };
    }
}
