using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelGame.UI
{
    public class SliceView : MonoBehaviour
    {
        [SerializeField] private RectTransform rootTransform;
        [SerializeField] private Image ui_image_slice_icon_value;
        [SerializeField] private TMP_Text ui_text_slice_amount_value;
        [SerializeField] private Sprite bombSprite;

        private void OnValidate()
        {
            if (rootTransform == null)
                rootTransform = (RectTransform)transform;

            if (ui_image_slice_icon_value == null)
                ui_image_slice_icon_value =
                    UIAutoBinder.FindComponentInChildren<Image>(transform, "ui_image_slice_icon_value");

            if (ui_text_slice_amount_value == null)
                ui_text_slice_amount_value =
                    UIAutoBinder.FindComponentInChildren<TMP_Text>(transform, "ui_text_slice_amount_value");
        }

        public void Configure(Sprite icon, int amount, bool isBomb)
        {
            if (isBomb)
            {
                ui_image_slice_icon_value.sprite = bombSprite != null ? bombSprite : icon;
                ui_text_slice_amount_value.text = string.Empty;
            }
            else
            {
                ui_image_slice_icon_value.sprite = icon;
                ui_image_slice_icon_value.preserveAspect = true;
                ui_text_slice_amount_value.text = amount.ToString();
            }
        }

        public void SetAngle(float angleDegrees)
        {
            if (rootTransform != null)
            {
                rootTransform.localRotation = Quaternion.Euler(0f, 0f, -angleDegrees);
            }
        }
        
        public void SetAngleAndRadius(float angleDegrees, float radius)
        {
            if (rootTransform == null) return;

            float rad = angleDegrees * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)); 

            rootTransform.anchoredPosition = dir * radius;
            rootTransform.localRotation = Quaternion.Euler(0f, 0f, -angleDegrees); // radial orientation face up
        }

        public RectTransform IconRectTransform => ui_image_slice_icon_value != null ? (RectTransform)ui_image_slice_icon_value.transform : rootTransform;
    }
}
