﻿using AutoMapper;
using NHS111.Models.Models.Domain;
using NHS111.Models.Models.Web.Clock;
using NHS111.Models.Models.Web.FromExternalServices;
using NHS111.Models.Models.Web.Validators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DayOfWeek = System.DayOfWeek;
using TimeOfDay = NHS111.Models.Models.Web.FromExternalServices.TimeOfDay;

namespace NHS111.Models.Models.Web
{
    public class ServiceViewModel : Business.DosService
    {
        private const string OpenAllHoursMessage = "Open today: 24 hours";
        private const string ServiceClosedMessage = "Closed";
        private readonly IEnumerable<long> _callbackCASIdList = new List<long> { 130, 133, 137, 138 };
        private readonly IEnumerable<long> _callBackPharmacyCASIdList = new List<long>() { 137, 138 };
        private readonly IClock _clock;
        private readonly IEnumerable<long> _gotoEDIdList = new List<long> { 40, 105, 120 };
        private readonly long _oohServiceId = 25;
        private List<string> _addressLines;

        public ServiceViewModel() : this(new SystemClock())
        {
        }

        public ServiceViewModel(IClock clock) : base(clock)
        {
            _clock = clock;
        }

        public List<string> AddressLines
        {
            get { return _addressLines ?? (_addressLines = BuildFormattedAddressLines(WebUtility.HtmlDecode(Address))); }
        }

        public string CurrentStatus
        {
            get
            {
                return !IsOpenToday
                    ? ServiceClosedMessage
                    : OpenAllHours
                    ? OpenAllHoursMessage
                    : CurrentRotaSession != null
                    ? ServiceOpeningTimesMessage
                    : ServiceClosedMessage;
            }
        }

        public bool IsCallbackService
        {
            get { return _callbackCASIdList.Contains(ServiceType.Id) || IsOohService; }
        }

        public bool IsCallbackServiceNotOfferingCallback
        {
            get { return IsCallbackService && !OnlineDOSServiceType.Equals(OnlineDOSServiceType.Callback); }
        }

        public bool IsCallbackServiceOfferingCallback
        {
            get { return IsCallbackService && OnlineDOSServiceType.Equals(OnlineDOSServiceType.Callback); }
        }

        public bool IsCASCallbackServiceWithNoAddress
        {
            get { return IsCallbackService && !IsOohService && !ShouldShowAddress; }
        }

        public bool IsGoToEdService
        {
            get { return _gotoEDIdList.Contains(ServiceType.Id); }
        }

        public bool IsNotACallbackServiceWithPublicName
        {
            get { return !string.IsNullOrEmpty(PublicNameOnly) && !IsCallbackService; }
        }

        public bool IsOohService
        {
            get { return ServiceType.Id.Equals(_oohServiceId); }
        }

        public bool IsOohServiceWithCallback
        {
            get { return ServiceType.Id.Equals(_oohServiceId) && OnlineDOSServiceType.Equals(OnlineDOSServiceType.Callback); }
        }

        public bool IsOohServiceWithCallbackAndNoPublicName
        {
            get { return IsOohServiceWithCallback && string.IsNullOrEmpty(PublicNameOnly); }
        }

        public bool IsOpenToday
        {
            get
            {
                return OpenAllHours ? true : !TodaysRotaSessions.All(c => _clock.Now.TimeOfDay > c.ClosingTime);
            }
        }

        public IEnumerable<RotaSession> NextOpenDayRotaSessions
        {
            get
            {
                return OpenAllHours || ServiceClosed ? new List<RotaSession>() : GetRotaSessions(NextRotaSession.Day).ToList();
            }
        }

        public string NextOpeningTimePrefixMessage
        {
            get
            {
                if (ServiceClosed || OpenAllHours)
                {
                    return "";
                }

                var rotaSession = CurrentRotaSession;
                string openingTense = (IsOpen) ? "Open" : "Opens";

                if (rotaSession == null)
                {
                    rotaSession = NextRotaSession;
                }

                return string.Format("{0} {1}:", openingTense, GetDayMessage(rotaSession.Day));
            }
        }

        public Dictionary<DayOfWeek, string> OpeningTimes
        {
            get
            {
                if (OpenAllHours)
                {
                    return new Dictionary<DayOfWeek, string>();
                }

                var daysOfWeek = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToArray();
                return daysOfWeek.ToDictionary(day => day, GetOpeningTimes);
            }
        }

        public bool ServiceClosed
        {
            get { return (RotaSessionsAndSpecifiedSessions == null || !RotaSessionsAndSpecifiedSessions.Any()); }
        }

        public string ServiceOpeningTimesMessage
        {
            get
            {
                if (OpenAllHours)
                {
                    return OpenAllHoursMessage;
                }

                if (ServiceClosed)
                {
                    return ServiceClosedMessage;
                }

                var rotaSession = CurrentRotaSession;
                string openingTense = (IsOpen) ? "Open" : "Opens";

                if (rotaSession == null)
                    rotaSession = NextRotaSession;

                return string.Format("{0} {1}: {2} until {3}",
                    openingTense,
                    GetDayMessage(rotaSession.Day),
                    DateTime.Today.Add(rotaSession.OpeningTime).ToString("HH:mm"),
                    DateTime.Today.Add(rotaSession.ClosingTime).ToString("HH:mm"));
            }
        }

