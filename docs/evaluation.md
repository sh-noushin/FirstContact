# Classification evaluation

- **Messages:** 40
- **Accuracy:** 90%
- **Handled automatically and correctly:** 85%
- **Sent to a person:** 7.5%
- **Confidently wrong:** 3

## By type

| Expected | Messages | Correct | Average confidence |
|---|---|---|---|
| FeatureRequest | 10 | 9 | 0.88 |
| Other | 10 | 8 | 0.86 |
| Problem | 10 | 10 | 0.89 |
| Question | 10 | 9 | 0.85 |

## Mistakes

| Message | Expected | Got | Confidence | Caught by the threshold |
|---|---|---|---|---|
| Wie lange werden signierte Dokumente aufbewahrt? | Question | Other | 0.90 | **no** |
| Können wir Dokumente auch per Smartphone freigeben? | FeatureRequest | Question | 0.90 | **no** |
| Wir sind sehr unzufrieden, das muss sich ändern. | Other | Problem | 0.70 | **no** |
| Können Sie mir helfen? | Other | Question | 0.60 | yes |
