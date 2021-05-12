using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using IPA.Utilities;
namespace GameSaber
{
    public class GameController : MonoBehaviour
    {
        //GameObjects
        StandardLevelFailedController levelFailController;
        BeatmapObjectCallbackController callbackController;
        BeatmapObjectSpawnController spawnController;
        BeatmapObjectSpawnMovementData spawnMovementData;
        NoteCutSoundEffectManager soundEffectManager;
        AudioTimeSyncController audioTimeSync;
        AudioSource songAudio;
        FlyingTextSpawner textSpawner;
        List<FlyingTextSpawner> commentTextSpawners = new List<FlyingTextSpawner>();
        public static Quaternion worldRotation = Quaternion.identity;
        public static Quaternion inverseWorldRotation = Quaternion.identity;


        //Game variables
        private int gameCount = 0;
        public static GameParams.DiffGameParams mapParams;
        public static IGame _game;
        private float gameStartTime = 0f;
        private bool startMessageSent = false;
        private bool playerChoseType = false;
        private float phaseThreshold = float.MaxValue;
        public static PlayerType playerType = PlayerType.First;
        public static bool IsPlayerTurn { get; private set; } = false;
        private List<int> _currentSpaces = new List<int>();

        private float beatTime = 0f;
        private float lastCommentTime = 0f;

        private bool _init = false;
        private void NewGame(bool initial)
        {
            startMessageSent = false;
            phaseThreshold = float.MaxValue;
            IsPlayerTurn = false;
            playerChoseType = false;
            _game =  Utilities.NewGameForType(mapParams.gameType);
            _game.playerTurnStart += PlayerTurn;
            _game.aiTurnStart += AiTurn;
            _game.gameHasEnded += GameEnd;
            if (initial)
                gameStartTime = mapParams.gameStart;
            else
                gameStartTime = songAudio.time + mapParams.gameTurnInterval + 2f;

            UpdateBeatmap(Utilities.GetPlayerSelectionBeatmapObjects(gameStartTime, mapParams.gameType, mapParams.gameEndTime));
            phaseThreshold = gameStartTime + 0.5f;
        }

        public void Awake()
        {
            try
            {
                LoadGameObjects();
                LoadTextSpawners();
                SetupGameLights();
                beatTime = 60f / BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap.level.beatsPerMinute;
            }
            catch (Exception ex)
            {
                Debug.Log($"Exception loading GameSaber {ex}");
                return;
            }
            if (mapParams != null)
                Initialize();

        }
        public void Initialize()
        {
            if (mapParams == null)
                return;
            _init = true;
            Plugin.Active = true;
            NewGame(true);
            BS_Utils.Utilities.BSEvents.noteWasCut += BSEvents_noteWasCut;
            BS_Utils.Utilities.BSEvents.noteWasMissed += BSEvents_noteWasMissed;
        }
        private void BSEvents_noteWasMissed(NoteData arg1, int arg2)
        {
            if (arg1 is GameNote)
                Logger.log.Debug($"GameNote Missed {(arg1 as GameNote).playerType}");
        }

