﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NHS111.Models.Models.Web;
using NHS111.Models.Models.Web.ITK;

namespace NHS111.Models.Mappers.WebMappings
{
    public class FromOutcomeViewModelToSubmitEncounterToServiceRequest : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<OutcomeViewModel, CaseDetails>()
                .ConvertUsing<FromOutcomeViewModelToCaseDetailsConverter>();

            Mapper.CreateMap<OutcomeViewModel, PatientDetails>()
                .ConvertUsing<FromOutcomeViewModelToPatientDetailsConverter>();

            Mapper.CreateMap<OutcomeViewModel, ServiceDetails>()
                .ConvertUsing<FromOutcomeViewModelToServiceDetailsConverter>();

        }
    }

    public class FromOutcomeViewModelToCaseDetailsConverter : ITypeConverter<OutcomeViewModel, CaseDetails>
    {
        public CaseDetails Convert(ResolutionContext context)
        {
            var outcome = (OutcomeViewModel)context.SourceValue;
            var caseDetails = (CaseDetails)context.DestinationValue ?? new CaseDetails();

            caseDetails.ExternalReference = outcome.SessionId.ToString();
            caseDetails.DispositionCode = outcome.Id;
            caseDetails.DispositionName = outcome.Title;

            return caseDetails;
        }
    }

    public class FromOutcomeViewModelToPatientDetailsConverter : ITypeConverter<OutcomeViewModel, PatientDetails>
    {
        public PatientDetails Convert(ResolutionContext context)
        {
            var outcome = (OutcomeViewModel)context.SourceValue;
            var patientDetails = (PatientDetails)context.DestinationValue ?? new PatientDetails();

            patientDetails.Forename = outcome.UserInfo.FirstName;
            patientDetails.Surname = outcome.UserInfo.LastName;
            patientDetails.ServiceAddressPostcode = outcome.SelectedService.PostCode;
            patientDetails.TelephoneNumber = outcome.UserInfo.TelephoneNumber;
            patientDetails.HomeAddress = new Address()
            {
                PostalCode = string.IsNullOrEmpty(outcome.AddressInfoViewModel.PostCode) ? null : outcome.AddressInfoViewModel.PostCode,
                StreetAddressLine1 =
                    !string.IsNullOrEmpty(outcome.AddressInfoViewModel.HouseNumber)
                        ? string.Format("{0} {1}", outcome.AddressInfoViewModel.HouseNumber, outcome.AddressInfoViewModel.AddressLine1)
                        : outcome.AddressInfoViewModel.AddressLine1,
                StreetAddressLine2 = outcome.AddressInfoViewModel.AddressLine2,
                StreetAddressLine3 = outcome.AddressInfoViewModel.City,
                StreetAddressLine4 = outcome.AddressInfoViewModel.County,
                StreetAddressLine5 = outcome.AddressInfoViewModel.PostCode
            };
            if (outcome.UserInfo.Year != null && outcome.UserInfo.Month != null && outcome.UserInfo.Day != null)
                patientDetails.DateOfBirth =
                    new DateTime(outcome.UserInfo.Year.Value, outcome.UserInfo.Month.Value, outcome.UserInfo.Day.Value);

            patientDetails.Gender = outcome.UserInfo.Gender;
            if (outcome.UserInfo.CurrentAddress != null) patientDetails.CurrentLocationPostcode = outcome.UserInfo.CurrentAddress.PostCode;
            return patientDetails;
        }
    }

    public class FromOutcomeViewModelToServiceDetailsConverter : ITypeConverter<OutcomeViewModel, ServiceDetails>
    {
        public ServiceDetails Convert(ResolutionContext context)
        {
            var outcome = (OutcomeViewModel)context.SourceValue;
            var serviceDetails = (ServiceDetails)context.DestinationValue ?? new ServiceDetails();

            serviceDetails.Id = outcome.SelectedService.Id.ToString();
            serviceDetails.Name = outcome.SelectedService.Name;
            serviceDetails.OdsCode = outcome.SelectedService.OdsCode;
            serviceDetails.PostCode = outcome.SelectedService.PostCode;

            return serviceDetails;
        }
    }
}
