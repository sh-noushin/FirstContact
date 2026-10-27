# FirstContact

AI-assisted triage for customer support requests.

Every incoming message is classified and sent where it belongs. Questions the
handbook covers are answered automatically with their sources. Anything the
system is not sure about goes to a person, and anything it could not answer
becomes a documentation task.

The goal is **first contact resolution**: resolve it on the first message where
possible, and learn from the times it is not.

---

## What it does

A customer writes in. The system then:

```
message
   │
   ▼
classify ──── confidence < 0.70 ───► a person decides
   │
   ├─ Frage          ─► answered from the handbook, with sources
   ├─ Problem        ─► support team
   ├─ Funktionswunsch ─► product team, grouped with similar wishes
   └─ Sonstiges      ─► support team
```

Five screens:

| Screen | What it shows |
|---|---|
| **Anfrage** | Send a message, see how it was classified and the handbook answer |
| **Manuelle Prüfung** | Requests the classifier was unsure about; one click sets the type |
| **Funktionswünsche** | Similar wishes grouped, so demand is visible per idea |
| **Wissenslücken** | Questions the handbook could not answer, ranked by how often |
| **Kennzahlen** | Self-service rate, routing split, average confidence |

---

## The rules it enforces

These are not conventions; each one has tests.

| Rule | Where it lives |
|---|---|
| Below **0.70** confidence nothing is automated; exactly 0.70 proceeds | `RoutingPolicy` |
| A request is routed **once**, and nothing happens before it is classified | `CustomerRequest` |
| An answer **without a source is not an answer** | `GroundedAnswer` |
| A source the search never returned is **dropped**, and if none survive the answer is discarded | `GroundedAnswer.KeepOnlyRealSources` |
| An unanswered question becomes **one** knowledge gap with a count, not many rows | `AnswerFromHandbook` |
| The same request can never be counted twice in a feature demand | `FeatureDemand.Record` |
| Personal data is removed **before** anything reaches the model | `HandleIncomingMessage` |
| If redaction fails, the message goes to a person rather than to the model | same |
| The self-service rate counts questions the bot **really answered**, not those it merely received | `CustomerRequest.AnsweredByBot` |

The last one matters more than it looks: "reached the bot" and "the bot answered"
are different numbers, and only the second one is worth reporting.

---

## Azure services used

| Service | Used for |
|---|---|
| `gpt-5-mini` | classifying a message, writing handbook answers |
| `text-embedding-3-small` | finding the right handbook article, grouping similar wishes |
| Azure AI Language | removing personal data before it reaches the model |

Grouping and search need **no model call** &mdash; they are arithmetic on vectors
that already exist. Only classification and answering cost tokens.

---

## Running it

You need the .NET 10 SDK, Node 20 or newer, and an Azure AI Services resource
with `gpt-5-mini` and `text-embedding-3-small` deployed.

Sign in once. There is no API key in the repository: authentication uses your
own identity.

```bash
az login
```

Copy the settings template and fill in your own endpoints:

```bash
cp src/FirstContact.Api/appsettings.Development.Example.json \
   src/FirstContact.Api/appsettings.Development.json
```

Then start both halves, in two terminals:

```bash
cd src/FirstContact.Api && dotnet run     # http://localhost:5080
cd web && npm install && npx ng serve     # http://localhost:4300
```

Open <http://localhost:4300>. The SQLite database and the handbook are created
on first start, so there is nothing else to set up.

### Try this

1. *"Wie lasse ich mehrere Personen nacheinander unterschreiben?"*
   &rarr; answered from the handbook, with its source. Note that the question and
   the article share almost no words: the embeddings matched the meaning.
2. *"Gibt es eine Schnittstelle zu SAP?"*
   &rarr; no answer invented; it appears under **Wissenslücken**.
3. *"Können Sie mir helfen?"*
   &rarr; too vague, so it lands in **Manuelle Prüfung**.
4. Two differently worded feature wishes
   &rarr; one group with two customers under **Funktionswünsche**.

---

## How it is put together

Four layers, with references pointing inward only. A test reads the project
files and fails if that is ever broken.

```
src/FirstContact.Domain          the rules. No framework, no Azure, no database
src/FirstContact.Application     use cases, and the ports they need
src/FirstContact.Infrastructure  Azure and SQLite behind those ports
src/FirstContact.Api            minimal API, composition root
web/                            Angular 22, one component per screen
```

Everything Azure sits behind an interface:

```csharp
public interface IClassifier
{
    Task<Classification> ClassifyAsync(string message, CancellationToken cancellationToken);
}
```

That is why the rules can be tested in milliseconds with no key, no network and
no cost &mdash; and why swapping `gpt-5-mini` for another model would change one
class and nothing else.

### Tests

```
Domain          76   rules, in isolation
Application     41   use cases against fakes
Infrastructure  38   real SQLite, real parsing
Api             13   the real app in memory, over HTTP
Angular         55   components and their rendered output
                ───
                223
```

No test calls Azure. The two model clients are deliberately thin so that the
parsing &mdash; the part that can go wrong &mdash; is pure and covered.

---

## Decisions worth knowing about

**The model is untrusted input.** Whatever it returns, the parsers never throw.
Unreadable JSON, an invented request type, a confidence of `1.4` or of `"hoch"`
&mdash; each one degrades to something safe, and an unusable answer always ends up
with a person.

**Failing closed.** If the Language service cannot be reached, the message is not
sent to the model at all. If the classifier cannot be reached, the request goes
to review. The customer's message is never lost and never guessed at.

**Timestamps are stored as UTC ticks.** SQLite cannot sort by `DateTimeOffset`,
which only surfaces once something is ordered.

**Enums are stored as text.** The database stays readable and reordering an enum
member cannot silently corrupt old rows.

**Grouping uses embeddings, not a model call.** Asking an LLM "is this the same
wish?" costs tokens, scales badly and is less predictable than cosine similarity
on vectors already computed.

---

## What is deliberately not here

This is a portfolio project, so some things are out of scope and said plainly
rather than hidden:

- **No sign-in.** It must not be put on a public URL as it stands.
- **`EnsureCreated`, not migrations.** Delete `firstcontact.db` to reset the
  schema. A real deployment would use EF Core migrations.
- **Search is in memory.** Every article is compared on every question. That is
  fine for a handbook of this size; for a large one, `VectorSearch` is the single
  class that Azure AI Search would replace.
- **Staff cannot yet edit handbook articles** from the UI. The knowledge gaps
  page shows what to write; adding it is the obvious next step.
