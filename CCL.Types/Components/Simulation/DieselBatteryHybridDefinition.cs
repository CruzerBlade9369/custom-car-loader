using CCL.Types.Proxies.Ports;
using System.Collections.Generic;
using UnityEngine;

namespace CCL.Types.Components.Simulation
{
    [AddComponentMenu("CCL/Components/Simulation/Diesel Battery Hybrid Definition")]
    public class DieselBatteryHybridDefinition : SimComponentDefinitionProxy
    {
        public float modeTransitionDelay = 1f;
        public float maxChargeBatteryLevel = 0.95f;
        public float chargeBatteryLevel = 0.6f;
        public float emergencyBatteryLevel = 0.4f;
        public float batteryRechargePower = 200000f;
        public float engineRechargeThrottle = 0.5f;

        [FuseId]
        public string powerFuseId = string.Empty;

        public override IEnumerable<PortReferenceDefinition> ExposedPortReferences => new[]
        {
            new PortReferenceDefinition(DVPortValueType.CONTROL, "THROTTLE"),
            new PortReferenceDefinition(DVPortValueType.VOLTS, "GENERATOR_VOLTS"),
            new PortReferenceDefinition(DVPortValueType.VOLTS, "VOLTAGE_REGULATOR_VOLTS"),
            new PortReferenceDefinition(DVPortValueType.VOLTS, "BATTERY_VOLTS"),
            new PortReferenceDefinition(DVPortValueType.ELECTRIC_CHARGE, "BATTERY_CHARGE_NORMALIZED"),
            new PortReferenceDefinition(DVPortValueType.OHMS, "SINGLE_MOTOR_EFFECTIVE_RESISTANCE"),
            new PortReferenceDefinition(DVPortValueType.OHMS, "EFFECTIVE_RESISTANCE"),
            new PortReferenceDefinition(DVPortValueType.POWER, "GOAL_POWER"),
            new PortReferenceDefinition(DVPortValueType.POWER, "GENERATOR_POWER_OUT_READ"),
            new PortReferenceDefinition(DVPortValueType.AMPS, "TM_TOTAL_AMPS"),
            new PortReferenceDefinition(DVPortValueType.AMPS, "TM_TRANSITION_CURRENT_LIMIT"),
            new PortReferenceDefinition(DVPortValueType.POWER, "TM_POWER_IN_READ"),
            new PortReferenceDefinition(DVPortValueType.STATE, "CHARGING_DISABLE_OVERRIDE"),
            new PortReferenceDefinition(DVPortValueType.CONTROL, "HYBRID_MODE_EXT_IN")
        };

        public override IEnumerable<PortDefinition> ExposedPorts => new[]
        {
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.CONTROL, "THROTTLE_OUT_ENGINE"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.CONTROL, "THROTTLE_OUT_BATTERY"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.OHMS, "SINGLE_MOTOR_EFFECTIVE_RESISTANCE_OUT_GENERATOR"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.OHMS, "SINGLE_MOTOR_EFFECTIVE_RESISTANCE_OUT_BATTERY"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.OHMS, "EFFECTIVE_RESISTANCE_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.POWER, "TM_POWER_IN_READ_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.VOLTS, "VOLTS_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.POWER, "GEN_GOAL_PWR_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.AMPS, "TM_TOTAL_AMPS_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.AMPS, "TM_TRANSITION_CURRENT_LIMIT_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "IS_CHARGING_BATTERY"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "ACTIVE_MODE"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "DIESEL_MODE_STATE"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "BATTERY_MODE_STATE")
        };
    }
}
