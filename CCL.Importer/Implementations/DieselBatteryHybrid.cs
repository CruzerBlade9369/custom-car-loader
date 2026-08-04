using CCL.Importer.Components.Simulation;
using LocoSim.Implementations;
using UnityEngine;

namespace CCL.Importer.Implementations
{
    internal class DieselBatteryHybrid : SimComponent
    {
        private readonly FuseReference powerFuseRef;

        private readonly PortReference throttle;
        private readonly PortReference generatorVoltage;
        private readonly PortReference voltRegVoltage;
        private readonly PortReference batteryVolts;
        private readonly PortReference batteryChargeNorm;
        private readonly PortReference smer;
        private readonly PortReference effectiveResistance;
        private readonly PortReference genGoalPower;
        private readonly PortReference genPowerOutRead;
        private readonly PortReference tmTotalAmps;
        private readonly PortReference tmTransitionCurrentLimit;
        private readonly PortReference tmPowerInRead;
        private readonly PortReference chargingOverride;
        private readonly PortReference hybridMode;

        private readonly Port throttleDeOut;
        private readonly Port throttleBatOut;
        private readonly Port smerGenOut;
        private readonly Port smerBatOut;
        private readonly Port effectiveResistanceOut;
        private readonly Port tmPowerInReadOut;
        private readonly Port voltageBusOut;
        private readonly Port genGoalPowerOut;
        private readonly Port tmTotalAmpsOut;
        private readonly Port tmTransitionCurrentLimitOut;
        private readonly Port isChargingBattery;
        private readonly Port activeMode;
        private readonly Port dieselModeState;
        private readonly Port batteryModeState;

        private readonly float modeTransitionDelay;
        private readonly float maxChargeBatteryLevel;
        private readonly float startChargeBatteryLevel;
        private readonly float emergencyBatteryLevel;
        private readonly float genTargetPwr;

        private HybridMode selectorMode;
        private HybridMode requestedMode;
        private HybridMode lastRequestedMode;
        private HybridMode currentMode;
        private float transitionTimer;
        private bool isTransitioning;
        private bool isCharging;
        private bool batteryLow;
        private bool allowedToCharge;
        private float chargingThrottle;

        private enum HybridMode
        {
            Transition = -1,
            Diesel = 0,
            Battery = 1
        }

        public DieselBatteryHybrid(DieselBatteryHybridDefinitionInternal def) : base(def.ID)
        {
            modeTransitionDelay = def.modeTransitionDelay;
            maxChargeBatteryLevel = def.maxChargeBatteryLevel;
            startChargeBatteryLevel = def.chargeBatteryLevel;
            emergencyBatteryLevel = def.emergencyBatteryLevel;
            genTargetPwr = def.batteryRechargePower;
            chargingThrottle = def.engineRechargeThrottle;

            powerFuseRef = AddFuseReference(def.powerFuseId);

            throttle = AddPortReference(def.throttle);
            generatorVoltage = AddPortReference(def.generatorVoltage);
            voltRegVoltage = AddPortReference(def.voltRegVoltage);
            batteryVolts = AddPortReference(def.batteryVoltage);
            batteryChargeNorm = AddPortReference(def.batteryChargeNorm);
            smer = AddPortReference(def.singleMotorEffectiveResistance);
            effectiveResistance = AddPortReference(def.effectiveResistance);
            genGoalPower = AddPortReference(def.genGoalPower);
            genPowerOutRead = AddPortReference(def.genPowerOutRead);
            tmTotalAmps = AddPortReference(def.tmTotalAmps);
            tmTransitionCurrentLimit = AddPortReference(def.transitionCurrentLimit);
            tmPowerInRead = AddPortReference(def.tmPowerIn);
            chargingOverride = AddPortReference(def.chargeDisableOverride);
            hybridMode = AddPortReference(def.hybridModeControl);

            throttleDeOut = AddPort(def.throttleDeOut);
            throttleBatOut = AddPort(def.throttleBatteryOut);
            smerGenOut = AddPort(def.SmerGen);
            smerBatOut = AddPort(def.SmerBatt);
            effectiveResistanceOut = AddPort(def.effectiveResistanceOut);
            tmPowerInReadOut = AddPort(def.tmPowerInReadOut);
            voltageBusOut = AddPort(def.voltageBusOut);
            genGoalPowerOut = AddPort(def.genGoalPowerOut);
            tmTotalAmpsOut = AddPort(def.tmTotalAmpsOut);
            tmTransitionCurrentLimitOut = AddPort(def.transitionCurrentLimitOut);
            isChargingBattery = AddPort(def.isChargingBattery);
            activeMode = AddPort(def.activeMode);
            dieselModeState = AddPort(def.dieselModeState);
            batteryModeState = AddPort(def.batteryModeState);

            allowedToCharge = false;
        }

        public override void Tick(float delta)
        {
            // Mode selector
            if (hybridMode.Value < 0.5)
            {
                selectorMode = HybridMode.Diesel;
            }

            else if (hybridMode.Value >= 0.5)
            {
                selectorMode = HybridMode.Battery;
            }

            // No switching without power
            if (!powerFuseRef.State)
            {
                return;
            }

            SimulateSwitching(delta);
            
            if (!isTransitioning || transitionTimer <= 0f)
            {
                SimulateModeStates();
            }

            if (allowedToCharge) isChargingBattery.Value = 1;
            else isChargingBattery.Value = 0;
        }

