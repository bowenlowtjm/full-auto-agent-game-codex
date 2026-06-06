using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class MenuController : MonoBehaviour
    {
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _button;

        private void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 56,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.94f, 0.98f, 1f) }
            };
            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.75f, 0.87f, 1f) }
            };
            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            float w = Screen.width;
            float h = Screen.height;

            GUI.Label(new Rect(0, h * 0.12f, w, 70), "PULLY", _title);
            GUI.Label(new Rect(0, h * 0.22f, w, 40), $"Best Score: {PlayerPrefs.GetInt("BestScore", 0)}", _body);

            if (GUI.Button(new Rect(w * 0.28f, h * 0.38f, w * 0.44f, 78), "PLAY", _button))
            {
                SceneManager.LoadScene("GameScene");
            }

            GUI.Label(new Rect(0, h * 0.52f, w, 30), "Arcade rhythm + reaction inspired flow", _body);
            GUI.Label(new Rect(0, h * 0.58f, w, 30), "Green tap · Red hold · Blue double · Yellow swipe · Purple two-finger", _body);
            GUI.Label(new Rect(0, h * 0.64f, w, 30), "Editor: Ctrl=double, Shift=hold, Alt=swipe, Cmd=2-finger", _body);
        }
    }
}
