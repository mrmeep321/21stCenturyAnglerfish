using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using Harmony;

namespace AnglerMain
{
    public sealed class AnglerMain : ModBehaviour
    {
        public static AnglerMain inst;

        public AnglerMain() : base()
        {
            inst = this;
        }

        public static AudioSource src;

        private void Awake()
        {}

        private void Start()
        {
            inst.ModHelper.Console.WriteLine($"My mod {nameof(AnglerMain)} is loaded!", MessageType.Success);

            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishController>("MoveTowardsTarget", typeof(AnglerMain), "move");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishAudioController>("OnChangeAnglerState", typeof(AnglerMain), "Sound");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishAudioController>("UpdateLoopingAudio", typeof(AnglerMain), "looper");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishController>("RotateTowardsTarget", typeof(AnglerMain), "Angle");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishAnimController>("OnChangeAnglerState", typeof(AnglerMain), "mover");

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                var playerBody = FindObjectOfType<PlayerBody>();
                GameObject gameObject = FindObjectOfType<PlayerBody>().gameObject;
                src = gameObject.AddComponent<AudioSource>();
                src.clip = inst.ModHelper.Assets.LoadAudio("Assets/boom.wav").Asset.clip;
                src.volume = 0.2f;
            };
        }

        public static bool Angle(AnglerfishController __instance, ref Vector3 targetPos, ref float normalTurnSpeed, ref float inPlaceTurnSpeed)
        {
            __instance._anglerBody.transform.LookAt(targetPos);
            return false;
        }

        public static bool Sound(AnglerfishAudioController __instance, ref AnglerfishController.AnglerState anglerState)
        {
            __instance.UpdateLoopingAudio(anglerState);
            if (anglerState == AnglerfishController.AnglerState.Chasing)
            {
                if (Time.time > AnglerfishAudioController.s_lastDetectTime + 2f)
                {
                    AnglerfishAudioController.s_lastDetectTime = Time.time;
                    //__instance._oneShotSource.PlayOneShot(global::AudioType.DBAnglerfishDetectTarget, 1f);

                    inst.ModHelper.Console.WriteLine("Vine Boom played");
                    src.PlayOneShot(inst.ModHelper.Assets.LoadAudio("boom.wav").Asset.clip);
                    return false;
                }
                MonoBehaviour.print("ANGLER DETECT TARGET SOUND BLOCKED");
            }
            return false;
        }

        private static bool move(AnglerfishController __instance, ref Vector3 targetPos, ref float moveSpeed, ref float maxAcceleration)
        {
            return false;
        }

        private static bool looper(AnglerfishAudioController __instance, ref AnglerfishController.AnglerState anglerState)
        {
            switch (anglerState)
            {
                case AnglerfishController.AnglerState.Lurking:
                    __instance._loopSource.AssignAudioLibraryClip(global::AudioType.DBAnglerfishLurking_LP);
                    __instance._loopSource.FadeIn(0.5f, true, false, 1f);
                    return false;
                case AnglerfishController.AnglerState.Chasing:
                    __instance._loopSource.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
                    return false;
            }
            __instance._loopSource.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.STOP, 0f);
            return false;
        }

        private static bool mover(AnglerfishAnimController __instance, AnglerfishController.AnglerState newState)
        {
            if (newState == AnglerfishController.AnglerState.Chasing)
            {
                __instance._moveTarget = 0f;
            }
            else if (newState == AnglerfishController.AnglerState.Investigating)
            {
                __instance._moveTarget = 0.5f;
            }
            else
            {
                __instance._moveTarget = 0f;
            }
            if (newState == AnglerfishController.AnglerState.Chasing)
            {
                __instance._jawTarget = 0f;
            }
            else if (newState == AnglerfishController.AnglerState.Consuming)
            {
                __instance._jawTarget = 0f;
            }
            else
            {
                __instance._jawTarget = 0.33f;
            }
            if (newState == AnglerfishController.AnglerState.Chasing)
            {
                __instance._spinesTarget = 0f;
            }
            else
            {
                __instance._spinesTarget = 0f;
            }
            __instance._moveStart = __instance._moveCurrent;
            __instance._jawStart = __instance._jawCurrent;
            for (int i = 0; i < __instance._spines.Length; i++)
            {
                __instance._spinesStart[i] = __instance._spinesCurrent[i];
            }
            __instance._lastStateChangeTime = Time.time;
            return false;
        }
    }
}
