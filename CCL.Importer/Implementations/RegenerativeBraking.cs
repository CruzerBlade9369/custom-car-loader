using CCL.Importer.Components.Simulation;
using LocoSim.Implementations;
using UnityEngine;

namespace CCL.Importer.Implementations
{
    internal class RegenerativeBraking : SimComponent
    {
        private readonly FuseReference powerFuseRef;

        private readonly PortReference tmPowerInRead;
        private readonly PortReference tmPowerOutRead;
        private readonly PortReference dynamicBrkActive;
        private readonly PortReference batteryChargeNorm;

        private readonly Port powerOut;
        private readonly Port isActive;

        private readonly float maxRegenPowerW;
        private readonly float maxRechargeNorm;

        public RegenerativeBraking(RegenerativeBrakingDefinitionInternal def) : base(def.ID)
        {
            powerFuseRef = AddFuseReference(def.powerFuseId);

            tmPowerInRead = AddPortReference(def.tmPowerInRead);
            tmPowerOutRead = AddPortReference(def.tmPowerOutRead);
            dynamicBrkActive = AddPortReference(def.dynamicBrkActive);
            batteryChargeNorm = AddPortReference(def.batteryChargeNorm);

            powerOut = AddPort(def.powerOut);
            isActive = AddPort(def.isActive);

            maxRegenPowerW = def.maxRegenPowerW;
            maxRechargeNorm = def.maximumBatteryRechargeLevel;
        }

        public override void Tick(float delta)
        {
            if (!powerFuseRef.State)
            {
                isActive.Value = 0f;
                return;
            }

            if (dynamicBrkActive.Value >= 0.1f && batteryChargeNorm.Value < maxRechargeNorm)
            {
                isActive.Value = 1f;
                powerOut.Value = Mathf.Clamp(tmPowerOutRead.Value, -maxRegenPowerW, 0);
            }
            else
            {
                isActive.Value = 0f;
                powerOut.Value = tmPowerInRead.Value;
            }
        }
    }
}