        public bool ShouldShowAddress
        {
            get
            {
                return OnlineDOSServiceType.Equals(OnlineDOSServiceType.ReferRingAndGo) ||
                       OnlineDOSServiceType.Equals(OnlineDOSServiceType.GoTo);
            }
        }

        public static string GetServiceTypeAliasValue(OutcomeViewModel model)
        {
            if (!model.OutcomeGroup.IsServiceFirst && !model.OutcomeGroup.IsAccidentAndEmergencySexualAssault)
                return string.Empty;
            if (model.DosCheckCapacitySummaryResult.ResultListEmpty)
                return "no-results";
            var firstService = model.DosCheckCapacitySummaryResult.Success.FirstService;
            var recommendedService = Mapper.Map<RecommendedServiceViewModel>(firstService);
            return recommendedService.IsCallbackServiceNotOfferingCallback ? string.Empty : firstService.ServiceTypeAlias;
        }

        public string GetOtherServicesServiceDisplayHtml(OutcomeGroup outcomeGroup)
        {
            var serviceDisplayHtml = GetServiceTypeAliasHtml(outcomeGroup);
            if (IsCallbackServiceNotOfferingCallback && !ShouldShowAddress)
                return serviceDisplayHtml;

            serviceDisplayHtml += GetOtherServicesSecondLineHtml();
            return serviceDisplayHtml;
        }

        public string GetServiceDisplayHtml(OutcomeGroup outcomeGroup)
        {
            var serviceDisplayHtml = GetServiceTypeAliasHtml(outcomeGroup);
            if (IsCallbackServiceOfferingCallback && !IsOohService)
                return serviceDisplayHtml;

            serviceDisplayHtml += GetServiceNameHtml();

            if (!ShouldShowAddress)
                return serviceDisplayHtml;

            serviceDisplayHtml += GetServiceAddressHtml();
            return serviceDisplayHtml;
        }

        public bool IsPharmacyCASCallback()
        {
            return _callBackPharmacyCASIdList.Contains(ServiceType.Id);
        }

        public bool ShouldShowOtherServicesServiceTypeDescription(bool isFromOtherServices)
        {
            return !string.IsNullOrEmpty(ServiceTypeDescription) && isFromOtherServices && !IsCallbackServiceNotOfferingCallback;
        }

        public bool ShouldShowServiceTypeDescription()
        {
            return !string.IsNullOrEmpty(ServiceTypeDescription) && !IsCallbackServiceNotOfferingCallback;
        }

        private RotaSession NextRotaSession
        {
            get
            {
                var closedTime = _clock.Now;

                if (CurrentRotaSession != null)
                {
                    return CurrentRotaSession;
                }
                else
                {
                    ServiceCareItemRotaSession nextSession = GetNextDayServiceOpens((int)closedTime.DayOfWeek);
                    return new RotaSession() { Day = (DayOfWeek)nextSession.StartDayOfWeek, OpeningTime = new TimeSpan(nextSession.StartTime.Hours, nextSession.StartTime.Minutes, 0), ClosingTime = new TimeSpan(nextSession.EndTime.Hours, nextSession.EndTime.Minutes, 0) };
                }
            }
        }
        public bool IsSarcService
        {
            get
            {
                return
                    PublicName.ToLower().Contains("sarc") ||
                    PublicName.ToLower().Contains("sexual assault referral centre") ||
                    Name.ToLower().Contains("sarc") ||
                    Name.ToLower().Contains("sexual assault referral centre");
            }
        }

        private static int IncrementDayOfWeek(int dayOfWeek)
        {
            if (dayOfWeek > 5)
                return 0;
            return dayOfWeek + 1;
        }

        private List<string> BuildFormattedAddressLines(string unformattedAddress)
        {
            return String.IsNullOrEmpty(unformattedAddress)
                ? new List<string>()
                : unformattedAddress.Split(',').ToList()
                    .Select(a => IsAPostcode(a) ? a.Trim() : TitleCaseAddressLine(a)).ToList();
        }

