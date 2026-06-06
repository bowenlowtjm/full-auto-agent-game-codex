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
        private int _streak;
        private float _combo = 1f;
        private string _lastEvent = "Hit targets with matching gesture to score";
        private float _lastEventAt;
        private float _hitFlash;
        private float _missFlash;

        private GUIStyle _hud;
        private GUIStyle _title;
        private GUIStyle _toast;
        private GUIStyle _button;

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
            _streak = 0;
            _combo = 1f;
        }

        private void Update()
        {
            if (ruleset == null) return;

            _timeLeft -= Time.deltaTime;
            _hitFlash = Mathf.Max(0f, _hitFlash - Time.deltaTime * 2.5f);
            _missFlash = Mathf.Max(0f, _missFlash - Time.deltaTime * 2.0f);

            if (Camera.main != null)
            {
                Color baseColor = new Color(0.08f, 0.08f, 0.12f, 1f);
                Color hitColor = new Color(0.08f, 0.18f, 0.11f, 1f);
                Color missColor = new Color(0.18f, 0.07f, 0.07f, 1f);
                Color target = Color.Lerp(baseColor, hitColor, _hitFlash);
                target = Color.Lerp(target, missColor, _missFlash);
                Camera.main.backgroundColor = Color.Lerp(Camera.main.backgroundColor, target, Time.deltaTime * 8f);
            }

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
                _streak += 1;
                _hitFlash = 1f;
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
            _streak = 0;
            _lives -= 1;
            _missFlash = 1f;
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

        private void EnsureStyles()
        {
            if (_hud != null) return;
            _title = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.95f, 0.95f, 1f) }
            };
            _hud = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.85f, 0.9f, 1f) }
            };
            _toast = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.92f, 0.45f) }
            };
            _button = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
        }

        private void OnGUI()
        {
            EnsureStyles();
            float w = Screen.width;

            GUI.Label(new Rect(18, 16, 300, 36), $"PULLY", _title);
            GUI.Label(new Rect(20, 48, 360, 28), $"Score: {_score}", _hud);
            GUI.Label(new Rect(20, 74, 360, 28), $"Combo: x{_combo:0.0}   Streak: {_streak}", _hud);
            GUI.Label(new Rect(20, 100, 360, 28), $"Lives: {_lives}    Time: {_timeLeft:0.0}", _hud);

            GUI.Label(new Rect(20, 132, w - 40, 24), "Input: click=tap | Ctrl=double | Shift=hold | Alt=swipe | Cmd=2-finger", _hud);
            if (!string.IsNullOrEmpty(_lastEvent) && Time.time - _lastEventAt <= 2.8f)
            {
                GUI.Label(new Rect(20, 160, w - 40, 28), $"Last: {_lastEvent}", _toast);
            }
        }
    }
}
