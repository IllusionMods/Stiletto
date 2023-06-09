using System.Linq;

namespace Stiletto.Models
{
    public class CustomPose : TextSerializer
    {
        private float _waistAngle;

        private float _mainThighAngle;
        private float _mainLegAngle;
        private float _mainAnkleAngle;

        private float _subThighAngle;
        private float _subLegAngle;
        private float _subAnkleAngle;

        public bool Split { get; set; }

        public float WaistAngle { get => _waistAngle; set => _waistAngle = value; }

        public float RightThighAngle { get => _mainThighAngle; set => _mainThighAngle = value; }

        public float RightLegAngle { get => _mainLegAngle; set => _mainLegAngle = value; }

        public float RightAnkleAngle { get => _mainAnkleAngle; set => _mainAnkleAngle = value; }

        public float LeftThighAngle
        {
            get
            {
                return Split ? _subThighAngle : _mainThighAngle;
            }
            set
            {
                if (!Split)
                {
                    _mainThighAngle = value;
                }
                _subThighAngle = value;
            }
        }

        public float LeftLegAngle
        {
            get
            {
                return Split ? _subLegAngle : _mainLegAngle;
            }
            set
            {
                if (!Split)
                {
                    _mainLegAngle = value;
                }
                _subLegAngle = value;
            }
        }

        public float LeftAnkleAngle
        {
            get
            {
                return Split ? _subAnkleAngle : _mainAnkleAngle;
            }
            set
            {
                if (!Split)
                {
                    _mainAnkleAngle = value;
                }
                _subAnkleAngle = value;
            }
        }

        public override void Deserialize(string value)
        {
            var args = value.Split(',').Select(x => x.Trim()).ToArray();

            if (args.Length <= 0) return;

            Split = args.Length > 4;
            float.TryParse(args.ElementAtOrDefault(0), out _waistAngle);

            float.TryParse(args.ElementAtOrDefault(1), out _mainThighAngle);
            float.TryParse(args.ElementAtOrDefault(2), out _mainLegAngle);
            float.TryParse(args.ElementAtOrDefault(3), out _mainAnkleAngle);

            float.TryParse(args.ElementAtOrDefault(4), out _subThighAngle);
            float.TryParse(args.ElementAtOrDefault(5), out _subLegAngle);
            float.TryParse(args.ElementAtOrDefault(6), out _subAnkleAngle);
        }

        public override string Serialize()
        {
            var output = $"{_waistAngle},{_mainThighAngle},{_mainLegAngle},{_mainAnkleAngle}";

            if (Split)
                output += $",{_subThighAngle},{_subLegAngle},{_subAnkleAngle}";

            return output;
        }
    }
}
