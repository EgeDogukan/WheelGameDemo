using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelGame.Config;
using WheelGame.Domain;

namespace WheelGame.UI
{
    public class WheelView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform wheelTransform;    
        [SerializeField] private SliceView slicePrefab;   
        [SerializeField] private TMP_Text ui_text_multiplier_value;

        [Header("Spin Settings")]
        [SerializeField] private float spinDuration = 2.0f;
        [SerializeField] private int extraFullRotations = 3;
        [SerializeField] private float pointerOffsetAngle = 0f;

        [Header("Layout")]
        [SerializeField] private float sliceRadius = 300f; 
        [SerializeField] private float sliceBaseAngleOffset = 0f; 

        [Header("Pointer Animation")]
        [SerializeField] private float tickStrength = 15f; // how many degrees the pointer kicks
        [SerializeField] private float tickDuration = 0.15f; 
        [SerializeField] private AudioSource audioSource; 
        [SerializeField] private AudioClip tickAudioClip; 

        [Header("Wheel Visual")]
        [SerializeField] private Image ui_image_wheel_background_value;
        [SerializeField] private Image ui_image_wheel_background_pointer_value;
        [SerializeField] private Sprite bronzeWheelSprite;
        [SerializeField] private Sprite silverWheelSprite;
        [SerializeField] private Sprite goldWheelSprite;
        [SerializeField] private Sprite bronzeWheelSpritePointer;
        [SerializeField] private Sprite silverWheelSpritePointer;
        [SerializeField] private Sprite goldWheelSpritePointer;

        [Header("Pooling")]
        [SerializeField] private Transform slicePoolRoot; 
        [SerializeField] private Transform flyPoolRoot;

        // Internal State
        private readonly List<SliceView> _sliceViews = new();   // active slices on wheel
        private readonly Stack<SliceView> _slicePool = new();   // inactive reusable slices
        private readonly Stack<GameObject> _flyIconPool = new(); // inactive fly icons

        private int _sliceCount;
        private RectTransform _pointerRect;
        private float _lastWheelAngle;
        private bool _isSpinning;