        private string GetDayMessage(DayOfWeek day)
        {
            if (_clock.Now.DayOfWeek == day)
                return "today";
            if (_clock.Now.AddDays(1).DayOfWeek == day)
                return "tomorrow";
            return CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)day];
        }

        private ServiceCareItemRotaSession GetNextDayServiceOpens(int closedDay)
        {
            int daysCounted = 0;
            var dayToCheck = closedDay;

            do
            {
                dayToCheck = IncrementDayOfWeek(dayToCheck);

                var sessionsForDay = RotaSessionsAndSpecifiedSessions.Where(rs => (int)rs.StartDayOfWeek == dayToCheck).OrderBy(rs => new TimeSpan(rs.StartTime.Hours, rs.StartTime.Minutes, 0));

                if (sessionsForDay.Any())
                    return sessionsForDay.FirstOrDefault();

                daysCounted++;
            }
            while (daysCounted < 7);

            return new ServiceCareItemRotaSession();
        }

        private string GetOpeningTimes(DayOfWeek day)
        {
            if (RotaSessionsAndSpecifiedSessions == null)
                return ServiceClosedMessage;

            var rotaSession = RotaSessionsAndSpecifiedSessions.FirstOrDefault(rs => (int)rs.StartDayOfWeek == (int)day);
            return rotaSession != null
                ? string.Format("{0} - {1}", GetTime(rotaSession.StartTime), GetTime(rotaSession.EndTime))
                : ServiceClosedMessage;
        }

        private string GetOtherServicesSecondLineHtml()
        {
            if (IsCASCallbackServiceWithNoAddress)
                return string.Empty;

            if (IsOohServiceWithCallbackAndNoPublicName)
                return string.Empty;

            if (IsNotACallbackServiceWithPublicName)
                return string.Format("<br />{0}", WebUtility.HtmlDecode(PublicName));

            if (!ShouldShowAddress || IsGoToEdService)
                return string.Format("<br />{0}", WebUtility.HtmlDecode(PublicName));

            var firstLineOfAddress = AddressLines.FirstOrDefault(a => !string.IsNullOrEmpty(a));
            return string.Format("<br />{0}", WebUtility.HtmlDecode(firstLineOfAddress));
        }

        private IEnumerable<RotaSession> GetRotaSessions(DayOfWeek day)
        {
            return RotaSessionsAndSpecifiedSessions.Where(rs => (int)rs.StartDayOfWeek == (int)day).Select(rs => new RotaSession()
            {
                OpeningTime = new TimeSpan(rs.StartTime.Hours, rs.StartTime.Minutes, 0),
                ClosingTime = new TimeSpan(rs.EndTime.Hours, rs.EndTime.Minutes, 0),
                Day = (DayOfWeek)rs.StartDayOfWeek,
            });
        }

        private string GetServiceAddressHtml()
        {
            var fullAddressHtml = AddressLines.Where(address => !string.IsNullOrEmpty(address)).Aggregate(string.Empty, (current, address) => current + string.Format("{0}<br />", WebUtility.HtmlDecode(address)));
            return string.Format("<br />{0}", fullAddressHtml);
        }

        private string GetServiceNameHtml()
        {
            if (IsCallbackServiceNotOfferingCallback)
                return string.Empty;

            if (IsGoToEdService)
                return string.Format("<br />{0}", WebUtility.HtmlDecode(PublicName));

            if ((IsOohService || ShouldShowAddress) && string.IsNullOrEmpty(PublicNameOnly))
                return string.Empty;

            return string.Format("<br />{0}", !string.IsNullOrEmpty(PublicNameOnly) ? WebUtility.HtmlDecode(PublicNameOnly) : WebUtility.HtmlDecode(PublicName));
        }

        private string GetServiceTypeAliasHtml(OutcomeGroup outcomeGroup)
        {
            if (IsPharmacyCASCallback() && outcomeGroup.IsPharmacy)
                return GetServiceTypePharmacyCASAliasHtml();

            var serviceTypeAlias = IsCallbackServiceNotOfferingCallback ? PublicName : ServiceTypeAlias;
            return string.Format("<b class=\"service-details__alias\">{0}</b>", WebUtility.HtmlDecode(serviceTypeAlias));
        }

        private string GetServiceTypePharmacyCASAliasHtml()
        {
            var serviceTypeAlias = IsCallbackServiceNotOfferingCallback ? PublicName : "Book a call with a pharmacist";
            return string.Format("<b class=\"service-details__alias\">{0}</b>", WebUtility.HtmlDecode(serviceTypeAlias));
        }

        private string GetTime(TimeOfDay time)
        {
            return DateTime.Today.Add(new TimeSpan(time.Hours, time.Minutes, 0)).ToString("h:mmtt").ToLower();
        }

        private bool IsAPostcode(string addressLine)
        {
            var postcodeRegex = new Regex(PostCodeFormatValidator.PostcodeRegex, RegexOptions.IgnoreCase);
            return postcodeRegex.IsMatch(addressLine.Replace(" ", ""));
        }

        private string TitleCaseAddressLine(string addressLine)
        {
            return new CultureInfo("en-US", false).TextInfo
                .ToTitleCase(addressLine.ToLower()).Trim();
        }
        public RotaSession CurrentRotaSession
        {
            get
            {
                return TodaysRotaSessions
                    .OrderBy(rs => rs.OpeningTime)
                    .FirstOrDefault(rs => _clock.Now.TimeOfDay < rs.ClosingTime);
            }
        }

    }
    public class RotaSession
    {
        public TimeSpan ClosingTime { get; set; }
        public DayOfWeek Day { get; set; }
        public TimeSpan OpeningTime { get; set; }
    }

}
