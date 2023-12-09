using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using IPALogger = IPA.Logging.Logger;

namespace GameSaber
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static string Name => "GameSaber";
        public static bool Active = false;
        [Init]
        public void Init(IPALogger logger)
        {
            Logger.log = logger;
            
            BS_Utils.Utilities.BSEvents.gameSceneLoaded += BSEvents_gameSceneLoaded;
        }
        [OnStart]
        public void OnApplicationStart()
        {
            SongCore.Collections.RegisterCapability("GameSaber");
            var harmony = new HarmonyLib.Harmony("GameSaber");
            harmony.PatchAll();

        }

        private void BSEvents_gameSceneLoaded()
        {
            Active = false;
            GameController.worldRotation = Quaternion.identity;
            GameController.inverseWorldRotation = Quaternion.identity;

            if (!BS_Utils.Plugin.LevelData.IsSet) return;
            if (!(BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.level is CustomPreviewBeatmapLevel)) return;
            if (BS_Utils.Plugin.LevelData.Mode != BS_Utils.Gameplay.Mode.Standard) return;
            
            var level = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.level as CustomPreviewBeatmapLevel;
            var songData = SongCore.Collections.RetrieveDifficultyData(BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap);
            if (songData == null) return;
            
            bool ticTacMap = songData.additionalDifficultyData._requirements.Contains("GameSaber");
          //  if (!ticTacMap) return;
            string path = Path.Combine(level.customLevelPath, "GameParams.json");
            if (!File.Exists(path) && ticTacMap)
            {
                BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Invalid GameSaber Params");
                return;
            }

            GameParams mapParams = null;
            GameParams.DiffGameParams diffParams = null;
            if(ticTacMap)
            {
                try
                {
                    mapParams = Newtonsoft.Json.JsonConvert.DeserializeObject<GameParams>(File.ReadAllText(path));
                    diffParams = mapParams.games.FirstOrDefault(x => x.beatmapCharacteristicName == songData._beatmapCharacteristicName && x.beatmapDifficultyName == songData._difficulty.SerializedName());
                    if (diffParams == null || diffParams.gameType == GameType.None)
                    {
                        Logger.log.Error("Invalid GameParams, not initializing GameSaber");
                        Logger.log.Error($"1 {songData._beatmapCharacteristicName} | {songData._difficulty.SerializedName()}");
                        BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Invalid GameSaber Params");
                        return;
                    }
                    if (diffParams.gameType == GameType.ConnectFour && !songData.additionalDifficultyData._requirements.Contains("Mapping Extensions"))
                    {
                        Logger.log.Error("Invalid Difficulty requirements, ConnectFour requires Mapping Extensions, not initializing GameSaber");
                        Logger.log.Error($"in {songData._beatmapCharacteristicName} | {songData._difficulty.SerializedName()} Difficulty");
                        BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Invalid GameSaber Params");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.log.Error("Invalid GameParams, not initializing GameSaber");
                    Logger.log.Error($"2 {ex}");
                    BS_Utils.Gameplay.ScoreSubmission.DisableSubmission("Invalid GameSaber Params");
                    return;
                }
            }
         
            SharedCoroutineStarter.instance.StartCoroutine(DelayInit(diffParams));

        }
        
        private IEnumerator DelayInit(GameParams.DiffGameParams mapParams)
        {
            yield return new WaitForSeconds(0.1f);
            GameController.mapParams = mapParams;
            new GameObject("GameSaber Controller").AddComponent<GameController>();
        }
        [OnExit]
        public void OnApplicationQuit()
        {


        }

    }
}
