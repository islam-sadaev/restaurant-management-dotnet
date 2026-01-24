using AutoMapper;
using Restaurant.Models;
using Restaurant.ViewModels;

namespace Restaurant.Configuration
{

    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // 1. Van Registratie Stap1 naar Stap 2 (Confirm)
            CreateMap<RegisterViewModel, RegisterConfirmViewModel>();

            // 2. Van Confirm scherm naar de Database User (CustomUser)
            CreateMap<RegisterConfirmViewModel, CustomUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // 3. Van Database naar MyAccount Scherm
            CreateMap<CustomUser, AccountViewModel>();

            // 4. Van MyAccount Scherm terug naar Databasse (Update)
            CreateMap<AccountViewModel, CustomUser>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore());
        }
    }
}