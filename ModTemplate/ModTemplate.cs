using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using Harmony;

namespace ModTemplate
{
    public sealed class ModTemplate : ModBehaviour
    {
        public static ModTemplate inst;

        public ModTemplate() : base()
        {
            inst = this;
        }

        /*
         * 
         */

        public static AudioSource src;

        //public static AudioClip clip = behave.ModHelper.Assets.LoadAudio("boom.wav").Asset.clip;

        //public static ModBehaviour behave = ModTemplate.

        //public static ModTemplate superInst = new ModTemplate();

        private void Awake()
        {
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        private void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            inst.ModHelper.Console.WriteLine($"My mod {nameof(ModTemplate)} is loaded!", MessageType.Success);

            inst.ModHelper.Events.Subscribe<Flashlight>(Events.AfterStart);
            inst.ModHelper.Events.OnEvent += OnEvent;

            inst.ModHelper.Console.WriteLine("2", MessageType.Success);

            inst.ModHelper.Console.WriteLine("3", MessageType.Success);

            inst.ModHelper.HarmonyHelper.AddPrefix<BlackHoleVolume>("Awake", typeof(ModTemplate), "blackHolePatch");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishController>("MoveTowardsTarget", typeof(ModTemplate), "move");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishAudioController>("OnChangeAnglerState", typeof(ModTemplate), "Sound");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishAudioController>("UpdateLoopingAudio", typeof(ModTemplate), "looper");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishController>("RotateTowardsTarget", typeof(ModTemplate), "Angle");
            inst.ModHelper.HarmonyHelper.AddPrefix<AnglerfishAnimController>("OnChangeAnglerState", typeof(ModTemplate), "mover");
            inst.ModHelper.Console.WriteLine("haha funny mod");

            // Example of accessing game code.
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene != OWScene.SolarSystem) return;
                var playerBody = FindObjectOfType<PlayerBody>();
                base.ModHelper.Console.WriteLine($"Found player body, and it's called {playerBody.name}!", MessageType.Success);

                GameObject gameObject = FindObjectOfType<PlayerBody>().gameObject;
                src = gameObject.AddComponent<AudioSource>();
                src.clip = inst.ModHelper.Assets.LoadAudio("boom.wav").Asset.clip;
                src.volume = 0.2f;
            };
        }

        private void OnEvent(MonoBehaviour behavior, Events ev)
        {
            base.ModHelper.Console.WriteLine("behavior name: " + behavior.name);

            if(behavior.GetType() == typeof(Flashlight) && ev == Events.AfterStart)
            {
                base.ModHelper.Console.WriteLine("Flashlight has started!");
            }
        }

        public static bool blackHolePatch(BlackHoleVolume __instance)
        {
            inst.ModHelper.Console.WriteLine("Detected and deleted black hole");
            UnityEngine.Object.Destroy(__instance.gameObject);
            return false;
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
