using CCL.Importer.Implementations;
using LocoSim.Definitions;
using LocoSim.Implementations;

namespace CCL.Importer.Components.Simulation
{
    internal class TractionControlDefinitionInternal : SimComponentDefinition
    {
        public float maximumSlipTime = 0.1f;
        public float throttleReduction = 0.1f;

        public string powerFuseId = string.Empty;

        public readonly PortReferenceDefinition toggle =
            new(PortValueType.CONTROL, "TC_ENABLE");
        public readonly PortReferenceDefinition throttle =
            new(PortValueType.CONTROL, "THROTTLE_IN");
        public readonly PortReferenceDefinition wheelSpeed =
            new(PortValueType.GENERIC, "WHEEL_SPEED");
        public readonly PortReferenceDefinition forwardSpeed =
            new(PortValueType.GENERIC, "FORWARD_SPEED");

        public readonly PortDefinition controlledThrottle =
            new(PortType.READONLY_OUT, PortValueType.CONTROL, "CONTROLLED_THROTTLE");
        public readonly PortDefinition slipTime =
            new(PortType.READONLY_OUT, PortValueType.GENERIC, "SLIP_TIME");
        public readonly PortDefinition skipNotches =
            new(PortType.READONLY_OUT, PortValueType.GENERIC, "SKIP_NOTCHES");

        public override SimComponent InstantiateImplementation()
        {
            return new TractionControl(this);
        }
    }
}
