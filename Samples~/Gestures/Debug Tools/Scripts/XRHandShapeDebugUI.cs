using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands.Gestures;

namespace UnityEngine.XR.Hands.Samples.Gestures.DebugTools
{
    /// <summary>
    /// Controls the debug UI for <see cref="XRHandShape"/> that shows the target and tolerances on the UI controlled
    /// by a <see cref="XRAllFingerStatesDebugUI"/>.
    /// </summary>
    public class XRHandShapeDebugUI : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The hand shape that will be displayed on the debug UI.")]
        XRHandShape m_HandShape;

        [SerializeField]
        [Tooltip("The debug UI that will be used to display the finger states.")]
        XRAllFingerShapesDebugUI m_XRAllFingerShapesDebugUI;

        [SerializeField]
        [Tooltip("The label that will be used to display the name of the hand shape.")]
        TextMeshProUGUI m_HandShapeNameLabel;

        static readonly List<XRFingerShapeDebugBar> k_ReusableBarsToHide = new List<XRFingerShapeDebugBar>();

        /// <summary>
        /// The hand shape that will be displayed in the debug UI.
        /// </summary>
        public XRHandShape handShape
        {
            get => m_HandShape;
            set
            {
                m_HandShape = value;
                UpdateShapeNameLabel();
            }
        }

        void Start() => UpdateShapeNameLabel();

        void UpdateShapeNameLabel()
        {
            if (m_HandShapeNameLabel != null)
                m_HandShapeNameLabel.text = m_HandShape.name;
        }

        void Update()
        {
            // Track all the bars that have no target and tolerance so they can be hidden
            k_ReusableBarsToHide.Clear();
            foreach (var graph in m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs)
                k_ReusableBarsToHide.AddRange(graph.bars);

            foreach(var condition in m_HandShape.fingerShapeConditions)
            {
                foreach (var shapeCondition in condition.targets)
                {
                    var xrFingerShapeDebugGraph = m_XRAllFingerShapesDebugUI.xrFingerShapeDebugGraphs[(int)condition.fingerID];
                    var bar = xrFingerShapeDebugGraph.bars[(int)shapeCondition.shapeType];
                    bar.SetTargetAndTolerance(shapeCondition.desired, shapeCondition.tolerance);
                    k_ReusableBarsToHide.Remove(bar);
                }
            }

            foreach (var hiddenBar in k_ReusableBarsToHide)
                hiddenBar.HideTargetAndTolerance();
        }
    }
}
