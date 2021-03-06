﻿using NTMiner.ServiceContracts.DataObjects;
using System;

namespace NTMiner.Core.Profiles {
    public interface ICoinProfileSet {
        ICoinProfile GetCoinProfile(Guid coinId);
        void SetCoinProfileProperty(Guid coinId, string propertyName, object value);
    }
}