        private void SimulateSwitching(float delta)
        {
            // Force diesel when low battery, disable switching
            if (batteryChargeNorm.Value <= emergencyBatteryLevel)
            {
                requestedMode = HybridMode.Diesel;
                batteryLow = true;
            }
            else
            {
                requestedMode = selectorMode;
                batteryLow = false;
            }

            // Keep resetting timer when selector is changed
            if (requestedMode != lastRequestedMode)
            {
                lastRequestedMode = requestedMode;
                isTransitioning = true;
                transitionTimer = modeTransitionDelay;
            }

            // Transition delay
            if (isTransitioning)
            {
                transitionTimer -= delta;

                if (transitionTimer > 0f)
                {
                    ConfigureDisconnectAll();

                    // Mode to indicate switching
                    activeMode.Value = (int)HybridMode.Transition;

                    // Do not proceed to current modes while transitioning
                    return;
                }

                // Transition finished
                transitionTimer = 0f;
                isTransitioning = false;

                currentMode = requestedMode;
            }

            activeMode.Value = (int)currentMode;
        }

        private void SimulateModeStates()
        {
            // Lamps: 0 off, 1 on, 2 blink
            switch (currentMode)
            {
                case HybridMode.Diesel:
                    {
                        ConfigureDieselMode();

                        dieselModeState.Value = 1;
                        batteryModeState.Value = 0;

                        break;
                    }

                case HybridMode.Battery:
                    {
                        ConfigureBatteryMode();

                        dieselModeState.Value = 0;
                        batteryModeState.Value = 1;

                        // wrap the state values in an if block later when charging is implemented for emergency indicator

                        break;
                    }
            }
        }

        private void CheckIfAllowedToCharge()
        {
            if (!allowedToCharge)
            {
                if (currentMode == HybridMode.Battery
                    && chargingOverride.Value != 1
                    && batteryChargeNorm.Value <= startChargeBatteryLevel)
                {
                    allowedToCharge = true;
                }
            }
            else
            {
                if (currentMode != HybridMode.Battery
                    || chargingOverride.Value == 1
                    || batteryChargeNorm.Value >= maxChargeBatteryLevel)
                {
                    allowedToCharge = false;
                }
            }
        }

        private void SimulateCharging()
        {
            if (allowedToCharge)
            {
                throttleDeOut.Value = chargingThrottle;

                float resistance = CalculateChargeResistance();

                smerGenOut.Value = resistance;
                effectiveResistanceOut.Value = resistance;
                genGoalPowerOut.Value = genTargetPwr;

                tmPowerInReadOut.Value = CalculateChargeAmount();
                tmTotalAmpsOut.Value = CalculateChargeAmps();

                isCharging = true;
                isChargingBattery.Value = 1;
            }
            else
            {
                throttleDeOut.Value = 0f;

                smerGenOut.Value = float.PositiveInfinity;
                effectiveResistanceOut.Value = float.PositiveInfinity;
                genGoalPowerOut.Value = 0f;

                tmPowerInReadOut.Value = tmPowerInRead.Value;
                tmTotalAmpsOut.Value = 0f;

                isCharging = false;
                isChargingBattery.Value = 0;
            }
        }

        private float CalculateChargeResistance()
        {
            float v = batteryVolts.Value;
            float voltsSq = v * v;

            float resistance = voltsSq / genTargetPwr;

            return resistance;
        }

        private float CalculateChargeAmps()
        {
            float resistance = CalculateChargeResistance();

            if (resistance <= 0f || float.IsInfinity(resistance))
                return 0f;

            return generatorVoltage.Value / resistance;
        }

        private float CalculateChargeAmount()
        {
            float chargeIn = -Mathf.Min(genPowerOutRead.Value, genTargetPwr);
            float powerDraw = tmPowerInRead.Value;

            return powerDraw + chargeIn;
        }

        private void ConfigureDisconnectAll()
        {
            voltageBusOut.Value = 0f;

            throttleDeOut.Value = 0f;

            smerGenOut.Value = float.PositiveInfinity;
            effectiveResistanceOut.Value = float.PositiveInfinity;
            genGoalPowerOut.Value = 0f;

            tmPowerInReadOut.Value = 0f;
            throttleBatOut.Value = 0f;
            smerBatOut.Value = float.PositiveInfinity;
        }

        private void ConfigureDieselMode()
        {
            voltageBusOut.Value = generatorVoltage.Value;

            throttleDeOut.Value = throttle.Value;

            smerGenOut.Value = smer.Value;
            effectiveResistanceOut.Value = effectiveResistance.Value;
            genGoalPowerOut.Value = genGoalPower.Value;

            tmTotalAmpsOut.Value = tmTotalAmps.Value;
            tmTransitionCurrentLimitOut.Value = tmTransitionCurrentLimit.Value;

            tmPowerInReadOut.Value = 0f;
            throttleBatOut.Value = 0f;
            smerBatOut.Value = float.PositiveInfinity;

            allowedToCharge = false;
        }

        private void ConfigureBatteryMode()
        {
            voltageBusOut.Value = voltRegVoltage.Value;

            CheckIfAllowedToCharge();
            SimulateCharging();

            tmTransitionCurrentLimitOut.Value = float.PositiveInfinity;

            throttleBatOut.Value = throttle.Value;
            smerBatOut.Value = smer.Value;
        }
    }
}
