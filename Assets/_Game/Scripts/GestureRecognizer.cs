using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Pully.Game
{
    public class GestureRecognizer : MonoBehaviour
    {
        [SerializeField] private RulesetDefinition ruleset;
        [SerializeField] private TargetSpawner spawner;
        [SerializeField] private GameManager gameManager;

        private readonly Dictionary<int, PointerState> _touchStates = new();
        private float _lastMouseTapTime = -10f;
        private Target _lastMouseTarget;

        private struct PointerState
        {
            public float startTime;
            public Vector2 startPos;
            public Target target;
        }

        public void Initialize(RulesetDefinition rs, TargetSpawner ts, GameManager gm)
        {
            ruleset = rs;
            spawner = ts;
            gameManager = gm;
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            if (ruleset == null || spawner == null || gameManager == null) return;
            HandleTouches();
            HandleMouse();
        }

        private void HandleTouches()
        {
            var active = ETouch.activeTouches;
            int beganCount = 0;
            Target beganTarget = null;

            foreach (var t in active)
            {
                if (t.phase == TouchPhase.Began)
                {
                    var target = spawner.FindTargetAtScreenPos(t.screenPosition);
                    _touchStates[t.touchId] = new PointerState
                    {
                        startPos = t.screenPosition,
                        startTime = Time.time,
                        target = target
                    };
                    if (target != null)
                    {
                        beganCount++;
                        beganTarget = target;
                    }
                }
            }

            if (beganCount >= 2 && beganTarget != null)
            {
                gameManager.OnGesturePerformed(beganTarget, RulesetDefinition.Gesture.TwoFingerTap);
            }

            foreach (var t in active)
            {
                if (t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) continue;
                if (!_touchStates.TryGetValue(t.touchId, out var st)) continue;
                _touchStates.Remove(t.touchId);
                if (st.target == null) continue;

                var duration = Time.time - st.startTime;
                var distance = Vector2.Distance(st.startPos, t.screenPosition);

                RulesetDefinition.Gesture g;
                if (duration >= ruleset.longPressDuration) g = RulesetDefinition.Gesture.LongPress;
                else if (distance >= ruleset.swipeMinDistance) g = RulesetDefinition.Gesture.SwipeTap;
                else g = RulesetDefinition.Gesture.SingleTap;

                gameManager.OnGesturePerformed(st.target, g);
            }
        }

        private void HandleMouse()
        {
            if (Mouse.current == null || Keyboard.current == null) return;

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                var pos = Mouse.current.position.value;
                var target = spawner.FindTargetAtScreenPos(pos);
                if (target == null) return;

                bool ctrl = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed;
                bool shift = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
                bool alt = Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed;
                bool cmd = Keyboard.current.leftMetaKey.isPressed || Keyboard.current.rightMetaKey.isPressed;

                RulesetDefinition.Gesture gesture;
                if (shift) gesture = RulesetDefinition.Gesture.LongPress;
                else if (alt) gesture = RulesetDefinition.Gesture.SwipeTap;
                else if (cmd) gesture = RulesetDefinition.Gesture.TwoFingerTap;
                else if (ctrl || (Time.time - _lastMouseTapTime <= ruleset.doubleTapWindow && _lastMouseTarget == target))
                    gesture = RulesetDefinition.Gesture.DoubleTap;
                else gesture = RulesetDefinition.Gesture.SingleTap;

                _lastMouseTapTime = Time.time;
                _lastMouseTarget = target;
                gameManager.OnGesturePerformed(target, gesture);
            }
        }
    }
}
