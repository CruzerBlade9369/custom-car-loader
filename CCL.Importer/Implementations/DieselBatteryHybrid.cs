using CCL.Importer.Components.Simulation;
using LocoSim.Implementations;

namespace CCL.Importer.Implementations
{
    internal class DieselBatteryHybrid : SimComponent
    {
        private readonly FuseReference powerFuseRef;

        private readonly PortReference throttle;
        private readonly PortReference generatorVoltage;
        private readonly PortReference batteryVoltage;
        private readonly PortReference batteryChargeNorm;
        private readonly PortReference smer;
        private readonly PortReference effectiveResistance;
        private readonly PortReference genGoalPower;
        private readonly PortReference genGoalRpmNorm;
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
        private readonly Port genGoalRpmNormOut;
        private readonly Port chargingBatteryState;
        private readonly Port activeMode;
        private readonly Port dieselModeState;
        private readonly Port batteryModeState;

        private readonly float modeTransitionDelay;
        private readonly float maxChargeBatteryLevel;
        private readonly float startChargeBatteryLevel;
        private readonly float emergencyBatteryLevel;
        private readonly float maxBatteryRechargePowerW;

        private HybridMode selectorMode;
        private HybridMode requestedMode;
        private HybridMode lastRequestedMode;
        private HybridMode currentMode;
        private float transitionTimer;
        private bool isTransitioning;
        
        private bool batteryLow;

        private bool allowedToCharge;

        private float engineTargetRpmNorm;
        private float genTargetRpmNorm;
        private float genTargetPwr;

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
            maxBatteryRechargePowerW = def.maxBatteryRechargePowerW;

            powerFuseRef = AddFuseReference(def.powerFuseId);

            throttle = AddPortReference(def.throttle);
            generatorVoltage = AddPortReference(def.generatorVoltage);
            batteryVoltage = AddPortReference(def.batteryVoltage);
            batteryChargeNorm = AddPortReference(def.batteryChargeNorm);
            smer = AddPortReference(def.singleMotorEffectiveResistance);
            tmPowerInRead = AddPortReference(def.tmPowerIn);
            chargingOverride = AddPortReference(def.chargeDisableOverride);
            hybridMode = AddPortReference(def.hybridModeControl);

            throttleDeOut = AddPort(def.throttleDeOut);
            throttleBatOut = AddPort(def.throttleBatteryOut);
            smerGenOut = AddPort(def.SmerGen);
            smerBatOut = AddPort(def.SmerBatt);
            tmPowerInReadOut = AddPort(def.tmPowerInReadOut);
            voltageBusOut = AddPort(def.voltageBusOut);
            chargingBatteryState = AddPort(def.isChargingBattery);
            activeMode = AddPort(def.activeMode);
            dieselModeState = AddPort(def.dieselModeState);
            batteryModeState = AddPort(def.batteryModeState);

            allowedToCharge = false;

            engineTargetRpmNorm = 0.5f;
            genTargetRpmNorm = 0.5f;
            genTargetPwr = 200000f;
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

            CheckIfAllowedToCharge();
            SimulateCharging();
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
                if (batteryChargeNorm.Value >= maxChargeBatteryLevel)
                {
                    allowedToCharge = false;
                }
            }
        }

        private void SimulateCharging()
        {
            // placeholder targets for now
            if (allowedToCharge)
            {
                throttleDeOut.Value = engineTargetRpmNorm;

                float resistance = CalculateChargerResistance();

                smerGenOut.Value = resistance;
                effectiveResistanceOut.Value = resistance;
                genGoalPowerOut.Value = genTargetPwr;
                genGoalRpmNormOut.Value = genTargetRpmNorm;

                chargingBatteryState.Value = 1;
            }
            else
            {
                //throttleDeOut.Value = 0f;

                smerGenOut.Value = float.PositiveInfinity;
                effectiveResistanceOut.Value = float.PositiveInfinity;
                genGoalPowerOut.Value = 0f;
                genGoalRpmNormOut.Value = 0f;

                chargingBatteryState.Value = 0;
            }
        }

        private float CalculateChargerResistance()
        {
            float genV = generatorVoltage.Value;
            float currentVoltsSq = genV * genV;

            float resistance = currentVoltsSq / genTargetPwr;

            return resistance;
        }

        private void ConfigureDisconnectAll()
        {
            voltageBusOut.Value = 0f;

            throttleDeOut.Value = 0f;

            smerGenOut.Value = float.PositiveInfinity;
            effectiveResistanceOut.Value = float.PositiveInfinity;
            genGoalPowerOut.Value = 0f;
            genGoalRpmNormOut.Value = 0f;

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
            genGoalRpmNormOut.Value = genGoalRpmNorm.Value;

            tmPowerInReadOut.Value = 0f;
            throttleBatOut.Value = 0f;
            smerBatOut.Value = float.PositiveInfinity;
        }

        private void ConfigureBatteryMode()
        {
            voltageBusOut.Value = batteryVoltage.Value;

            //throttleDeOut.Value = 0f;

            //smerGenOut.Value = float.PositiveInfinity;
            //effectiveResistanceOut.Value = float.PositiveInfinity;
            //genGoalPowerOut.Value = 0f;
            //genGoalRpmNormOut.Value = 0f;

            tmPowerInReadOut.Value = tmPowerInRead.Value;
            throttleBatOut.Value = throttle.Value;
            smerBatOut.Value = smer.Value;
        }
    }
}
