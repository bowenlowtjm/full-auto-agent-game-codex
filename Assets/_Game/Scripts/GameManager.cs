using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private RulesetDefinition ruleset;

        private float _timeLeft;
        private int _score;
        private int _lives;
        private float _combo = 1f;

        public int Score => _score;
        public int Lives => _lives;
        public float ComboMultiplier => _combo;
        public float TimeLeft => _timeLeft;

        public void Initialize(RulesetDefinition rs)
        {
            ruleset = rs;
            _timeLeft = ruleset.roundSeconds;
            _score = 0;
            _lives = ruleset.lives;
            _combo = 1f;
        }

        private void Update()
        {
            if (ruleset == null) return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                EndGame();
            }
        }

        public void OnGesturePerformed(Target target, RulesetDefinition.Gesture gesture)
        {
            if (target == null || target.Resolved) return;
            if (target.Rule.requiredGesture == gesture)
            {
                _score += ScoreCalculator.ScoreFor(target.Rule.baseReward, _combo);
                _combo = ScoreCalculator.NextCombo(_combo, ruleset.comboStep, ruleset.comboCap);
                target.Resolve();
            }
            else
            {
                Penalize();
                target.Resolve();
            }
        }

        public void OnTargetExpired(Target _)
        {
            Penalize();
        }

        private void Penalize()
        {
            _combo = ScoreCalculator.ResetCombo();
            _lives -= 1;
            if (_lives <= 0)
            {
                _lives = 0;
                EndGame();
            }
        }

        private void EndGame()
        {
            PlayerPrefs.SetInt("CurrentScore", _score);
            int best = PlayerPrefs.GetInt("BestScore", 0);
            if (_score > best)
            {
                PlayerPrefs.SetInt("BestScore", _score);
            }
            PlayerPrefs.Save();
            SceneManager.LoadScene("GameOverScene");
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20, 20, 300, 40), $"Score: {_score}");
            GUI.Label(new Rect(20, 50, 300, 40), $"Combo: x{_combo:0.0}");
            GUI.Label(new Rect(20, 80, 300, 40), $"Lives: {_lives}");
            GUI.Label(new Rect(20, 110, 300, 40), $"Time: {_timeLeft:0.0}");
            GUI.Label(new Rect(20, 145, 600, 40), "Mouse: left=single/double, right=long press. Touch supports tap/hold/swipe/two-finger.");
        }
    }
}
