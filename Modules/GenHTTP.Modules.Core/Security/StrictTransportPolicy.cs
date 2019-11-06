﻿using System;

namespace GenHTTP.Modules.Core.Security
{

    public class StrictTransportPolicy
    {

        #region Get-/Setters

        public TimeSpan MaximumAge { get; }

        public bool IncludeSubdomains { get; }

        public bool Preload { get; }

        #endregion

        #region Initialization

        public StrictTransportPolicy(TimeSpan maximumAge, bool includeSubdomains, bool preload)
        {
            MaximumAge = maximumAge;
            IncludeSubdomains = includeSubdomains;
            Preload = preload;
        }

        #endregion

    }

}
