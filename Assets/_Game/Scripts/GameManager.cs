using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pully.Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private RulesetDefinition ruleset;
        [SerializeField] private TargetSpawner spawner;

        private float _timeLeft;
        private int _score;
        private int _lives;
        private int _streak;
        private float _combo = 1f;
        private string _lastEvent = "Match gesture to score";
        private float _lastEventAt;
        private float _hitFlash;
        private float _missFlash;

        private GUIStyle _hud;
        private GUIStyle _title;
        private GUIStyle _toast;
        private GUIStyle _chip;

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

        public void BindSpawner(TargetSpawner targetSpawner)
        {
            spawner = targetSpawner;
        }

        private void Update()
        {
            if (ruleset == null) return;

            _timeLeft -= Time.deltaTime;
            _hitFlash = Mathf.Max(0f, _hitFlash - Time.deltaTime * 2.6f);
            _missFlash = Mathf.Max(0f, _missFlash - Time.deltaTime * 2.2f);

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
                _lastEvent = $"Perfect +{reward}";
                _lastEventAt = Time.time;
                target.Resolve();
            }
            else
            {
                _lastEvent = $"Miss: wanted {ShortGesture(target.Rule.requiredGesture)}";
                _lastEventAt = Time.time;
                Penalize();
                target.Resolve();
            }
        }

        public void OnTargetExpired(Target target)
        {
            _lastEvent = $"Too slow: {ShortGesture(target.Rule.requiredGesture)}";
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

        private static string ShortGesture(RulesetDefinition.Gesture g)
        {
            return g switch
            {
                RulesetDefinition.Gesture.SingleTap => "Tap",
                RulesetDefinition.Gesture.DoubleTap => "Double",
                RulesetDefinition.Gesture.LongPress => "Hold",
                RulesetDefinition.Gesture.SwipeTap => "Swipe",
                RulesetDefinition.Gesture.TwoFingerTap => "2-Finger",
                _ => g.ToString()
            };
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
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(1f, 0.92f, 0.45f) }
            };
            _chip = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }

        private void DrawGestureLegend(float y, float w)
        {
            float chipW = (w - 40f) / 5f;
            float x = 20f;
            DrawChip(new Rect(x + chipW * 0, y, chipW - 8, 28), "Tap", Color.green);
            DrawChip(new Rect(x + chipW * 1, y, chipW - 8, 28), "Hold", Color.red);
            DrawChip(new Rect(x + chipW * 2, y, chipW - 8, 28), "Double", Color.blue);
            DrawChip(new Rect(x + chipW * 3, y, chipW - 8, 28), "Swipe", Color.yellow);
            DrawChip(new Rect(x + chipW * 4, y, chipW - 8, 28), "2-Finger", new Color(0.6f, 0.2f, 0.8f));
        }

        private void DrawChip(Rect rect, string text, Color color)
        {
            var old = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, 0.9f);
            GUI.Box(rect, text, _chip);
            GUI.color = old;
        }

        private void DrawCurrentObjective(float y, float w)
        {
            if (spawner == null || spawner.ActiveTargets.Count == 0)
            {
                GUI.Label(new Rect(20, y, w - 40, 24), "Objective: wait for next target...", _hud);
                return;
            }

            Target best = null;
            float maxLife = -1f;
            foreach (var t in spawner.ActiveTargets)
            {
                if (t == null) continue;
                if (t.Lifetime01 > maxLife)
                {
                    maxLife = t.Lifetime01;
                    best = t;
                }
            }

            if (best != null)
            {
                string danger = best.Lifetime01 > 0.75f ? " !!" : "";
                GUI.Label(new Rect(20, y, w - 40, 24),
                    $"Priority: {best.Rule.shape} {ColorName(best.Rule.color)} -> {ShortGesture(best.Rule.requiredGesture)}{danger}", _hud);
            }
        }

        private static string ColorName(Color c)
        {
            if (c == Color.green) return "Green";
            if (c == Color.red) return "Red";
            if (c == Color.blue) return "Blue";
            if (c == Color.yellow) return "Yellow";
            if (Mathf.Abs(c.r - 0.6f) < 0.05f && Mathf.Abs(c.b - 0.8f) < 0.05f) return "Purple";
            return "Color";
        }

        private void OnGUI()
        {
            EnsureStyles();
            float w = Screen.width;

            GUI.Label(new Rect(18, 16, 300, 36), "PULLY", _title);
            GUI.Label(new Rect(20, 48, 360, 28), $"Score: {_score}", _hud);
            GUI.Label(new Rect(20, 74, 360, 28), $"Combo: x{_combo:0.0}   Streak: {_streak}", _hud);
            GUI.Label(new Rect(20, 100, 360, 28), $"Lives: {_lives}    Time: {_timeLeft:0.0}", _hud);

            DrawGestureLegend(132, w);
            DrawCurrentObjective(166, w);

            GUI.Label(new Rect(20, 192, w - 40, 24), "Editor controls: Click Tap | Ctrl Double | Shift Hold | Alt Swipe | Cmd 2-Finger", _hud);
            if (!string.IsNullOrEmpty(_lastEvent) && Time.time - _lastEventAt <= 2.8f)
            {
                GUI.Label(new Rect(20, 218, w - 40, 30), $"Last: {_lastEvent}", _toast);
            }
        }
    }
}
