using CCL.Types.Proxies.Ports;
using System.Collections.Generic;
using UnityEngine;

namespace CCL.Types.Components.Simulation
{
    [AddComponentMenu("CCL/Components/Simulation/Traction Control Definition")]
    public class TractionControlDefinition : SimComponentDefinitionProxy
    {
        public float maximumSlipTime = 0.1f;
        public float throttleReduction = 0.1f;

        [FuseId]
        public string powerFuseId = string.Empty;

        public override IEnumerable<PortReferenceDefinition> ExposedPortReferences => new[]
        {
            new PortReferenceDefinition(DVPortValueType.CONTROL, "TC_ENABLE"),
            new PortReferenceDefinition(DVPortValueType.CONTROL, "THROTTLE_IN"),
            new PortReferenceDefinition(DVPortValueType.GENERIC, "WHEEL_SPEED"),
            new PortReferenceDefinition(DVPortValueType.GENERIC, "FORWARD_SPEED")
        };

        public override IEnumerable<PortDefinition> ExposedPorts => new[]
        {
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.CONTROL, "CONTROLLED_THROTTLE"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.GENERIC, "SLIP_TIME"),
            new PortDefinition(DVPortType.READONLY_OUT, DVPortValueType.GENERIC, "SKIP_NOTCHES")
        };

        public IEnumerable<string> GetDebugPorts() => new[]
        {
            "SLIP_TIME",
            "SKIP_NOTCHES"
        };
    }
}
