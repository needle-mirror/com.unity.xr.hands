using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.XR.Hands.Samples.Gestures.DebugTools
{
    /// <summary>
    /// Controls the debug UI for a single <see cref="XRFingerState"/> that shows the value and optionally a target
    /// and range on the UI controlled.
    /// </summary>
    public class XRFingerShapeDebugBar : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The container that determines the width of the max length bar, and holds the target and range indicators.")]
        RectTransform m_BarContainer;

        [SerializeField]
        [Tooltip("The bar that displays the value by being scaled in its local x axis from 0 to 1.")]
        Transform m_ValueBar;

        [SerializeField]
        [Tooltip("The target indicator that displays the target value by being positioned in its anchored x position from 0 to the bar container width.")]
        RectTransform m_TargetIndicator;

        [SerializeField]
        [Tooltip("The range indicator that displays the range by being positioned in its anchored x position from the range center and with the width of the range, proportional to the bar container.")]
        RectTransform m_RangeIndicator;

        float m_RangeRectHeight;

        /// <summary>
        /// The container that determines the width of the max length bar, and holds the target and range indicators.
        /// </summary>
        public RectTransform barContainer
        {
            get => m_BarContainer;
            set => m_BarContainer = value;
        }

        /// <summary>
        /// The bar that displays the value by being scaled in its local x axis from <c>0</c> to <c>1</c>.
        /// </summary>
        public Transform valueBar
        {
            get => m_ValueBar;
            set => m_ValueBar = value;
        }

        /// <summary>
        /// The <see cref="RectTransform"/> that displays the target value by being positioned in its anchored x
        /// position from <c>0</c> to the <see cref="barContainer"/> width.
        /// </summary>
        public RectTransform targetIndicator
        {
            get => m_TargetIndicator;
            set => m_TargetIndicator = value;
        }

        /// <summary>
        /// The <see cref="RectTransform"/> that displays the range by being positioned in its anchored x position from
        /// the range center and with the width of the range, proportional to the <see cref="barContainer"/>.
        /// </summary>
        public RectTransform rangeIndicator
        {
            get => m_RangeIndicator;
            set => m_RangeIndicator = value;
        }

        void Awake() => m_RangeRectHeight = m_RangeIndicator.rect.height;

        /// <summary>
        /// Set the value to display on the bar. This scales the <see cref="valueBar"/> in the X direction.
        /// </summary>
        /// <param name="value">The normalized value to display on the bar.</param>
        public void SetValue(float value) => m_ValueBar.localScale = new Vector3(value, 1f, 1f);

        /// <summary>
        /// Hide the value display for the bar.
        /// </summary>
        public void HideValue() => SetValue(0f);

        /// <summary>
        /// Set the target value and tolerance to display on the bar. The tolerance is converted to a range that centers
        /// on the target and extends out in both directions by the tolerance.
        /// </summary>
        /// <param name="target">The normalized target value.</param>
        /// <param name="tolerance">The tolerance to show around the target value.</param>
        public void SetTargetAndTolerance(float target, float tolerance)
        {
            m_TargetIndicator.gameObject.SetActive(true);
            m_RangeIndicator.gameObject.SetActive(true);
            var containerWidth = m_BarContainer.rect.width;
            m_TargetIndicator.anchoredPosition = new Vector3(target * containerWidth, 0f, 0f);
            var min = Mathf.Clamp01(target - tolerance);
            var max = Mathf.Clamp01(target + tolerance);
            var toleranceWidth = max - min;
            var toleranceCenter = (min + max) / 2f;
            m_RangeIndicator.anchoredPosition = new Vector2(toleranceCenter * containerWidth, 0f);
            m_RangeIndicator.sizeDelta = new Vector2(toleranceWidth * containerWidth, m_RangeRectHeight);
        }

        /// <summary>
        /// Hide the target and tolerance range for the bar.
        /// </summary>
        public void HideTargetAndTolerance()
        {
            m_TargetIndicator.gameObject.SetActive(false);
            m_RangeIndicator.gameObject.SetActive(false);
        }
    }
}