        private void OnValidate()
        {
            if (wheelTransform == null)
            {
                var t = UIAutoBinder.FindComponentInChildren<RectTransform>(transform, "ui_wheel_visual");
                if (t != null) wheelTransform = t;
            }

            if (slicePrefab == null)
            {
                slicePrefab = GetComponentInChildren<SliceView>(true);
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (ui_text_multiplier_value == null)
            {
                ui_text_multiplier_value = UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_multiplier_value");
            }
        }

        private void Awake()
        {
            if (ui_image_wheel_background_pointer_value != null)
                _pointerRect = ui_image_wheel_background_pointer_value.GetComponent<RectTransform>();

            EnsurePoolRoots();
        }

        private void Update()
        {
            if (!_isSpinning || _sliceCount == 0 || wheelTransform == null) 
                return;

            HandlePointerTick();
        }

        // pooling helpers

        private void EnsurePoolRoots()
        {
            if (wheelTransform == null) return;

            // check both slice and fly icon roots
            if (slicePoolRoot == null)
            {
                var go = new GameObject("ui_slice_pool");
                go.transform.SetParent(wheelTransform.parent != null ? wheelTransform.parent : transform, false);
                go.SetActive(false);
                slicePoolRoot = go.transform;
            }

            if (flyPoolRoot == null)
            {
                var go = new GameObject("ui_fly_pool");
                // Parent to Canvas level so scaling is consistent
                go.transform.SetParent(wheelTransform.GetComponentInParent<Canvas>().transform, false);
                go.SetActive(false);
                flyPoolRoot = go.transform;
            }
        }

        // pointer tick ani.

        private void HandlePointerTick()
        {
            float currentAngle = wheelTransform.localEulerAngles.z;
            float anglePerSegment = 360f / _sliceCount;

            int lastIndex = (int)(_lastWheelAngle / anglePerSegment);
            int currentIndex = (int)(currentAngle / anglePerSegment);

            if (lastIndex != currentIndex)
            {
                PlayTickEffect();
            }

            _lastWheelAngle = currentAngle;
        }

        private void PlayTickEffect()
        {
            if (audioSource != null && tickAudioClip != null)
                audioSource.PlayOneShot(tickAudioClip);

            if (_pointerRect != null)
            {
                _pointerRect.DOKill(true); 
                _pointerRect.localRotation = Quaternion.identity;     
                _pointerRect.DOPunchRotation(new Vector3(0, 0, tickStrength), tickDuration, 10, 1f);
            }
        }

        // building wheel with pooling

        public void BuildFromLayout(
            WheelLayoutConfig layoutConfig,
            IReadOnlyList<int> calculatedAmounts)
        {
            // 1. Safety Checks
            if (layoutConfig == null || layoutConfig.slices == null) return;
            int targetCount = layoutConfig.slices.Count;
            if (targetCount == 0) return;
            
            // Validate amounts list matches slice count
            if (calculatedAmounts == null || calculatedAmounts.Count != targetCount)
            {
                Debug.LogError($"WheelView.BuildFromLayout: calculatedAmounts count ({calculatedAmounts?.Count ?? 0}) doesn't match slice count ({targetCount})");
                return;
            }

            if (wheelTransform != null)
                wheelTransform.localRotation = Quaternion.identity;

            _sliceCount = targetCount;
            float angleStep = 360f / _sliceCount;
            EnsurePoolRoots();
            
            // CASE A: Too many slices? Remove extras and pool them
            while (_sliceViews.Count > targetCount)
            {
                int lastIndex = _sliceViews.Count - 1;
                var sliceToRemove = _sliceViews[lastIndex];
                _sliceViews.RemoveAt(lastIndex);

                if (Application.isPlaying)
                {
                    sliceToRemove.gameObject.SetActive(false);
                    sliceToRemove.transform.SetParent(slicePoolRoot, false);
                    _slicePool.Push(sliceToRemove);
                }
                else
                {
                    DestroyImmediate(sliceToRemove.gameObject);
                }
            }

            // CASE B: Too few slices? Get from pool or create new
            while (_sliceViews.Count < targetCount)
            {
                SliceView sliceView;
                
                if (Application.isPlaying && _slicePool.Count > 0)
                {
                    sliceView = _slicePool.Pop();
                    sliceView.transform.SetParent(wheelTransform, false);
                    sliceView.gameObject.SetActive(true);
                }
                else
                {
                    sliceView = Instantiate(slicePrefab, wheelTransform);
                }

                _sliceViews.Add(sliceView);
            }

            // 3. Update Data on all slices (Reuse)
            for (int i = 0; i < targetCount; i++)
            {
                var sliceView = _sliceViews[i];
                var sliceConfig = layoutConfig.slices[i];

                sliceView.gameObject.name = $"ui_slice_{i}";
                sliceView.transform.SetAsLastSibling(); // Ensure visual order

                bool isBomb = sliceConfig.sliceType == SliceType.Bomb;
                int amount = calculatedAmounts[i]; // Use pre-calculated amount from controller

                sliceView.Configure(sliceConfig.icon, amount, isBomb);

                float angle = i * angleStep + sliceBaseAngleOffset;
                sliceView.SetAngleAndRadius(angle, sliceRadius);
            }

            SetWheelSprite(layoutConfig.zoneType);
        }

        // spin animation

        public void SpinToSlice(int sliceIndex, Action onComplete)
        {
            if (wheelTransform == null || _sliceCount == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _isSpinning = true;
            _lastWheelAngle = wheelTransform.localEulerAngles.z; 

            float segmentAngle = 360f / _sliceCount;
            float currentSliceAngle = sliceIndex * segmentAngle + sliceBaseAngleOffset;
            float destinationAngle = currentSliceAngle - pointerOffsetAngle;
            float targetTotalRotation = destinationAngle - (360f * (extraFullRotations + 1));

            wheelTransform
                .DORotate(new Vector3(0f, 0f, targetTotalRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    _isSpinning = false;
                    onComplete?.Invoke();
                });
        }

        // visuals and multiplier

        private void SetWheelSprite(ZoneType zoneType)
        {
            if (ui_image_wheel_background_value == null)
                return;

            Sprite target = bronzeWheelSprite;
            Sprite targetPointer = bronzeWheelSpritePointer;

            switch (zoneType)
            {
                case ZoneType.Normal:
                    target = bronzeWheelSprite;
                    targetPointer = bronzeWheelSpritePointer;
                    break;
                case ZoneType.SafeSilver:
                    target = silverWheelSprite;
                    targetPointer = silverWheelSpritePointer;
                    break;
                case ZoneType.SuperGold:
                    target = goldWheelSprite;
                    targetPointer = goldWheelSpritePointer;
                    break;
            }

            ui_image_wheel_background_value.sprite = target;
            ui_image_wheel_background_pointer_value.sprite = targetPointer;
        }

        public void SetMultiplier(float factor)
        {
            if (ui_text_multiplier_value == null)
                return;

            ui_text_multiplier_value.text = $"x{factor:0.0}";
        }

        // fly ani. using pooling

        public void PlayCollectAnimation(int sliceIndex, RectTransform target, float duration, Action onComplete)
        {
            if (sliceIndex < 0 || sliceIndex >= _sliceViews.Count || target == null)
            {
                onComplete?.Invoke();
                return;
            }

            EnsurePoolRoots();

            var sliceView = _sliceViews[sliceIndex];
            var iconRect = sliceView.IconRectTransform;
            var sourceImage = iconRect.GetComponent<Image>();

            GameObject flyObj;
            if (_flyIconPool.Count > 0)
            {
                flyObj = _flyIconPool.Pop();
            }
            else
            {
                flyObj = new GameObject("ui_reward_fly_icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            }

            var flyRect = (RectTransform)flyObj.transform;
            var flyImage = flyObj.GetComponent<Image>();
            var canvas = wheelTransform.GetComponentInParent<Canvas>();
            flyImage.raycastTarget = false;
            if (canvas != null)
            {
                flyRect.SetParent(canvas.transform, worldPositionStays: false);
            }
 
            flyObj.SetActive(true);

            // configuring
            if (sourceImage != null)
            {
                flyImage.sprite = sourceImage.sprite;
                flyImage.preserveAspect = true;
            }
            flyImage.raycastTarget = false;

            flyRect.position = iconRect.position;
            flyRect.localScale = Vector3.one * 0.8f;
            flyRect.localRotation = Quaternion.identity;

            // and animating
            float half = duration * 0.5f;
            float midScale = 1.4f;

            var seq = DOTween.Sequence();
            
            seq.Append(flyRect.DOMove(target.position, duration).SetEase(Ease.InOutQuad));
            
            var scaleSeq = DOTween.Sequence()
                .Append(flyRect.DOScale(midScale, half).SetEase(Ease.OutQuad))
                .Append(flyRect.DOScale(0f, half).SetEase(Ease.InQuad));

            seq.Join(scaleSeq);

            seq.OnComplete(() =>
            {
                // back to pool
                if (flyObj != null) 
                {
                    flyObj.SetActive(false);
                    if (flyPoolRoot != null) flyRect.SetParent(flyPoolRoot);
                    _flyIconPool.Push(flyObj);
                }
                
                onComplete?.Invoke();
            });
        }
    }
}