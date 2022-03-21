using AutoMapper;
using TaxationService.Domain.Models;
using TaxationService.Domain.Models.TaxJarModel;
using TaxationService.Domain.Models.TaxServiceModel;

namespace TaxationService.Domain.Mappers
{
    public class TaxJarProfile : Profile
    {
        public TaxJarProfile()
        {
            //map tax rate for location request to taxJar rate request 
            CreateMap<TaxRateRequest, Rate>();

            CreateMap<LineItemRequest, TaxLineItem>();

            //map application service layer tax for order request to taxJar calculator layer request
            CreateMap<TaxForOrderRequest, Tax>()
                .ForMember(dest => dest.FromCity, act => act.MapFrom(src => src.Seller.City))
                .ForMember(dest => dest.FromState, act => act.MapFrom(src => src.Seller.State))
                .ForMember(dest => dest.FromCountry, act => act.MapFrom(src => src.Seller.Country))
                .ForMember(dest => dest.FromZip, act => act.MapFrom(src => src.Seller.Zip))
                .ForMember(dest => dest.ToCity, act => act.MapFrom(src => src.CustomerAddress.City))
                .ForMember(dest => dest.ToState, act => act.MapFrom(src => src.CustomerAddress.State))
                .ForMember(dest => dest.ToCountry, act => act.MapFrom(src => src.CustomerAddress.Country))
                .ForMember(dest => dest.ToZip, act => act.MapFrom(src => src.CustomerAddress.Zip))
                .ForMember(dest => dest.Amount, act => act.MapFrom(src => src.Amount))
                .ForMember(dest => dest.Shipping, act => act.MapFrom(src => src.Shipping));
        }
    }
}
