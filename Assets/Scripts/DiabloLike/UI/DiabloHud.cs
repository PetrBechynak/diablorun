using DiabloLike.Audio;
using DiabloLike.Combat;
using DiabloLike.Core;
using UnityEngine;

namespace DiabloLike.UI
{
    public sealed class DiabloHud : MonoBehaviour
    {
        private GameDirector director;
        private string chatInput = "";

        public void Configure(GameDirector gameDirector)
        {
            director = gameDirector;
        }

        private void OnGUI()
        {
            if (director == null)
            {
                return;
            }

            HandleHotkeys();
            var snapshot = director.CreateSnapshot();
            GUI.color = Color.white;
            GUI.Box(new Rect(16, 16, 430, 172), "");
            GUI.Label(new Rect(30, 28, 390, 24), $"Level {snapshot.PlayerLevel}   Enemies {snapshot.EnemiesAlive}");
            GUI.Label(new Rect(30, 58, 390, 44), director.QuestText);
            GUI.Label(new Rect(30, 102, 390, 26), director.ChatText);

            if (!director.IsPlayerDead)
            {
                GUI.SetNextControlName("chat");
                chatInput = GUI.TextField(new Rect(30, 134, 295, 28), chatInput);
                if (GUI.Button(new Rect(332, 134, 82, 28), "Ask"))
                {
                    director.AskChat(chatInput);
                    chatInput = "";
                }
            }

            GUI.Label(new Rect(18, Screen.height - 36, 620, 24), "WASD pohyb, mys miri, leve tlacitko utoci, Tab inventory.");
            DrawPlayerHealth(snapshot.PlayerHealth, snapshot.PlayerMaxHealth);
            DrawPlayerMana(director.PlayerMana, director.PlayerMaxMana);
            DrawInventory();
            DrawRewardChoice();
            DrawDeathScreen();
            GUI.color = Color.white;
        }

        private void HandleHotkeys()
        {
            var currentEvent = Event.current;
            if (currentEvent == null || currentEvent.type != EventType.KeyDown || currentEvent.keyCode != KeyCode.Tab)
            {
                return;
            }

            director.ToggleInventory();
            currentEvent.Use();
        }

        private static void DrawPlayerHealth(int current, int max)
        {
            var width = Mathf.Min(430f, Screen.width * 0.38f);
            var height = 34f;
            var x = 24f;
            var y = Screen.height - 92f;
            var value01 = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            DrawRect(new Rect(x - 4f, y - 4f, width + 8f, height + 8f), new Color(0.02f, 0.008f, 0.006f, 0.95f));
            DrawRect(new Rect(x, y, width, height), new Color(0.13f, 0.015f, 0.012f, 0.98f));
            DrawRect(new Rect(x, y, width * value01, height), new Color(0.72f, 0.02f, 0.015f, 0.98f));
            DrawRect(new Rect(x, y, width * value01, 5f), new Color(1f, 0.18f, 0.08f, 0.75f));

            GUI.color = Color.white;
            GUI.Label(new Rect(x + 14f, y + 7f, width - 28f, 24f), $"LIFE {current}/{max}");
        }

        private void DrawPlayerMana(int current, int max)
        {
            var width = Mathf.Min(340f, Screen.width * 0.3f);
            var height = 22f;
            var x = 24f;
            var y = Screen.height - 50f;
            var value01 = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;

            DrawRect(new Rect(x - 3f, y - 3f, width + 6f, height + 6f), new Color(0.01f, 0.012f, 0.035f, 0.95f));
            DrawRect(new Rect(x, y, width, height), new Color(0.025f, 0.035f, 0.12f, 0.98f));
            DrawRect(new Rect(x, y, width * value01, height), new Color(0.06f, 0.28f, 0.95f, 0.98f));

            GUI.color = Color.white;
            GUI.Label(new Rect(x + 12f, y + 2f, width - 24f, 20f), $"MANA {current}/{max}");
        }

