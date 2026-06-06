using UnityEngine;

namespace Pully.Game
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private RulesetDefinition ruleset;

        private void Awake()
        {
            if (Camera.main == null)
            {
                var camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 5;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f, 1f);
                camGo.tag = "MainCamera";
                camGo.transform.position = new Vector3(0f, 0f, -10f);
            }

            if (ruleset == null)
            {
                ruleset = RulesetFactory.CreateDefault();
            }

            var root = new GameObject("GameSystems");
            var gm = root.AddComponent<GameManager>();
            var spawner = root.AddComponent<TargetSpawner>();
            var recognizer = root.AddComponent<GestureRecognizer>();
            var container = new GameObject("TargetContainer").transform;

            gm.Initialize(ruleset);
            spawner.Initialize(ruleset, Camera.main, container);
            gm.BindSpawner(spawner);
            recognizer.Initialize(ruleset, spawner, gm);
        }
    }
}
