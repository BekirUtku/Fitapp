using AutoMapper;
using FitApp.Application.Features.Catalog;
using FitApp.Application.Features.Exercises;
using FitApp.Application.Features.Sessions;
using FitApp.Application.Features.Templates;
using FitApp.Application.Features.WeeklyPlans;
using FitApp.Domain.Entities;

namespace FitApp.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MuscleGroup, MuscleGroupDto>()
            .ForCtorParam("BodyRegion", opt => opt.MapFrom(s => s.BodyRegion.ToString()));

        CreateMap<Equipment, EquipmentDto>();

        CreateMap<Exercise, ExerciseListItemDto>()
            .ForCtorParam("MetricType", opt => opt.MapFrom(s => s.MetricType.ToString()))
            .ForCtorParam("PrimaryMuscleGroupName", opt => opt.MapFrom(s => s.PrimaryMuscleGroup!.Name))
            .ForCtorParam("EquipmentName", opt => opt.MapFrom(s => s.Equipment != null ? s.Equipment.Name : null));

        CreateMap<Exercise, ExerciseDetailDto>()
            .ForCtorParam("MetricType", opt => opt.MapFrom(s => s.MetricType.ToString()))
            .ForCtorParam("PrimaryMuscleGroupName", opt => opt.MapFrom(s => s.PrimaryMuscleGroup!.Name))
            .ForCtorParam("EquipmentName", opt => opt.MapFrom(s => s.Equipment != null ? s.Equipment.Name : null))
            .ForCtorParam("SecondaryMuscleGroupIds",
                opt => opt.MapFrom(s => s.SecondaryMuscles.Select(m => m.MuscleGroupId).ToList()));

        CreateMap<WorkoutTemplate, TemplateListItemDto>()
            .ForCtorParam("ExerciseCount", opt => opt.MapFrom(s => s.Exercises.Count));

        CreateMap<WorkoutTemplate, TemplateDetailDto>()
            .ForCtorParam("Exercises", opt => opt.MapFrom(s => s.Exercises.OrderBy(e => e.OrderIndex)));

        CreateMap<TemplateExercise, TemplateExerciseDto>()
            .ForCtorParam("ExerciseName", opt => opt.MapFrom(s => s.Exercise!.Name))
            .ForCtorParam("LoadType", opt => opt.MapFrom(s => s.LoadType.ToString()))
            .ForCtorParam("ProgressionRule", opt => opt.MapFrom(s => s.ProgressionRule.ToString()))
            .ForCtorParam("Sets", opt => opt.MapFrom(s => s.Sets.OrderBy(x => x.SetNumber)));

        CreateMap<TemplateSet, TemplateSetDto>()
            .ForCtorParam("SetType", opt => opt.MapFrom(s => s.SetType.ToString()));

        CreateMap<WeeklyPlan, WeeklyPlanListItemDto>();

        CreateMap<WeeklyPlan, WeeklyPlanDetailDto>()
            .ForCtorParam("Days", opt => opt.MapFrom(s => s.Days.OrderBy(d => d.DayOfWeek)));

        CreateMap<WeeklyPlanDay, WeeklyPlanDayDto>()
            .ForCtorParam("TemplateName", opt => opt.MapFrom(s => s.Template != null ? s.Template.Name : null));

        CreateMap<WorkoutSession, SessionListItemDto>()
            .ForCtorParam("Status", opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<WorkoutSession, SessionDetailDto>()
            .ForCtorParam("Status", opt => opt.MapFrom(s => s.Status.ToString()))
            .ForCtorParam("Exercises", opt => opt.MapFrom(s => s.Exercises.OrderBy(e => e.OrderIndex)));

        CreateMap<SessionExercise, SessionExerciseDto>()
            .ForCtorParam("ExerciseName", opt => opt.MapFrom(s => s.Exercise!.Name))
            .ForCtorParam("MetricType", opt => opt.MapFrom(s => s.Exercise!.MetricType.ToString()))
            .ForCtorParam("Sets", opt => opt.MapFrom(s => s.Sets.OrderBy(x => x.SetNumber)));

        CreateMap<WorkoutSet, WorkoutSetDto>()
            .ForCtorParam("SetType", opt => opt.MapFrom(s => s.SetType.ToString()));
    }
}