        private void BSEvents_noteWasCut(NoteData arg1, NoteCutInfo arg2, int arg3)
        {

            if (arg1 is GameNote)
            {
                GameNote note = arg1 as GameNote;
                if (!playerChoseType && note.selectionNum == -1)
                {
                    StartGame(note.playerType == PlayerType.First);
                    DestroyGameNotes();
                }
                if (IsPlayerTurn && note.selectionNum != -1)
                {
                    EndPlayerTurn(note.selectionNum);
                    DestroyGameNotes();
                }
            }
        }
        public void DestroyGameNotes()
        {
            foreach (GameNoteController gameNoteController in UnityEngine.Object.FindObjectsOfType<GameNoteController>())
            {
                if (gameNoteController.noteData is GameNote)
                    gameNoteController.Dissolve(0f);
            }
        }
        public void PlayerTurn(List<int> spaces)
        {
            if (mapParams == null || _game == null) return;
            _currentSpaces = spaces;
            SpawnText("<#00FFFF>Your Turn</color>\n Current Board\n" + _game.GetGameText(), _game.TurnLength);
            //Update Beatmap with selection objects
            float time = songAudio.time + spawnMovementData.spawnAheadTime + mapParams.gameTurnInterval;
            UpdateBeatmap(Utilities.GetTurnSelectionBeatmapObjects(time, spaces, playerType, mapParams.gameType));
            phaseThreshold = time + 0.5f;
            //Update phase threshold
            IsPlayerTurn = true;
        }
        public void EndPlayerTurn(int selection)
        {
            IsPlayerTurn = false;
            _game.PlayerTurn(selection);
        }
        public void AiTurn(List<int> spaces)
        {
            if (mapParams == null || _game == null) return;
            _currentSpaces = spaces;
            SpawnText("<#FF0000>Ai Turn</color>\n Current Board\n" + _game.GetGameText(), _game.TurnLength);
        }
        public void GameEnd(bool win, bool tie)
        {
            gameCount++;
            if (win)
            {
                SpawnText("You Win!\n" + _game.GetGameText(), _game.TurnLength);
                NewGame(false);
            }
            else
            {
                if (!tie)
                {
                    SpawnText("Game Over\n" + _game.GetGameText(), _game.TurnLength);
                    GameOver();
                }
                else
                {
                    SpawnText("Tie Game\n" + _game.GetGameText(), _game.TurnLength);
                    NewGame(false);
                }
            }


        }
        public void Update()
        {
            if (!_init) return;
            if (_game == null) return;
            if (!_game.Started)
            {
                if (songAudio.time > lastCommentTime + 2.25f + beatTime * 2f)
                {
                    SpawnIntroText(2.25f);
                    lastCommentTime = songAudio.time;
                }
                if (!startMessageSent && songAudio.time >= gameStartTime - 3f)
                {
                    SpawnText(_game.GetStartText(), 3f);
                    startMessageSent = true;
                }
                if (!playerChoseType && songAudio.time >= phaseThreshold)
                {
                    StartGame(true);
                }
                //Debug game start
                if (Input.GetKeyDown(KeyCode.X))
                    StartGame(true);
                else if (Input.GetKeyDown(KeyCode.O))
                    StartGame(false);
            }
            else
            {
                if (songAudio.time > lastCommentTime + 2.25f + beatTime * 2f)
                {
                    SpawnGameText(2.25f);
                    lastCommentTime = songAudio.time;
                }

                if (Input.GetKeyDown(KeyCode.K) && IsPlayerTurn)
                    EndPlayerTurn(_currentSpaces[UnityEngine.Random.Range(0, _currentSpaces.Count)]);

                if (IsPlayerTurn && songAudio.time > phaseThreshold)
                {
                    EndPlayerTurn(_currentSpaces[UnityEngine.Random.Range(0, _currentSpaces.Count)]);
                }
                if (songAudio.time > mapParams.gameEndTime)
                {
                    //  NewGame(true);
                    _game = null;
                    _init = false;
                    mapParams = null;
                    CleanBeatmap();
                    SpawnText("Well Done!", 5f);
                }

            }
        }

        public async void GameOver()
        {
            _init = false;
            await Task.Delay((int)(_game.TurnLength / 3 * 1000));
            levelFailController.HandleLevelFailed();
        }
        string lastComment = "";
        public void SpawnIntroText(float duration = 2f)
        {
            string text = "";
            int index = _game.rand.Next(Utilities.IntroTexts.Count);
            text = Utilities.IntroTexts[index];
            if (text == lastComment)
            {
                index = index + 1 == Utilities.IntroTexts.Count ? 0 : index + 1;
                text = Utilities.IntroTexts[index];
            }
            lastComment = text;
            var spawner = commentTextSpawners[_game.rand.Next(2)];
            Vector3 pos = new Vector3(spawner.gameObject.transform.localPosition.x * 1.5f, spawner.gameObject.transform.localPosition.y + 3f, 0);
            float initialDuration = spawner.GetField<float, FlyingTextSpawner>("_duration");
            spawner.SetField("_duration", duration);
            spawner.SpawnText(pos, worldRotation, inverseWorldRotation, text);
            spawner.SetField("_duration", initialDuration);
        }
        public void SpawnGameText(float duration = 2f)
        {
            string text = "";
            //   while (text == lastComment)
            int index = _game.rand.Next(Utilities.GameTexts.Count);
            text = Utilities.GameTexts[index];
            if (text == lastComment)
            {
                index = index + 1 == Utilities.GameTexts.Count ? 0 : index + 1;
                text = Utilities.GameTexts[index];
            }

            lastComment = text;
            text = text.Replace("$time", (songAudio.clip.length - songAudio.time).ToString());
            text = text.Replace("$gameCount", gameCount.ToString());

            var spawner = commentTextSpawners[_game.rand.Next(2)];
            Vector3 pos = new Vector3(spawner.gameObject.transform.localPosition.x * 1.5f, spawner.gameObject.transform.localPosition.y + 3f, 0);
            float initialDuration = spawner.GetField<float, FlyingTextSpawner>("_duration");
            spawner.SetField("_duration", duration);
            spawner.SpawnText(pos, worldRotation, inverseWorldRotation, text);
            spawner.SetField("_duration", initialDuration);
        }

