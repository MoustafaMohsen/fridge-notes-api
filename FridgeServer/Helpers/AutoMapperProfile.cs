using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreUserIdentity.Models;
using FridgeServer.Models;
using FridgeServer.Models.Dto;

namespace FridgeServer.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(d => d.UserName,opt => opt.MapFrom(src => src.UserName))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.userFriends, opt => opt.MapFrom(src => src.userFriends))
                .ForMember(d => d.invitationcode, opt => opt.MapFrom(src=>src.secretId) )
                //Ignores
                .ForMember(d => d.role, opt => opt.Ignore() )
                .ForMember(d => d.token, opt => opt.Ignore() )
                ;
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(d => d.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(d => d.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ForMember(d => d.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(d => d.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(d => d.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(d => d.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()  ) )
                .ForMember(d => d.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(d => d.userFriends, opt => opt.MapFrom(src => src.userFriends))
                .ForMember(d => d.secretId, opt => opt.MapFrom(src=>src.invitationcode))
                //Ignores
                .ForMember(d => d.AccessFailedCount, opt => opt.Ignore())
                .ForMember(d => d.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(d => d.EmailConfirmed, opt => opt.Ignore())

                .ForMember(d => d.LockoutEnabled, opt => opt.Ignore())
                .ForMember(d => d.LockoutEnd, opt => opt.Ignore())

                .ForMember(d => d.PasswordHash, opt => opt.Ignore())
                .ForMember(d => d.PhoneNumber, opt => opt.Ignore())
                .ForMember(d => d.PhoneNumberConfirmed, opt => opt.Ignore())

                .ForMember(d => d.SecurityStamp, opt => opt.Ignore())
                .ForMember(d => d.TwoFactorEnabled, opt => opt.Ignore())

                .ForMember(d => d.userGroceries, opt => opt.Ignore())

                ;

            CreateMap<_IdentityUserDto, UserDto>()
                .ForMember(d=> d.userFriends ,opt=>opt.Ignore())
                .ForMember(d=> d.invitationcode ,opt=>opt.Ignore())
                ;

            CreateMap<UserDto, _IdentityUserDto>()
                ;
        }
    }
}
