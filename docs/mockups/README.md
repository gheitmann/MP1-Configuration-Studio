# Tab design exploration

`tab-styles.png` contains ten compact tab-navigation variants for the dark theme. They were rendered with the real palette and Segoe UI sizes at 100%.

| # | Variant | Character |
| --- | --- | --- |
| 1 | Quiet underline | No box around inactive tabs; active tab uses accent text and a 2 px underline. |
| 2 | Plain text | Quietest option; active text is accent-colored, with no underline or fill. |
| 3 | Segmented pill | Active tab is a filled accent pill; good for touch, visually stronger. |
| 4 | Soft chips | Active chip uses the selection surface plus an accent dot; calm but clearly grouped. Implemented in 1.7. |
| 5 | Folder tabs | Thin outlined tabs; closer to a classic settings dialog. |
| 6 | Breadcrumb row | Tabs read as a path rather than buttons; very quiet, but less discoverable. |
| 7 | Icon + label | Adds a small icon per tab; helps scanning, needs a consistent icon set. |
| 8 | Uppercase bar | Compact uppercase labels; space-efficient but less friendly. |
| 9 | Fine cards | Thin cards with an accent outline for the active tab; structured but slightly busier. |
| 10 | Minimal with counts | Underline plus small counts; useful where sections contain many settings. |

`shell-100-current.png` shows the current shell at 100% with the implemented variant 4. `welcome-100.png` shows the readable first-run dialog without a select-all highlight.

## Font comparison

`font-styles.png` compares ten type families. Each mockup applies the family to the whole window: navigation, toolbar, tabs, page content, help column, and buttons. Individual 1280×720 versions are in `fonts/`.

| # | Typeface | Note |
| --- | --- | --- |
| 1 | Segoe UI | Current Windows UI font; regular heading. |
| 2 | Segoe UI Variable | Windows 11, Text body + Display heading. |
| 3 | Segoe UI + Segoe UI Semibold | Implemented: same body as before, stronger heading. |
| 4 | Calibri + Calibri Light | Familiar Office look; smaller apparent size. |
| 5 | Calibri + Bold | Compact, stronger heading. |
| 6 | Candara + Light | Softer, rounded forms. |
| 7 | Corbel + Light | Humanist forms. |
| 8 | Tahoma | Classic Windows UI. |
| 9 | Arial | Neutral, widely available. |
| 10 | Verdana | Very readable but wider. |