        public async void StartGame(bool playerIsX)
        {
            playerChoseType = true;
            playerType = playerIsX ? PlayerType.First : PlayerType.Second;
            if (playerIsX)
            {
                SpawnText("You Are First", 2f);
            }
            else
            {
                SpawnText("You Are Second", 2f);
            }
            await Task.Delay(2000);
            _game.Start(playerIsX, mapParams.gameTurnInterval);
        }

        public void UpdateBeatmap(List<BeatmapObjectData> newObjects)
        {
            BeatmapData beatmapData = callbackController.GetField<IReadonlyBeatmapData, BeatmapObjectCallbackController>("_beatmapData") as BeatmapData;
            List<BeatmapObjectData> objects;
            BeatmapLineData[] linesData = beatmapData.GetField<BeatmapLineData[], BeatmapData>("_beatmapLinesData");
            objects = linesData[0].beatmapObjectsData.ToList();
            objects.AddRange(newObjects);
            objects = objects.OrderBy(o => o.time).ToList();
            linesData[0].SetField<BeatmapLineData, List<BeatmapObjectData>>("_beatmapObjectsData", objects);
            beatmapData.SetField<BeatmapData, BeatmapLineData[]>("_beatmapLinesData", linesData);
        }

        public void CleanBeatmap()
        {
            BeatmapData beatmapData = callbackController.GetField<IReadonlyBeatmapData, BeatmapObjectCallbackController>("_beatmapData") as BeatmapData;
            List<BeatmapObjectData> objects;
            BeatmapLineData[] linesData = beatmapData.GetField<BeatmapLineData[], BeatmapData>("_beatmapLinesData");
            objects = linesData[0].beatmapObjectsData.ToList();
            objects.RemoveAll(x => x is GameNote || x is GameObstacle);
            objects = objects.OrderBy(o => o.time).ToList();
            linesData[0].SetField<BeatmapLineData, List<BeatmapObjectData>>("_beatmapObjectsData", objects);
            beatmapData.SetField<BeatmapData, BeatmapLineData[]>("_beatmapLinesData", linesData);
        }
        public void SpawnText(string text, float duration)
        {
            if (!textSpawner) return;
            Vector3 position = new Vector3(0, 10, 0);
            float initialDuration = textSpawner.GetField<float, FlyingTextSpawner>("_duration");
            textSpawner.SetField("_duration", duration);
            textSpawner.SpawnText(position, worldRotation, inverseWorldRotation, text);
            textSpawner.SetField("_duration", initialDuration);
        }

        private void LoadGameObjects()
        {
            levelFailController = Resources.FindObjectsOfTypeAll<StandardLevelFailedController>().LastOrDefault();
            callbackController = Resources.FindObjectsOfTypeAll<BeatmapObjectCallbackController>().LastOrDefault();
            spawnController = Resources.FindObjectsOfTypeAll<BeatmapObjectSpawnController>().LastOrDefault();
            spawnMovementData = spawnController.GetField<BeatmapObjectSpawnMovementData, BeatmapObjectSpawnController>("_beatmapObjectSpawnMovementData");
            soundEffectManager = Resources.FindObjectsOfTypeAll<NoteCutSoundEffectManager>().LastOrDefault();
            audioTimeSync = Resources.FindObjectsOfTypeAll<AudioTimeSyncController>().LastOrDefault();
            songAudio = audioTimeSync.GetField<AudioSource, AudioTimeSyncController>("_audioSource");
        }

