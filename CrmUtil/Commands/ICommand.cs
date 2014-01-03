﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmUtil.Commands
{
    interface ICommand : IDisposable
    {
        void Execute();
        void Dispose();
    }
}