﻿using System;
using NHS111.Models.Models.Business;
using NHS111.Models.Models.Business.Enums;

namespace NHS111.Business.DOS
{
    public interface IServiceAvailabilityProfile
    {
        int ProfileId { get; set; }

        string ProfileName { get; set; }

        DispositionTimePeriod GetServiceAvailability(DateTime dispositionDateTime, int timeFrameMinutes);
    }
}
