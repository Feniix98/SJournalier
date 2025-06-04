using System.IO;
using Life.Network;
using Life;
using Newtonsoft.Json;
using Life.DB;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Linq;
using Mirror;
using Life.UI;
using Life.InventorySystem;
using UnityEngine;

namespace SJournalier
{
    public class SJournalier : Plugin
    {

        public SJournalier(IGameAPI api) : base(api)
        {
        }

        public JournalierConfig config;

        public void InitJournalierConfig()
        {
            string Path = pluginsPath + "/SJournalier";
            string configPath = Path + "/config.json";

            if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

            if (!File.Exists(configPath))
            {
                JournalierConfig n = new JournalierConfig
                {
                    Money = 700,
                    CooldownHours = 24,
                };
                File.WriteAllText(configPath, JsonConvert.SerializeObject(n, Formatting.Indented));
            }

            config = JsonConvert.DeserializeObject<JournalierConfig>(File.ReadAllText(configPath));
        }

        public class JournalierConfig
        {
            public double Money { get; set; }
            public int CooldownHours { get; set; }
        }

        public override async void OnPluginInit()
        {
            base.OnPluginInit();
            InitJournalierConfig();
            await LifeDB.db.CreateTableAsync<Journalier>();

            new SChatCommand("/journalierconfig", new string[] { "/jc" }, "Permet de configurer SJournalier", "/journalierconfig", (player, args) =>
            {
                if (!player.IsAdmin) return;
                UIPanel panel = new UIPanel(nameof(SJournalier) + " " + "-" + " " + "Config", UIPanel.PanelType.Tab);

                panel.AddTabLine("Argent :", config.Money.ToString() + "€", -1, delegate
                {
                    SJournalierMoneyConfig(player);
                });
                panel.AddTabLine("Cooldown :", config.CooldownHours.ToString(), -1, delegate
                {
                    SJournalierCooldownConfig(player);
                });

                panel.AddButton("Fermer", ui => player.ClosePanel(panel));
                panel.AddButton("Configurer", ui => panel.SelectTab());

                player.ShowPanelUI(panel);
            }).Register();
        }

        public void SJournalierMoneyConfig(Player player)
        {
            UIPanel panel = new UIPanel(nameof(SJournalier) + " " + "-" + " " + "Argent", UIPanel.PanelType.Input);

            panel.SetText("Veuillez choisir le nouveau montant.");
            panel.SetInputPlaceholder("Montant...");

            panel.AddButton("Fermer", ui => player.ClosePanel(panel));
            panel.AddButton("Sauvegarder", ui =>
            {
                string str = panel.inputText;
                if (double.TryParse(str, out double newMoney) && newMoney >= 0)
                {
                    config.Money = newMoney;
                    SaveConfig();
                    player.Notify("SJournalier", $"Montant défini à {newMoney}€", NotificationManager.Type.Success);
                }
                else
                {
                    player.Notify("SJournalier", "Entrée invalide. Veuillez entrer un montant valide (ex: 750)", NotificationManager.Type.Error);
                }
                player.ClosePanel(panel);
            });

            player.ShowPanelUI(panel);
        }

        public void SJournalierCooldownConfig(Player player)
        {
            UIPanel panel = new UIPanel(nameof(SJournalier) + " " + "-" + " " + "Cooldown", UIPanel.PanelType.Input);

            panel.SetText("Nouveau cooldown en heures...");
            panel.SetInputPlaceholder("Cooldown...");

            panel.AddButton("Annuler", ui => player.ClosePanel(panel));
            panel.AddButton("Valider", ui =>
            {
                string str = panel.inputText;

                if (int.TryParse(str, out int newCooldown) && newCooldown > 0)
                {
                    config.CooldownHours = newCooldown;
                    SaveConfig();
                    player.Notify("SJournalier", $"Cooldown défini à {newCooldown}h", NotificationManager.Type.Success);
                }
                else
                {
                    player.Notify("SJournalier", "Entrée invalide. Veuillez entrer un nombre d'heures valide.", NotificationManager.Type.Error);
                }

                player.ClosePanel(panel);
            });

            player.ShowPanelUI(panel);
        }

        public override async void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
        {
            base.OnPlayerSpawnCharacter(player, conn, character);
            if (await CreateOrUpdateCooldown(player))
            {
                UIPanel panel = new UIPanel(nameof(SJournalier), UIPanel.PanelType.Text);

                panel.SetText("Votre journalier est disponible ! Il est d'une valeur de" + " " + config.Money.ToString() + "€");

                panel.AddButton("Fermer", _ => player.ClosePanel(panel));
                panel.AddButton("Récuperer", delegate
                {
                    player.AddMoney(config.Money, "SJOURNALIER");
                    player.Notify(nameof(SJournalier), "Journalier récuperer !", NotificationManager.Type.Success);
                    player.ClosePanel(panel);
                });

                player.ShowPanelUI(panel);
            }
        }

        public void SaveConfig() => File.WriteAllText(pluginsPath + "/SJournalier/config.json", JsonConvert.SerializeObject(config, Formatting.Indented));

        async Task<bool> CreateOrUpdateCooldown(Player player)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long cooldown = config.CooldownHours * 3600;
            List<Journalier> list = await Journalier.GetJournalier(j => j.PlayerId == player.account.id);
            Journalier journalier = list.FirstOrDefault();

            if (journalier != null)
            {
                long lastClaim = now - journalier.LastUse;
                if (lastClaim > cooldown)
                {
                    journalier.LastUse = now;
                    await journalier.Write();
                    return true;
                }
                else
                {
                    long remainingTime = cooldown - lastClaim;
                    TimeSpan timeLeft = TimeSpan.FromSeconds(remainingTime);
                    player.Notify(nameof(Journalier), $"Vous devez encore attendre {timeLeft.Hours}h {timeLeft.Minutes}m {timeLeft.Seconds}s avant de récuperer un autre journalier !", NotificationManager.Type.Warning);
                    return false;
                }
            }
            else
            {
                await new Journalier
                {
                    PlayerId = player.account.id,
                    LastUse = now,
                }.Write();
                return true;
            }
        }
    }
}
