using FirstContact.Application.Features;
using FirstContact.Application.Knowledge;
using FirstContact.Application.Requests;
using FirstContact.Application.Tests.Fakes;
using FirstContact.Domain.Knowledge;
using FirstContact.Domain.Requests;

namespace FirstContact.Application.Tests.Requests;

/// <summary>
/// The whole pipeline, running against a fake classifier. No Azure, no database,
/// so the rules can be checked in milliseconds.
/// </summary>
public class HandleIncomingMessageTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 9, 0, 0, TimeSpan.Zero);

    private readonly FakeClassifier _classifier = new();
    private readonly InMemoryRequestRepository _repository = new();
    private readonly TriageOptions _options = new();
    private readonly StubKnowledgeSearch _search = new();
    private readonly StubAnswerWriter _writer = new();
    private readonly InMemoryKnowledgeGapRepository _gaps = new();
    private readonly StubRedactor _redactor = new();
    private readonly StubFeatureMatcher _matcher = new();
    private readonly InMemoryFeatureDemandRepository _demands = new();

    [Fact]
    public async Task A_confident_question_goes_straight_to_self_service()
    {
        _classifier.Answer = Of(RequestType.Question, 0.95);

        CustomerRequest request = await Handle("Wie exportiere ich ein Dokument?");

        Assert.Equal(Destination.SelfService, request.Destination);
        Assert.Equal(RequestStatus.Routed, request.Status);
        Assert.False(request.DecidedByHuman);
    }

    [Theory]
    [InlineData(RequestType.Problem, Destination.SupportTeam)]
    [InlineData(RequestType.FeatureRequest, Destination.ProductTeam)]
    [InlineData(RequestType.Other, Destination.SupportTeam)]
    public async Task Each_type_reaches_the_team_that_handles_it(RequestType type, Destination expected)
    {
        _classifier.Answer = Of(type, 0.9);

        CustomerRequest request = await Handle("Eine Nachricht");

        Assert.Equal(expected, request.Destination);
    }

    [Fact]
    public async Task An_unsure_classification_waits_for_a_person()
    {
        _classifier.Answer = Of(RequestType.Problem, 0.4);

        CustomerRequest request = await Handle("Der Export dauert ewig, geht das schneller?");

        Assert.Equal(Destination.HumanReview, request.Destination);
        Assert.Equal(RequestStatus.AwaitingReview, request.Status);
    }

    [Fact]
    public async Task A_classification_exactly_at_the_threshold_is_acted_on()
    {
        _classifier.Answer = Of(RequestType.Question, 0.70);

        CustomerRequest request = await Handle("Eine Frage");

        Assert.Equal(Destination.SelfService, request.Destination);
    }

    [Fact]
    public async Task The_threshold_can_be_raised_so_more_requests_reach_a_person()
    {
        _options.ConfidenceThreshold = 0.90;
        _classifier.Answer = Of(RequestType.Question, 0.80);

        CustomerRequest request = await Handle("Eine Frage");

        Assert.Equal(Destination.HumanReview, request.Destination);
    }

    [Fact]
    public async Task An_unusable_answer_from_the_model_reaches_a_person_instead_of_guessing()
    {
        _classifier.Answer = Classification.Unclear("Antwort war nicht auswertbar");

        CustomerRequest request = await Handle("Eine Nachricht");

        Assert.Equal(Destination.HumanReview, request.Destination);
    }

    [Fact]
    public async Task The_request_is_stored_with_the_time_it_arrived()
    {
        CustomerRequest request = await Handle("Eine Frage");

        Assert.Same(request, Assert.Single(_repository.All));
        Assert.Equal(1, _repository.Saves);
        Assert.Equal(Now, request.ReceivedAt);
    }

    [Fact]
    public async Task The_classifier_sees_the_trimmed_message()
    {
        await Handle("   Wie exportiere ich?   ");

        Assert.Equal("Wie exportiere ich?", _classifier.SeenMessage);
        Assert.Equal(1, _classifier.Calls);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task An_empty_message_is_rejected_before_the_model_is_called(string? message)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => Handle(message!));

        Assert.Equal(0, _classifier.Calls);
        Assert.Empty(_repository.All);
    }

    private async Task<CustomerRequest> Handle(string message)
    {
        var answering = new AnswerFromHandbook(
            _search, _writer, _gaps, new FixedClock(Now), new KnowledgeOptions());

        var grouping = new GroupFeatureWish(_matcher, _demands, new FixedClock(Now));

        TriagedRequest triaged = await new HandleIncomingMessage(
                _redactor, _classifier, _repository, answering, grouping, new FixedClock(Now), _options)
            .HandleAsync(message, CancellationToken.None);

        return triaged.Request;
    }

    [Fact]
    public async Task The_model_only_ever_sees_the_redacted_text()
    {
        _redactor.Replacement = "Mein Name ist [PERSON], meine Adresse [EMAIL]";

        await Handle("Mein Name ist Anna Schmidt, meine Adresse anna@firma.de");

        Assert.Equal("Mein Name ist [PERSON], meine Adresse [EMAIL]", _classifier.SeenMessage);
    }

    [Fact]
    public async Task The_original_wording_is_still_stored_for_the_support_team()
    {
        _redactor.Replacement = "Mein Name ist [PERSON]";

        CustomerRequest request = await Handle("Mein Name ist Anna Schmidt");

        Assert.Equal("Mein Name ist Anna Schmidt", request.Message);
    }

    [Fact]
    public async Task A_message_that_could_not_be_cleaned_goes_to_a_person_untouched_by_the_model()
    {
        // Failing closed: no answer is better than sending personal data on.
        _redactor.Fails = true;

        CustomerRequest request = await Handle("Mein Name ist Anna Schmidt");

        Assert.Equal(Destination.HumanReview, request.Destination);
        Assert.Equal(0, _classifier.Calls);
    }

    private static Classification Of(RequestType type, double confidence) =>
        Classification.Create(type, Confidence.Create(confidence), Urgency.Medium, "Testfall");
}
