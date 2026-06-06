using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class MenuController : MonoBehaviour
    {
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _button;
        private GUIStyle _subtle;

        private void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 58,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.94f, 0.98f, 1f) }
            };
            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 21,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.75f, 0.87f, 1f) }
            };
            _subtle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.68f, 0.74f, 0.85f) }
            };
            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            float w = Screen.width;
            float h = Screen.height;

            GUI.Label(new Rect(0, h * 0.10f, w, 70), "PULLY", _title);
            GUI.Label(new Rect(0, h * 0.20f, w, 40), "Fast gesture arcade", _body);
            GUI.Label(new Rect(0, h * 0.25f, w, 34), $"Best Score: {PlayerPrefs.GetInt("BestScore", 0)}", _body);

            if (GUI.Button(new Rect(w * 0.26f, h * 0.37f, w * 0.48f, 82), "PLAY", _button))
            {
                SceneManager.LoadScene("GameScene");
            }

            GUI.Label(new Rect(0, h * 0.52f, w, 32), "How to score:", _body);
            GUI.Label(new Rect(0, h * 0.58f, w, 30), "Green=Tap   Red=Hold   Blue=Double", _body);
            GUI.Label(new Rect(0, h * 0.63f, w, 30), "Yellow=Swipe   Purple=Two-Finger", _body);
            GUI.Label(new Rect(0, h * 0.70f, w, 26), "Editor: Ctrl=Double | Shift=Hold | Alt=Swipe | Cmd=2-Finger", _subtle);
        }
    }
}
