using CCL.Importer.Implementations;
using LocoSim.Definitions;
using LocoSim.Implementations;

namespace CCL.Importer.Components.Simulation
{
    internal class DieselBatteryHybridDefinitionInternal : SimComponentDefinition
    {
        public float modeTransitionDelay = 1f;
        public float maxChargeBatteryLevel = 0.95f;
        public float chargeBatteryLevel = 0.6f;
        public float emergencyBatteryLevel = 0.4f;
        public float batteryRechargePower = 200000f;
        public float engineRechargeThrottle = 0.5f;

        public string powerFuseId = string.Empty;

        public readonly PortReferenceDefinition throttle =
            new(PortValueType.CONTROL, "THROTTLE");
        public readonly PortReferenceDefinition generatorVoltage =
            new(PortValueType.VOLTS, "GENERATOR_VOLTS");
        public readonly PortReferenceDefinition voltRegVoltage =
            new(PortValueType.VOLTS, "VOLTAGE_REGULATOR_VOLTS");
        public readonly PortReferenceDefinition batteryVoltage =
            new(PortValueType.VOLTS, "BATTERY_VOLTS");
        public readonly PortReferenceDefinition batteryChargeNorm =
            new(PortValueType.ELECTRIC_CHARGE, "BATTERY_CHARGE_NORMALIZED");
        public readonly PortReferenceDefinition singleMotorEffectiveResistance =
            new(PortValueType.OHMS, "SINGLE_MOTOR_EFFECTIVE_RESISTANCE");
        public readonly PortReferenceDefinition effectiveResistance =
            new(PortValueType.OHMS, "EFFECTIVE_RESISTANCE");
        public readonly PortReferenceDefinition genGoalPower =
            new(PortValueType.POWER, "GOAL_POWER");
        public readonly PortReferenceDefinition genPowerOutRead =
            new(PortValueType.POWER, "GENERATOR_POWER_OUT_READ");
        public readonly PortReferenceDefinition tmTotalAmps =
            new(PortValueType.POWER, "TM_TOTAL_AMPS");
        public readonly PortReferenceDefinition transitionCurrentLimit =
            new(PortValueType.POWER, "TM_TRANSITION_CURRENT_LIMIT");
        public readonly PortReferenceDefinition tmPowerIn =
            new(PortValueType.POWER, "TM_POWER_IN_READ");
        public readonly PortReferenceDefinition chargeDisableOverride =
            new(PortValueType.STATE, "CHARGING_DISABLE_OVERRIDE");
        public readonly PortReferenceDefinition hybridModeControl =
            new(PortValueType.CONTROL, "HYBRID_MODE_EXT_IN");

        public readonly PortDefinition throttleDeOut =
            new(PortType.READONLY_OUT, PortValueType.CONTROL, "THROTTLE_OUT_ENGINE");
        public readonly PortDefinition throttleBatteryOut =
            new(PortType.READONLY_OUT, PortValueType.CONTROL, "THROTTLE_OUT_BATTERY");
        public readonly PortDefinition SmerGen =
            new(PortType.READONLY_OUT, PortValueType.OHMS, "SINGLE_MOTOR_EFFECTIVE_RESISTANCE_OUT_GENERATOR");
        public readonly PortDefinition SmerBatt =
            new(PortType.READONLY_OUT, PortValueType.OHMS, "SINGLE_MOTOR_EFFECTIVE_RESISTANCE_OUT_BATTERY");
        public readonly PortDefinition effectiveResistanceOut =
            new(PortType.READONLY_OUT, PortValueType.OHMS, "EFFECTIVE_RESISTANCE_OUT");
        public readonly PortDefinition tmPowerInReadOut =
            new(PortType.READONLY_OUT, PortValueType.POWER, "TM_POWER_IN_READ_OUT");
        public readonly PortDefinition voltageBusOut =
            new(PortType.READONLY_OUT, PortValueType.VOLTS, "VOLTS_OUT");
        public readonly PortDefinition genGoalPowerOut =
            new(PortType.READONLY_OUT, PortValueType.POWER, "GEN_GOAL_PWR_OUT");
        public readonly PortDefinition tmTotalAmpsOut =
            new(PortType.READONLY_OUT, PortValueType.POWER, "TM_TOTAL_AMPS_OUT");
        public readonly PortDefinition transitionCurrentLimitOut =
            new(PortType.READONLY_OUT, PortValueType.POWER, "TM_TRANSITION_CURRENT_LIMIT_OUT");
        public readonly PortDefinition isChargingBattery =
            new(PortType.READONLY_OUT, PortValueType.STATE, "IS_CHARGING_BATTERY");
        public readonly PortDefinition activeMode =
            new(PortType.READONLY_OUT, PortValueType.STATE, "ACTIVE_MODE");
        public readonly PortDefinition dieselModeState =
            new(PortType.READONLY_OUT, PortValueType.STATE, "DIESEL_MODE_STATE");
        public readonly PortDefinition batteryModeState =
            new(PortType.READONLY_OUT, PortValueType.STATE, "BATTERY_MODE_STATE");

        public override SimComponent InstantiateImplementation()
        {
            return new DieselBatteryHybrid(this);
        }
    }
}
