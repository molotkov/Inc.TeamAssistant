using Inc.TeamAssistant.Appraiser.Primitives;

namespace Inc.TeamAssistant.Reviewer.All;

internal static class Messages
{
    public static MessageId Reviewer_GetStarted = new(nameof(Reviewer_GetStarted));
    public static MessageId Reviewer_CreateTeamHelp = new(nameof(Reviewer_CreateTeamHelp));
    public static MessageId Reviewer_MoveToReviewHelp = new(nameof(Reviewer_MoveToReviewHelp));
    public static MessageId Reviewer_EnterTeamName = new(nameof(Reviewer_EnterTeamName));
    public static MessageId Reviewer_ConnectToTeam = new(nameof(Reviewer_ConnectToTeam));
    public static MessageId Reviewer_SelectTeam = new(nameof(Reviewer_SelectTeam));
    public static MessageId Reviewer_EnterRequestForReview = new(nameof(Reviewer_EnterRequestForReview));
    public static MessageId Reviewer_TeamMinError = new(nameof(Reviewer_TeamMinError));
    public static MessageId Reviewer_JoinToTeamSuccess = new(nameof(Reviewer_JoinToTeamSuccess));
    public static MessageId Reviewer_TeamNotFoundError = new(nameof(Reviewer_TeamNotFoundError));
    public static MessageId Reviewer_NeedReview = new(nameof(Reviewer_NeedReview));
    public static MessageId Reviewer_ReviewDeclined = new(nameof(Reviewer_ReviewDeclined));
    public static MessageId Reviewer_NewTaskForReview = new(nameof(Reviewer_NewTaskForReview));
    public static MessageId Reviewer_CancelHelp = new(nameof(Reviewer_CancelHelp));
    public static MessageId Reviewer_CancelDialogFail = new(nameof(Reviewer_CancelDialogFail));
    public static MessageId Reviewer_BeginDialogFail = new(nameof(Reviewer_BeginDialogFail));
    public static MessageId Reviewer_Accepted = new(nameof(Reviewer_Accepted));
}