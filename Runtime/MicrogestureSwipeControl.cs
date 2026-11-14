using System;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Scripting;

namespace UnityEngine.XR.Hands
{
    /// <summary>
    /// Represents the thumb swipe microgesture as a normalized 2D direction, with each direction also available as a
    /// button.
    /// </summary>
    /// <remarks>
    /// When the runtime recognizes a thumb swipe, the matching direction button becomes pressed. The value is the
    /// normalized sum of the pressed directions' vectors: <see cref="Vector2.left"/> for <see cref="swipeLeft"/>,
    /// <see cref="Vector2.right"/> for <see cref="swipeRight"/>, <see cref="Vector2.up"/> for
    /// <see cref="swipeForward"/>, and <see cref="Vector2.down"/> for <see cref="swipeBackward"/>. When no direction
    /// button is pressed, the value is <see cref="Vector2.zero"/>.
    ///
    /// To respond to each swipe, use the <see cref="UnityEngine.InputSystem.InputAction.performed"/> callback or
    /// <see cref="UnityEngine.InputSystem.InputAction.WasPerformedThisFrame"/> on an action bound to this control,
    /// rather than polling the value.
    /// </remarks>
    /// <seealso cref="XRHandDevice.microgestureSwipe"/>
    /// <seealso cref="XRCommonHandGestures.IsMicrogestureActive(XRHandMicrogesture)"/>
    [Preserve, InputControlLayout(displayName = "Microgesture Swipe")]
    public class MicrogestureSwipeControl : Vector2Control
    {
        [Preserve, InputControlLayout(hideInUI = true)]
        internal class MicrogestureSwipeAxisControl : AxisControl
        {
            int m_Component;

            protected override void FinishSetup()
            {
                base.FinishSetup();
                m_Component = name == "x" ? 0 : 1;
            }

            public override unsafe float ReadUnprocessedValueFromState(void* statePtr)
            {
                var value = ((MicrogestureSwipeControl)parent).ReadUnprocessedValueFromState(statePtr);
                return value[m_Component];
            }

            public override unsafe void WriteValueIntoState(float value, void* statePtr)
            {
                throw new NotSupportedException("Write the swipe value through MicrogestureSwipeControl instead of a single axis.");
            }
        }

        internal const string k_LayoutName = "MicrogestureSwipe";
        internal const string k_AxisLayoutName = "MicrogestureSwipeAxis";

        // x/y replace the Vector2Control float children, which would read past this 4-bit block. They declare the full
        // block here because the device builds its cached-value invalidation tree from layout blocks before FinishSetup.
        // Explicit offset 0 keeps y off the inherited offset 4 and the buttons from being pushed to byte 1.
        /// <summary>
        /// The button that's pressed when the runtime recognizes a thumb swipe to the left.
        /// </summary>
        [InputControl(name = "x", layout = k_AxisLayoutName, offset = 0, bit = 0, sizeInBits = 4, format = "BIT", synthetic = true)]
        [InputControl(name = "y", layout = k_AxisLayoutName, offset = 0, bit = 0, sizeInBits = 4, format = "BIT", synthetic = true)]
        [InputControl(offset = 0, bit = 0, displayName = "Swipe Left")]
        public ButtonControl swipeLeft { get; set; }

        /// <summary>
        /// The button that's pressed when the runtime recognizes a thumb swipe to the right.
        /// </summary>
        [InputControl(offset = 0, bit = 1, displayName = "Swipe Right")]
        public ButtonControl swipeRight { get; set; }

        /// <summary>
        /// The button that's pressed when the runtime recognizes a thumb swipe forward.
        /// </summary>
        [InputControl(offset = 0, bit = 2, displayName = "Swipe Forward")]
        public ButtonControl swipeForward { get; set; }

        /// <summary>
        /// The button that's pressed when the runtime recognizes a thumb swipe backward.
        /// </summary>
        [InputControl(offset = 0, bit = 3, displayName = "Swipe Backward")]
        public ButtonControl swipeBackward { get; set; }

        /// <summary>
        /// Creates a new thumb swipe microgesture control.
        /// </summary>
        /// <remarks>
        /// The Input System creates this control from the device layout. You don't need to call this constructor.
        /// </remarks>
        public MicrogestureSwipeControl()
        {
            m_StateBlock.sizeInBits = 4;
            m_StateBlock.format = InputStateBlock.FormatBit;
        }

        /// <summary>
        /// Completes initialization after the control hierarchy is in place.
        /// </summary>
        protected override void FinishSetup()
        {
            swipeLeft = GetChildControl<ButtonControl>("swipeLeft");
            swipeRight = GetChildControl<ButtonControl>("swipeRight");
            swipeForward = GetChildControl<ButtonControl>("swipeForward");
            swipeBackward = GetChildControl<ButtonControl>("swipeBackward");
            base.FinishSetup();
        }

        /// <summary>
        /// Reads the value of the control from the given state, without applying processors.
        /// </summary>
        /// <param name="statePtr">State containing the control's state block.</param>
        /// <returns>The swipe direction, or <see cref="Vector2.zero"/> if no swipe is active.</returns>
        public override unsafe Vector2 ReadUnprocessedValueFromState(void* statePtr)
        {
            var forwardIsPressed = swipeForward.ReadValueFromState(statePtr) >= swipeForward.pressPointOrDefault;
            var backwardIsPressed = swipeBackward.ReadValueFromState(statePtr) >= swipeBackward.pressPointOrDefault;
            var leftIsPressed = swipeLeft.ReadValueFromState(statePtr) >= swipeLeft.pressPointOrDefault;
            var rightIsPressed = swipeRight.ReadValueFromState(statePtr) >= swipeRight.pressPointOrDefault;

            return DpadControl.MakeDpadVector(forwardIsPressed, backwardIsPressed, leftIsPressed, rightIsPressed);
        }

        /// <summary>
        /// Writes the given swipe direction into the given state by setting the matching direction button.
        /// </summary>
        /// <param name="value">The swipe direction to write, such as <see cref="Vector2.left"/>, or
        /// <see cref="Vector2.zero"/> for no swipe.</param>
        /// <param name="statePtr">State containing the control's state block.</param>
        public override unsafe void WriteValueIntoState(Vector2 value, void* statePtr)
        {
            var forwardIsPressed = swipeForward.IsValueConsideredPressed(value.y);
            var backwardIsPressed = swipeBackward.IsValueConsideredPressed(-value.y);
            var leftIsPressed = swipeLeft.IsValueConsideredPressed(-value.x);
            var rightIsPressed = swipeRight.IsValueConsideredPressed(value.x);

            swipeForward.WriteValueIntoState(forwardIsPressed ? 1f : 0f, statePtr);
            swipeBackward.WriteValueIntoState(backwardIsPressed ? 1f : 0f, statePtr);
            swipeLeft.WriteValueIntoState(leftIsPressed ? 1f : 0f, statePtr);
            swipeRight.WriteValueIntoState(rightIsPressed ? 1f : 0f, statePtr);
        }
    }
}
