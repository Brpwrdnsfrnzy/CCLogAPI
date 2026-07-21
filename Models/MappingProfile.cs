using AutoMapper;

namespace CCLogAPI.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateLogDto, Log>()
                .ForMember(dest => dest.ID, opt => opt.Ignore())
                .ForMember(dest => dest.Credits, opt => opt.Ignore())
                .ForMember(dest => dest.DiaryEntries, opt => opt.Ignore())
                .ForMember(dest => dest.GoodsReceivedNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.LastUpdatedByNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.LogTypeNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.LoggedByNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.SignOffReasonNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.SignedOffByNavigation, opt => opt.Ignore())
                .ForMember(dest => dest.StatusNavigation, opt => opt.Ignore())
                // Only set DateLogged when provided by the DTO so DB default can apply otherwise
                .ForMember(dest => dest.DateLogged, opt => opt.Condition(src => src.DateLogged.HasValue));
        }
    }
}