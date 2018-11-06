namespace Toggl.Multivac
{
    public struct RatingViewConfiguration
    {
        public int DayCount { get; }

        public RatingViewCriterion Criterion { get; }

        public RatingViewConfiguration(int dayCount, RatingViewCriterion criterion)
        {
            DayCount = dayCount;
            Criterion = criterion;
        }
    }

    public enum RatingViewCriterion
    {
        None,
        Stop,
        Start,
        Continue
    }

    public static class RatingViewCriterionExtensions
    {
        public static RatingViewCriterion ToRatingViewCriterion(this string criterion)
        {
            switch (criterion)
            {
                case "stop":
                    return RatingViewCriterion.Stop;
                case "start":
                    return RatingViewCriterion.Start;
                case "continue":
                    return RatingViewCriterion.Continue;
                default:
                    return RatingViewCriterion.None;
            }
        }
    }
}
