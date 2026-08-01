using CCL.Importer.Components.Simulation;
using LocoSim.Implementations;
using System;
using UnityEngine;

namespace CCL.Importer.Implementations
{
    internal class TractionControl : SimComponent
    {
        private readonly FuseReference powerFuseRef;

        private readonly PortReference toggleOn;
        private readonly PortReference throttle;
        private readonly PortReference wheelSpeed;
        private readonly PortReference forwardSpeed;

        private readonly Port controlledThrottle;
        private readonly Port slipTimeOut;
        private readonly Port skipNotchesOut;

        private readonly float maxTime;
        private readonly float notchValue;

        private float slipTime;
        private int skipNotches;
        private int maxNotches = 9;

        public TractionControl(TractionControlDefinitionInternal def) : base(def.ID)
        {
            powerFuseRef = AddFuseReference(def.powerFuseId);

            maxTime = def.maximumSlipTime;
            notchValue = def.throttleReduction;

            toggleOn = AddPortReference(def.toggle);
            throttle = AddPortReference(def.throttle);
            wheelSpeed = AddPortReference(def.wheelSpeed);
            forwardSpeed = AddPortReference(def.forwardSpeed);

            controlledThrottle = AddPort(def.controlledThrottle);

            slipTimeOut = AddPort(def.slipTime);
            skipNotchesOut = AddPort(def.skipNotches);
        }

        public override void Tick(float delta)
        {
            if (!powerFuseRef.State || toggleOn.Value < 0.1)
            {
                slipTime = 0f;
                skipNotches = 0;
                controlledThrottle.Value = throttle.Value;
                return;
            }

            float wheelSpdAbs = Mathf.Abs(wheelSpeed.Value);
            float fwdSpdAbsKmh = Mathf.Abs(forwardSpeed.Value) * 3.6f;
            if (wheelSpdAbs > fwdSpdAbsKmh + 0.5f)
            {
                slipTime += delta;

                if (slipTime > maxTime)
                {
                    slipTime -= maxTime;
                    skipNotches++;
                }
            }
            else
            {
                slipTime -= delta;
                if (slipTime < -maxTime)
                {
                    slipTime += maxTime;
                    skipNotches--;
                }
            }

            skipNotches = Mathf.Clamp(skipNotches, 0, maxNotches);
            skipNotchesOut.Value = skipNotches;

            if (skipNotches > 0)
            {
                controlledThrottle.Value = Mathf.Clamp01(throttle.Value - notchValue * skipNotches);
            }
            else
            {
                controlledThrottle.Value = throttle.Value;
            }

            slipTime = Mathf.Clamp(slipTime, -maxTime, maxTime);

            slipTimeOut.Value = slipTime;
        }
    }
}
