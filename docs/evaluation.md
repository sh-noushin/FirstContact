# Classification evaluation

- **Messages:** 40
- **Accuracy:** 95%
- **Handled automatically and correctly:** 82.5%
- **Sent to a person:** 15%
- **Confidently wrong:** 1

## By type

| Expected | Messages | Correct | Average confidence |
|---|---|---|---|
| FeatureRequest | 10 | 8 | 0.84 |
| Other | 10 | 10 | 0.70 |
| Problem | 10 | 10 | 0.85 |
| Question | 10 | 10 | 0.83 |

## Mistakes

| Message | Expected | Got | Confidence | Caught by the threshold |
|---|---|---|---|---|
| Können wir Dokumente auch per Smartphone freigeben? | FeatureRequest | Question | 0.90 | **no** |
| Das ist alles nicht besonders benutzerfreundlich. | FeatureRequest | Other | 0.30 | yes |
