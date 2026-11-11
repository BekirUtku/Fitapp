namespace FitApp.Application.Features.WeeklyPlans;

public record WeeklyPlanListItemDto(Guid Id, string Name, bool IsActive);

public record WeeklyPlanDayDto(int DayOfWeek, Guid? TemplateId, string? TemplateName);

public record WeeklyPlanDetailDto(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyList<WeeklyPlanDayDto> Days);

public record CreateWeeklyPlanRequest(string Name);

public record UpdateWeeklyPlanRequest(string Name, bool IsActive);

public record AssignDayRequest(int DayOfWeek, Guid? TemplateId);