        private void DrawInventory()
        {
            if (!director.IsInventoryOpen)
            {
                return;
            }

            var width = 340f;
            var height = 264f;
            var x = Screen.width - width - 24f;
            var y = Screen.height - height - 28f;
            DrawRect(new Rect(x, y, width, height), new Color(0.025f, 0.018f, 0.015f, 0.94f));
            DrawRect(new Rect(x + 10f, y + 10f, width - 20f, 2f), new Color(0.55f, 0.06f, 0.025f, 1f));

            GUI.color = Color.white;
            GUI.Label(new Rect(x + 18f, y + 22f, width - 36f, 24f), "Inventory");
            GUI.Label(new Rect(x + 18f, y + 50f, width - 36f, 24f), $"Equipped: {director.EquippedWeapon.Name}");
            GUI.Label(new Rect(x + 228f, y + 50f, 90f, 24f), $"Armor {director.PlayerArmor}");

            DrawWeaponSlot(new Rect(x + 18f, y + 84f, 206f, 66f), WeaponCatalog.Sword);
            DrawWeaponSlot(new Rect(x + 18f, y + 158f, 206f, 66f), director.HasWandUpgrade ? WeaponCatalog.EmberWand : WeaponCatalog.MagicWand);
            DrawArmorSlots(new Rect(x + 246f, y + 84f, 76f, 140f));

            GUI.color = Color.white;
            GUI.Label(new Rect(x + 18f, y + 232f, width - 36f, 22f), "Klikni na zbran pro equip. Tab zavre panel.");
        }

