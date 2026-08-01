using CCL.Creator.Utility;
using CCL.Types.Components.Simulation.Electric;
using UnityEditor;
using UnityEngine;

namespace CCL.Creator.Inspector.SimComponents
{
    [CustomEditor(typeof(BatteryCustomCurveDefinition))]
    internal class BatteryCustomCurveDefinitionEditor : Editor
    {
        private BatteryCustomCurveDefinition _def = null!;
        private float _nominalVoltage;
        private bool _hasCalculatedNominalVoltage;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            _def = (BatteryCustomCurveDefinition)target;

            float minVoltage = _def.numSeriesCells * _def.chargeToVoltageCurve.Evaluate(0);
            float maxVoltage = _def.numSeriesCells * _def.chargeToVoltageCurve.Evaluate(1);
            float totalResistance = _def.numSeriesCells * _def.internalResistance;

            EditorHelpers.DrawHeader("Calculated Values");
            EditorGUILayout.LabelField("Min Voltage", $"{minVoltage:F2} V");
            EditorGUILayout.LabelField("Max Voltage", $"{maxVoltage:F2} V");
            EditorGUILayout.LabelField("Total Resistance", $"{totalResistance:F2} Ω");

            if (_hasCalculatedNominalVoltage)
            {
                EditorGUILayout.LabelField("Nominal Cell Voltage", $"{_nominalVoltage:F2} V");
                EditorGUILayout.LabelField("Nominal Pack Voltage", $"{_nominalVoltage * _def.numSeriesCells:F2} V");
            }

            if (GUILayout.Button("Calculate Nominal Cell Voltage"))
            {
                _nominalVoltage = CalculateNominalCellVoltage(_def.chargeToVoltageCurve);
                _hasCalculatedNominalVoltage = true;
            }
        }

        public float CalculateNominalCellVoltage(AnimationCurve curve, int samples = 100)
        {
            float sum = 0f;

            for (int i = 0; i < samples; i++)
            {
                float soc = i / (samples - 1f);
                sum += curve.Evaluate(soc);
            }

            return sum / samples;
        }
    }
}
