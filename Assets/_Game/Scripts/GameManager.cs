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
        private string _lastEvent = "Hit targets with matching gesture to score";
        private float _lastEventAt;

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
                int reward = ScoreCalculator.ScoreFor(target.Rule.baseReward, _combo);
                _score += reward;
                _combo = ScoreCalculator.NextCombo(_combo, ruleset.comboStep, ruleset.comboCap);
                _lastEvent = $"HIT +{reward} ({target.Rule.shape}/{target.Rule.requiredGesture})";
                _lastEventAt = Time.time;
                target.Resolve();
            }
            else
            {
                _lastEvent = $"MISS expected {target.Rule.requiredGesture}, got {gesture}";
                _lastEventAt = Time.time;
                Penalize();
                target.Resolve();
            }
        }

        public void OnTargetExpired(Target target)
        {
            _lastEvent = $"EXPIRED {target.Rule.shape}/{target.Rule.requiredGesture}";
            _lastEventAt = Time.time;
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
            GUI.Label(new Rect(20, 145, 900, 40), "Mouse: click=single tap, Ctrl+click=double, Shift+click=long, Alt+click=swipe, Cmd+click=two-finger");
            if (!string.IsNullOrEmpty(_lastEvent) && Time.time - _lastEventAt <= 2.5f)
            {
                GUI.Label(new Rect(20, 175, 1000, 40), $"Last: {_lastEvent}");
            }
        }
    }
}
