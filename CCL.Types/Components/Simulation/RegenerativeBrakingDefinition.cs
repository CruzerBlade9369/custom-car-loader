using CCL.Types.Proxies.Ports;
using System.Collections.Generic;
using UnityEngine;

namespace CCL.Types.Components.Simulation
{
    [AddComponentMenu("CCL/Components/Simulation/Regenerative Braking Definition")]
    public class RegenerativeBrakingDefinition : SimComponentDefinitionProxy
    {
        public float maxRegenPower = 200000f;
        public float maximumBatteryRechargeLevel = 0.95f;

        [FuseId]
        public string powerFuseId = string.Empty;

        public override IEnumerable<PortReferenceDefinition> ExposedPortReferences => new[]
        {
            new PortReferenceDefinition(DVPortValueType.POWER, "TM_POWER_IN_READ"),
            new PortReferenceDefinition(DVPortValueType.POWER, "TM_POWER_OUT_READ"),
            new PortReferenceDefinition(DVPortValueType.STATE, "DYNAMIC_BRAKE_ACTIVE"),
            new PortReferenceDefinition(DVPortValueType.ELECTRIC_CHARGE, "BATTERY_CHARGE_NORMALIZED")
        };

        public override IEnumerable<PortDefinition> ExposedPorts => new[]
        {
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.POWER, "POWER_OUT"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.STATE, "REGEN_BRAKE_ACTIVE")
        };
    }
}
