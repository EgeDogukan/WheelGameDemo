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

        // Internal State
        private readonly List<SliceView> _sliceViews = new();
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
            {                   // ended up not using it 
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
        }

        private void Update()
        {
            if (!_isSpinning || _sliceCount == 0 || wheelTransform == null) 
            {
                return;
            }

            HandlePointerTick();
        }

        // logic to detect when a peg passes the pointer
        private void HandlePointerTick()
        {
            
            float currentAngle = wheelTransform.localEulerAngles.z;
            
            // calculate the segment size
            float anglePerSegment = 360f / _sliceCount;

            // check current index vs last frame index
            // here i use simple modulo math to determine which "slice" is currently at the top.
            int lastIndex = (int)(_lastWheelAngle / anglePerSegment);
            int currentIndex = (int)(currentAngle / anglePerSegment);

            // if the integer index changed, we crossed a line!
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

        public void BuildFromLayout(
            WheelLayoutConfig layoutConfig,
            IRewardProgressionStrategy progression,
            int zoneIndex)
        {
            ClearExistingSlices();

            if (wheelTransform != null)
                wheelTransform.localRotation = Quaternion.identity;

            if (layoutConfig == null || layoutConfig.slices == null)
                return;

            _sliceCount = layoutConfig.slices.Count;
            if (_sliceCount == 0)
                return;

            float angleStep = 360f / _sliceCount;
            
            for (int i = 0; i < _sliceCount; i++)
            {
                var sliceConfig = layoutConfig.slices[i];
                var sliceView = Instantiate(slicePrefab, wheelTransform);
                sliceView.gameObject.name = $"ui_slice_{i}";

                bool isBomb = sliceConfig.sliceType == SliceType.Bomb;
                int amount = 0;

                if (!isBomb)
                    amount = progression.GetAmount(sliceConfig.baseAmount, zoneIndex);

                sliceView.Configure(sliceConfig.icon, amount, isBomb);

                float angle = i * angleStep + sliceBaseAngleOffset;
                sliceView.SetAngleAndRadius(angle, sliceRadius);

                _sliceViews.Add(sliceView);
            }

            SetWheelSprite(layoutConfig.zoneType);
        }

        public void SpinToSlice(int sliceIndex, Action onComplete)
        {
            if (wheelTransform == null || _sliceCount == 0)
            {
                onComplete?.Invoke();
                return;
            }

            _isSpinning = true;
            _lastWheelAngle = wheelTransform.localEulerAngles.z; // Init tracker

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

        private void ClearExistingSlices()
        {
            foreach (var sliceView in _sliceViews)
            {
                if (sliceView != null)
                {
                    if (Application.isPlaying)
                        Destroy(sliceView.gameObject);
                    else
                        DestroyImmediate(sliceView.gameObject);
                }
            }
            _sliceViews.Clear();
        }

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

        public void PlayCollectAnimation(int sliceIndex, RectTransform target, float duration, Action onComplete)
        {
            if (sliceIndex < 0 || sliceIndex >= _sliceViews.Count ||
                target == null || wheelTransform == null)
            {
                onComplete?.Invoke();
                return;
            }

            var sliceView = _sliceViews[sliceIndex];
            var iconRect = sliceView.IconRectTransform;

            var canvas = wheelTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                onComplete?.Invoke();
                return;
            }

            // create temp icon to animate
            var flyObj  = new GameObject("ui_reward_fly_icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var flyRect = (RectTransform)flyObj.transform;
            flyRect.SetParent(canvas.transform, worldPositionStays: false);

            var flyImage = flyObj.GetComponent<Image>();
            flyImage.raycastTarget = false;
            var sourceImage = iconRect.GetComponent<Image>();
            if (sourceImage != null)
            {
                flyImage.sprite = sourceImage.sprite;
                flyImage.preserveAspect = true;
            }
            flyImage.raycastTarget = false;

            flyRect.position   = iconRect.position;
            flyRect.localScale = Vector3.one * 0.8f; 

            float half = duration * 0.5f;
            float midScale = 1.4f;

            var seq = DOTween.Sequence();
            var moveTween = flyRect.DOMove(target.position, duration).SetEase(Ease.InOutQuad);
                                           seq.Append(moveTween);
            var scaleSeq = DOTween.Sequence()
                                            .Append(flyRect.DOScale(midScale, half).SetEase(Ease.OutQuad))
                                            .Append(flyRect.DOScale(0f, half).SetEase(Ease.InQuad));

            seq.Join(scaleSeq);

            seq.OnComplete(() =>
            {
                Destroy(flyObj);
                onComplete?.Invoke();
            });
        }
    }
}