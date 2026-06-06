using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class GameOverController : MonoBehaviour
    {
        private GUIStyle _title;
        private GUIStyle _body;
        private GUIStyle _button;
        private GUIStyle _ribbon;

        private void EnsureStyles()
        {
            if (_title != null) return;
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 52,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.9f, 0.92f) }
            };
            _body = new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            _ribbon = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.92f, 0.4f) }
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            float w = Screen.width;
            float h = Screen.height;
            int score = PlayerPrefs.GetInt("CurrentScore", 0);
            int best = PlayerPrefs.GetInt("BestScore", 0);
            bool isBest = score >= best && score > 0;

            GUI.Label(new Rect(0, h * 0.16f, w, 60), "GAME OVER", _title);
            GUI.Label(new Rect(0, h * 0.30f, w, 44), $"Score: {score}", _body);
            GUI.Label(new Rect(0, h * 0.36f, w, 44), $"Best: {best}", _body);
            if (isBest)
            {
                GUI.Label(new Rect(0, h * 0.43f, w, 38), "NEW BEST!", _ribbon);
            }

            if (GUI.Button(new Rect(w * 0.3f, h * 0.52f, w * 0.4f, 64), "RETRY", _button))
            {
                SceneManager.LoadScene("GameScene");
            }
            if (GUI.Button(new Rect(w * 0.3f, h * 0.62f, w * 0.4f, 64), "MENU", _button))
            {
                SceneManager.LoadScene("MenuScene");
            }
        }
    }
}
