using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboticArmService
{
    public enum StatusInfo
    {
        Listen=0,
        ReadyToAcceptPathinfo=1,
        PaintProcessInprogress=2,
        CompletedArmPath=3,
        Wait=4
    }
}
