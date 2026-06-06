using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class MenuController : MonoBehaviour
    {
        private void OnGUI()
        {
            float w = Screen.width;
            float h = Screen.height;
            GUI.Label(new Rect(w * 0.35f, h * 0.20f, 300, 50), "PULLY");
            GUI.Label(new Rect(w * 0.30f, h * 0.28f, 300, 40), $"Best Score: {PlayerPrefs.GetInt("BestScore", 0)}");

            if (GUI.Button(new Rect(w * 0.35f, h * 0.40f, w * 0.3f, 60), "Play"))
            {
                SceneManager.LoadScene("GameScene");
            }

            GUI.Label(new Rect(w * 0.10f, h * 0.55f, w * 0.9f, 50), "Green circle=tap | Red circle=hold | Blue square=double | Yellow triangle=swipe | Purple star=2-finger");
        }
    }
}
