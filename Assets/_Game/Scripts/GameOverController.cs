using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class GameOverController : MonoBehaviour
    {
        private void OnGUI()
        {
            float w = Screen.width;
            float h = Screen.height;
            int score = PlayerPrefs.GetInt("CurrentScore", 0);
            int best = PlayerPrefs.GetInt("BestScore", 0);

            GUI.Label(new Rect(w * 0.35f, h * 0.2f, 300, 40), "GAME OVER");
            GUI.Label(new Rect(w * 0.35f, h * 0.28f, 300, 40), $"Score: {score}");
            GUI.Label(new Rect(w * 0.35f, h * 0.34f, 300, 40), $"Best: {best}");

            if (GUI.Button(new Rect(w * 0.35f, h * 0.45f, w * 0.3f, 60), "Retry"))
            {
                SceneManager.LoadScene("GameScene");
            }
            if (GUI.Button(new Rect(w * 0.35f, h * 0.56f, w * 0.3f, 60), "Menu"))
            {
                SceneManager.LoadScene("MenuScene");
            }
        }
    }
}
