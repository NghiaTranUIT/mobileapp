using StoreKit;
using Toggl.Foundation.Services;

namespace Toggl.Daneel.Services
{
    public sealed class RatingServiceIos : IRatingService
    {
        public void AskForRating()
        {
            SKStoreReviewController.RequestReview();
        }
    }
}
