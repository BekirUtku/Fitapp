namespace FitApp.Domain.Enums;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public enum BodyRegion
{
    Upper = 0,
    Lower = 1,
    Core = 2
}

public enum MetricType
{
    WeightReps = 0,
    BodyweightReps = 1,
    WeightedBodyweight = 2,
    Duration = 3,
    DistanceDuration = 4
}

public enum SetType
{
    Normal = 0,
    WarmUp = 1,
    DropSet = 2,
    Failure = 3,
    Amrap = 4,
    RestPause = 5,
    Cluster = 6,
    Tempo = 7
}

public enum LoadType
{
    Absolute = 0,
    PercentageOf1Rm = 1
}

public enum ProgressionRule
{
    None = 0,
    DoubleProgression = 1,
    Linear = 2,
    PercentageBased = 3
}

public enum SessionStatus
{
    InProgress = 0,
    Completed = 1,
    Aborted = 2
}

public enum RecordType
{
    Estimated1Rm = 0,
    MaxWeight = 1,
    MaxReps = 2,
    MaxVolume = 3,
    MaxDuration = 4
}
