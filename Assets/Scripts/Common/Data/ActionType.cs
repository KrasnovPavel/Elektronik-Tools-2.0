﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elektronik.Common.Data
{
    public enum ActionType : byte
    {
        Create = 0,
        Tint = 1,
        Move = 2,
        Remove = 3,
        Fuse = 4,
        Connect = 5,
    }
}
