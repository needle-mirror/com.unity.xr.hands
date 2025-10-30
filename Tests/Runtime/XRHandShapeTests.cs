// the bug this test is testing only appears in the editor during link mode, hence it being
// in the runtime tests folder and ifdef'd wth UNITY_EDITOR.
#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.XR.Hands.Gestures;
using UnityEngine;

namespace Unity.XR.Hands.Runtime.Tests
{
    [TestFixture]
    class XRHandShapeTests
    {
        [Test]
        public void TestUpdateFingerShapeType()
        {
            var handShape = ScriptableObject.CreateInstance<XRHandShape>();
            var condition = new XRFingerShapeCondition
            {
                targets = new[]
                {
                    new XRFingerShapeCondition.Target { shapeType = XRFingerShapeType.BaseCurl },
                    new XRFingerShapeCondition.Target { shapeType = XRFingerShapeType.FullCurl }
                }
            };
            handShape.fingerShapeConditions = new List<XRFingerShapeCondition> { condition };

            var newShapeType = XRFingerShapeType.Pinch;
            var conditionIndex = 0;
            var targetIndex = 1;

            handShape.UpdateFingerShapeType(newShapeType, conditionIndex, targetIndex);
            handShape.fingerShapeConditions[conditionIndex].UpdateTypesNeededIfDirty();

            Assert.AreEqual(newShapeType, handShape.fingerShapeConditions[conditionIndex].targets[targetIndex].shapeType);
        }
    }
}
#endif // UNITY_EDITOR