        private void DrawArmorSlots(Rect rect)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x, rect.y - 20f, rect.width, 20f), "Armor");
            DrawArmorSlot(new Rect(rect.x, rect.y, 64f, 30f), "Helm");
            DrawArmorSlot(new Rect(rect.x, rect.y + 36f, 64f, 30f), "Chest");
            DrawArmorSlot(new Rect(rect.x, rect.y + 72f, 64f, 30f), "Boots");
        }

        private static void DrawArmorSlot(Rect rect, string label)
        {
            DrawRect(rect, new Color(0.08f, 0.07f, 0.065f, 1f));
            DrawRect(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f), new Color(0.14f, 0.13f, 0.12f, 1f));
            GUI.color = new Color(0.78f, 0.74f, 0.68f, 1f);
            GUI.Label(new Rect(rect.x + 7f, rect.y + 6f, rect.width - 14f, rect.height - 8f), label);
        }

        private void DrawWeaponSlot(Rect rect, WeaponDefinition weapon)
        {
            var equipped = director.EquippedWeapon.Id == weapon.Id;
            DrawRect(rect, equipped ? new Color(0.18f, 0.06f, 0.025f, 1f) : new Color(0.08f, 0.055f, 0.04f, 1f));
            DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, rect.height - 8f), new Color(0.13f, 0.085f, 0.055f, 1f));

            var iconRect = new Rect(rect.x + 10f, rect.y + 8f, 50f, 50f);
            if (weapon.Id == WeaponId.MagicWand || weapon.Id == WeaponId.EmberWand)
            {
                DrawWandIcon(iconRect);
            }
            else
            {
                DrawSwordIcon(iconRect);
            }

            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 72f, rect.y + 9f, rect.width - 84f, 22f), equipped ? $"{weapon.Name}  EQUIPPED" : weapon.Name);
            GUI.Label(new Rect(rect.x + 72f, rect.y + 31f, rect.width - 84f, 28f), $"{weapon.Description} DMG {weapon.Damage}  Mana {weapon.ManaCost}");

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                DiabloAudio.Play(GameSfx.UiClick, 0.04f);
                director.EquipWeapon(weapon.Id);
            }
        }

        private void DrawWandIcon(Rect slot)
        {
            DrawRect(new Rect(slot.x + 12f, slot.y + 34f, 28f, 5f), new Color(0.48f, 0.28f, 0.12f, 1f));
            DrawRect(new Rect(slot.x + 29f, slot.y + 10f, 5f, 29f), new Color(0.6f, 0.36f, 0.16f, 1f));
            DrawRect(new Rect(slot.x + 23f, slot.y + 5f, 16f, 16f), new Color(0.1f, 0.42f, 1f, 1f));
            DrawRect(new Rect(slot.x + 27f, slot.y + 9f, 8f, 8f), new Color(0.75f, 0.92f, 1f, 1f));
        }

        private static void DrawSwordIcon(Rect slot)
        {
            DrawRect(new Rect(slot.x + 22f, slot.y + 7f, 6f, 34f), new Color(0.75f, 0.78f, 0.82f, 1f));
            DrawRect(new Rect(slot.x + 15f, slot.y + 35f, 20f, 5f), new Color(0.48f, 0.29f, 0.12f, 1f));
            DrawRect(new Rect(slot.x + 23f, slot.y + 39f, 4f, 9f), new Color(0.32f, 0.18f, 0.08f, 1f));
        }

        private void DrawDeathScreen()
        {
            if (!director.IsPlayerDead)
            {
                return;
            }

            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.02f, 0f, 0f, 0.72f));
            var width = 420f;
            var height = 180f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;

            DrawRect(new Rect(x, y, width, height), new Color(0.05f, 0.012f, 0.01f, 0.96f));
            DrawRect(new Rect(x + 20f, y + 18f, width - 40f, 2f), new Color(0.75f, 0.03f, 0.02f, 1f));
            GUI.color = Color.white;
            GUI.Label(new Rect(x + 34f, y + 42f, width - 68f, 34f), "YOU DIED");
            GUI.Label(new Rect(x + 34f, y + 78f, width - 68f, 28f), "Runovy kruh si vzal dalsi obet.");
            if (GUI.Button(new Rect(x + 135f, y + 120f, 150f, 34f), "Restart"))
            {
                director.RestartRun();
            }
        }

        private void DrawRewardChoice()
        {
            if (!director.IsRewardChoiceOpen)
            {
                return;
            }

            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.01f, 0.005f, 0.002f, 0.55f));
            var width = 620f;
            var height = 230f;
            var x = (Screen.width - width) * 0.5f;
            var y = (Screen.height - height) * 0.5f;
            DrawRect(new Rect(x, y, width, height), new Color(0.045f, 0.025f, 0.018f, 0.98f));
            DrawRect(new Rect(x + 18f, y + 16f, width - 36f, 2f), new Color(0.75f, 0.08f, 0.03f, 1f));

            GUI.color = Color.white;
            GUI.Label(new Rect(x + 24f, y + 34f, width - 48f, 28f), "Choose One Upgrade");

            for (var i = 0; i < director.CurrentRewardOptions.Count; i++)
            {
                var option = director.CurrentRewardOptions[i];
                DrawRewardButton(new Rect(x + 24f + i * 198f, y + 78f, 178f, 118f), option, i);
            }
        }

        private void DrawRewardButton(Rect rect, ChestRewardOption option, int rewardIndex)
        {
            DrawRect(rect, option.IsRare ? new Color(0.16f, 0.04f, 0.18f, 1f) : new Color(0.11f, 0.07f, 0.045f, 1f));
            DrawRect(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f), new Color(0.16f, 0.1f, 0.065f, 1f));
            GUI.color = option.TitleColor;
            GUI.Label(new Rect(rect.x + 14f, rect.y + 14f, rect.width - 28f, 24f), option.Title);
            GUI.color = option.IsRare ? new Color(1f, 0.65f, 1f, 1f) : Color.white;
            GUI.Label(new Rect(rect.x + 14f, rect.y + 44f, rect.width - 28f, 52f), option.Body);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                DiabloAudio.Play(GameSfx.UiClick, 0.04f);
                director.ChooseChestReward(rewardIndex);
            }
        }

        private static void DrawRect(Rect rect, Color color)
        {
            var oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = oldColor;
        }
    }
}