        private void SetupGameLights()
        {
            LightSwitchEventEffect[] lights = Resources.FindObjectsOfTypeAll<LightSwitchEventEffect>();
            foreach (var light in lights)
            {
                Logger.log.Info("Setting light");
                GameColorSO lightC0 = ScriptableObject.CreateInstance<GameColorSO>();
                GameColorSO lightC1 = ScriptableObject.CreateInstance<GameColorSO>();
                GameColorSO lightH0 = ScriptableObject.CreateInstance<GameColorSO>();
                GameColorSO lightH1 = ScriptableObject.CreateInstance<GameColorSO>();
                lightC0.Init(light.GetField<ColorSO, LightSwitchEventEffect>("_lightColor0"),
                    light.GetField<ColorSO, LightSwitchEventEffect>("_lightColor1"), ColorType.ColorA);
                lightC1.Init(light.GetField<ColorSO, LightSwitchEventEffect>("_lightColor0"),
                    light.GetField<ColorSO, LightSwitchEventEffect>("_lightColor1"), ColorType.ColorB);
                lightH0.Init(light.GetField<ColorSO, LightSwitchEventEffect>("_highlightColor0"),
                    light.GetField<ColorSO, LightSwitchEventEffect>("_highlightColor1"), ColorType.ColorA);
                lightH1.Init(light.GetField<ColorSO, LightSwitchEventEffect>("_highlightColor0"),
                    light.GetField<ColorSO, LightSwitchEventEffect>("_highlightColor1"), ColorType.ColorB);

                light.SetField<LightSwitchEventEffect, ColorSO>("_lightColor0", lightC0);
                light.SetField<LightSwitchEventEffect, ColorSO>("_lightColor1", lightC1);
                light.SetField<LightSwitchEventEffect, ColorSO>("_highlightColor0", lightH0);
                light.SetField<LightSwitchEventEffect, ColorSO>("_highlightColor1", lightH1);
            }
        }

        private void LoadTextSpawners()
        {
            var installer = Resources.FindObjectsOfTypeAll<EffectPoolsManualInstaller>().LastOrDefault();
            if (installer != null)
            {
                var containers = Resources.FindObjectsOfTypeAll<MonoInstallerBase>();
                foreach (MonoInstallerBase a in containers)
                {
                    try
                    {
                        if (textSpawner == null)
                        {

                            //Get Main Text Spawner
                            var container = a.GetProperty<DiContainer, MonoInstallerBase>("Container");
                            if (container == null) continue;
                            if(!container.HasBinding(typeof(FlyingTextEffect.Pool))) continue;
                            textSpawner = container.InstantiateComponentOnNewGameObject<FlyingTextSpawner>("GameSaber GameText Spawner");
                            textSpawner.SetField("_targetYPos", 3.0f);
                            //Get Comment Text Spawners
                            var commentSpawnerL = container.InstantiateComponentOnNewGameObject<FlyingTextSpawner>("GameSaber CommentSpawnerL");
                            commentSpawnerL.gameObject.transform.localPosition += new Vector3(-2f, 0, 0);
                            commentSpawnerL.SetField("_shake", true);
                            commentSpawnerL.SetField("_xSpread", 1.5f);
                            commentSpawnerL.SetField("_targetZPos", 6.8f);
                            commentSpawnerL.SetField("_targetYPos", 3.0f);
                            var commentSpawnerR = container.InstantiateComponentOnNewGameObject<FlyingTextSpawner>("GameSaber CommentSpawnerR");
                            commentSpawnerR.gameObject.transform.localPosition += new Vector3(2f, 0, 0);
                            commentSpawnerR.SetField("_shake", true);
                            commentSpawnerR.SetField("_xSpread", 1.5f);
                            commentSpawnerR.SetField("_targetZPos", 6.8f);
                            commentSpawnerR.SetField("_targetYPos", 3.0f);
                            commentTextSpawners.Add(commentSpawnerL);
                            commentTextSpawners.Add(commentSpawnerR);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.log.Debug($"Exception creating textSpawners {ex}");
                    }

                }
            }
        }
    }
}
