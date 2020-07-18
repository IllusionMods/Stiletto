using System.Linq;
using static SaveData;

namespace Stiletto.Models
{
    public class CharaDisplayData
    {
        public CharaDisplayData(HeelInfo heelInfo)
        {
            Name = heelInfo.chaControl.fileParam.fullname;
            ChaControl = heelInfo.chaControl;
            HeelInfo = heelInfo;
        }

        public CharaDisplayData(CharaData charaData)
        {
            Name = charaData.Name;
            ChaControl = charaData.chaCtrl;
            HeelInfo = StilettoContext.HeelInfos.FirstOrDefault(x => x.chaControl == ChaControl);
        }

        public string Name { get; }

        public ChaControl ChaControl { get; }

        public HeelInfo HeelInfo { get; }

        public string HeelName => HeelInfo?.heelName;

        public string AnimationPath => HeelInfo?.animationPath;

        public string AnimationName => HeelInfo?.animationName;
    }
}
