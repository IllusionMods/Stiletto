using Stiletto.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stiletto.Models
{
    public class CustomPose
    {
        [FileProperty("angleThigh")]
        public float ThighAngle { get; set; }

        [FileProperty("angleLeg")]
        public float LegAngle { get; set; }

        [FileProperty("angleWaist")]
        public float WaistAngle { get; set; }

        [FileProperty("ankleAngle")]
        public float AnkleAngle{ get; set; }
    }
}
