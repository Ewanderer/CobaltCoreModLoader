﻿using CobaltCoreModding.Definitions.ExternalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    public interface ICharacterRegistry
    {
        bool RegisterCharacter(ExternalCharacter character);
    }
}
